using Improbable.Collections;

namespace Improbable.Sandbox.MetricsRecorder
{
    internal interface IBaseMetric
    {
        void ReportAndClear(Map<string, double> metrics, long delayMs);
    }
}