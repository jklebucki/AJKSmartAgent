using Praxiara.Contracts.Approvals;
using Praxiara.Contracts.Browser;

namespace Praxiara.Application.Abstractions;

public interface IToolPolicy
{
    ValueTask<ToolAuthorization> AuthorizeAsync(
        ToolPolicyContext context,
        ProposedToolCall toolCall,
        CancellationToken cancellationToken);
}