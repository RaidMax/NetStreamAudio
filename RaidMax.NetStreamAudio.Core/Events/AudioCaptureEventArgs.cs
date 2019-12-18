using RaidMax.NetStreamAudio.Shared.Interfaces;
using System;

namespace RaidMax.NetStreamAudio.Core.Events
{
    /// <summary>
    /// Implementation of IAudioCaptureEventARgs
    /// </summary>
    public class AudioCaptureEventArgs : EventArgs, IAudioCaptureEventArgs
    {
        /// <inheritdoc/>
        public byte[] Buffer { get; set; }

        /// <inheritdoc/>
        public int BytesRecorded { get; set; }
    }
}
