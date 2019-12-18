namespace RaidMax.NetStreamAudio.Shared.Configuration
{
    /// <summary>
    /// Specifies configuration options for an audio player application
    /// </summary>
    public class AudioPlayerConfiguration
    {
        /// <summary>
        /// Buffer latency for playback
        /// <remarks>short values can cause clipping, large values will cause delay in playback</remarks>
        /// </summary>
        public int TargetLatencyMilliseconds { get; set; }
    }
}
