# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS builder

WORKDIR /build

# Copy project files
COPY Backend.csproj .

# Restore dependencies
RUN dotnet restore

# Copy all source code
COPY . .

# Build the application
RUN dotnet build -c Release -o /build/output

# Publish the application
RUN dotnet publish -c Release -o /publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0

WORKDIR /app

# Create a non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published application from builder
COPY --from=builder /publish .

# Create directory for logs and give ownership to appuser
RUN mkdir -p /app/logs && chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    AllowedCorsOrigin__Url=""

# Start the application
ENTRYPOINT ["dotnet", "Backend.dll"]
