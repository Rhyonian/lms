# lms

## Lms.Worker

`Lms.Worker` is a .NET worker service responsible for background processing in the LMS platform. It uses Quartz.NET to schedule recurring jobs and integrates with Redis and PostgreSQL for caching and data access.

### Scheduled jobs

- **scorm_extract_and_index** – runs every 15 minutes to extract SCORM packages and refresh their search index.
- **recompute_progress** – runs every 30 minutes (starting at 5 minutes past the hour) to recompute learner progress.
- **nightly_rollups** – runs nightly at 2 AM to perform reporting rollups.

### Configuration

Configuration values are provided via `appsettings.json` and environment variables.

```json
{
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "PostgreSql": {
    "ConnectionString": "Host=localhost;Port=5432;Username=lms;Password=password;Database=lms"
  }
}
```

Update the connection strings to match your infrastructure before running the worker.

### Running the worker

The worker can be launched with the standard .NET hosting command:

```bash
DOTNET_ENVIRONMENT=Development dotnet run --project Lms.Worker
```

The worker logs execution of each job to the console. Ensure Redis and PostgreSQL are available before starting the service.
