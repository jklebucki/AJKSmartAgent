using Praxiara.Contracts.Browser;

namespace Praxiara.Application.Abstractions;

public sealed record AgentPlanningContext(
    Guid TaskId,
    string UserGoal,
    BrowserObservation Observation,
    IReadOnlySet<string> AllowedTools,
    IReadOnlyList<string> ApplicableSkillIds);