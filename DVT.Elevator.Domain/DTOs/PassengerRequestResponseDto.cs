namespace DVT.Elevator.Domain.DTOs;

public class PassengerRequestResponseDto
{
    public int RequestId { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? AssignedElevatorId { get; set; }
    public int EstimatedArrivalTime { get; set; }
}
