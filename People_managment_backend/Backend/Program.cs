using Backend.Models;
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
var sqlConn = builder.Configuration.GetConnectionString("Default");

if (string.IsNullOrWhiteSpace(sqlConn))
{
    throw new Exception("ConnectionStrings:Default missing.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(sqlConn);
});

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

app.UseAuthorization();
app.MapControllers();

app.Run();
