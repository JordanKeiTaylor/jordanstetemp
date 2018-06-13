using System.Diagnostics;
using Improbable.Collections;

namespace Shared.MetricsRecorder
{
    public class MetricsRecorder
    {
        private static readonly Stopwatch MetricsTimer = Stopwatch.StartNew();
        private readonly List<IBaseMetric> _allMetrics = new List<IBaseMetric>();
        private readonly string _prefix;

        public MetricsRecorder(string prefix = "fabric_test_metrics_")
        {
            this._prefix = prefix;
        }

        public TimeBasedMetric CreateTimeBasedMetric(string v)
        {
            var metric = new TimeBasedMetric(MetricName(v));
            _allMetrics.Add(metric);
            return metric;
        }

        public MinMaxAvgMetric CreateValueMetric(string v)
        {
            var metric = new MinMaxAvgMetric(MetricName(v));
            _allMetrics.Add(metric);
            return metric;
        }

        public StaticValueMetric CreateStaticMetric(string key, double value)
        {
            var metric = new StaticValueMetric(MetricName(key), value);
            _allMetrics.Add(metric);
            return metric;
        }

        public void WriteMetrics(Map<string, double> metrics)
        {
            long delayMs = MetricsTimer.ElapsedMilliseconds;
            MetricsTimer.Restart();
            foreach (var e in _allMetrics)
            {
                e.ReportAndClear(metrics, delayMs);
            }
        }

        private string MetricName(string name)
        {
            return _prefix + name;
        }

        public class TimeBasedMetric : IBaseMetric
        {
            private readonly string _name;
            private readonly object _lock = new object();
            private double _counter;

            internal TimeBasedMetric(string name)
            {
                this._name = name;
                _counter = 0.0;
            }

            public void ReportAndClear(Map<string, double> metrics, long delayMs)
            {
                double value;
                lock (_lock)
                {
                    value = _counter;
                    _counter = 0.0;
                }

                var perSecond = (value * 1000) / delayMs;
                metrics.Add(_name + "_per_second", perSecond);
            }

            public void Increment()
            {
                lock (_lock)
                {
                    _counter = _counter + 1.0;
                }
            }

            public void Increment(double amount)
            {
                lock (_lock)
                {
                    _counter = _counter + amount;
                }
            }
        }

        public class MinMaxAvgMetric : IBaseMetric
        {
            private readonly string _name;
            private readonly object _lock = new object();

            private int _samples = 0;
            private double _min = 0.0;
            private double _max = 0.0;
            private double _total = 0.0;

            internal MinMaxAvgMetric(string name)
            {
                this._name = name;
            }

            public void Record(double value)
            {
                lock (_lock)
                {
                    if (_samples == 0)
                    {
                        _min = value;
                        _max = value;
                        _total = value;
                        _samples = 1;
                    }
                    else
                    {
                        _samples += 1;
                        _total += value;
                        if (value < _min)
                        {
                            _min = value;
                        }

                        if (value > _max)
                        {
                            _max = value;
                        }
                    }
                }
            }

            public void ReportAndClear(Map<string, double> metrics, long delayMs)
            {
                lock (_lock)
                {
                    if (_samples > 0)
                    {
                        metrics.Add(_name + "_avg", _total / _samples);
                        metrics.Add(_name + "_min", _min);
                        metrics.Add(_name + "_max", _max);
                    }

                    _samples = 0;
                }
            }
        }

        public class StaticValueMetric : IBaseMetric
        {
            private readonly string _key;
            private double _value;

            public StaticValueMetric(string key, double value)
            {
                this._key = key;
                this._value = value;
            }

            public void Set(double value)
            {
                this._value = value;
            }

            public void ReportAndClear(Map<string, double> metrics, long delayMs)
            {
                metrics.Add(_key, _value);
            }
        }
    }
}