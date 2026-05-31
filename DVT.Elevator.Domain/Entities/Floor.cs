using DVT.Elevator.Domain.Common;

namespace DVT.Elevator.Domain.Entities;

public class Floor : BaseEntity
{
    public int FloorNumber { get; set; }
    public int BuildingId { get; set; }
    
    // Navigation properties
    public Building Building { get; set; } = null!;
}
