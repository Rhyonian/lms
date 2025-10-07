using Lms.Worker.Configuration;
using Lms.Worker.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Quartz;
using StackExchange.Redis;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection("Redis"));
builder.Services.Configure<PostgresOptions>(builder.Configuration.GetSection("PostgreSql"));

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<RedisOptions>>().Value;
    if (string.IsNullOrWhiteSpace(options.ConnectionString))
    {
        throw new InvalidOperationException("Redis connection string is not configured.");
    }

    return ConnectionMultiplexer.Connect(options.ConnectionString);
});

builder.Services.AddSingleton(sp =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<PostgresOptions>>().Value;
    if (string.IsNullOrWhiteSpace(options.ConnectionString))
    {
        throw new InvalidOperationException("PostgreSQL connection string is not configured.");
    }

    return NpgsqlDataSource.Create(options.ConnectionString);
});

builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();

    q.AddJob<ScormExtractAndIndexJob>(options => options.WithIdentity("scorm_extract_and_index"));
    q.AddTrigger(options => options
        .ForJob(new JobKey("scorm_extract_and_index"))
        .WithIdentity("scorm_extract_and_index_trigger")
        .WithCronSchedule("0 0/15 * * * ?"));

    q.AddJob<RecomputeProgressJob>(options => options.WithIdentity("recompute_progress"));
    q.AddTrigger(options => options
        .ForJob(new JobKey("recompute_progress"))
        .WithIdentity("recompute_progress_trigger")
        .WithCronSchedule("0 5/30 * * * ?"));

    q.AddJob<NightlyRollupsJob>(options => options.WithIdentity("nightly_rollups"));
    q.AddTrigger(options => options
        .ForJob(new JobKey("nightly_rollups"))
        .WithIdentity("nightly_rollups_trigger")
        .WithCronSchedule("0 0 2 * * ?"));
});

builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});

builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
});

var host = builder.Build();

await host.RunAsync();
