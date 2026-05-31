using DVT.Elevator.Domain.DTOs;
using DVT.Elevator.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DVT.Elevator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ElevatorTypesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public ElevatorTypesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Get all elevator types
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ElevatorTypeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ElevatorTypeDto>>> GetElevatorTypes()
    {
        var types = await _unitOfWork.ElevatorTypes.GetAllAsync();

        var result = types.Select(t => new ElevatorTypeDto
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description,
            MaxSpeed = t.MaxSpeed,
            DefaultCapacity = t.DefaultCapacity
        });

        return Ok(result);
    }
}
