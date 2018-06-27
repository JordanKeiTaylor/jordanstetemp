using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Improbable.Behaviour;
using Improbable.Context;
using Improbable.Log;

namespace Improbable.Worker
{
    public abstract class GenericTickWorker : GenericWorker
    {
        private const string LoggerName = "GenericTickWorker.cs";
        private readonly NamedLogger _logger = Logger.DefaultWithName(LoggerName);

        private readonly int _tickTimeMs;
        
        private readonly TickTimeRollingMetric _tickTimeRollingMetric;
        
        protected GenericTickWorker(int tickTimeMs, string workerType, string workerId, string host, ushort port)
            : base(workerType, workerId, host, port)
        {
            _tickTimeMs = tickTimeMs;
            _tickTimeRollingMetric = new TickTimeRollingMetric(30);
        }

        public abstract int Run();

        protected abstract Dictionary<string, ITickBehaviour> GetBehaviours();
        
        protected int RunEventLoop()
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
                double offsetMs = frameTimer.ElapsedMilliseconds;
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
