using System;

namespace RaidMax.NetStreamAudio.Shared.Interfaces
{
    /// <summary>
    /// Defines the capabilities of a timer interval
    /// </summary>
    public interface ITimerInterval : IStartStop
    {
        /// <summary>
        /// Fires when the timer ticks
        /// </summary>
        event EventHandler<EventArgs> OnTimerTick;
    }
}
