using DVT.Elevator.Domain.DTOs;
using DVT.Elevator.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DVT.Elevator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ElevatorsController : ControllerBase
{
    private readonly IElevatorService _elevatorService;
    private readonly ILogger<ElevatorsController> _logger;
    
    public ElevatorsController(IElevatorService elevatorService, ILogger<ElevatorsController> logger)
    {
        _elevatorService = elevatorService;
        _logger = logger;
    }
    
    /// <summary>
    /// Get all elevators
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ElevatorDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ElevatorDto>>> GetAllElevators()
    {
        var elevators = await _elevatorService.GetAllElevatorsAsync();
        return Ok(elevators);
    }
    
    /// <summary>
    /// Get elevators by building ID
    /// </summary>
    [HttpGet("building/{buildingId}")]
    [ProducesResponseType(typeof(IEnumerable<ElevatorDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ElevatorDto>>> GetElevatorsByBuilding(int buildingId)
    {
        var elevators = await _elevatorService.GetElevatorsByBuildingAsync(buildingId);
        return Ok(elevators);
    }
    
    /// <summary>
    /// Get elevator by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ElevatorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ElevatorDto>> GetElevatorById(int id)
    {
        var elevator = await _elevatorService.GetElevatorByIdAsync(id);
        
        if (elevator == null)
            return NotFound($"Elevator with ID {id} not found");
        
        return Ok(elevator);
    }
    
    /// <summary>
    /// Get real-time elevator status
    /// </summary>
    [HttpGet("{id}/status")]
    [ProducesResponseType(typeof(ElevatorStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ElevatorStatusDto>> GetElevatorStatus(int id)
    {
        var status = await _elevatorService.GetElevatorStatusAsync(id);
        
        if (status == null)
            return NotFound($"Elevator with ID {id} not found");
        
        return Ok(status);
    }
    
    /// <summary>
    /// Get all elevator statuses for a building
    /// </summary>
    [HttpGet("building/{buildingId}/statuses")]
    [ProducesResponseType(typeof(IEnumerable<ElevatorStatusDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ElevatorStatusDto>>> GetAllElevatorStatuses(int buildingId)
    {
        var statuses = await _elevatorService.GetAllElevatorStatusesAsync(buildingId);
        return Ok(statuses);
    }
    
    /// <summary>
    /// Create a new elevator
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ElevatorDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ElevatorDto>> CreateElevator([FromBody] CreateElevatorDto dto)
    {
        try
        {
            var elevator = await _elevatorService.CreateElevatorAsync(dto);
            return CreatedAtAction(nameof(GetElevatorById), new { id = elevator.Id }, elevator);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    /// <summary>
    /// Set elevator maintenance mode
    /// </summary>
    [HttpPut("{id}/maintenance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SetMaintenanceMode(int id, [FromBody] bool inMaintenance)
    {
        var result = await _elevatorService.SetMaintenanceModeAsync(id, inMaintenance);
        
        if (!result)
            return NotFound($"Elevator with ID {id} not found");
        
        return Ok(new { Message = $"Elevator maintenance mode set to {inMaintenance}" });
    }
}
