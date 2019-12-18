namespace RaidMax.NetStreamAudio.Shared.Configuration
{
    /// <summary>
    /// Specifies configuration options for the audio capture application
    /// </summary>
    public class AudioCaptureConfiguration
    {
        /// <summary>
        /// Desired sample rate of the audio capture device
        /// </summary>
        public int DesiredSampleRate { get; set; }

        /// <summary>
        /// Desired channel count for the audio capture device
        /// </summary>
        public int DesiredChannelCount { get; set; }
    }
}
