using Microsoft.Extensions.Logging;
using Npgsql;
using Quartz;

namespace Lms.Worker.Jobs;

public class RecomputeProgressJob : IJob
{
    private readonly ILogger<RecomputeProgressJob> _logger;
    private readonly NpgsqlDataSource _dataSource;

    public RecomputeProgressJob(
        ILogger<RecomputeProgressJob> logger,
        NpgsqlDataSource dataSource)
    {
        _logger = logger;
        _dataSource = dataSource;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Starting recompute progress job at {Time}", DateTimeOffset.UtcNow);

        await using var connection = await _dataSource.OpenConnectionAsync(context.CancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT recompute_all_progress()";
        await command.ExecuteNonQueryAsync(context.CancellationToken);

        _logger.LogInformation("Finished recompute progress job");
    }
}
