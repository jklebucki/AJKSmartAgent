using Praxiara.Contracts.Browser;

namespace Praxiara.Application.Abstractions;

public interface IActionVerifier
{
    ValueTask<VerificationResult> VerifyAsync(
        ProposedToolCall toolCall,
        ToolExecutionResult executionResult,
        BrowserObservation observationAfter,
        CancellationToken cancellationToken);
}