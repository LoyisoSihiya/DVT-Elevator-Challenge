using DVT.Elevator.Domain.DTOs;

namespace DVT.Elevator.Domain.Interfaces;

public interface IFloorService
{
    Task<IEnumerable<FloorDto>> GetFloorsByBuildingAsync(int buildingId);
    Task<FloorStatusDto?> GetFloorStatusAsync(int buildingId, int floorNumber);
}
