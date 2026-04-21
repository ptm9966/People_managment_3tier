using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

public sealed class SqlServerReadinessHealthCheck : IHealthCheck
{
    private readonly AppDbContext _dbContext;

    public SqlServerReadinessHealthCheck(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);

            return canConnect
                ? HealthCheckResult.Healthy("SQL Server connection is ready.")
                : HealthCheckResult.Unhealthy("SQL Server connection is not ready.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("SQL Server connection failed.", ex);
        }
    }
}
