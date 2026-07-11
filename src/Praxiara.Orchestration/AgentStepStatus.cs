namespace Praxiara.Orchestration;

public enum AgentStepStatus
{
    Executed,
    AwaitingApproval,
    Reobserve,
    Denied,
    Completed,
    Failed
}