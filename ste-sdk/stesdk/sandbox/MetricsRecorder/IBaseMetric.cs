using Improbable.Collections;

namespace Improbable.sandbox.MetricsRecorder
{
    internal interface IBaseMetric
    {
        void ReportAndClear(Map<string, double> metrics, long delayMs);
    }
}