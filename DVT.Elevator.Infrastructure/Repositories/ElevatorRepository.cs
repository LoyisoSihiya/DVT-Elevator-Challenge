using DVT.Elevator.Domain.Entities;
using DVT.Elevator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DVT.Elevator.Infrastructure.Repositories;

public class ElevatorRepository : Repository<Domain.Entities.Elevator>
{
    public ElevatorRepository(ElevatorDbContext context) : base(context)
    {
    }
    
    public override async Task<Domain.Entities.Elevator?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(e => e.ElevatorType)
            .Include(e => e.Building)
            .FirstOrDefaultAsync(e => e.Id == id);
    }
    
    public override async Task<IEnumerable<Domain.Entities.Elevator>> GetAllAsync()
    {
        return await _dbSet
            .Include(e => e.ElevatorType)
            .Include(e => e.Building)
            .ToListAsync();
    }
    
    public override async Task<IEnumerable<Domain.Entities.Elevator>> FindAsync(System.Linq.Expressions.Expression<Func<Domain.Entities.Elevator, bool>> predicate)
    {
        return await _dbSet
            .Include(e => e.ElevatorType)
            .Include(e => e.Building)
            .Where(predicate)
            .ToListAsync();
    }
}
