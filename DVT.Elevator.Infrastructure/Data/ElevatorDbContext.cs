using DVT.Elevator.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DVT.Elevator.Infrastructure.Data;

public class ElevatorDbContext : DbContext
{
    public ElevatorDbContext(DbContextOptions<ElevatorDbContext> options) : base(options)
    {
    }
    
    public DbSet<Building> Buildings { get; set; }
    public DbSet<Domain.Entities.Elevator> Elevators { get; set; }
    public DbSet<Floor> Floors { get; set; }
    public DbSet<PassengerRequest> PassengerRequests { get; set; }
    public DbSet<ElevatorType> ElevatorTypes { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Apply configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ElevatorDbContext).Assembly);
    }
}
