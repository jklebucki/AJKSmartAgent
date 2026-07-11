namespace Praxiara.Application.Abstractions;

public interface IAgentPlanner
{
    ValueTask<PlannerDecision> DecideAsync(AgentPlanningContext context, CancellationToken cancellationToken);
}