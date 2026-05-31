using DVT.Elevator.Domain.Interfaces;
using DVT.Elevator.Domain.Enums;
using DVT.Elevator.Infrastructure.Hubs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace DVT.Elevator.Infrastructure.Services;

public class ElevatorSimulationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ElevatorSimulationService> _logger;
    private readonly int _movementIntervalMs;
    
    public ElevatorSimulationService(
        IServiceProvider serviceProvider, 
        ILogger<ElevatorSimulationService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        var configValue = configuration["ElevatorSimulation:MovementIntervalMs"];
        _movementIntervalMs = !string.IsNullOrEmpty(configValue) ? int.Parse(configValue) : 2000;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Elevator Simulation Service started");

        // Clean up any stale requests left from previous runs
        await RecoverStaleRequestsAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessElevatorMovementsAsync();
                await ProcessPendingRequestsAsync();

                await Task.Delay(_movementIntervalMs, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in elevator simulation service");
                await Task.Delay(5000, stoppingToken);
            }
        }

        _logger.LogInformation("Elevator Simulation Service stopped");
    }

    /// <summary>
    /// On startup, reset any Assigned requests whose elevator has already
    /// passed their source floor back to Pending so they get re-dispatched.
    /// </summary>
    private async Task RecoverStaleRequestsAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            // On startup, all Assigned/InProgress requests are stale.
            // Reset them to Pending and also reset their elevators to Stationary/Idle.
            var staleRequests = await unitOfWork.PassengerRequests.FindAsync(r =>
                r.Status == RequestStatus.Assigned ||
                r.Status == RequestStatus.InProgress);

            int count = 0;
            foreach (var request in staleRequests)
            {
                // Reset the assigned elevator back to idle/stationary
                if (request.AssignedElevatorId.HasValue)
                {
                    var elevator = await unitOfWork.Elevators.GetByIdAsync(request.AssignedElevatorId.Value);
                    if (elevator != null)
                    {
                        elevator.PassengerCount = 0;
                        elevator.Stop(); // sets Direction=Idle, Status=Stationary, TargetFloor=CurrentFloor
                        await unitOfWork.Elevators.UpdateAsync(elevator);
                    }
                }

                request.Status = RequestStatus.Pending;
                request.AssignedElevatorId = null;
                request.UpdatedAt = DateTime.UtcNow;
                await unitOfWork.PassengerRequests.UpdateAsync(request);
                count++;
            }

            if (count > 0)
            {
                await unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Recovered {Count} stale request(s) and reset their elevators", count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during stale request recovery");
        }
    }
    
    private async Task ProcessElevatorMovementsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var hubService = scope.ServiceProvider.GetRequiredService<IElevatorHubService>();

        var elevators = await unitOfWork.Elevators.FindAsync(e =>
            e.IsAvailable &&
            e.Status != ElevatorStatus.Maintenance);

        foreach (var elevator in elevators)
        {
            if (elevator.CurrentFloor == elevator.TargetFloor)
            {
                if (elevator.Direction != ElevatorDirection.Idle)
                {
                    await HandleElevatorArrivalAsync(elevator, unitOfWork, hubService);
                }
                continue;
            }

            if (elevator.CurrentFloor < elevator.TargetFloor)
            {
                elevator.MoveUp();
                _logger.LogDebug("Elevator {ElevatorId} moved up to floor {Floor}",
                    elevator.Id, elevator.CurrentFloor);
            }
            else if (elevator.CurrentFloor > elevator.TargetFloor)
            {
                elevator.MoveDown();
                _logger.LogDebug("Elevator {ElevatorId} moved down to floor {Floor}",
                    elevator.Id, elevator.CurrentFloor);
            }

            await unitOfWork.Elevators.UpdateAsync(elevator);

            // Broadcast movement via SignalR
            await hubService.BroadcastElevatorMovementAsync(
                elevator.BuildingId,
                elevator.Id,
                elevator.CurrentFloor,
                elevator.Direction.ToString());
        }

        await unitOfWork.SaveChangesAsync();
    }
    
    private async Task HandleElevatorArrivalAsync(Domain.Entities.Elevator elevator, IUnitOfWork unitOfWork, IElevatorHubService hubService)
    {
        _logger.LogInformation("Elevator {ElevatorId} arrived at floor {Floor}",
            elevator.Id, elevator.CurrentFloor);

        var pickupRequests = await unitOfWork.PassengerRequests.FindAsync(r =>
            r.AssignedElevatorId == elevator.Id &&
            r.SourceFloor == elevator.CurrentFloor &&
            r.Status == RequestStatus.Assigned);

        var dropoffRequests = await unitOfWork.PassengerRequests.FindAsync(r =>
            r.AssignedElevatorId == elevator.Id &&
            r.DestinationFloor == elevator.CurrentFloor &&
            r.Status == RequestStatus.InProgress);

        // Handle dropoffs first
        foreach (var request in dropoffRequests)
        {
            elevator.UnloadPassengers(request.PassengerCount);
            request.Complete();
            await unitOfWork.PassengerRequests.UpdateAsync(request);

            // Broadcast request completed
            await hubService.BroadcastRequestCompletedAsync(elevator.BuildingId, request.Id);

            _logger.LogInformation("Passengers unloaded from elevator {ElevatorId} at floor {Floor}",
                elevator.Id, elevator.CurrentFloor);
        }

        // Handle pickups
        foreach (var request in pickupRequests)
        {
            if (elevator.LoadPassengers(request.PassengerCount))
            {
                request.MarkInProgress();
                elevator.TargetFloor = request.DestinationFloor;
                await unitOfWork.PassengerRequests.UpdateAsync(request);

                _logger.LogInformation("Passengers loaded onto elevator {ElevatorId} at floor {Floor}, heading to {TargetFloor}",
                    elevator.Id, elevator.CurrentFloor, request.DestinationFloor);
            }
            else
            {
                _logger.LogWarning("Elevator {ElevatorId} overloaded, cannot accept request {RequestId}",
                    elevator.Id, request.Id);

                // Broadcast capacity warning
                await hubService.BroadcastCapacityWarningAsync(
                    elevator.BuildingId,
                    elevator.Id,
                    $"Elevator {elevator.Name} is at capacity");
            }
        }

        if (elevator.PassengerCount == 0)
            elevator.Stop();
        else
            elevator.UpdateDirection(); // sets status back to Moving and correct direction

        await unitOfWork.Elevators.UpdateAsync(elevator);
        await unitOfWork.SaveChangesAsync();

        // Broadcast updated elevator status after arrival
        await hubService.BroadcastElevatorStatusAsync(elevator.BuildingId, new Domain.DTOs.ElevatorStatusDto
        {
            Id = elevator.Id,
            Name = elevator.Name,
            CurrentFloor = elevator.CurrentFloor,
            TargetFloor = elevator.TargetFloor,
            Direction = elevator.Direction.ToString(),
            Status = elevator.Status.ToString(),
            PassengerCount = elevator.PassengerCount,
            MaxCapacity = elevator.MaxCapacity,
            IsAvailable = elevator.IsAvailable,
            LastUpdated = DateTime.UtcNow
        });
    }
    
    private async Task ProcessPendingRequestsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var dispatchStrategy = scope.ServiceProvider.GetRequiredService<IElevatorDispatchStrategy>();
        var hubService = scope.ServiceProvider.GetRequiredService<IElevatorHubService>();

        var pendingRequests = await unitOfWork.PassengerRequests.FindAsync(r =>
            r.Status == RequestStatus.Pending);

        foreach (var request in pendingRequests)
        {
            var availableElevators = await unitOfWork.Elevators.FindAsync(e =>
                e.BuildingId == request.BuildingId &&
                e.IsAvailable &&
                e.Status != ElevatorStatus.Maintenance);

            var bestElevator = await dispatchStrategy.FindBestElevatorAsync(request, availableElevators);

            if (bestElevator != null)
            {
                request.AssignElevator(bestElevator.Id);

                // Only update target if elevator isn't already at the source floor
                if (bestElevator.CurrentFloor != request.SourceFloor)
                {
                    bestElevator.TargetFloor = request.SourceFloor;
                    bestElevator.UpdateDirection();
                }
                else
                {
                    // Elevator is already at the source floor — load immediately
                    if (bestElevator.LoadPassengers(request.PassengerCount))
                    {
                        request.MarkInProgress();  // skip Assigned, go straight to InProgress
                        bestElevator.TargetFloor = request.DestinationFloor;
                        bestElevator.UpdateDirection();
                        _logger.LogInformation("Elevator {ElevatorId} already at source floor — loaded immediately, heading to {TargetFloor}",
                            bestElevator.Id, request.DestinationFloor);
                    }
                }

                await unitOfWork.PassengerRequests.UpdateAsync(request);
                await unitOfWork.Elevators.UpdateAsync(bestElevator);

                _logger.LogInformation("Pending request {RequestId} assigned to elevator {ElevatorId}",
                    request.Id, bestElevator.Id);
            }
        }

        await unitOfWork.SaveChangesAsync();
    }
}
