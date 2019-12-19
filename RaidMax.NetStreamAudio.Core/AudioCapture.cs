using Microsoft.Extensions.Logging;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using RaidMax.NetStreamAudio.Core.Events;
using RaidMax.NetStreamAudio.Core.Helpers;
using RaidMax.NetStreamAudio.Shared.Configuration;
using RaidMax.NetStreamAudio.Shared.Enumerations;
using RaidMax.NetStreamAudio.Shared.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RaidMax.NetStreamAudio.Core
{
    public class AudioCapture : IAudioCapture
    {
        public ManualResetEventSlim StopFinished { get; } = new ManualResetEventSlim();
        private WasapiLoopbackCapture captureInstance;
        private readonly ILogger _logger;
        private readonly IAudioServer _audioServer;
        private readonly NetStreamAudioConfiguration _config;
        private WaveFormat targetFormat;

        public AudioCapture(ILogger<AudioCapture> logger, NetStreamAudioConfiguration config, IAudioServer audioServer)
        {
            _logger = logger;
            _audioServer = audioServer;
            _config = config;
        }

        /// <inheritdoc/>
        public async Task<IStopResult> Start(CancellationToken token)
        {
            if (captureInstance != null)
            {
                throw new InvalidOperationException("Capture must be stopped before starting");
            }

            _logger.LogDebug("Starting AudioCapture");

            var stopResult = new StopResult();

            try
            {
                captureInstance = new WasapiLoopbackCapture()
                {
                    ShareMode = NAudio.CoreAudioApi.AudioClientShareMode.Shared
                };

                targetFormat = new WaveFormat(_config.AudioCapture.DesiredSampleRate, _config.AudioCapture.DesiredChannelCount);

                captureInstance.DataAvailable += OnDataAvailable;
                captureInstance.RecordingStopped += OnCaptureStopped;
                captureInstance.StartRecording();

                var audioServerResult = await _audioServer.Start(token);
                stopResult.ResultType = audioServerResult.ResultType;
            }

            catch (TaskCanceledException)
            {

            }

            catch (OperationCanceledException)
            {

            }

            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error in AudioCapture");
                stopResult.ResultType = StopResultType.Unexpected;
            }

            finally
            {
                Stop();
            }

            return stopResult;
        }

        /// <summary>
        /// Event callback executed when audio stops being captured from the source device
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCaptureStopped(object sender, StoppedEventArgs e)
        {
            _logger.LogDebug("Stopped capturing");

            if (e.Exception != null)
            {
                _logger.LogError(e.Exception, "Capture stopped because of an error");
            }
        }

        /// <summary>
        /// Event callback executed when audio data from the device buffer is available
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            if (e.BytesRecorded > 0)
            {
                var buffer = new byte[e.BytesRecorded];
                Buffer.BlockCopy(e.Buffer, 0, buffer, 0, e.BytesRecorded);

                /* todo: this needs to actually work
                // we want to treat our sample as a raw stream
                var sourceStream = new RawSourceWaveStream(e.Buffer, 0, e.BytesRecorded, captureInstance.WaveFormat);
                // we want to convert from the captureInstance format to the desired output format
                using var resampler = new ResamplerDmoStream(sourceStream, targetFormat);

                int result = resampler.Read(buffer, 0, buffer.Length);
                _logger.LogDebug("Resample read result {0}", result);*/

                try
                {
                    await _audioServer.Serve(new AudioCaptureEventArgs()
                    {
                        Buffer = buffer,
                        BytesRecorded = buffer.Length
                    });
                }

                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to serve sound data to audio server");
                }
            }
        }

        /// <inheritdoc/>
        private void Stop()
        {
            _audioServer.StopFinished.Wait();

            _logger.LogDebug("Stopping AudioCapture");
            captureInstance.DataAvailable -= OnDataAvailable;
            captureInstance.StopRecording();
            captureInstance.Dispose();

            StopFinished.Set();
            // todo: do we want to unhook callback events?
        }
    }
}
