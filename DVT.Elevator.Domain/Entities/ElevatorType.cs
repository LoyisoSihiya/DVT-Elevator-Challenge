using DVT.Elevator.Domain.Common;

namespace DVT.Elevator.Domain.Entities;

public class ElevatorType : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int MaxSpeed { get; set; } // Floors per minute
    public int DefaultCapacity { get; set; } // Maximum passengers
    
    // Navigation properties
    public ICollection<Elevator> Elevators { get; set; } = new List<Elevator>();
}
