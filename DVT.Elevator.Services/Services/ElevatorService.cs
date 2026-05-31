using AutoMapper;
using DVT.Elevator.Domain.DTOs;
using DVT.Elevator.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using ElevatorEntity = DVT.Elevator.Domain.Entities.Elevator;

namespace DVT.Elevator.Services.Services;

public class ElevatorService : IElevatorService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ElevatorService> _logger;

    public ElevatorService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ElevatorService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ElevatorDto> CreateElevatorAsync(CreateElevatorDto dto)
    {
        _logger.LogInformation("Creating elevator: {ElevatorName} in building {BuildingId}", dto.Name, dto.BuildingId);

        var building = await _unitOfWork.Buildings.GetByIdAsync(dto.BuildingId);
        if (building == null)
            throw new ArgumentException($"Building with ID {dto.BuildingId} not found");

        var elevatorType = await _unitOfWork.ElevatorTypes.GetByIdAsync(dto.ElevatorTypeId);
        if (elevatorType == null)
            throw new ArgumentException($"Elevator type with ID {dto.ElevatorTypeId} not found");

        var elevator = _mapper.Map<ElevatorEntity>(dto);

        await _unitOfWork.Elevators.AddAsync(elevator);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Elevator created with ID: {ElevatorId}", elevator.Id);

        return _mapper.Map<ElevatorDto>(elevator);
    }

    public async Task<IEnumerable<ElevatorDto>> GetAllElevatorsAsync()
    {
        var elevators = await _unitOfWork.Elevators.GetAllAsync();
        return _mapper.Map<IEnumerable<ElevatorDto>>(elevators);
    }

    public async Task<IEnumerable<ElevatorDto>> GetElevatorsByBuildingAsync(int buildingId)
    {
        var elevators = await _unitOfWork.Elevators.FindAsync(e => e.BuildingId == buildingId);
        return _mapper.Map<IEnumerable<ElevatorDto>>(elevators);
    }

    public async Task<ElevatorDto?> GetElevatorByIdAsync(int id)
    {
        var elevator = await _unitOfWork.Elevators.GetByIdAsync(id);
        if (elevator == null) return null;
        return _mapper.Map<ElevatorDto>(elevator);
    }

    public async Task<ElevatorStatusDto?> GetElevatorStatusAsync(int id)
    {
        var elevator = await _unitOfWork.Elevators.GetByIdAsync(id);
        if (elevator == null) return null;
        return _mapper.Map<ElevatorStatusDto>(elevator);
    }

    public async Task<bool> SetMaintenanceModeAsync(int id, bool inMaintenance)
    {
        var elevator = await _unitOfWork.Elevators.GetByIdAsync(id);
        if (elevator == null) return false;

        _logger.LogInformation("Setting elevator {ElevatorId} maintenance to {Mode}", id, inMaintenance);

        elevator.SetMaintenanceMode(inMaintenance);
        await _unitOfWork.Elevators.UpdateAsync(elevator);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<ElevatorStatusDto>> GetAllElevatorStatusesAsync(int buildingId)
    {
        var elevators = await _unitOfWork.Elevators.FindAsync(e => e.BuildingId == buildingId);
        return _mapper.Map<IEnumerable<ElevatorStatusDto>>(elevators);
    }
}
