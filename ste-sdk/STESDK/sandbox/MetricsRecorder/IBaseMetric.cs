using Improbable.Collections;

namespace stesdk.sandbox.MetricsRecorder
{
    internal interface IBaseMetric
    {
        void ReportAndClear(Map<string, double> metrics, long delayMs);
    }
}