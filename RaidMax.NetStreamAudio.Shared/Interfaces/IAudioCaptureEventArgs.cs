namespace RaidMax.NetStreamAudio.Shared.Interfaces
{
    /// <summary>
    /// Defines the event args when audio is captured
    /// </summary>
    public interface IAudioCaptureEventArgs
    {
        /// <summary>
        /// Buffer containing recorded data. Note that it might not be completely full.
        /// </summary>
        public byte[] Buffer { get; set; }

        /// <summary>
        /// The number of recorded bytes in Buffer.
        /// </summary>
        public int BytesRecorded { get; set; }
    }
}
