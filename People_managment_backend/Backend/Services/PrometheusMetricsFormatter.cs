using System.Globalization;
using System.Text;

namespace Backend.Services;

public static class PrometheusMetricsFormatter
{
    public static string Format(ApiMetricsSnapshot snapshot)
    {
        var builder = new StringBuilder();

        AppendMetricHeader(builder, "backend_request_count", "counter", "Total number of HTTP requests handled by the backend.");
        builder.Append("backend_request_count ")
            .Append(snapshot.RequestCount.ToString(CultureInfo.InvariantCulture))
            .AppendLine();

        AppendMetricHeader(builder, "backend_failed_request_count", "counter", "Total number of failed HTTP requests handled by the backend.");
        builder.Append("backend_failed_request_count ")
            .Append(snapshot.FailedRequestCount.ToString(CultureInfo.InvariantCulture))
            .AppendLine();

        AppendMetricHeader(builder, "backend_request_duration_ms_sum", "gauge", "Total duration in milliseconds for all handled HTTP requests.");
        builder.Append("backend_request_duration_ms_sum ")
            .Append(snapshot.TotalRequestDurationMs.ToString(CultureInfo.InvariantCulture))
            .AppendLine();

        AppendMetricHeader(builder, "backend_request_duration_ms_avg", "gauge", "Average duration in milliseconds for handled HTTP requests.");
        builder.Append("backend_request_duration_ms_avg ")
            .Append(snapshot.AverageRequestDurationMs.ToString(CultureInfo.InvariantCulture))
            .AppendLine();

        AppendMetricHeader(builder, "backend_endpoint_request_count", "counter", "Total number of HTTP requests grouped by method and path.");
        foreach (var endpoint in snapshot.Endpoints)
        {
            builder.Append("backend_endpoint_request_count")
                .Append(Labels(endpoint))
                .Append(' ')
                .Append(endpoint.RequestCount.ToString(CultureInfo.InvariantCulture))
                .AppendLine();
        }

        AppendMetricHeader(builder, "backend_endpoint_failed_request_count", "counter", "Total number of failed HTTP requests grouped by method and path.");
        foreach (var endpoint in snapshot.Endpoints)
        {
            builder.Append("backend_endpoint_failed_request_count")
                .Append(Labels(endpoint))
                .Append(' ')
                .Append(endpoint.FailedRequestCount.ToString(CultureInfo.InvariantCulture))
                .AppendLine();
        }

        AppendMetricHeader(builder, "backend_endpoint_request_duration_ms_sum", "gauge", "Total request duration in milliseconds grouped by method and path.");
        foreach (var endpoint in snapshot.Endpoints)
        {
            builder.Append("backend_endpoint_request_duration_ms_sum")
                .Append(Labels(endpoint))
                .Append(' ')
                .Append(endpoint.TotalRequestDurationMs.ToString(CultureInfo.InvariantCulture))
                .AppendLine();
        }

        AppendMetricHeader(builder, "backend_endpoint_request_duration_ms_avg", "gauge", "Average request duration in milliseconds grouped by method and path.");
        foreach (var endpoint in snapshot.Endpoints)
        {
            builder.Append("backend_endpoint_request_duration_ms_avg")
                .Append(Labels(endpoint))
                .Append(' ')
                .Append(endpoint.AverageRequestDurationMs.ToString(CultureInfo.InvariantCulture))
                .AppendLine();
        }

        return builder.ToString();
    }

    private static void AppendMetricHeader(StringBuilder builder, string name, string type, string help)
    {
        builder.Append("# HELP ")
            .Append(name)
            .Append(' ')
            .Append(help)
            .AppendLine();
        builder.Append("# TYPE ")
            .Append(name)
            .Append(' ')
            .Append(type)
            .AppendLine();
    }

    private static string Labels(EndpointMetricsSnapshot endpoint)
    {
        return $"{{method=\"{Escape(endpoint.Method)}\",path=\"{Escape(endpoint.Path)}\"}}";
    }

    private static string Escape(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal);
    }
}
