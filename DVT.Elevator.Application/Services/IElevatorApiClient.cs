using DVT.Elevator.Application.Models;

namespace DVT.Elevator.Application.Services;

public interface IElevatorApiClient
{
    // Buildings
    Task<List<BuildingModel>> GetBuildingsAsync();
    Task<BuildingModel?> GetBuildingByIdAsync(int id);
    Task<BuildingModel?> CreateBuildingAsync(CreateBuildingModel model);

    // Elevators
    Task<List<ElevatorModel>> GetElevatorsByBuildingAsync(int buildingId);
    Task<List<ElevatorStatusModel>> GetElevatorStatusesAsync(int buildingId);
    Task<ElevatorStatusModel?> GetElevatorStatusAsync(int elevatorId);
    Task<bool> SetMaintenanceModeAsync(int elevatorId, bool inMaintenance);
    Task<ElevatorModel?> CreateElevatorAsync(CreateElevatorModel model);
    Task<List<ElevatorTypeModel>> GetElevatorTypesAsync();

    // Passenger Requests
    Task<PassengerRequestResponseModel?> RequestElevatorAsync(CreatePassengerRequestModel request);
    Task<List<PassengerRequestModel>> GetActiveRequestsAsync(int buildingId);
    Task<List<PassengerRequestModel>> GetRequestHistoryAsync(int buildingId);

    // Health
    Task<bool> IsApiHealthyAsync();
}
