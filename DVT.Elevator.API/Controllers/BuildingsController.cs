using DVT.Elevator.Domain.DTOs;
using DVT.Elevator.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DVT.Elevator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BuildingsController : ControllerBase
{
    private readonly IBuildingService _buildingService;
    private readonly ILogger<BuildingsController> _logger;
    
    public BuildingsController(IBuildingService buildingService, ILogger<BuildingsController> logger)
    {
        _buildingService = buildingService;
        _logger = logger;
    }
    
    /// <summary>
    /// Get all buildings
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BuildingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BuildingDto>>> GetAllBuildings()
    {
        var buildings = await _buildingService.GetAllBuildingsAsync();
        return Ok(buildings);
    }
    
    /// <summary>
    /// Get building by ID with details
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BuildingDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BuildingDetailDto>> GetBuildingById(int id)
    {
        var building = await _buildingService.GetBuildingByIdAsync(id);
        
        if (building == null)
            return NotFound($"Building with ID {id} not found");
        
        return Ok(building);
    }
    
    /// <summary>
    /// Create a new building
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BuildingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BuildingDto>> CreateBuilding([FromBody] CreateBuildingDto dto)
    {
        try
        {
            var building = await _buildingService.CreateBuildingAsync(dto);
            return CreatedAtAction(nameof(GetBuildingById), new { id = building.Id }, building);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
