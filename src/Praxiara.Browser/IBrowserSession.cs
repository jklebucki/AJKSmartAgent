using Praxiara.Contracts.Browser;

namespace Praxiara.Browser;

public interface IBrowserSession : IAsyncDisposable
{
    Guid SessionId { get; }

    ValueTask<BrowserObservation> ObserveAsync(CancellationToken cancellationToken);

    ValueTask<ToolExecutionResult> ExecuteAsync(
        ProposedToolCall toolCall,
        CancellationToken cancellationToken);

    ValueTask StartTraceAsync(CancellationToken cancellationToken);

    ValueTask<string> StopTraceAsync(CancellationToken cancellationToken);
}