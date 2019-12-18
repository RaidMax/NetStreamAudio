using Microsoft.Extensions.Logging;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using RaidMax.NetStreamAudio.Shared.Configuration;
using RaidMax.NetStreamAudio.Shared.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RaidMax.NetStreamAudio.Core
{
    public class AudioPlayer : IAudioPlayer
    {
        public ManualResetEventSlim StopFinished { get; } = new ManualResetEventSlim(true);
        private readonly ILogger _logger;
        private readonly IAudioClient _audioClient;
        private readonly NetStreamAudioConfiguration _config;
        private WaveOutEvent waveOut;
        private BufferedWaveProvider waveProvider;

        public AudioPlayer(ILogger<AudioPlayer> logger, NetStreamAudioConfiguration config, IAudioClient audioClient)
        {
            _logger = logger;
            _audioClient = audioClient;
            _config = config;
        }

        /// <inheritdoc/>
        public async Task Start(CancellationToken token)
        {
            _logger.LogDebug("Starting AudioPlayer");

            StopFinished.Reset();
            _audioClient.OnAudioReceived += OnAudioReceived;

            waveOut = new WaveOutEvent()
            {
                Volume = 1.0f,
                DesiredLatency = _config.AudioPlayer.TargetLatencyMilliseconds
            };

            // we want to know which device we're playing on so we can set the format
            // todo: allow user's to specify which device they want to use
            using var deviceEnumerator = new MMDeviceEnumerator();
            var defaultDevice = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            var deviceMixFormat = defaultDevice.AudioClient.MixFormat;
            var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(deviceMixFormat.SampleRate, deviceMixFormat.Channels);

            _logger.LogInformation("Playing on {0} at {1}:{2}", defaultDevice.FriendlyName, deviceMixFormat.SampleRate, deviceMixFormat.Channels);

            waveProvider = new BufferedWaveProvider(waveFormat);
            waveOut.Init(waveProvider);
            waveOut.Play();

            try
            {
                await _audioClient.Start(token);
            }

            catch (TaskCanceledException)
            {

            }

            catch (OperationCanceledException)
            {

            }

            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error in AudioPlayer");
            }

            finally
            {
                Stop();
            }
        }

        /// <summary>
        /// Performs stop logic when exiting
        /// </summary>
        private void Stop()
        {
            _audioClient.OnAudioReceived -= OnAudioReceived;
            _audioClient.StopFinished.Wait();

            _logger.LogDebug("Stopping AudioPlayer");

            waveOut.Stop();
            waveOut.Dispose();
            StopFinished.Set();
        }

        /// <summary>
        /// Event callback executed when audio is received from the audio client
        /// </summary>
        /// <param name="sender">source of the event</param>
        /// <param name="e">event args</param>
        private void OnAudioReceived(object sender, IAudioClientEventArgs e)
        {
            try
            {
                waveProvider.AddSamples(e.Buffer, 0, e.Buffer.Length);
            }

            catch (InvalidOperationException)
            {
                _logger.LogWarning("Encountered full buffer when adding samples to BufferedWaveProvider");
                _logger.LogDebug("Buffer size is {0} bytes", waveProvider.BufferLength);
                _logger.LogDebug("New audio data is {0} bytes", e.Buffer.Length);
            }
        }
    }
}
