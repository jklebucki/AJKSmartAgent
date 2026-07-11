using System.Text.Json;

using Praxiara.Application.Abstractions;
using Praxiara.Contracts.Approvals;
using Praxiara.Contracts.Browser;
using Praxiara.Domain.Policy;

namespace Praxiara.Policy;

public sealed class RiskPolicy(
    IReadOnlyDictionary<string, ToolPolicyDescriptor> toolCatalog,
    IReadOnlySet<string> allowedDomains) : IToolPolicy
{
    public ValueTask<ToolAuthorization> AuthorizeAsync(
        ToolPolicyContext context,
        ProposedToolCall toolCall,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!toolCatalog.TryGetValue(toolCall.ToolName, out var descriptor))
        {
            return ValueTask.FromResult(ToolAuthorization.Denied("The tool is not present in the policy catalog."));
        }

        if (!IsAllowedHost(context.CurrentUrl))
        {
            return ValueTask.FromResult(ToolAuthorization.Denied("The current domain is not allowlisted."));
        }

        if (descriptor.RequiredPermission is { } permission && !context.Permissions.Contains(permission))
        {
            return ValueTask.FromResult(ToolAuthorization.Denied($"The permission '{permission}' is required."));
        }

        if (toolCall.ToolName == "browser_navigate" && !HasAllowedNavigationTarget(toolCall.Arguments))
        {
            return ValueTask.FromResult(ToolAuthorization.Denied("The navigation target is missing or not allowlisted."));
        }

        if (descriptor.RiskLevel < RiskLevel.R4BusinessCommit)
        {
            return ValueTask.FromResult(ToolAuthorization.Automatic());
        }

        var preview = new ApprovalPreview(
            descriptor.DisplayName,
            context.Environment,
            BuildPreviewFacts(toolCall.Arguments),
            descriptor.Consequence,
            descriptor.RiskLevel == RiskLevel.R5Critical);

        return ValueTask.FromResult(ToolAuthorization.WithApproval(preview));
    }

    private bool IsAllowedHost(Uri uri) =>
        uri.Scheme is "https" or "http" && allowedDomains.Contains(uri.IdnHost);

    private bool HasAllowedNavigationTarget(IReadOnlyDictionary<string, JsonElement> arguments) =>
        arguments.TryGetValue("url", out var value) &&
        value.ValueKind == JsonValueKind.String &&
        Uri.TryCreate(value.GetString(), UriKind.Absolute, out var target) &&
        IsAllowedHost(target);

    private static Dictionary<string, string> BuildPreviewFacts(
        IReadOnlyDictionary<string, JsonElement> arguments) =>
        arguments
            .Where(item => item.Value.ValueKind is JsonValueKind.String or JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False)
            .ToDictionary(
                item => item.Key,
                item => item.Value.ToString().Length <= 256
                    ? item.Value.ToString()
                    : $"{item.Value.ToString()[..253]}...",
                StringComparer.Ordinal);
}