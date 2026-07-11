namespace Praxiara.Domain.Tasks;

public sealed class InvalidTaskStateTransitionException(AgentTaskState current, AgentTaskState requested)
    : InvalidOperationException($"Transition from '{current}' to '{requested}' is not allowed.")
{
    public AgentTaskState Current { get; } = current;

    public AgentTaskState Requested { get; } = requested;
}