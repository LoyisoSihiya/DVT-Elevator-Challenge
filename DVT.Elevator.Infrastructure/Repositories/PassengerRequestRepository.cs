using DVT.Elevator.Domain.Entities;
using DVT.Elevator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DVT.Elevator.Infrastructure.Repositories;

public class PassengerRequestRepository : Repository<PassengerRequest>
{
    public PassengerRequestRepository(ElevatorDbContext context) : base(context)
    {
    }
    
    public override async Task<PassengerRequest?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(pr => pr.AssignedElevator)
            .Include(pr => pr.Building)
            .FirstOrDefaultAsync(pr => pr.Id == id);
    }
    
    public override async Task<IEnumerable<PassengerRequest>> FindAsync(System.Linq.Expressions.Expression<Func<PassengerRequest, bool>> predicate)
    {
        return await _dbSet
            .Include(pr => pr.AssignedElevator)
            .Include(pr => pr.Building)
            .Where(predicate)
            .ToListAsync();
    }
}
