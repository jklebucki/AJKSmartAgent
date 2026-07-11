namespace Praxiara.Orchestration;

public sealed record AgentStepRequest(
    Guid TaskId,
    Guid SessionId,
    string UserId,
    string UserGoal,
    string Environment,
    IReadOnlySet<string> AllowedTools,
    IReadOnlySet<string> Permissions,
    IReadOnlyList<string> ApplicableSkillIds);