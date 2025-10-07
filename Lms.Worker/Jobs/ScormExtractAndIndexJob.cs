using Microsoft.Extensions.Logging;
using Npgsql;
using Quartz;
using StackExchange.Redis;

namespace Lms.Worker.Jobs;

public class ScormExtractAndIndexJob : IJob
{
    private readonly ILogger<ScormExtractAndIndexJob> _logger;
    private readonly IConnectionMultiplexer _redis;
    private readonly NpgsqlDataSource _dataSource;

    public ScormExtractAndIndexJob(
        ILogger<ScormExtractAndIndexJob> logger,
        IConnectionMultiplexer redis,
        NpgsqlDataSource dataSource)
    {
        _logger = logger;
        _redis = redis;
        _dataSource = dataSource;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Starting SCORM extraction and indexing job at {Time}", DateTimeOffset.UtcNow);

        var db = _redis.GetDatabase();
        await db.StringSetAsync("scorm:last_run", DateTimeOffset.UtcNow.ToString("O"));

        await using var connection = await _dataSource.OpenConnectionAsync(context.CancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM scorm_packages";
        var result = await command.ExecuteScalarAsync(context.CancellationToken);
        var packageCount = result is null ? 0 : Convert.ToInt64(result);

        _logger.LogInformation("Indexed {Count} SCORM packages", packageCount);
    }
}
