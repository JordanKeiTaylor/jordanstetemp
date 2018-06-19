using System;
using System.Diagnostics;
using System.Threading;

namespace Improbable.Shared
{
    public class ExponentialBackoff
    {
        private readonly TimeSpan _maximum;
        private readonly TimeSpan _minimum;
        private readonly double _multiple;
        private readonly TimeSpan _safeTime;
        private TimeSpan _nextDelay;
        private readonly Stopwatch _lastDelayTime;

        /// <summary>
        /// Wraps the logic to implement an Exponetial Backoff in the case of errors.
        /// 
        /// For example: 
        /// 
        /// private readonly ExponentialBackoff backoff = new ExponentialBackoff(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(60), 2.0, TimeSpan.FromSeconds(120));
        /// 
        /// whill initially wait 1 second, it'll double each time it's called, untill it reaches 60 seconds.
        /// It'll reset the time out when the Delay function doesn't get called for 120 seconds.
        /// 
        /// </summary>
        /// <returns>ExponentialBackoff.</returns>
        /// <param name="min">The minimum time to wait before attempting a reconnection.</param>
        /// <param name="max">The maximum time to wait before attempting a reconnection.</param>
        /// <param name="multiple">The amount to increace the delay when faliures occour.</param>
        /// <param name="safeTime">The time between faliures that resets the delay time.</param>
        public ExponentialBackoff(TimeSpan min, TimeSpan max, double multiple, TimeSpan safeTime)
        {
            _minimum = min;
            _maximum = max;
            _multiple = multiple;
            _safeTime = safeTime;
            _nextDelay = _minimum;
            _lastDelayTime = Stopwatch.StartNew();
        }

        /// <summary>
        /// The main interface to this class once created - call this on faliure and it'll sleep for
        /// an amount of time between min and max.
        /// 
        /// </summary>
        public void Delay()
        {
            CalculateNextDelay();
            Thread.Sleep(_nextDelay);
        }

        private void CalculateNextDelay()
        {
            if (IsWithinSafeTime())
            {
                _nextDelay = _minimum;
            }
            else
            {
                _nextDelay = TimeSpan.FromTicks(Math.Min(_maximum.Ticks, (long)(_nextDelay.Ticks * _multiple)));
            }
        }

        private bool IsWithinSafeTime()
        {
            if (_lastDelayTime.ElapsedMilliseconds < (_safeTime.TotalMilliseconds + _nextDelay.TotalMilliseconds))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Alternative option for Delay() - this version will throw the exception if it hit's the maximum time limit.
        /// </summary>
        public void DelayOrThrowException(Exception e)
        {
            CalculateNextDelay();
            if (_nextDelay.Ticks < _maximum.Ticks)
            {
                Thread.Sleep(_nextDelay);
            }
            else
            {
                throw e;
            }
        }

        public bool AtMaximum()
        {
            return (_nextDelay.Ticks < _maximum.Ticks);
        }

        public TimeSpan TaskDelay()
        {
            CalculateNextDelay();
            return _nextDelay;
        }
    }
}
