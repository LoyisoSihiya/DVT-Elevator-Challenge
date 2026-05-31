using AutoMapper;
using DVT.Elevator.Domain.DTOs;
using DVT.Elevator.Domain.Enums;
using DVT.Elevator.Domain.Interfaces;

namespace DVT.Elevator.Services.Services;

public class FloorService : IFloorService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public FloorService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<FloorDto>> GetFloorsByBuildingAsync(int buildingId)
    {
        var floors = await _unitOfWork.Floors.FindAsync(f => f.BuildingId == buildingId);
        return _mapper.Map<IEnumerable<FloorDto>>(floors.OrderBy(f => f.FloorNumber));
    }

    public async Task<FloorStatusDto?> GetFloorStatusAsync(int buildingId, int floorNumber)
    {
        var floor = await _unitOfWork.Floors.FirstOrDefaultAsync(f =>
            f.BuildingId == buildingId && f.FloorNumber == floorNumber);

        if (floor == null) return null;

        var activeRequests = await _unitOfWork.PassengerRequests.FindAsync(r =>
            r.BuildingId == buildingId &&
            r.SourceFloor == floorNumber &&
            (r.Status == RequestStatus.Pending || r.Status == RequestStatus.Assigned));

        return new FloorStatusDto
        {
            FloorNumber = floorNumber,
            BuildingId = buildingId,
            WaitingPassengers = activeRequests.Sum(r => r.PassengerCount),
            ActiveRequests = _mapper.Map<List<PassengerRequestDto>>(activeRequests)
        };
    }
}
