using RaidMax.NetStreamAudio.Shared.Enumerations;
using RaidMax.NetStreamAudio.Shared.Interfaces;

namespace RaidMax.NetStreamAudio.Core.Helpers
{
    /// <summary>
    /// Implementation of IStopResult
    /// </summary>
    public class StopResult : IStopResult
    {
        /// <inheritdoc/>
        public StopResultType ResultType { get; set; } = StopResultType.Expected;
    }
}
