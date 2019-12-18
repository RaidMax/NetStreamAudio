using RaidMax.NetStreamAudio.Shared.Interfaces;

namespace RaidMax.NetStreamAudio.Core.Events
{
    /// <summary>
    /// Implementation of IAudioClientEventArgs
    /// </summary>
    public class AudioClientEventArgs : IAudioClientEventArgs
    {
        /// <inheritdoc/>
        public byte[] Buffer { get; set; }

        /// <inheritdoc/>
        public int BytesReceived { get; set; }
    }
}
