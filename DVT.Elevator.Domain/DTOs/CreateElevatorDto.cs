namespace DVT.Elevator.Domain.DTOs;

public class CreateElevatorDto
{
    public string Name { get; set; } = string.Empty;
    public int InitialFloor { get; set; }
    public int MaxCapacity { get; set; }
    public int Speed { get; set; }
    public int ElevatorTypeId { get; set; }
    public int BuildingId { get; set; }
}
