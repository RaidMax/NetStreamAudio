namespace RaidMax.NetStreamAudio.Shared.Configuration
{
    /// <summary>
    /// Specifies configuration options for an audio server application
    /// </summary>
    public class AudioServerConfiguration 
    {
        /// <summary>
        /// Port to bind on (if using a network based server)
        /// </summary>
        public int Port { get; set; }
    }
}
