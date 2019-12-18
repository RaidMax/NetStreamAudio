using System.Collections.Generic;

namespace RaidMax.NetStreamAudio.Shared.Configuration
{
    /// <summary>
    /// Specifies the base configuration options for the entire application configuration file
    /// </summary>
    public class NetStreamAudioConfiguration
    {
        /// <summary>
        /// Collection of audio server configurations
        /// </summary>
        public Dictionary<string, AudioServerConfiguration> ServerTypes { get; set; }

        /// <summary>
        /// Collection of audio client configurations
        /// </summary>
        public Dictionary<string, AudioClientConfiguration> ClientTypes { get; set; }

        /// <summary>
        /// Configuration for the Audio Player
        /// </summary>
        public AudioPlayerConfiguration AudioPlayer { get; set; }

        /// <summary>
        /// Configuration for the Audio Capture
        /// </summary>
        public AudioCaptureConfiguration AudioCapture { get; set; }

        /// <summary>
        /// Specifies the level to be logged
        /// </summary>
        public string LogLevel { get; set; }
    }
}
