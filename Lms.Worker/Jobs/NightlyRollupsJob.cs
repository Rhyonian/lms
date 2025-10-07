using Microsoft.Extensions.Logging;
using Npgsql;
using Quartz;

namespace Lms.Worker.Jobs;

public class NightlyRollupsJob : IJob
{
    private readonly ILogger<NightlyRollupsJob> _logger;
    private readonly NpgsqlDataSource _dataSource;

    public NightlyRollupsJob(
        ILogger<NightlyRollupsJob> logger,
        NpgsqlDataSource dataSource)
    {
        _logger = logger;
        _dataSource = dataSource;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Starting nightly rollups job at {Time}", DateTimeOffset.UtcNow);

        await using var connection = await _dataSource.OpenConnectionAsync(context.CancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "CALL perform_nightly_rollups()";
        await command.ExecuteNonQueryAsync(context.CancellationToken);

        _logger.LogInformation("Nightly rollups completed successfully");
    }
}
