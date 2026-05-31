using DVT.Elevator.Domain.DTOs;
using Microsoft.AspNetCore.SignalR;

namespace DVT.Elevator.Infrastructure.Hubs;

public class ElevatorHubService : IElevatorHubService
{
    private readonly IHubContext<ElevatorHub> _hubContext;
    
    public ElevatorHubService(IHubContext<ElevatorHub> hubContext)
    {
        _hubContext = hubContext;
    }
    
    public async Task BroadcastElevatorStatusAsync(int buildingId, ElevatorStatusDto status)
    {
        await _hubContext.Clients
            .Group($"Building_{buildingId}")
            .SendAsync("ElevatorStatusChanged", status);
    }
    
    public async Task BroadcastElevatorMovementAsync(int buildingId, int elevatorId, int currentFloor, string direction)
    {
        await _hubContext.Clients
            .Group($"Building_{buildingId}")
            .SendAsync("ElevatorMoved", new
            {
                ElevatorId = elevatorId,
                CurrentFloor = currentFloor,
                Direction = direction,
                Timestamp = DateTime.UtcNow
            });
    }
    
    public async Task BroadcastNewRequestAsync(int buildingId, PassengerRequestDto request)
    {
        await _hubContext.Clients
            .Group($"Building_{buildingId}")
            .SendAsync("NewRequest", request);
    }
    
    public async Task BroadcastRequestCompletedAsync(int buildingId, int requestId)
    {
        await _hubContext.Clients
            .Group($"Building_{buildingId}")
            .SendAsync("RequestCompleted", new
            {
                RequestId = requestId,
                CompletedAt = DateTime.UtcNow
            });
    }
    
    public async Task BroadcastCapacityWarningAsync(int buildingId, int elevatorId, string message)
    {
        await _hubContext.Clients
            .Group($"Building_{buildingId}")
            .SendAsync("CapacityWarning", new
            {
                ElevatorId = elevatorId,
                Message = message,
                Timestamp = DateTime.UtcNow
            });
    }
}
