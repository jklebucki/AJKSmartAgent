using Praxiara.Contracts.Browser;

namespace Praxiara.Application.Abstractions;

public sealed record PlannerDecision(ProposedToolCall? ToolCall, bool IsComplete, string Summary);