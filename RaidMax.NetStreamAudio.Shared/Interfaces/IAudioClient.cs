using System;

namespace RaidMax.NetStreamAudio.Shared.Interfaces
{
    /// <summary>
    /// Defines the capabilities of IAudioClient
    /// </summary>
    public interface IAudioClient : IStartStop
    {
        /// <summary>
        /// Fires when audio is received
        /// </summary>
        event EventHandler<IAudioClientEventArgs> OnAudioReceived;
    }
}
