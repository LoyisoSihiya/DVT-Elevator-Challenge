using DVT.Elevator.Domain.Entities;

namespace DVT.Elevator.Domain.Interfaces;

public interface IElevatorDispatchStrategy
{
    Task<Domain.Entities.Elevator?> FindBestElevatorAsync(PassengerRequest request, IEnumerable<Domain.Entities.Elevator> availableElevators);
}
