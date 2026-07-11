using Praxiara.Domain.Tasks;

namespace Praxiara.UnitTests;

public sealed class AgentTaskTests
{
    [Fact]
    public void TransitionToAllowsDocumentedWorkflow()
    {
        var task = AgentTask.Create("user-42", "Find invoice 1001", DateTimeOffset.UtcNow);

        task.TransitionTo(AgentTaskState.Planning);
        task.TransitionTo(AgentTaskState.Executing);
        task.TransitionTo(AgentTaskState.Verifying);
        task.TransitionTo(AgentTaskState.Completed);

        Assert.Equal(AgentTaskState.Completed, task.State);
    }

    [Fact]
    public void TransitionToRejectsSkippingVerification()
    {
        var task = AgentTask.Create("user-42", "Send invoice 1001", DateTimeOffset.UtcNow);
        task.TransitionTo(AgentTaskState.Planning);
        task.TransitionTo(AgentTaskState.Executing);

        var exception = Assert.Throws<InvalidTaskStateTransitionException>(
            () => task.TransitionTo(AgentTaskState.Completed));

        Assert.Equal(AgentTaskState.Executing, exception.Current);
        Assert.Equal(AgentTaskState.Completed, exception.Requested);
    }
}