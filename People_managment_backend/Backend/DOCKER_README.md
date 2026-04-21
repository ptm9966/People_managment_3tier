# Docker Setup for People Management Backend

This directory contains Docker configuration for the ASP.NET 9 backend application.

## Files Created

- **Dockerfile** - Multi-stage Docker build for ASP.NET 9
- **.dockerignore** - Files/folders to exclude from Docker build
- **appsettings.Production.json** - Production configuration template

## Security Features

✅ **Non-root User**: Application runs as `appuser` (not root)  
✅ **CORS Configuration**: Pass CORS origin as environment variable  
✅ **Multi-stage Build**: Optimized image size (SDK only in build stage)  
✅ **Standard ASP.NET**: Uses standard dotnet CLI startup

## Building the Docker Image

### Basic Build
```bash
docker build -t people-management-api ./People_managment_backend/Backend
```

## Running the Container

### Option 1: Default CORS Origin (http://localhost:5173)
```bash
docker run -p 5000:8080 people-management-api
```

### Option 2: Custom CORS Origin
```bash
docker run -p 5000:8080 \
  -e AllowedCorsOrigin__Url=http://localhost:3000 \
  people-management-api
```

### Option 3: From Docker Compose (Recommended)
From the root directory:
```bash
docker-compose up -d
```

This starts both frontend and backend with proper CORS configuration.

## Environment Variables

### AllowedCorsOrigin__Url
Controls which frontend URL can make requests to the API.

In .NET configuration, colons (`:`) in keys are replaced with double underscores (`__`) when using environment variables.  
Configuration key: `AllowedCorsOrigin:Url` → Environment variable: `AllowedCorsOrigin__Url`

**Default**: `http://localhost:5173` (from appsettings.json)

**Common Values**:
- Local development: `http://localhost:3000` or `http://localhost:5173`
- Docker network: `http://frontend` or `http://localhost`
- Production: `https://yourdomain.com`

### ASPNETCORE_ENVIRONMENT
- Values: `Development`, `Production`
- Default: `Production` (in Dockerfile)

### ASPNETCORE_URLS
- Controls the port and protocol
- Default: `http://+:8080`
- Note: Inside container, use port 8080; map to any external port

## Docker Compose Configuration

The `docker-compose.yml` includes:

```yaml
backend:
  ports:
    - "5000:8080"                      # External:Internal port mapping
  environment:
    - AllowedCorsOrigin__Url=http://localhost:3000
```

To override CORS origin in docker-compose, modify the `environment` section or use:
```bash
AllowedCorsOrigin__Url=https://my-frontend.com docker-compose up
```

## How It Works

1. **Build Stage**:
   - Uses .NET 9 SDK
   - Restores NuGet packages
   - Builds and publishes application

2. **Runtime Stage**:
   - Uses lightweight .NET 9 runtime image
   - Creates non-root user (`appuser`)
   - Runs with reduced privileges

3. **Configuration**:
   - ASP.NET automatically reads environment variables
   - `AllowedCorsOrigin__Url` environment variable overrides appsettings.json
   - Configuration hierarchy: appsettings.json → environment variables

## Database Connection

The application reads the database connection string from `appsettings.json`:

```json
"ConnectionStrings": {
  "Default": "Server=localhost,1433;Database=devpeopledb;..."
}
```

**For Docker**, update this to connect to a database container or external SQL Server.

### Option 1: SQL Server in Docker Compose
```yaml
db:
  image: mcr.microsoft.com/mssql/server:latest
  environment:
    - ACCEPT_EULA=Y
    - MSSQL_SA_PASSWORD=YourPassword123!
  ports:
    - "1433:1433"

backend:
  environment:
    - ConnectionStrings__Default=Server=db,1433;Database=devpeopledb;User Id=sa;Password=YourPassword123!;Encrypt=False;TrustServerCertificate=True;
```

### Option 2: External SQL Server
Update connection string in docker run or compose file.

## Troubleshooting

## Troubleshooting

### CORS Errors
- Check `AllowedCorsOrigin__Url` environment variable matches your frontend URL exactly
- Ensure frontend URL includes protocol (http:// or https://)
- For docker-compose, use service name: `http://frontend` or `http://localhost:3000`
- View logs to see what CORS origin is configured: `docker logs people-management-api`

### Application crashes on startup
```bash
docker logs people-management-api
```

Check for:
- Missing database connection
- Invalid CORS URL format
- Missing appsettings.json

### Testing CORS
```bash
curl -H "Origin: http://localhost:3000" \
     -H "Access-Control-Request-Method: GET" \
     http://localhost:5000/api/people
```

## Example Docker Run Commands

**Development (local frontend on port 3000)**:
```bash
docker run -p 5000:8080 \
  -e AllowedCorsOrigin__Url=http://localhost:3000 \
  people-management-api
```

**Production (frontend on subdomain)**:
```bash
docker run -p 5000:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e AllowedCorsOrigin__Url=https://app.yourdomain.com \
  people-management-api
```

**With external SQL Server**:
```bash
docker run -p 5000:8080 \
  -e AllowedCorsOrigin__Url=http://localhost:3000 \
  -e ConnectionStrings__Default="Server=your-sql-server.com;Database=devpeopledb;User Id=sa;Password=..." \
  people-management-api
```

## Logs

View logs from running container:
```bash
docker logs people-management-api
docker logs people-management-api -f  # Follow logs
```

## Additional Notes

- ASP.NET 9 automatically reads environment variables for configuration
- Use double underscores (`__`) to override nested configuration keys in environment variables
- This approach allows one image to be used across different environments
- For production, consider using secrets management for sensitive data (connection strings, passwords)
- The non-root user prevents potential security vulnerabilities from container escapes
- Standard ASP.NET startup means no custom scripts needed
