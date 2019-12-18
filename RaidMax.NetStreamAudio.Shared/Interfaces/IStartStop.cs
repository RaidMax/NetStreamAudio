using System.Threading;
using System.Threading.Tasks;

namespace RaidMax.NetStreamAudio.Shared.Interfaces
{
    /// <summary>
    /// Represents a start and stop mechanism
    /// </summary>
    public interface IStartStop
    {
        /// <summary>
        /// Starts and waits for some work to be completed
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns></returns>
        Task Start(CancellationToken token);

        /// <summary>
        /// Waiter that signals when the works is finished
        /// </summary>
        ManualResetEventSlim StopFinished { get; }
    }
}
