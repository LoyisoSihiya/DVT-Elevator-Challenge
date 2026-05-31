using DVT.Elevator.Domain.DTOs;

namespace DVT.Elevator.Domain.Interfaces;

public interface IBuildingService
{
    Task<BuildingDto> CreateBuildingAsync(CreateBuildingDto dto);
    Task<IEnumerable<BuildingDto>> GetAllBuildingsAsync();
    Task<BuildingDetailDto?> GetBuildingByIdAsync(int id);
}
