using DVT.Elevator.Domain.Common;

namespace DVT.Elevator.Domain.Entities;

public class Building : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int TotalFloors { get; set; }
    
    // Navigation properties
    public ICollection<Floor> Floors { get; set; } = new List<Floor>();
    public ICollection<Elevator> Elevators { get; set; } = new List<Elevator>();
}
