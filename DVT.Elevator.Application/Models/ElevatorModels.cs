namespace DVT.Elevator.Application.Models;

public class BuildingModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TotalFloors { get; set; }
    public int ElevatorCount { get; set; }
}

public class ElevatorModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CurrentFloor { get; set; }
    public int TargetFloor { get; set; }
    public string Direction { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int PassengerCount { get; set; }
    public int MaxCapacity { get; set; }
    public bool IsAvailable { get; set; }
    public int Speed { get; set; }
    public string? ElevatorTypeName { get; set; }
    public int BuildingId { get; set; }
}

public class ElevatorStatusModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CurrentFloor { get; set; }
    public int TargetFloor { get; set; }
    public string Direction { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int PassengerCount { get; set; }
    public int MaxCapacity { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class PassengerRequestModel
{
    public int Id { get; set; }
    public int SourceFloor { get; set; }
    public int DestinationFloor { get; set; }
    public int PassengerCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? AssignedElevatorId { get; set; }
    public DateTime RequestTime { get; set; }
    public string Direction { get; set; } = string.Empty;
}

public class CreatePassengerRequestModel
{
    public int SourceFloor { get; set; }
    public int DestinationFloor { get; set; }
    public int PassengerCount { get; set; }
    public int BuildingId { get; set; }
}

public class PassengerRequestResponseModel
{
    public int RequestId { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? AssignedElevatorId { get; set; }
    public int EstimatedArrivalTime { get; set; }
}

public class FloorStatusModel
{
    public int FloorNumber { get; set; }
    public int WaitingPassengers { get; set; }
    public bool HasActiveRequest { get; set; }
    public List<int> ElevatorIds { get; set; } = new();
}

public class ApiErrorModel
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class CreateBuildingModel
{
    public string Name { get; set; } = string.Empty;
    public int TotalFloors { get; set; }
}

public class CreateElevatorModel
{
    public string Name { get; set; } = string.Empty;
    public int InitialFloor { get; set; }
    public int MaxCapacity { get; set; }
    public int Speed { get; set; }
    public int ElevatorTypeId { get; set; }
    public int BuildingId { get; set; }
}

public class ElevatorTypeModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int MaxSpeed { get; set; }
    public int DefaultCapacity { get; set; }
}

public class ElevatorMovedModel
{
    public int ElevatorId { get; set; }
    public int CurrentFloor { get; set; }
    public string Direction { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
