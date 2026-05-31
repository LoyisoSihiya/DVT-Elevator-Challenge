namespace DVT.Elevator.Domain.DTOs;

public class CreatePassengerRequestDto
{
    public int SourceFloor { get; set; }
    public int DestinationFloor { get; set; }
    public int PassengerCount { get; set; }
    public int BuildingId { get; set; }
}
