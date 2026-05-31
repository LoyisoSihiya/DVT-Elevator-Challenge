using DVT.Elevator.Domain.Common;
using DVT.Elevator.Domain.Enums;

namespace DVT.Elevator.Domain.Entities;

public class PassengerRequest : BaseEntity
{
    public int SourceFloor { get; set; }
    public int DestinationFloor { get; set; }
    public int PassengerCount { get; set; }
    public DateTime RequestTime { get; set; } = DateTime.UtcNow;
    public ElevatorDirection Direction { get; set; }
    public RequestStatus Status { get; set; } = RequestStatus.Pending;
    public int? AssignedElevatorId { get; set; }
    public int BuildingId { get; set; }
    public DateTime? CompletedTime { get; set; }
    
    // Navigation properties
    public Elevator? AssignedElevator { get; set; }
    public Building Building { get; set; } = null!;
    
    public void AssignElevator(int elevatorId)
    {
        AssignedElevatorId = elevatorId;
        Status = RequestStatus.Assigned;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void MarkInProgress()
    {
        Status = RequestStatus.InProgress;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Complete()
    {
        Status = RequestStatus.Completed;
        CompletedTime = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Cancel()
    {
        Status = RequestStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }
}
