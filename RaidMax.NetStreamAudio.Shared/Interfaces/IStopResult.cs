using RaidMax.NetStreamAudio.Shared.Enumerations;

namespace RaidMax.NetStreamAudio.Shared.Interfaces
{
    /// <summary>
    /// Describes the results from a stop operation
    /// </summary>
    public interface IStopResult
    {
        /// <summary>
        /// Indicates the state of the stop result
        /// </summary>
        public StopResultType ResultType { get; set; }
    }
}
