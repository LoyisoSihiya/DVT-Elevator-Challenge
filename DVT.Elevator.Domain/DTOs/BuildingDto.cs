namespace DVT.Elevator.Domain.DTOs;

public class BuildingDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TotalFloors { get; set; }
    public DateTime CreatedAt { get; set; }
}
