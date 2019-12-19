using Microsoft.Extensions.Logging;
using RaidMax.NetStreamAudio.Core.Helpers;
using RaidMax.NetStreamAudio.Shared.Configuration;
using RaidMax.NetStreamAudio.Shared.Enumerations;
using RaidMax.NetStreamAudio.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RaidMax.NetStreamAudio.Core.Servers
{
    /// <summary>
    /// Implementation of IAudioServer.
    /// Provides the functionality of an audio server using UDP
    /// </summary>
    public class UdpAudioServer : IAudioServer
    {
        public ManualResetEventSlim StopFinished { get; } = new ManualResetEventSlim(true);
        private const int MAX_CLIENT_ZOMBIE_TIME_SECONDS = 60;
        private readonly ILogger _logger;
        private readonly AudioServerConfiguration _config;
        private readonly Dictionary<string, UdpSocketState> _socketStates;
        private readonly IDateTimeProvider _dateTimeProvider;
        private IPEndPoint bindingEndpoint;
        private Socket udpServerSocket;
        private CancellationToken token;

        public UdpAudioServer(ILogger<UdpAudioServer> logger, Func<string, AudioServerConfiguration> configurationResolver, IDateTimeProvider dateTimeProvider)
        {
            _logger = logger;
            _config = configurationResolver(typeof(UdpAudioServer).Name);
            _socketStates = new Dictionary<string, UdpSocketState>();
            _dateTimeProvider = dateTimeProvider;
        }

        /// <inheritdoc/>
        public async Task<IStopResult> Start(CancellationToken token)
        {
            _logger.LogDebug("Starting UdpAudioServer");

            StopFinished.Reset();
            this.token = token;
            var stopResult = new StopResult();

            try
            {
                bindingEndpoint = new IPEndPoint(IPAddress.Any, _config.Port);

                udpServerSocket = new Socket(
                    addressFamily: bindingEndpoint.AddressFamily,
                    socketType: SocketType.Dgram,
                    protocolType: ProtocolType.Udp);
                udpServerSocket.Bind(bindingEndpoint);

                _logger.LogInformation("Binding to {0}", bindingEndpoint.ToString());
                _logger.LogInformation("Waiting for clients to connect");


                while (true)
                {
                    var newClientState = new UdpSocketState()
                    {
                        StateSocket = udpServerSocket
                    };

                    udpServerSocket.BeginReceiveFrom(newClientState.Buffer,
                        0, newClientState.Buffer.Length,
                        SocketFlags.None,
                        ref newClientState.RemoteEndPoint,
                        new AsyncCallback(OnDataReceivedFromClient),
                        newClientState);

                    await newClientState.ReceiveWaiter.WaitAsync(token);
                }
            }

            catch (TaskCanceledException)
            {

            }

            catch (OperationCanceledException)
            {

            }

            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error in UdpAudioServer");
                stopResult.ResultType = StopResultType.Unexpected;
            }

            finally
            {
                Stop();
            }

            return stopResult;
        }

        /// <inheritdoc/>
        public void ProcessCommand(IAudioCommand audioCommand)
        {
            string source = audioCommand.Source?.ToString();
            _logger.LogDebug("Received {0} command from {1}", audioCommand.Command.ToString(), source);

            // todo: we probably want to abtract this out somewhere else
            switch (audioCommand.Command)
            {
                case AudioCommandType.Attach:
                    AttachClient(audioCommand.Source);
                    break;
                case AudioCommandType.Detach:
                    DetachClient(audioCommand.Source);
                    break;
                case AudioCommandType.Start:
                case AudioCommandType.Stop:
                    throw new NotImplementedException();
                case AudioCommandType.KeepAlive:
                    UpdateLastMessageTime(audioCommand.Source);
                    break;
            }
        }

        /// <inheritdoc/>
        public async Task Serve(IAudioCaptureEventArgs audioEventArgs)
        {
            // todo: we probably don't need to evaluate this on every serve
            ClearExpiredClients();

            foreach (var state in _socketStates.Values)
            {
                int bytesSent = 0;

                while (bytesSent < audioEventArgs.BytesRecorded)
                {
                    int nextChunkSize = Math.Min(UdpSocketState.MAX_BUFFER_LENGTH, audioEventArgs.BytesRecorded - bytesSent);

                    try
                    {
                        udpServerSocket.BeginSendTo(audioEventArgs.Buffer, bytesSent, nextChunkSize, SocketFlags.None, state.RemoteEndPoint, new AsyncCallback(OnDataSentToClient), state);
                    }

                    catch (SocketException e)
                    {
                        _logger.LogWarning(e, "Could not send audio chunk to {0}, skipping...", state.RemoteEndPoint.ToString());
                        continue;
                    }

                    await state.SendWaiter.WaitAsync(token);
                    bytesSent += UdpSocketState.MAX_BUFFER_LENGTH;
                }
            }
        }

        /// <inheritdoc/>
        private void Stop()
        {
            _logger.LogDebug("Stopping UdpAudioServer");

            _socketStates.Clear();
            udpServerSocket.Close();
            udpServerSocket.Dispose();

            StopFinished.Set();
        }

        /// <summary>
        /// Clears all the clients that have not sent a keep alive since the max zombie time elapsed
        /// </summary>
        private void ClearExpiredClients()
        {
            lock (_socketStates)
            {
                var currentStateKeys = _socketStates.Keys.ToList();

                foreach (var key in currentStateKeys)
                {
                    if (_socketStates[key].LastMessageTime < _dateTimeProvider.CurrentDateTime.AddSeconds(-MAX_CLIENT_ZOMBIE_TIME_SECONDS))
                    {
                        _logger.LogInformation("Removing zombie client {0}", key);
                        _socketStates.Remove(key);
                    }
                }
            }
        }

        /// <summary>
        /// Attaches client to the socket state collection
        /// </summary>
        /// <param name="endpoint">source socket of the remote client</param>
        private void AttachClient(IPEndPoint endpoint)
        {
            string source = endpoint.ToString();

            if (!_socketStates.ContainsKey(source))
            {
                _logger.LogInformation("Adding {0} to remote endpoint list", source);
                _socketStates.Add(source, new UdpSocketState()
                {
                    RemoteEndPoint = endpoint,
                    StateSocket = udpServerSocket,
                    LastMessageTime = _dateTimeProvider.CurrentDateTime
                });
            }

            else
            {
                _logger.LogWarning("Found duplicate {0} when trying to attach", source);
            }
        }

        /// <summary>
        /// Detaches the source from the socket states
        /// </summary>
        /// <param name="endpoint">source socket of the remote client</param>
        private void DetachClient(EndPoint endpoint)
        {
            string source = endpoint.ToString();

            if (_socketStates.ContainsKey(source))
            {
                _logger.LogInformation("Removing {0} from remote endpoint list", source);
                _socketStates.Remove(source);
            }

            else
            {
                _logger.LogWarning("Could not find {0} when trying to detach", source);
            }
        }

        /// <summary>
        /// Updates the last time a message was received from the given endpoint
        /// </summary>
        /// <param name="endpoint">source socket of the remote client</param>
        private void UpdateLastMessageTime(EndPoint endpoint)
        {
            string source = endpoint.ToString();

            if (_socketStates.ContainsKey(source))
            {
                _socketStates[source].LastMessageTime = _dateTimeProvider.CurrentDateTime;
            }

            else
            {
                _logger.LogWarning("Could not find {0} when trying to update last message", source);
            }
        }

        /// <summary>
        /// Event callback executed when client data is recieved from the UDP socket
        /// </summary>
        /// <param name="result"></param>
        private void OnDataReceivedFromClient(IAsyncResult result)
        {
            var state = (UdpSocketState)result.AsyncState;
            int bytesRead = 0;

            try
            {
                bytesRead = state.StateSocket.EndReceiveFrom(result, ref state.RemoteEndPoint);
            }

            catch (SocketException e)
            {
                string source = state.RemoteEndPoint.ToString();
                _logger.LogError(e, "Could not complete receiving data from {0}, so we are dropping them", source);
                DetachClient(state.RemoteEndPoint);
            }

            catch (ObjectDisposedException)
            {
                _logger.LogWarning("Receive socket disposed");
            }

            if (bytesRead > 0)
            {
                var audioCommand = UdpAudioCommand.Parse(state.Buffer);
                audioCommand.Source = (IPEndPoint)state.RemoteEndPoint;
                ProcessCommand(audioCommand as IAudioCommand);
            }

            else
            {
                _logger.LogWarning("Expected to receive data, but received 0 bytes");
            }

            // we could finish receiving data before the original thread context starts waiting for us to finish
            if (state.ReceiveWaiter.CurrentCount == 0)
            {
                state.ReceiveWaiter.Release(1);
            }
        }

        /// <summary>
        /// Event callback executed when data has finished sending
        /// </summary>
        /// <param name="result"></param>
        private void OnDataSentToClient(IAsyncResult result)
        {
            var state = (UdpSocketState)result.AsyncState;
            try
            {
                int bytesSent = state.StateSocket.EndSendTo(result);
            }

            catch (SocketException e)
            {
                _logger.LogError(e, "Could not send data to {0}", state.RemoteEndPoint.ToString());
            }

            finally
            {
                state.SendWaiter.Release(1);
            }
        }
    }
}
