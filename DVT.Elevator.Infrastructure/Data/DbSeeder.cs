using DVT.Elevator.Domain.Entities;
using DVT.Elevator.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DVT.Elevator.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ElevatorDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ElevatorDbContext>>();
        
        try
        {
            // Ensure database is created
            await context.Database.MigrateAsync();
            
            // Check if data already exists
            if (await context.ElevatorTypes.AnyAsync())
            {
                logger.LogInformation("Database already seeded");
                return;
            }
            
            logger.LogInformation("Seeding database...");
            
            // Seed Elevator Types
            var elevatorTypes = new List<ElevatorType>
            {
                new ElevatorType
                {
                    Name = "Passenger Elevator",
                    Description = "Standard passenger elevator for general use",
                    MaxSpeed = 60, // 60 floors per minute (1 floor per second)
                    DefaultCapacity = 10
                },
                new ElevatorType
                {
                    Name = "Freight Elevator",
                    Description = "Heavy-duty elevator for cargo and freight",
                    MaxSpeed = 30, // 30 floors per minute
                    DefaultCapacity = 20
                },
                new ElevatorType
                {
                    Name = "Glass Elevator",
                    Description = "Scenic glass elevator with panoramic views",
                    MaxSpeed = 45, // 45 floors per minute
                    DefaultCapacity = 8
                },
                new ElevatorType
                {
                    Name = "High-Speed Elevator",
                    Description = "Express elevator for tall buildings",
                    MaxSpeed = 120, // 120 floors per minute (2 floors per second)
                    DefaultCapacity = 12
                }
            };
            
            await context.ElevatorTypes.AddRangeAsync(elevatorTypes);
            await context.SaveChangesAsync();
            
            // Seed Buildings
            var building1 = new Building
            {
                Name = "DVT Tower",
                TotalFloors = 20
            };
            
            var building2 = new Building
            {
                Name = "Innovation Center",
                TotalFloors = 15
            };
            
            await context.Buildings.AddRangeAsync(building1, building2);
            await context.SaveChangesAsync();
            
            // Seed Floors for Building 1
            var floors1 = new List<Floor>();
            for (int i = 0; i < building1.TotalFloors; i++)
            {
                floors1.Add(new Floor
                {
                    FloorNumber = i,
                    BuildingId = building1.Id
                });
            }
            
            // Seed Floors for Building 2
            var floors2 = new List<Floor>();
            for (int i = 0; i < building2.TotalFloors; i++)
            {
                floors2.Add(new Floor
                {
                    FloorNumber = i,
                    BuildingId = building2.Id
                });
            }
            
            await context.Floors.AddRangeAsync(floors1);
            await context.Floors.AddRangeAsync(floors2);
            await context.SaveChangesAsync();
            
            // Seed Elevators for Building 1
            var elevators1 = new List<Domain.Entities.Elevator>
            {
                new Domain.Entities.Elevator
                {
                    Name = "Elevator A",
                    CurrentFloor = 0,
                    TargetFloor = 0,
                    Direction = ElevatorDirection.Idle,
                    Status = ElevatorStatus.Stationary,
                    PassengerCount = 0,
                    MaxCapacity = 10,
                    Speed = 60,
                    ElevatorTypeId = elevatorTypes[0].Id,
                    BuildingId = building1.Id,
                    IsAvailable = true
                },
                new Domain.Entities.Elevator
                {
                    Name = "Elevator B",
                    CurrentFloor = 0,
                    TargetFloor = 0,
                    Direction = ElevatorDirection.Idle,
                    Status = ElevatorStatus.Stationary,
                    PassengerCount = 0,
                    MaxCapacity = 10,
                    Speed = 60,
                    ElevatorTypeId = elevatorTypes[0].Id,
                    BuildingId = building1.Id,
                    IsAvailable = true
                },
                new Domain.Entities.Elevator
                {
                    Name = "Elevator C (High-Speed)",
                    CurrentFloor = 0,
                    TargetFloor = 0,
                    Direction = ElevatorDirection.Idle,
                    Status = ElevatorStatus.Stationary,
                    PassengerCount = 0,
                    MaxCapacity = 12,
                    Speed = 120,
                    ElevatorTypeId = elevatorTypes[3].Id,
                    BuildingId = building1.Id,
                    IsAvailable = true
                }
            };
            
            // Seed Elevators for Building 2
            var elevators2 = new List<Domain.Entities.Elevator>
            {
                new Domain.Entities.Elevator
                {
                    Name = "Elevator 1",
                    CurrentFloor = 0,
                    TargetFloor = 0,
                    Direction = ElevatorDirection.Idle,
                    Status = ElevatorStatus.Stationary,
                    PassengerCount = 0,
                    MaxCapacity = 8,
                    Speed = 45,
                    ElevatorTypeId = elevatorTypes[2].Id,
                    BuildingId = building2.Id,
                    IsAvailable = true
                },
                new Domain.Entities.Elevator
                {
                    Name = "Elevator 2",
                    CurrentFloor = 0,
                    TargetFloor = 0,
                    Direction = ElevatorDirection.Idle,
                    Status = ElevatorStatus.Stationary,
                    PassengerCount = 0,
                    MaxCapacity = 20,
                    Speed = 30,
                    ElevatorTypeId = elevatorTypes[1].Id,
                    BuildingId = building2.Id,
                    IsAvailable = true
                }
            };
            
            await context.Elevators.AddRangeAsync(elevators1);
            await context.Elevators.AddRangeAsync(elevators2);
            await context.SaveChangesAsync();
            
            logger.LogInformation("Database seeded successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }
}
