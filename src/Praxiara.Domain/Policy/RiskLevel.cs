namespace Praxiara.Domain.Policy;

public enum RiskLevel
{
    R0ReadOnly = 0,
    R1Navigation = 1,
    R2DraftInput = 2,
    R3PersistDraft = 3,
    R4BusinessCommit = 4,
    R5Critical = 5
}

public enum SkillExecutionMode
{
    Observe,
    Assist,
    Execute
}