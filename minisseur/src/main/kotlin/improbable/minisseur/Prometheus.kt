package improbable.minisseur

import com.fasterxml.jackson.annotation.*
import okhttp3.HttpUrl
import okhttp3.Request

data class QueryResponse(
        val status: String,
        val data: QueryResponseData
)

data class QueryResponseData(
        val resultType: String,
        val result: List<QueryResult>
)

data class QueryResult(
        val metric: Any,
        val value: QueryResultValue
)

@JsonPropertyOrder(value = ["timestamp", "value"])
@JsonFormat(shape=JsonFormat.Shape.ARRAY)
data class QueryResultValue(
        val timestamp: Long,
        val value: OptionalDouble
)

// The prometheus API can return funny stuff like +Inf here so this class wraps these values
data class OptionalDouble @JsonCreator(mode=JsonCreator.Mode.DELEGATING) constructor(@get:JsonProperty val raw: String) {
    fun isValid() = raw.toDoubleOrNull() != null
    fun lessThan(value: Double) = isValid() && raw.toDouble() < value
    fun greaterThan(value: Double) = isValid() && raw.toDouble() > value
    fun toDouble() = raw.toDoubleOrNull()
    override fun toString() = raw
}

@JsonIgnoreProperties(ignoreUnknown = true)
data class TargetsResponse(
        val status: String,
        val data: TargetsResponseData
)

@JsonIgnoreProperties(ignoreUnknown = true)
data class TargetsResponseData(
        val activeTargets: List<ActiveTarget>
)

@JsonIgnoreProperties(ignoreUnknown = true)
data class ActiveTarget(
        val lastError: String,
        val health: String,
        val labels: ActiveTargetLabels
)

@JsonIgnoreProperties(ignoreUnknown = true)
data class ActiveTargetLabels(
        val instance: String
)

internal fun queryRequest(server: String, query: String): Request {
    val url = HttpUrl.parse(server)!!
            .newBuilder()
            .addPathSegments("/api/v1/query")
            .addQueryParameter("query", query)
            .build()

    return Request.Builder()
            .url(url)
            .get()
            .build()
}

internal fun targetsRequest(server: String): Request {
    val url = HttpUrl.parse(server)!!
            .newBuilder()
            .addPathSegments("/api/v1/targets")
            .build()

    return Request.Builder()
            .url(url)
            .get()
            .build()
}
