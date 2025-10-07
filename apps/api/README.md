# LMS API

This service exposes local email/password authentication backed by JWT tokens. External OIDC sign-in hooks remain stubbed for future integration.

## Configuration

Create a `.env` file (or otherwise supply environment variables) with the JWT secret:

```bash
cp .env.sample .env
```

## Running locally

```bash
cd apps/api
export ASPNETCORE_ENVIRONMENT=Development
export JWT_SECRET=devsecret
# dotnet run  (no restore in this environment)
```

When the API starts in development mode it logs the seeded administrator credentials:

```
Admin login: admin@example.com / Admin123!
```

## Authentication examples

```bash
# Login and capture the token
curl -X POST http://localhost:5000/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@example.com","password":"Admin123!"}'

# Access the current user endpoint with the token
curl http://localhost:5000/me \
  -H "Authorization: Bearer <token>"

# Logout (stateless)
curl -X POST http://localhost:5000/auth/logout -i
```
# Lms.Api

ASP.NET Core 9 minimal API service for the LMS backend.
