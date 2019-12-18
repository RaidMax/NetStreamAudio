using RaidMax.NetStreamAudio.Shared.Interfaces;
using System;

namespace RaidMax.NetStreamAudio.Core
{
    /// <summary>
    /// Implementation of IDateTimeProvider
    /// Provides the current date & time
    /// </summary>
    public class DateTimeProvider : IDateTimeProvider
    {
        /// <inheritdoc/>
        public DateTime CurrentDateTime => DateTime.UtcNow;
    }
}
