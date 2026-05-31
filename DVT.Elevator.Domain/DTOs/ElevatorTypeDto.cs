namespace DVT.Elevator.Domain.DTOs;

public class ElevatorTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int MaxSpeed { get; set; }
    public int DefaultCapacity { get; set; }
}
