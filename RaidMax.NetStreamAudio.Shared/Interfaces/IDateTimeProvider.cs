using System;
using System.Collections.Generic;
using System.Text;

namespace RaidMax.NetStreamAudio.Shared.Interfaces
{
    /// <summary>
    /// Defines the capabilites of an IDateTimeProvider
    /// </summary>
    public interface IDateTimeProvider
    {
        /// <summary>
        /// The Date and Time now!
        /// </summary>
        DateTime CurrentDateTime { get; }
    }
}
