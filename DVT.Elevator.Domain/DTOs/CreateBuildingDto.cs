namespace DVT.Elevator.Domain.DTOs;

public class CreateBuildingDto
{
    public string Name { get; set; } = string.Empty;
    public int TotalFloors { get; set; }
}
