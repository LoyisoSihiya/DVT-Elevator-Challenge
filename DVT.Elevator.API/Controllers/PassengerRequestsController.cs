using DVT.Elevator.Domain.DTOs;
using DVT.Elevator.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DVT.Elevator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PassengerRequestsController : ControllerBase
{
    private readonly IPassengerRequestService _requestService;
    private readonly ILogger<PassengerRequestsController> _logger;
    
    public PassengerRequestsController(
        IPassengerRequestService requestService, 
        ILogger<PassengerRequestsController> logger)
    {
        _requestService = requestService;
        _logger = logger;
    }
    
    /// <summary>
    /// Request an elevator
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PassengerRequestResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PassengerRequestResponseDto>> RequestElevator([FromBody] CreatePassengerRequestDto dto)
    {
        try
        {
            var response = await _requestService.RequestElevatorAsync(dto);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    /// <summary>
    /// Get active requests for a building
    /// </summary>
    [HttpGet("building/{buildingId}/active")]
    [ProducesResponseType(typeof(IEnumerable<PassengerRequestDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PassengerRequestDto>>> GetActiveRequests(int buildingId)
    {
        var requests = await _requestService.GetActiveRequestsAsync(buildingId);
        return Ok(requests);
    }
    
    /// <summary>
    /// Get request history for a building
    /// </summary>
    [HttpGet("building/{buildingId}/history")]
    [ProducesResponseType(typeof(IEnumerable<PassengerRequestDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PassengerRequestDto>>> GetRequestHistory(
        int buildingId, 
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 50)
    {
        var requests = await _requestService.GetRequestHistoryAsync(buildingId, pageNumber, pageSize);
        return Ok(requests);
    }
    
    /// <summary>
    /// Get request by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PassengerRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PassengerRequestDto>> GetRequestById(int id)
    {
        var request = await _requestService.GetRequestByIdAsync(id);
        
        if (request == null)
            return NotFound($"Request with ID {id} not found");
        
        return Ok(request);
    }
}
