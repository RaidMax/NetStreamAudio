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
        private CancellationToken token;

        public TimerInterval(int interval)
        {
            _timer = new Timer(new TimerCallback(TimerTicked));
            _interval = interval;
        }

        /// <inheritdoc/>
        public Task Start(CancellationToken token)
        {
            StopFinished.Reset();
            this.token = token;
            _timer.Change(_interval, _interval);
 
            return Task.CompletedTask;
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
            if (!token.IsCancellationRequested)
            {
                OnTimerTick?.Invoke(this, new EventArgs());
            }

            else
            {
                Stop();
            }
        }
    }
}
