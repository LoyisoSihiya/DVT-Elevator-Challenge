using DVT.Elevator.Domain.DTOs;

namespace DVT.Elevator.Domain.Interfaces;

public interface IElevatorService
{
    Task<ElevatorDto> CreateElevatorAsync(CreateElevatorDto dto);
    Task<IEnumerable<ElevatorDto>> GetAllElevatorsAsync();
    Task<IEnumerable<ElevatorDto>> GetElevatorsByBuildingAsync(int buildingId);
    Task<ElevatorDto?> GetElevatorByIdAsync(int id);
    Task<ElevatorStatusDto?> GetElevatorStatusAsync(int id);
    Task<bool> SetMaintenanceModeAsync(int id, bool inMaintenance);
    Task<IEnumerable<ElevatorStatusDto>> GetAllElevatorStatusesAsync(int buildingId);
}
