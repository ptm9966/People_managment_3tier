# People Management Full Stack Application

This repository contains a simple people management application with:

- a React frontend built with Vite
- an ASP.NET Core 9 Web API backend
- a SQL Server database
- Docker support for local full-stack startup

The application lets users create, view, update, and delete people records.

## Tech Stack

- Frontend: React 19, Vite, Axios, React Hook Form, Tailwind CSS
- Backend: ASP.NET Core 9, Entity Framework Core, SQL Server
- Database: Microsoft SQL Server
- Containerization: Docker, Docker Compose, Nginx

## Repository Structure

```text
people_managment_react_asp.net9/
├── People_managment_backend/
│   ├── Backend.sln
│   └── Backend/
│       ├── Controllers/              # API controllers
│       ├── Migrations/               # EF Core migrations
│       ├── Models/                   # Entity models and DbContext
│       ├── Services/                 # Metrics and health check helpers
│       ├── Properties/               # Launch settings
│       ├── sql_schema/               # SQL reference files
│       ├── Program.cs                # App startup and middleware
│       ├── Dockerfile                # Backend runtime image
│       ├── Dockerfile.migrations     # Migration runner image
│       ├── appsettings.json          # Default config
│       └── appsettings.Production.json
├── People_managment_frontend/
│   ├── public/                       # Static assets
│   ├── src/
│   │   ├── components/               # UI components
│   │   ├── pages/                    # Page-level components
│   │   ├── App.jsx                   # Root app component
│   │   └── main.jsx                  # Frontend entry point
│   ├── Dockerfile                    # Frontend build + nginx image
│   ├── nginx.conf                    # Nginx config for production image
│   ├── package.json                  # Frontend scripts and deps
│   └── .env*                         # Environment files
├── docker-compose.yml                # Full local container orchestration
└── README.md
```

## Project Overview

### Frontend

The frontend is a Vite-based React app. It calls the backend API using the environment variable `VITE_BASE_API_URL`.

Important frontend paths:

- `src/components/person/Person.jsx`: CRUD requests to the backend
- `src/pages/`: top-level pages
- `Dockerfile`: production frontend image served by Nginx

### Backend

The backend is an ASP.NET Core Web API project using Entity Framework Core with SQL Server.

Important backend paths:

- `Controllers/PeopleController.cs`: CRUD API for people
- `Models/Person.cs`: person entity
- `Models/AppDbContext.cs`: EF Core DbContext
- `Program.cs`: CORS, database config, health checks, metrics, Swagger
- `SqlServerReadinessHealthCheck.cs`: readiness check tied to DB connectivity
- `Services/`: request metrics and Prometheus formatter

## API Endpoints

### People API

Base route:

```text
/api/people
```

Available endpoints:

- `GET /api/people`
- `GET /api/people/{id}`
- `POST /api/people`
- `PUT /api/people/{id}`
- `DELETE /api/people/{id}`

### Health Endpoints

- `GET /healthz`: liveness probe
- `GET /ready`: readiness probe, healthy only after SQL Server is reachable

### Metrics Endpoint

- `GET /metrics`: Prometheus-format backend metrics

Examples of exposed metrics:

- `backend_request_count`
- `backend_failed_request_count`
- `backend_request_duration_ms_sum`
- `backend_request_duration_ms_avg`
- endpoint-level metrics with `method` and `path` labels

## Local Setup

## Prerequisites

- .NET 9 SDK
- Node.js 20+
- npm
- SQL Server instance running locally or remotely

## Backend Setup

1. Go to the backend project:

```powershell
cd People_managment_backend/Backend
```

2. Update database settings in `appsettings.json` or provide environment variables:

```text
DB_SERVER
DB_PORT
DB_DATABASE
DB_USER
DB_PASSWORD
DB_ENCRYPT
DB_TRUST_SERVER_CERTIFICATE
```

The backend also supports a connection string placeholder in:

```json
"ConnectionStrings": {
  "Default": "Server={DB_SERVER},{DB_PORT};Database={DB_DATABASE};User Id={DB_USER};Password={DB_PASSWORD};Encrypt={DB_ENCRYPT};TrustServerCertificate={DB_TRUST_SERVER_CERTIFICATE};"
}
```

3. Apply migrations:

```powershell
dotnet ef database update
```

4. Run the API:

```powershell
dotnet run
```

By default, Swagger is available after startup.

## Frontend Setup

1. Go to the frontend project:

```powershell
cd People_managment_frontend
```

2. Install dependencies:

```powershell
npm install
```

3. Set the API base URL in `.env` or another Vite env file:

```env
VITE_BASE_API_URL=http://localhost:5000/api
```

4. Start the development server:

```powershell
npm run dev
```

Useful frontend scripts:

- `npm run dev`
- `npm run build`
- `npm run preview`
- `npm run lint`
- `npm run build:dev`
- `npm run build:prod`

## Docker Setup

The repository includes Docker support for:

- SQL Server database
- migration runner
- backend API
- frontend app served with Nginx

## Docker Files

### Backend `Dockerfile`

Location:

- `People_managment_backend/Backend/Dockerfile`

What it does:

- uses a multi-stage .NET 9 build
- restores, builds, and publishes the API
- runs the app in a smaller ASP.NET runtime image
- creates a non-root user for safer runtime execution
- exposes port `8080`

### Backend `Dockerfile.migrations`

Location:

- `People_managment_backend/Backend/Dockerfile.migrations`

What it does:

- builds the backend project
- installs `dotnet-ef`
- runs `dotnet ef database update`
- creates the SQL connection string from `DB_*` environment variables

### Frontend `Dockerfile`

Location:

- `People_managment_frontend/Dockerfile`

What it does:

- builds the Vite app using Node.js
- injects `VITE_BASE_API_URL` at build time
- serves the static build with Nginx
- exposes port `80`

## Docker Compose

Root file:

- `docker-compose.yml`

Current container flow:

1. `db` starts and becomes healthy
2. `migrations` runs after the database is ready
3. `backend` starts after migrations complete successfully
4. `frontend` starts after the backend is running

### Start everything

From the repository root:

```powershell
docker compose up --build
```

### Run in detached mode

```powershell
docker compose up -d --build
```

### Stop everything

```powershell
docker compose down
```

### Remove volumes too

```powershell
docker compose down -v
```

## Docker Service Ports

- Frontend: `http://localhost:8080`
- Backend: `http://localhost:5000`
- SQL Server: `localhost:1433`

## Docker Environment Notes

Inside Docker:

- backend and migrations connect to SQL Server using the service name `db`
- the browser reaches the backend using `http://localhost:5000`
- the browser reaches the frontend using `http://localhost:8080`

Key compose variables currently used:

- `AllowedCorsOrigin__Url=http://localhost:8080`
- `VITE_BASE_API_URL=http://localhost:5000/api`
- `DB_SERVER=db`

## Observability

The backend exposes:

- `/healthz` for liveness
- `/ready` for readiness
- `/metrics` for Prometheus metrics

This makes the API easier to monitor in Docker, Kubernetes, or any external observability stack.

## Notes

- The frontend code uses `VITE_BASE_API_URL`
- If you update frontend env examples, make sure they use `VITE_BASE_API_URL` instead of `VITE_API_URL`
- Sensitive values such as SQL passwords should ideally be moved to secrets or environment-specific configuration for production

## Future Improvements

- add authentication and authorization
- add request validation and centralized exception handling
- add automated tests for backend and frontend
- add Prometheus and Grafana services to Docker Compose
- move secrets to a safer config strategy
