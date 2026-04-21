using Backend.Models;
using Backend.Services;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ===========================
// 1️⃣ Swagger (enabled for all envs)
// ===========================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ===========================
// 2️⃣ Single Allowed CORS Origin
// ===========================

// Read a single string value: "AllowedCorsOrigin:Url"
var allowedOrigin = builder.Configuration["AllowedCorsOrigin:Url"];

if (string.IsNullOrWhiteSpace(allowedOrigin))
{
    throw new Exception("AllowedCorsOrigin:Url not configured in appsettings.json.");
}

// Trim trailing slash
allowedOrigin = allowedOrigin.TrimEnd('/');

const string corsPolicyName = "AllowedCorsOrigin";

builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName, policy =>
    {
        policy.WithOrigins(allowedOrigin)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ===========================
// 3️⃣ SQL Server
// ===========================
var sqlServer = builder.Configuration["DB_SERVER"] ?? builder.Configuration["SqlServer:Server"];
var sqlPort = builder.Configuration["DB_PORT"] ?? builder.Configuration["SqlServer:Port"] ?? "1433";
var sqlDatabase = builder.Configuration["DB_DATABASE"] ?? builder.Configuration["SqlServer:Database"];
var sqlUser = builder.Configuration["DB_USER"] ?? builder.Configuration["SqlServer:User"];
var sqlPassword = builder.Configuration["DB_PASSWORD"] ?? builder.Configuration["SqlServer:Password"];
var sqlEncrypt = builder.Configuration["DB_ENCRYPT"] ?? builder.Configuration["SqlServer:Encrypt"] ?? "True";
var sqlTrustServerCertificate = builder.Configuration["DB_TRUST_SERVER_CERTIFICATE"] ?? builder.Configuration["SqlServer:TrustServerCertificate"] ?? "True";
var sqlConnTemplate = builder.Configuration.GetConnectionString("Default");

if (string.IsNullOrWhiteSpace(sqlServer) ||
    string.IsNullOrWhiteSpace(sqlDatabase) ||
    string.IsNullOrWhiteSpace(sqlUser) ||
    string.IsNullOrWhiteSpace(sqlPassword))
{
    throw new Exception("SQL Server configuration is incomplete. Configure DB_SERVER, DB_DATABASE, DB_USER, DB_PASSWORD or the SqlServer section.");
}

var sqlConn = new SqlConnectionStringBuilder
{
    DataSource = $"{sqlServer},{sqlPort}",
    InitialCatalog = sqlDatabase,
    UserID = sqlUser,
    Password = sqlPassword,
    Encrypt = bool.TryParse(sqlEncrypt, out var encryptValue) ? encryptValue : true,
    TrustServerCertificate = bool.TryParse(sqlTrustServerCertificate, out var trustServerCertificateValue) ? trustServerCertificateValue : true
}.ConnectionString;

if (!string.IsNullOrWhiteSpace(sqlConnTemplate))
{
    sqlConn = sqlConnTemplate
        .Replace("{DB_SERVER}", sqlServer)
        .Replace("{DB_PORT}", sqlPort)
        .Replace("{DB_DATABASE}", sqlDatabase)
        .Replace("{DB_USER}", sqlUser)
        .Replace("{DB_PASSWORD}", sqlPassword)
        .Replace("{DB_ENCRYPT}", sqlEncrypt)
        .Replace("{DB_TRUST_SERVER_CERTIFICATE}", sqlTrustServerCertificate);
}

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(sqlConn);
});

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" })
    .AddCheck<SqlServerReadinessHealthCheck>("sql-server", tags: new[] { "ready" });

builder.Services.AddSingleton<ApiMetricsStore>();
builder.Services.AddControllers();

var app = builder.Build();

// ===========================
// 4️⃣ Swagger for all environments
// ===========================
app.UseSwagger();
app.UseSwaggerUI();

// ===========================
// 5️⃣ Middleware
// ===========================
app.UseCors(corsPolicyName);
app.UseMiddleware<ApiMetricsMiddleware>();

app.UseAuthorization();
app.MapHealthChecks("/healthz", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});
app.MapHealthChecks("/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapGet("/metrics", (ApiMetricsStore metricsStore) =>
{
    var metrics = PrometheusMetricsFormatter.Format(metricsStore.GetSnapshot());
    return Results.Text(metrics, "text/plain; version=0.0.4; charset=utf-8");
});
app.MapControllers();

app.Run();
