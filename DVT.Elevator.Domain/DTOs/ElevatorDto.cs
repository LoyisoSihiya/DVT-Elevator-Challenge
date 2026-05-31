namespace DVT.Elevator.Domain.DTOs;

public class ElevatorDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CurrentFloor { get; set; }
    public int TargetFloor { get; set; }
    public string Direction { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int PassengerCount { get; set; }
    public int MaxCapacity { get; set; }
    public int Speed { get; set; }
    public int BuildingId { get; set; }
    public string ElevatorTypeName { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
}
