using DVT.Elevator.Domain.Entities;
using DVT.Elevator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DVT.Elevator.Infrastructure.Repositories;

public class BuildingRepository : Repository<Building>
{
    public BuildingRepository(ElevatorDbContext context) : base(context)
    {
    }
    
    public override async Task<Building?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(b => b.Floors)
            .Include(b => b.Elevators)
                .ThenInclude(e => e.ElevatorType)
            .FirstOrDefaultAsync(b => b.Id == id);
    }
}
