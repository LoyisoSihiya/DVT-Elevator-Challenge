using DVT.Elevator.Domain.Entities;
using DVT.Elevator.Domain.Enums;
using DVT.Elevator.Domain.Interfaces;
using ElevatorEntity = DVT.Elevator.Domain.Entities.Elevator;

namespace DVT.Elevator.Services.Strategies;

public class NearestElevatorDispatchStrategy : IElevatorDispatchStrategy
{
    public Task<ElevatorEntity?> FindBestElevatorAsync(PassengerRequest request, IEnumerable<ElevatorEntity> availableElevators)
    {
        var list = availableElevators.ToList();

        if (!list.Any())
            return Task.FromResult<ElevatorEntity?>(null);

        var suitable = list.Where(e => e.CanAcceptRequest(request.PassengerCount)).ToList();

        if (!suitable.Any())
            return Task.FromResult<ElevatorEntity?>(null);

        var best = suitable
            .Select(e => new { Elevator = e, Score = CalculateScore(e, request) })
            .OrderByDescending(x => x.Score)
            .First().Elevator;

        return Task.FromResult<ElevatorEntity?>(best);
    }

    private static double CalculateScore(ElevatorEntity elevator, PassengerRequest request)
    {
        double score = 100.0;

        // Closer elevators score higher
        score -= elevator.CalculateDistance(request.SourceFloor) * 2;

        // Idle elevators get a bonus
        if (elevator.Direction == ElevatorDirection.Idle)
        {
            score += 20;
        }
        else if (elevator.Direction == request.Direction)
        {
            bool isOnTheWay = request.Direction == ElevatorDirection.Up
                ? elevator.CurrentFloor <= request.SourceFloor && elevator.TargetFloor >= request.SourceFloor
                : elevator.CurrentFloor >= request.SourceFloor && elevator.TargetFloor <= request.SourceFloor;

            score += isOnTheWay ? 15 : 5;
        }
        else
        {
            score -= 10;
        }

        // More available capacity scores higher
        double capacityRatio = (double)(elevator.MaxCapacity - elevator.PassengerCount) / elevator.MaxCapacity;
        score += capacityRatio * 10;

        // Penalise near-full elevators
        if (elevator.PassengerCount + request.PassengerCount > elevator.MaxCapacity * 0.8)
            score -= 15;

        return score;
    }
}
