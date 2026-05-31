using DVT.Elevator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DVT.Elevator.Infrastructure.Data.Configurations;

public class PassengerRequestConfiguration : IEntityTypeConfiguration<PassengerRequest>
{
    public void Configure(EntityTypeBuilder<PassengerRequest> builder)
    {
        builder.HasKey(pr => pr.Id);
        
        builder.Property(pr => pr.SourceFloor)
            .IsRequired();
        
        builder.Property(pr => pr.DestinationFloor)
            .IsRequired();
        
        builder.Property(pr => pr.PassengerCount)
            .IsRequired();
        
        builder.Property(pr => pr.RequestTime)
            .IsRequired();
        
        builder.Property(pr => pr.Direction)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);
        
        builder.Property(pr => pr.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        // FK → Buildings (RESTRICT: cannot delete building with requests)
        builder.HasOne(pr => pr.Building)
            .WithMany()
            .HasForeignKey(pr => pr.BuildingId)
            .OnDelete(DeleteBehavior.Restrict);

        // FK → Elevators (SET NULL: if elevator deleted, request loses assignment)
        builder.HasOne(pr => pr.AssignedElevator)
            .WithMany(e => e.PassengerRequests)
            .HasForeignKey(pr => pr.AssignedElevatorId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes for query performance
        builder.HasIndex(pr => pr.BuildingId);
        builder.HasIndex(pr => pr.Status);
        builder.HasIndex(pr => new { pr.BuildingId, pr.Status });
        builder.HasIndex(pr => pr.AssignedElevatorId);
    }
}
