using Microsoft.AspNetCore.SignalR;

namespace DVT.Elevator.Infrastructure.Hubs;

public class ElevatorHub : Hub
{
    public async Task SubscribeToBuilding(int buildingId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Building_{buildingId}");
    }
    
    public async Task UnsubscribeFromBuilding(int buildingId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Building_{buildingId}");
    }
}
