using AutoMapper;
using DVT.Elevator.Domain.DTOs;
using DVT.Elevator.Domain.Entities;
using DVT.Elevator.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace DVT.Elevator.Services.Services;

public class BuildingService : IBuildingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<BuildingService> _logger;

    public BuildingService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<BuildingService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BuildingDto> CreateBuildingAsync(CreateBuildingDto dto)
    {
        _logger.LogInformation("Creating building: {BuildingName} with {TotalFloors} floors", dto.Name, dto.TotalFloors);

        var building = _mapper.Map<Building>(dto);

        for (int i = 0; i < dto.TotalFloors; i++)
        {
            building.Floors.Add(new Floor
            {
                FloorNumber = i,
                BuildingId = building.Id
            });
        }

        await _unitOfWork.Buildings.AddAsync(building);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Building created with ID: {BuildingId}", building.Id);

        return _mapper.Map<BuildingDto>(building);
    }

    public async Task<IEnumerable<BuildingDto>> GetAllBuildingsAsync()
    {
        var buildings = await _unitOfWork.Buildings.GetAllAsync();
        return _mapper.Map<IEnumerable<BuildingDto>>(buildings);
    }

    public async Task<BuildingDetailDto?> GetBuildingByIdAsync(int id)
    {
        var building = await _unitOfWork.Buildings.GetByIdAsync(id);
        if (building == null) return null;
        return _mapper.Map<BuildingDetailDto>(building);
    }
}
