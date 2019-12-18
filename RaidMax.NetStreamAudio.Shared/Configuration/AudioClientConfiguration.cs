namespace RaidMax.NetStreamAudio.Shared.Configuration
{
    /// <summary>
    /// Specifies configuration options for an audio client application
    /// </summary>
    public class AudioClientConfiguration
    {
        /// <summary>
        /// Remote host of the audio server
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Remote port of the audio server
        /// </summary>
        public int Port { get; set; }
    }
}
