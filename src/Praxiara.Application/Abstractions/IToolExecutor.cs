using Praxiara.Contracts.Browser;

namespace Praxiara.Application.Abstractions;

public interface IToolExecutor
{
    ValueTask<ToolExecutionResult> ExecuteAsync(
        Guid sessionId,
        ProposedToolCall toolCall,
        CancellationToken cancellationToken);
}