using System;
using System.Diagnostics;
using Improbable.Collections;

namespace Shared
{
    public class MetricsRecorder
    {
        private List<BaseMetric> allMetrics = new List<BaseMetric>();
        private static Stopwatch metricsTimer = Stopwatch.StartNew();
        private readonly string prefix;

        public MetricsRecorder(string prefix = "fabric_test_metrics_")
        {
            this.prefix = prefix;
        }

        private string metricName(string name)
        {
            return prefix + name;
        }

        public TimeBasedMetric CreateTimeBasedMetric(string v)
        {
            var metric = new TimeBasedMetric(metricName(v));
            allMetrics.Add(metric);
            return metric;
        }

        public MinMaxAvgMetric CreateValueMetric(string v)
        {
            var metric = new MinMaxAvgMetric(metricName(v));
            allMetrics.Add(metric);
            return metric;
        }

        public StaticValueMetric CreateStaticMetric(string key, double value)
        {
            var metric = new StaticValueMetric(metricName(key), value);
            allMetrics.Add(metric);
            return metric;
        }

        public void WriteMetrics(Map<string, double> metrics)
        {
            long delayMs = metricsTimer.ElapsedMilliseconds;
            metricsTimer.Restart();
            foreach (var e in allMetrics)
            {
                e.ReportAndClear(metrics, delayMs);
            }
        }

        interface BaseMetric
        {
            void ReportAndClear(Map<string, double> metrics, long delayMs);
        }


        public class TimeBasedMetric : BaseMetric
        {
            private double counter;
            private string name;

            private readonly Object Lock = new Object();

            internal TimeBasedMetric(string name)
            {
                this.name = name;
                counter = 0.0;
            }

            public void ReportAndClear(Map<string, double> metrics, long delayMs)
            {
                double value;
                lock (Lock)
                {
                    value = counter;
                    counter = 0.0;
                }
                var perSecond = (value * 1000) / delayMs;
                metrics.Add(name + "_per_second", perSecond);
            }

            public void Increment()
            {
                lock (Lock)
                {
                    counter = counter + 1.0;
                }
            }

            public void Increment(double amount)
            {
                lock (Lock)
                {
                    counter = counter + amount;
                }
            }
        }

        public class MinMaxAvgMetric : BaseMetric
        {
            private int samples = 0;
            private double min = 0.0;
            private double max = 0.0;
            private double total = 0.0;
            private string name;

            private readonly object Lock = new object();

            internal MinMaxAvgMetric(string name)
            {
                this.name = name;
            }

            public void Record(double value)
            {
                lock (Lock)
                {
                    if (samples == 0)
                    {
                        min = value;
                        max = value;
                        total = value;
                        samples = 1;
                    }
                    else
                    {
                        samples += 1;
                        total += value;
                        if (value < min) min = value;
                        if (value > max) max = value;
                    }
                }
            }

            public void ReportAndClear(Map<string, double> metrics, long delayMs)
            {
                lock (Lock)
                {
                    if (samples > 0)
                    {
                        metrics.Add(name + "_avg", (total / samples));
                        metrics.Add(name + "_min", min);
                        metrics.Add(name + "_max", max);
                    }
                    samples = 0;
                }
            }
        }

        public class StaticValueMetric : BaseMetric
        {
            private string key;
            private double value;

            public StaticValueMetric(string key, double value)
            {
                this.key = key;
                this.value = value;
            }

            public void Set(double value)
            {
                this.value = value;
            }

            public void ReportAndClear(Map<string, double> metrics, long delayMs)
            {
                metrics.Add(key, value);
            }
        }
    }
}