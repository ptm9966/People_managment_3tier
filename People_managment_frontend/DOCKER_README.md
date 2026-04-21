# Docker Setup for People Management Frontend

This directory contains Docker configuration for the React frontend application.

## Files Created

- **Dockerfile** - Multi-stage Docker build for the React app
- **nginx.conf** - Nginx configuration for serving the SPA
- **.dockerignore** - Files/folders to exclude from Docker build
- **.env.example** - Example environment variables

## Building the Docker Image

### Option 1: Build with default backend URL
```bash
docker build -t people-management-ui .
```

### Option 2: Build with custom backend URL
```bash
docker build -t people-management-ui --build-arg VITE_API_URL=http://your-backend-url:port .
```

**Examples:**
```bash
# Development (local backend)
docker build -t people-management-ui --build-arg VITE_API_URL=http://localhost:5000 .

# Production (remote backend)
docker build -t people-management-ui --build-arg VITE_API_URL=https://api.example.com .

# Docker network (when using docker-compose)
docker build -t people-management-ui --build-arg VITE_API_URL=http://backend:5000 .
```

## Running the Container

### Basic Run
```bash
docker run -p 80:80 people-management-ui
```

### With custom backend URL
```bash
docker run -p 80:80 \
  --build-arg VITE_API_URL=http://your-backend-url:port \
  people-management-ui
```

### Access the Application
- Frontend: http://localhost

## Using Docker Compose (Recommended for Full Stack)

From the root directory of the project, run:

```bash
docker-compose up -d
```

This will:
- Build and start both frontend and backend
- Frontend available at: http://localhost
- Backend available at: http://localhost:5000

**Stop the containers:**
```bash
docker-compose down
```

## Environment Variables

The `VITE_API_URL` environment variable controls where the React app sends API requests.

- **Build Time**: Set via `--build-arg VITE_API_URL=...` during build
- **Default Value**: `http://localhost:5000`
- **Common Values**:
  - Local development: `http://localhost:5000`
  - Docker network: `http://backend:5000`
  - Production: `https://api.yourdomain.com`

## How It Works

1. **Build Stage**: 
   - Installs npm dependencies
   - Builds the React app with Vite
   - Backend URL is embedded at build time

2. **Production Stage**:
   - Uses lightweight nginx Alpine image
   - Serves static files with optimizations
   - Handles SPA routing properly

3. **Nginx Configuration**:
   - Gzip compression enabled
   - Browser caching for static assets
   - Proper routing for React Router

## Troubleshooting

### API calls failing from browser
- Check that `VITE_API_URL` is correct and accessible from the client browser
- For docker-compose, use `http://backend:5000` (service name)
- For local development, use `http://localhost:5000` or your machine IP

### CORS errors
- Ensure backend has CORS headers configured to allow requests from frontend

### Static files not loading
- Verify nginx.conf is properly copied to container
- Check file permissions in built dist folder
