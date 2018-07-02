using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Improbable.Behaviour;
using Improbable.Log;

namespace Improbable
{
    /// <summary>
    /// A basic worker that periodically executes <see cref="ITickBehaviour"/> objects.
    /// </summary>
    /// <seealso cref="GenericWorker"/>
    public abstract class GenericTickWorker : GenericWorker
    {
        private const string LoggerName = "GenericTickWorker.cs";
        private readonly NamedLogger _logger = Logger.DefaultWithName(LoggerName);

        private readonly int _tickTimeMs;
        
        private readonly TickTimeRollingMetric _tickTimeRollingMetric;
        
        /// <inheritdoc />
        /// <summary>
        /// Initializes a GenericTickWorker. Once in <see cref="GenericTickWorker.Run"/>, the worker will attempt to
        /// periodically execute all contained <see cref="ITickBehaviour"/> classes once every tickTimeMs. 
        /// </summary>
        /// <param name="tickTimeMs">Time in milliseconds between ticks</param>
        /// <param name="workerType">Type of worker</param>
        /// <param name="workerId">Unique ID of worker</param>
        /// <param name="hostname">SpatialOS deployment hostname</param>
        /// <param name="port">SpatialOS deployment port</param>
        protected GenericTickWorker(int tickTimeMs, string workerType, string workerId, string hostname, ushort port)
            : base(workerType, workerId, hostname, port)
        {
            _tickTimeMs = tickTimeMs;
            _tickTimeRollingMetric = new TickTimeRollingMetric(30);
        }

        /// <inheritdoc />
        /// <summary>
        /// Test Constructor
        /// </summary>
        protected GenericTickWorker(int tickTimeMs)
        {
            _tickTimeMs = tickTimeMs;
            _tickTimeRollingMetric = new TickTimeRollingMetric(30);
        }

        /// <summary>
        /// Implement this method to return all behaviours the worker will execute. 
        /// </summary>
        /// <returns>Dictionary of <see cref="ITickBehaviour"/></returns>
        protected abstract Dictionary<string, ITickBehaviour> GetBehaviours();
        
        public int Run()
        {
            var behaviours = GetBehaviours();

            // run loop
            var frameTimer = new Stopwatch();
            while (GetContext().IsConnected)
            {
                frameTimer.Restart();

                // process messages
                FetchAndProcessOps(0);

                // process behaviours
                foreach (var behaviour in behaviours)
                {
                    try
                    {
                        behaviour.Value.Tick();
                    }
                    catch (Exception e)
                    {
                        _logger.Error("Caught exception during Tick() for behaviour [" + behaviour.Key + "]", e);
                    }
                }
                
                GetContext().GetDispatcher().OnMetrics(metricOp =>
                {
                    var avgSleepTimeMs = _tickTimeMs - _tickTimeRollingMetric.GetAvg();
                    if (avgSleepTimeMs < 0.0)
                    {
                        metricOp.Metrics.Load = 1.0 + ((-avgSleepTimeMs) / _tickTimeMs);
                    }
                    else
                    {
                        metricOp.Metrics.Load = (_tickTimeMs - avgSleepTimeMs) / _tickTimeMs;
                    }

                    GetContext().GetConnection().SendMetrics(metricOp.Metrics);
                });

                _tickTimeRollingMetric.Record(frameTimer.ElapsedMilliseconds);

                // wait for the next frame to ensure frame rate isn't too fast
                double waitTimeMs = _tickTimeMs - frameTimer.ElapsedMilliseconds;
                if (waitTimeMs > 10.0)
                {
                    Thread.Sleep((int)Math.Floor(waitTimeMs));
                }
                else if (waitTimeMs < -10.0)
                {
                    _logger.Warn("Worker fell behind by " + waitTimeMs + "ms.");
                }
            }

            return 1;
        }

        protected void FetchAndProcessOps(double waitTime)
        {
            GetContext().GetDispatcher().Process(GetContext().GetConnection().GetOpList((uint)waitTime));
            while (GetContext().IsDispatcherInCritical)
            {
                GetContext().GetDispatcher().Process(GetContext().GetConnection().GetOpList(0));
            }
        }

        protected int GetTickTimeMs()
        {
            return _tickTimeMs;
        }

        private class TickTimeRollingMetric {

            private double _total;
            private readonly Queue _values;
            private readonly int _size;

            private readonly object _lock = new object();

            public TickTimeRollingMetric(int size) {
                _size = size;
                _values = new Queue(size + 1);
            }

            public void Record(double value) {
                lock (_lock) {
                    _total += value;
                    _values.Enqueue(value);
                    while (_values.Count > _size) {
                        _total = _total - (double)_values.Dequeue();
                    }
                }
            }

            public double GetAvg() {
                lock (_lock) {
                    if (_values.Count == 0) {
                        return 0.0;
                    }
                    return _total / _values.Count;
                }
            }
        }
    }
}
