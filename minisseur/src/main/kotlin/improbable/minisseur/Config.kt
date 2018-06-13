package improbable.minisseur

import com.fasterxml.jackson.annotation.JsonIgnoreProperties

@JsonIgnoreProperties(ignoreUnknown = true)
data class Config(
        val expectations: List<Expectation>,
        val metrics: List<Metric>
)

data class Expectation(
        val name: String,
        val metricName: String,
        val minValue: DoubleValue?,
        val maxValue: DoubleValue?
)

data class Metric(
        val prometheusExpressionTemplate: String,
        val name: String,
        val initialDelaySeconds: Int,
        val fallback: Fallback
)

data class Fallback(
        val valueFallback: DoubleValue
)

data class DoubleValue(
        val value: Double
)