namespace Praxiara.Domain.Tasks;

public sealed class AgentTask
{
    private static readonly Dictionary<AgentTaskState, AgentTaskState[]> AllowedTransitions =
        new Dictionary<AgentTaskState, AgentTaskState[]>
        {
            [AgentTaskState.Created] = [AgentTaskState.AwaitingLogin, AgentTaskState.Planning, AgentTaskState.Cancelled],
            [AgentTaskState.AwaitingLogin] = [AgentTaskState.Planning, AgentTaskState.Failed, AgentTaskState.Cancelled],
            [AgentTaskState.Planning] = [AgentTaskState.Executing, AgentTaskState.AwaitingApproval, AgentTaskState.ManualTakeover, AgentTaskState.Failed, AgentTaskState.Cancelled],
            [AgentTaskState.Executing] = [AgentTaskState.Verifying, AgentTaskState.AwaitingApproval, AgentTaskState.ManualTakeover, AgentTaskState.Failed, AgentTaskState.Cancelled],
            [AgentTaskState.Verifying] = [AgentTaskState.Planning, AgentTaskState.Completed, AgentTaskState.ManualTakeover, AgentTaskState.Failed, AgentTaskState.Cancelled],
            [AgentTaskState.AwaitingApproval] = [AgentTaskState.Executing, AgentTaskState.Failed, AgentTaskState.Cancelled],
            [AgentTaskState.ManualTakeover] = [AgentTaskState.Planning, AgentTaskState.Failed, AgentTaskState.Cancelled],
            [AgentTaskState.Completed] = [],
            [AgentTaskState.Failed] = [],
            [AgentTaskState.Cancelled] = []
        };

    private AgentTask(Guid id, string userId, string goal, DateTimeOffset createdAt)
    {
        Id = id;
        UserId = userId;
        Goal = goal;
        CreatedAt = createdAt;
        State = AgentTaskState.Created;
    }

    public Guid Id { get; }

    public string UserId { get; }

    public string Goal { get; }

    public DateTimeOffset CreatedAt { get; }

    public AgentTaskState State { get; private set; }

    public static AgentTask Create(string userId, string goal, DateTimeOffset createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(goal);

        return new AgentTask(Guid.CreateVersion7(), userId.Trim(), goal.Trim(), createdAt);
    }

    public void TransitionTo(AgentTaskState nextState)
    {
        if (!AllowedTransitions[State].Contains(nextState))
        {
            throw new InvalidTaskStateTransitionException(State, nextState);
        }

        State = nextState;
    }
}