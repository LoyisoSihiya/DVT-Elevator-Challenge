namespace DVT.Elevator.Domain.DTOs;

public class PassengerRequestDto
{
    public int Id { get; set; }
    public int SourceFloor { get; set; }
    public int DestinationFloor { get; set; }
    public int PassengerCount { get; set; }
    public DateTime RequestTime { get; set; }
    public string Direction { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? AssignedElevatorId { get; set; }
    public string? AssignedElevatorName { get; set; }
    public int BuildingId { get; set; }
}
