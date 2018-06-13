package improbable.minisseur

import com.fasterxml.jackson.databind.ObjectMapper
import com.fasterxml.jackson.module.kotlin.KotlinModule
import com.fasterxml.jackson.module.kotlin.readValue
import okhttp3.OkHttpClient
import org.slf4j.LoggerFactory
import picocli.CommandLine
import picocli.CommandLine.Option
import picocli.CommandLine.Parameters
import java.io.FileInputStream
import java.io.FileOutputStream
import java.io.PrintWriter
import java.time.Instant

class Main : Runnable {
    private val LOG = LoggerFactory.getLogger(Main::class.java)
    private val MAPPER = ObjectMapper().registerModule(KotlinModule())

    @Option(names = ["--server"])
    private var server: String = "http://localhost:9092"

    @Option(names = ["--metric-log-csv"])
    private var metricLogCsv: String? = null

    @Option(names = ["--check-interval"])
    private var checkIntervalMs: Long = 10000

    @Option(names = ["--test-length"])
    private var testLengthMs: Long = 0

    @Option(names = ["--project"])
    private var project = "local"

    @Option(names = ["--deployment"])
    private var deployment = "local"

    @Parameters(arity = "1", paramLabel = "FILE", description = ["Configuration file."])
    private var configFile: String? = null

    private fun substituteQuery(query: String) = query
            .replace("%project%", project)
            .replace("%deployment%", deployment)


    private fun fetchMetrics(metrics: List<Metric>): Map<String, QueryResultValue> {
        val client = OkHttpClient()

        return metrics.map { metric ->
            val query = substituteQuery(metric.prometheusExpressionTemplate)
            val request = queryRequest(server, query)
            val response = client.newCall(request).execute()
            val queryResponse = MAPPER.readValue<QueryResponse>(response.body()!!.byteStream())

            val metricValue = queryResponse.data.result[0].value

            if (!metricValue.value.isValid()) {
                LOG.info("Metric ${metric.name} has invalid value: ${metricValue.value.raw}")
            }

            metric.name to metricValue
        }.toMap()
    }

    private fun fetchTargets(): TargetsResponse {
        val client = OkHttpClient()
        val response = client.newCall(targetsRequest(server)).execute()

        return MAPPER.readValue(response.body()!!.byteStream())
    }

    private fun testMetrics(expectations: List<Expectation>, metrics: Map<String, QueryResultValue>): Int {
        var failedMetrics = 0
        expectations.forEach { expectation ->
            val metricValue = metrics[expectation.metricName]!!

            if (expectation.minValue != null && metricValue.value.lessThan(expectation.minValue.value)) {
                LOG.warn(">> Expectation ${expectation.name} violated: ${metricValue} < ${expectation.minValue.value}")
                failedMetrics += 1
            }

            if (expectation.maxValue != null && metricValue.value.greaterThan(expectation.maxValue.value)) {
                LOG.warn(">> Expectation ${expectation.name} violated: ${metricValue} > ${expectation.maxValue.value}")
                failedMetrics += 1
            }
        }

        LOG.info("${metrics.count() - failedMetrics} / ${metrics.count()} succeeded.")

        return failedMetrics
    }

    private fun testTargets(targetsResponse: TargetsResponse) {
        targetsResponse.data.activeTargets.forEach { target ->
            if (target.health != "up") {
                LOG.warn(">> Target ${target.labels.instance} state is ${target.health}")
            }
        }

        val allTargets = targetsResponse.data.activeTargets.size
        val upTargets = targetsResponse.data.activeTargets.count { it.health == "up" }
        LOG.info(">> ${upTargets} / ${allTargets} targets up")
    }

    private fun logMetrics(metrics: Map<String, QueryResultValue>) = FileOutputStream(metricLogCsv, true).use { fileOutputStream ->
        PrintWriter(fileOutputStream).use { writer ->
            metrics.forEach { metric ->
                writer.println("${metric.key},${metric.value.timestamp},${metric.value.value.toDouble()}")
            }
        }
    }

    override fun run() {
        val startTime = Instant.now()
        val config = FileInputStream(configFile).use { fileInputStream -> MAPPER.readValue<Config>(fileInputStream) }
        var failedMetrics = 0
        var checkedMetrics = 0

        if (testLengthMs == 0L) {
            LOG.info("Running forever.")
        } else {
            LOG.info("Starting test for ${testLengthMs}ms.")
        }

        while (testLengthMs == 0L || startTime.plusMillis(testLengthMs) > Instant.now()) {
            LOG.info("Testing targets")
            val targets = fetchTargets()
            testTargets(targets)

            LOG.info("Testing ${config.metrics.count()} metrics")
            val metrics = fetchMetrics(config.metrics)

            if (metricLogCsv != null) {
                logMetrics(metrics)
            }

            failedMetrics += testMetrics(config.expectations, metrics)
            checkedMetrics += metrics.count()

            LOG.info("Waiting ${checkIntervalMs}ms...\n")
            Thread.sleep(checkIntervalMs)
        }

        if (failedMetrics == 0) {
            LOG.info("Test complete!")
        } else {
            LOG.error("${failedMetrics} / ${checkedMetrics} checks failed")
            System.exit(1)
        }
    }
}

fun main(args: Array<String>) {
    CommandLine.run(Main(), System.out, *args)
}
