using System.Diagnostics;

namespace Backend.Services;

public sealed class ApiMetricsMiddleware
{
    private readonly RequestDelegate _next;

    public ApiMetricsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ApiMetricsStore metricsStore)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            var path = context.Request.Path.HasValue ? context.Request.Path.Value! : "/";
            metricsStore.Record(
                context.Request.Method,
                path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
    }
}
