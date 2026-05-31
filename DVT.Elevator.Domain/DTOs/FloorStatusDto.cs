namespace DVT.Elevator.Domain.DTOs;

public class FloorStatusDto
{
    public int FloorNumber { get; set; }
    public int BuildingId { get; set; }
    public int WaitingPassengers { get; set; }
    public List<PassengerRequestDto> ActiveRequests { get; set; } = new();
}
