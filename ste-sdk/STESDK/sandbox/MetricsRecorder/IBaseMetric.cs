using Improbable.Collections;

namespace Improbable.Enterprise.Sandbox.MetricsRecorder
{
    internal interface IBaseMetric
    {
        void ReportAndClear(Map<string, double> metrics, long delayMs);
    }
}