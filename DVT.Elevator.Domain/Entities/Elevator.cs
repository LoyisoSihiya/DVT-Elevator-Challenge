using DVT.Elevator.Domain.Common;
using DVT.Elevator.Domain.Enums;

namespace DVT.Elevator.Domain.Entities;

public class Elevator : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int CurrentFloor { get; set; }
    public int TargetFloor { get; set; }
    public ElevatorDirection Direction { get; set; } = ElevatorDirection.Idle;
    public ElevatorStatus Status { get; set; } = ElevatorStatus.Stationary;
    public int PassengerCount { get; set; }
    public int MaxCapacity { get; set; }
    public int Speed { get; set; } // Floors per minute
    public int ElevatorTypeId { get; set; }
    public int BuildingId { get; set; }
    public bool IsAvailable { get; set; } = true;
    
    // Navigation properties
    public ElevatorType ElevatorType { get; set; } = null!;
    public Building Building { get; set; } = null!;
    public ICollection<PassengerRequest> PassengerRequests { get; set; } = new List<PassengerRequest>();
    
    // Domain methods
    public void MoveUp()
    {
        if (Status == ElevatorStatus.Maintenance || Status == ElevatorStatus.Overloaded)
            return;
            
        CurrentFloor++;
        Direction = ElevatorDirection.Up;
        Status = ElevatorStatus.Moving;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void MoveDown()
    {
        if (Status == ElevatorStatus.Maintenance || Status == ElevatorStatus.Overloaded)
            return;
            
        if (CurrentFloor > 0)
        {
            CurrentFloor--;
            Direction = ElevatorDirection.Down;
            Status = ElevatorStatus.Moving;
            UpdatedAt = DateTime.UtcNow;
        }
    }
    
    public void Stop()
    {
        Direction = ElevatorDirection.Idle;
        Status = ElevatorStatus.Stationary;
        TargetFloor = CurrentFloor;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public bool LoadPassengers(int count)
    {
        if (PassengerCount + count > MaxCapacity)
        {
            Status = ElevatorStatus.Overloaded;
            UpdatedAt = DateTime.UtcNow;
            return false;
        }
        
        PassengerCount += count;
        Status = ElevatorStatus.Loading;
        UpdatedAt = DateTime.UtcNow;
        return true;
    }
    
    public void UnloadPassengers(int count)
    {
        PassengerCount = Math.Max(0, PassengerCount - count);
        
        if (Status == ElevatorStatus.Overloaded && PassengerCount <= MaxCapacity)
        {
            Status = ElevatorStatus.Stationary;
        }
        
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void UpdateDirection()
    {
        if (CurrentFloor < TargetFloor)
        {
            Direction = ElevatorDirection.Up;
            Status = ElevatorStatus.Moving;
        }
        else if (CurrentFloor > TargetFloor)
        {
            Direction = ElevatorDirection.Down;
            Status = ElevatorStatus.Moving;
        }
        else
        {
            Direction = ElevatorDirection.Idle;
            Status = ElevatorStatus.Stationary;
        }

        UpdatedAt = DateTime.UtcNow;
    }
    
    public void SetMaintenanceMode(bool inMaintenance)
    {
        if (inMaintenance)
        {
            Status = ElevatorStatus.Maintenance;
            IsAvailable = false;
            Direction = ElevatorDirection.Idle;
        }
        else
        {
            Status = ElevatorStatus.Stationary;
            IsAvailable = true;
        }
        
        UpdatedAt = DateTime.UtcNow;
    }
    
    public int CalculateDistance(int fromFloor)
    {
        return Math.Abs(CurrentFloor - fromFloor);
    }
    
    public bool CanAcceptRequest(int passengerCount)
    {
        return IsAvailable 
            && Status != ElevatorStatus.Maintenance 
            && Status != ElevatorStatus.Overloaded
            && (PassengerCount + passengerCount) <= MaxCapacity;
    }
}
