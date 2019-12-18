namespace RaidMax.NetStreamAudio.Shared.Interfaces
{
    /// <summary>
    /// Defines the arguments of an audio client event (data received)
    /// </summary>
    public interface IAudioClientEventArgs
    {
        /// <summary>
        /// Buffer containing transmitted audio data.
        /// </summary>
        public byte[] Buffer { get; set; }

        /// <summary>
        /// The number of transmitted bytes in Buffer.
        /// </summary>
        public int BytesReceived { get; set; }
    }
}
