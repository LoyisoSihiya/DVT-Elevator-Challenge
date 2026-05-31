using AutoMapper;
using DVT.Elevator.Domain.DTOs;
using DVT.Elevator.Domain.Entities;
using DVT.Elevator.Domain.Enums;
using DVT.Elevator.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using ElevatorEntity = DVT.Elevator.Domain.Entities.Elevator;

namespace DVT.Elevator.Services.Services;

public class PassengerRequestService : IPassengerRequestService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IElevatorDispatchStrategy _dispatchStrategy;
    private readonly ILogger<PassengerRequestService> _logger;

    public PassengerRequestService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IElevatorDispatchStrategy dispatchStrategy,
        ILogger<PassengerRequestService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _dispatchStrategy = dispatchStrategy;
        _logger = logger;
    }

    public async Task<PassengerRequestResponseDto> RequestElevatorAsync(CreatePassengerRequestDto dto)
    {
        _logger.LogInformation("Request from floor {Source} to {Destination} for {Count} passengers",
            dto.SourceFloor, dto.DestinationFloor, dto.PassengerCount);

        var building = await _unitOfWork.Buildings.GetByIdAsync(dto.BuildingId);
        if (building == null)
            throw new ArgumentException($"Building with ID {dto.BuildingId} not found");

        if (dto.SourceFloor < 0 || dto.SourceFloor >= building.TotalFloors)
            throw new ArgumentException($"Invalid source floor: {dto.SourceFloor}");

        if (dto.DestinationFloor < 0 || dto.DestinationFloor >= building.TotalFloors)
            throw new ArgumentException($"Invalid destination floor: {dto.DestinationFloor}");

        if (dto.SourceFloor == dto.DestinationFloor)
            throw new ArgumentException("Source and destination floors cannot be the same");

        var request = _mapper.Map<PassengerRequest>(dto);
        await _unitOfWork.PassengerRequests.AddAsync(request);
        await _unitOfWork.SaveChangesAsync();

        var availableElevators = await _unitOfWork.Elevators.FindAsync(e =>
            e.BuildingId == dto.BuildingId && e.IsAvailable);

        var bestElevator = await _dispatchStrategy.FindBestElevatorAsync(request, availableElevators);

        if (bestElevator != null)
        {
            request.AssignElevator(bestElevator.Id);
            bestElevator.TargetFloor = dto.SourceFloor;
            bestElevator.UpdateDirection();

            await _unitOfWork.PassengerRequests.UpdateAsync(request);
            await _unitOfWork.Elevators.UpdateAsync(bestElevator);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Request {RequestId} assigned to elevator {ElevatorId}", request.Id, bestElevator.Id);

            return new PassengerRequestResponseDto
            {
                RequestId = request.Id,
                Message = $"Elevator {bestElevator.Name} assigned",
                AssignedElevatorId = bestElevator.Id,
                EstimatedArrivalTime = CalculateEstimatedArrivalTime(bestElevator, dto.SourceFloor)
            };
        }

        _logger.LogWarning("No available elevator for request {RequestId}. Queued.", request.Id);

        return new PassengerRequestResponseDto
        {
            RequestId = request.Id,
            Message = "Request queued. No elevator currently available.",
            AssignedElevatorId = null,
            EstimatedArrivalTime = 0
        };
    }

    public async Task<IEnumerable<PassengerRequestDto>> GetActiveRequestsAsync(int buildingId)
    {
        var requests = await _unitOfWork.PassengerRequests.FindAsync(r =>
            r.BuildingId == buildingId &&
            (r.Status == RequestStatus.Pending ||
             r.Status == RequestStatus.Assigned ||
             r.Status == RequestStatus.InProgress));

        return _mapper.Map<IEnumerable<PassengerRequestDto>>(requests.OrderBy(r => r.RequestTime));
    }

    public async Task<IEnumerable<PassengerRequestDto>> GetRequestHistoryAsync(int buildingId, int pageNumber = 1, int pageSize = 50)
    {
        var all = await _unitOfWork.PassengerRequests.FindAsync(r => r.BuildingId == buildingId);

        var paged = all
            .OrderByDescending(r => r.RequestTime)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);

        return _mapper.Map<IEnumerable<PassengerRequestDto>>(paged);
    }

    public async Task<PassengerRequestDto?> GetRequestByIdAsync(int id)
    {
        var request = await _unitOfWork.PassengerRequests.GetByIdAsync(id);
        if (request == null) return null;
        return _mapper.Map<PassengerRequestDto>(request);
    }

    private static int CalculateEstimatedArrivalTime(ElevatorEntity elevator, int targetFloor)
    {
        int distance = Math.Abs(elevator.CurrentFloor - targetFloor);
        double secondsPerFloor = 60.0 / elevator.Speed;
        return (int)(distance * secondsPerFloor);
    }
}
