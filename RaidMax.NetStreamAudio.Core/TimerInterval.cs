using RaidMax.NetStreamAudio.Core.Helpers;
using RaidMax.NetStreamAudio.Shared.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RaidMax.NetStreamAudio.Core
{
    /// <summary>
    /// Implementation of ITimerInterval.
    /// Uses System.Threading.Timer for implementation.
    /// </summary>
    public class TimerInterval : ITimerInterval
    {
        public ManualResetEventSlim StopFinished { get; } = new ManualResetEventSlim(true);
        public event EventHandler<EventArgs> OnTimerTick;
        private readonly Timer _timer;
        private readonly int _interval;

        public TimerInterval(int interval)
        {
            _timer = new Timer(new TimerCallback(TimerTicked));
            _interval = interval;
        }

        /// <inheritdoc/>
        public async Task<IStopResult> Start(CancellationToken token)
        {
            StopFinished.Reset();
            _timer.Change(_interval, _interval);

            try
            {
                await Task.Delay(-1, token);
            }

            catch { }

            finally
            {
                Stop();
            }

            return new StopResult();
        }

        /// <summary>
        /// Performs stop logic when exiting
        /// </summary>
        private void Stop()
        {
            if (!StopFinished.IsSet)
            {
                _timer.Change(0, Timeout.Infinite);
                _timer.Dispose();
                StopFinished.Set();
            }
        }

        /// <summary>
        /// Event callback executed when the timer interval has elapsed
        /// </summary>
        /// <param name="state">state object of the timer</param>
        private void TimerTicked(object state)
        {
            if (!StopFinished.IsSet)
            {
                OnTimerTick?.Invoke(this, new EventArgs());
            }
        }
    }
}
