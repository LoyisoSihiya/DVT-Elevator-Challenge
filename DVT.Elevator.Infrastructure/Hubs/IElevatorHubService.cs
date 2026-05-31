using DVT.Elevator.Domain.DTOs;

namespace DVT.Elevator.Infrastructure.Hubs;

public interface IElevatorHubService
{
    Task BroadcastElevatorStatusAsync(int buildingId, ElevatorStatusDto status);
    Task BroadcastElevatorMovementAsync(int buildingId, int elevatorId, int currentFloor, string direction);
    Task BroadcastNewRequestAsync(int buildingId, PassengerRequestDto request);
    Task BroadcastRequestCompletedAsync(int buildingId, int requestId);
    Task BroadcastCapacityWarningAsync(int buildingId, int elevatorId, string message);
}
