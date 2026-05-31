namespace DVT.Elevator.Domain.DTOs;

public class BuildingDetailDto : BuildingDto
{
    public List<ElevatorDto> Elevators { get; set; } = new();
    public List<FloorDto> Floors { get; set; } = new();
}
