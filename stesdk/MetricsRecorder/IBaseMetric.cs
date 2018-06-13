using Improbable.Collections;

namespace Shared.MetricsRecorder
{
    internal interface IBaseMetric
    {
        void ReportAndClear(Map<string, double> metrics, long delayMs);
    }
}