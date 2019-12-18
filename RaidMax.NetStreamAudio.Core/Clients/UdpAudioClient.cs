using Microsoft.Extensions.Logging;
using RaidMax.NetStreamAudio.Core.Events;
using RaidMax.NetStreamAudio.Core.Helpers;
using RaidMax.NetStreamAudio.Shared.Configuration;
using RaidMax.NetStreamAudio.Shared.Enumerations;
using RaidMax.NetStreamAudio.Shared.Interfaces;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RaidMax.NetStreamAudio.Core.Players
{
    /// <summary>
    /// UDP Implementation of IAudioClient
    /// </summary>
    public class UdpAudioClient : IAudioClient
    {
        public event EventHandler<IAudioClientEventArgs> OnAudioReceived;
        public ManualResetEventSlim StopFinished { get; } = new ManualResetEventSlim(true);
        private readonly AudioClientConfiguration _config;
        private readonly IPEndPoint _serverEndpoint;
        private readonly ILogger _logger;
        private readonly ITimerInterval _timerInterval;
        private const int KEEPALIVE_INTERVAL = 5000;
        private UdpClient udpClient;

        public UdpAudioClient(ILogger<UdpAudioClient> logger, Func<string, AudioClientConfiguration> configurationResolver)
        {
            _logger = logger;
            _config = configurationResolver(typeof(UdpAudioClient).Name);
            // we want to get the IPAddress of the host 
            // todo: definitely need to support IPv6 
            // todo: this might be better outside of the constructor
            var serverHost = Dns.GetHostAddresses(_config.Host)
                .First(_host => _host.AddressFamily == AddressFamily.InterNetwork);
            _serverEndpoint = new IPEndPoint(serverHost, _config.Port);
            _timerInterval = new TimerInterval(KEEPALIVE_INTERVAL);
            _timerInterval.OnTimerTick += OnTimerTick;
        }

        /// <inheritdoc/>
        public async Task Start(CancellationToken token)
        {
            _logger.LogDebug("Starting UdpAudioClient");

            StopFinished.Reset();

            if (udpClient != null)
            {
                throw new InvalidOperationException("Client must be stopped before starting");
            }

            udpClient = new UdpClient();
            _logger.LogDebug("Attaching to {0}", _serverEndpoint.ToString());

            var attachCommand = GenerateAttachCommand().GeneratePayload();

            try
            {
                await udpClient.SendAsync(attachCommand, attachCommand.Length, _serverEndpoint);

                var state = new UdpSocketState()
                {
                    UdpClient = udpClient,
                    RemoteEndPoint = _serverEndpoint
                };

                await _timerInterval.Start(token);

                while (true)
                {
                    udpClient.BeginReceive(new AsyncCallback(OnDataReceivedFromServer), state);
                    await state.ReceiveWaiter.WaitAsync(token);
                }
            }

            catch (TaskCanceledException)
            {

            }

            catch (OperationCanceledException)
            {

            }

            catch (SocketException e)
            {
                _logger.LogError(e, "Failed to communicate with {0}", _serverEndpoint.ToString());
            }

            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error in UdpAudioClient");
            }

            finally
            {
                await Stop();
            }
        }

        /// <summary>
        /// Performs stop logic when exiting
        /// </summary>
        private async Task Stop()
        {
            _logger.LogDebug("Stopping UdpAudioClient");

            var detachCommand = GenerateDetachCommand().GeneratePayload();
            await udpClient.SendAsync(detachCommand, detachCommand.Length, _serverEndpoint);

            udpClient.Dispose();
            StopFinished.Set();
        }

        /// <summary>
        /// Generates an attach command
        /// </summary>
        /// <returns></returns>
        private UdpAudioCommand GenerateAttachCommand()
        {
            return new UdpAudioCommand()
            {
                Command = AudioCommandType.Attach,
            };
        }

        /// <summary>
        /// Generates a detach command
        /// </summary>
        /// <returns></returns>
        private UdpAudioCommand GenerateDetachCommand()
        {
            return new UdpAudioCommand()
            {
                Command = AudioCommandType.Detach
            };
        }

        /// <summary>
        /// Generates a keep alive command
        /// </summary>
        /// <returns></returns>
        private UdpAudioCommand GenerateKeepAliveCommand()
        {
            return new UdpAudioCommand()
            {
                Command = AudioCommandType.KeepAlive,
                Data = DateTime.UtcNow.ToFileTimeUtc().ToString()
            };
        }

        /// <summary>
        /// Event callback executed when data is received from the socket
        /// </summary>
        /// <param name="result"></param>
        private void OnDataReceivedFromServer(IAsyncResult result)
        {
            var state = (UdpSocketState)result.AsyncState;
            var endpoint = state.RemoteEndPoint as IPEndPoint;
            byte[] receivedBytes = null;

            try
            {
                receivedBytes = state.UdpClient.EndReceive(result, ref endpoint);
            }

            catch (SocketException e)
            {
                _logger.LogError(e, "Failed to while receiving data from {0}", endpoint.ToString());
            }

            catch (ObjectDisposedException)
            {
                _logger.LogWarning("Receive socket disposed");
            }

            if (receivedBytes?.Length > 0)
            {
                OnAudioReceived?.Invoke(this, new AudioClientEventArgs()
                {
                    Buffer = receivedBytes,
                    BytesReceived = receivedBytes.Length
                });
            }

            state.ReceiveWaiter.Release(1);
        }

        /// <summary>
        /// Event callback executed when our interval timer ticks.
        /// Sends the 
        /// </summary>
        /// <param name="sender">source of the event</param>
        /// <param name="e">timer event args</param>
        private void OnTimerTick(object sender, EventArgs e)
        {
            var payload = GenerateKeepAliveCommand().GeneratePayload();
            _logger.LogDebug("Sending keep alive command");

            try
            {
                // since this is an event we don't need to use async
                udpClient.Send(payload, payload.Length, _serverEndpoint);
            }

            catch (SocketException ex)
            {
                _logger.LogError(ex, "Failed to send keep alive command");
            }
        }
    }
}
