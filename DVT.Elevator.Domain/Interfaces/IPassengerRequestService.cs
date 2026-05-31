using DVT.Elevator.Domain.DTOs;

namespace DVT.Elevator.Domain.Interfaces;

public interface IPassengerRequestService
{
    Task<PassengerRequestResponseDto> RequestElevatorAsync(CreatePassengerRequestDto dto);
    Task<IEnumerable<PassengerRequestDto>> GetActiveRequestsAsync(int buildingId);
    Task<IEnumerable<PassengerRequestDto>> GetRequestHistoryAsync(int buildingId, int pageNumber = 1, int pageSize = 50);
    Task<PassengerRequestDto?> GetRequestByIdAsync(int id);
}
