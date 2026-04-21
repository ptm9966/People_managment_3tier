using System.Collections.Concurrent;

namespace Backend.Services;

public sealed class ApiMetricsStore
{
    private long _requestCount;
    private long _failedRequestCount;
    private long _totalDurationMs;

    private readonly ConcurrentDictionary<string, EndpointMetric> _metricsByRoute = new();

    public void Record(string method, string path, int statusCode, long durationMs)
    {
        Interlocked.Increment(ref _requestCount);

        if (statusCode >= 400)
        {
            Interlocked.Increment(ref _failedRequestCount);
        }

        Interlocked.Add(ref _totalDurationMs, durationMs);

        var routeKey = $"{method.ToUpperInvariant()} {path}";
        var endpointMetric = _metricsByRoute.GetOrAdd(routeKey, _ => new EndpointMetric(method.ToUpperInvariant(), path));
        endpointMetric.Record(statusCode, durationMs);
    }

    public ApiMetricsSnapshot GetSnapshot()
    {
        var requestCount = Interlocked.Read(ref _requestCount);
        var failedRequestCount = Interlocked.Read(ref _failedRequestCount);
        var totalDurationMs = Interlocked.Read(ref _totalDurationMs);

        var endpoints = _metricsByRoute
            .OrderBy(metric => metric.Key)
            .Select(metric => metric.Value.ToSnapshot())
            .ToArray();

        return new ApiMetricsSnapshot(
            requestCount,
            failedRequestCount,
            totalDurationMs,
            requestCount == 0 ? 0 : Math.Round((double)totalDurationMs / requestCount, 2),
            endpoints);
    }

    public sealed class EndpointMetric
    {
        private long _requestCount;
        private long _failedRequestCount;
        private long _totalDurationMs;

        public EndpointMetric(string method, string path)
        {
            Method = method;
            Path = path;
        }

        public string Method { get; }

        public string Path { get; }

        public void Record(int statusCode, long durationMs)
        {
            Interlocked.Increment(ref _requestCount);

            if (statusCode >= 400)
            {
                Interlocked.Increment(ref _failedRequestCount);
            }

            Interlocked.Add(ref _totalDurationMs, durationMs);
        }

        public EndpointMetricsSnapshot ToSnapshot()
        {
            var requestCount = Interlocked.Read(ref _requestCount);
            var failedRequestCount = Interlocked.Read(ref _failedRequestCount);
            var totalDurationMs = Interlocked.Read(ref _totalDurationMs);

            return new EndpointMetricsSnapshot(
                Method,
                Path,
                requestCount,
                failedRequestCount,
                totalDurationMs,
                requestCount == 0 ? 0 : Math.Round((double)totalDurationMs / requestCount, 2));
        }
    }
}

public sealed record ApiMetricsSnapshot(
    long RequestCount,
    long FailedRequestCount,
    long TotalRequestDurationMs,
    double AverageRequestDurationMs,
    EndpointMetricsSnapshot[] Endpoints);

public sealed record EndpointMetricsSnapshot(
    string Method,
    string Path,
    long RequestCount,
    long FailedRequestCount,
    long TotalRequestDurationMs,
    double AverageRequestDurationMs);
