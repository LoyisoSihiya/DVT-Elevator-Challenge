using DVT.Elevator.Domain.Entities;

namespace DVT.Elevator.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<Building> Buildings { get; }
    IRepository<Domain.Entities.Elevator> Elevators { get; }
    IRepository<Floor> Floors { get; }
    IRepository<PassengerRequest> PassengerRequests { get; }
    IRepository<ElevatorType> ElevatorTypes { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
