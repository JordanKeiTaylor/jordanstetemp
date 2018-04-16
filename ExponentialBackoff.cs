using System;
using System.Diagnostics;
using System.Threading;

namespace Shared
{
    public class ExponentialBackoff
    {
        private readonly TimeSpan maximum;
        private readonly TimeSpan minimum;
        private readonly double multiple;
        private readonly TimeSpan safeTime;
        private TimeSpan nextDelay;
        private Stopwatch lastDelayTime;

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
            this.minimum = min;
            this.maximum = max;
            this.multiple = multiple;
            this.safeTime = safeTime;
            this.nextDelay = minimum;
            this.lastDelayTime = Stopwatch.StartNew();
        }

        /// <summary>
        /// The main interface to this class once created - call this on faliure and it'll sleep for
        /// an amount of time between min and max.
        /// 
        /// </summary>
        public void Delay()
        {
            CalculateNextDelay();
            Thread.Sleep(nextDelay);
        }

        private void CalculateNextDelay()
        {
            if (IsWithinSafeTime())
            {
                nextDelay = minimum;
            }
            else
            {
                nextDelay = TimeSpan.FromTicks(Math.Min(maximum.Ticks, (long)(nextDelay.Ticks * multiple)));
            }
        }

        private bool IsWithinSafeTime()
        {
            if (lastDelayTime.ElapsedMilliseconds < (safeTime.TotalMilliseconds + nextDelay.TotalMilliseconds))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Alternative option for Delay() - this version will throw the exception if it hit's the maximum time limit.
        /// </summary>
        public void DelayOrThrowException(Exception e)
        {
            CalculateNextDelay();
            if (nextDelay.Ticks < maximum.Ticks)
            {
                Thread.Sleep(nextDelay);
            }
            else
            {
                throw e;
            }
        }

        public Boolean AtMaximum()
        {
            return (nextDelay.Ticks < maximum.Ticks);
        }

        public TimeSpan TaskDelay()
        {
            CalculateNextDelay();
            return nextDelay;
        }
    }
}
