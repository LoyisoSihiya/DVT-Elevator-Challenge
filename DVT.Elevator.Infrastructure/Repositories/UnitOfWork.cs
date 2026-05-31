using DVT.Elevator.Domain.Interfaces;
using DVT.Elevator.Domain.Entities;
using DVT.Elevator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace DVT.Elevator.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ElevatorDbContext _context;
    private IDbContextTransaction? _transaction;
    
    private IRepository<Building>? _buildings;
    private IRepository<Domain.Entities.Elevator>? _elevators;
    private IRepository<Floor>? _floors;
    private IRepository<PassengerRequest>? _passengerRequests;
    private IRepository<ElevatorType>? _elevatorTypes;
    
    public UnitOfWork(ElevatorDbContext context)
    {
        _context = context;
    }
    
    public IRepository<Building> Buildings => 
        _buildings ??= new BuildingRepository(_context);
    
    public IRepository<Domain.Entities.Elevator> Elevators => 
        _elevators ??= new ElevatorRepository(_context);
    
    public IRepository<Floor> Floors => 
        _floors ??= new Repository<Floor>(_context);
    
    public IRepository<PassengerRequest> PassengerRequests => 
        _passengerRequests ??= new PassengerRequestRepository(_context);
    
    public IRepository<ElevatorType> ElevatorTypes => 
        _elevatorTypes ??= new Repository<ElevatorType>(_context);
    
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
    
    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }
    
    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
    
    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
    
    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
