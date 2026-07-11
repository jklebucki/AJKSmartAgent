using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Praxiara.Api.Hubs;

[Authorize]
public sealed class TaskEventsHub : Hub
{
    public Task SubscribeToTask(Guid taskId)
    {
        if (taskId == Guid.Empty)
        {
            throw new HubException("A valid task id is required.");
        }

        return Groups.AddToGroupAsync(Context.ConnectionId, $"task:{taskId:N}");
    }

    public Task UnsubscribeFromTask(Guid taskId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, $"task:{taskId:N}");
}