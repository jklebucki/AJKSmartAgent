namespace Praxiara.Domain.Tasks;

public enum AgentTaskState
{
    Created,
    AwaitingLogin,
    Planning,
    Executing,
    Verifying,
    AwaitingApproval,
    ManualTakeover,
    Completed,
    Failed,
    Cancelled
}