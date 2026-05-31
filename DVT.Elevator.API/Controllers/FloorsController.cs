using DVT.Elevator.Domain.DTOs;
using DVT.Elevator.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DVT.Elevator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FloorsController : ControllerBase
{
    private readonly IFloorService _floorService;
    private readonly ILogger<FloorsController> _logger;
    
    public FloorsController(IFloorService floorService, ILogger<FloorsController> logger)
    {
        _floorService = floorService;
        _logger = logger;
    }
    
    /// <summary>
    /// Get all floors for a building
    /// </summary>
    [HttpGet("building/{buildingId}")]
    [ProducesResponseType(typeof(IEnumerable<FloorDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FloorDto>>> GetFloorsByBuilding(int buildingId)
    {
        var floors = await _floorService.GetFloorsByBuildingAsync(buildingId);
        return Ok(floors);
    }
    
    /// <summary>
    /// Get floor status
    /// </summary>
    [HttpGet("building/{buildingId}/floor/{floorNumber}/status")]
    [ProducesResponseType(typeof(FloorStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FloorStatusDto>> GetFloorStatus(int buildingId, int floorNumber)
    {
        var status = await _floorService.GetFloorStatusAsync(buildingId, floorNumber);
        
        if (status == null)
            return NotFound($"Floor {floorNumber} in building {buildingId} not found");
        
        return Ok(status);
    }
}
