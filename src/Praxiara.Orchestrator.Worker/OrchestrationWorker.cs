namespace Praxiara.Orchestrator.Worker;

public sealed partial class OrchestrationWorker(ILogger<OrchestrationWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LogWorkerStarted(logger);

        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            LogWorkerStopping(logger);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Praxiara orchestration worker started.")]
    private static partial void LogWorkerStarted(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Praxiara orchestration worker is stopping.")]
    private static partial void LogWorkerStopping(ILogger logger);
}