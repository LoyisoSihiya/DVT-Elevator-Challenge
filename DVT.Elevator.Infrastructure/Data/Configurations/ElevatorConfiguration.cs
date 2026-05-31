using DVT.Elevator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DVT.Elevator.Infrastructure.Data.Configurations;

public class ElevatorConfiguration : IEntityTypeConfiguration<Domain.Entities.Elevator>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Elevator> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.CurrentFloor)
            .IsRequired();
        
        builder.Property(e => e.TargetFloor)
            .IsRequired();
        
        builder.Property(e => e.Direction)
            .IsRequired()
            .HasConversion<string>();
        
        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>();
        
        builder.Property(e => e.PassengerCount)
            .IsRequired();
        
        builder.Property(e => e.MaxCapacity)
            .IsRequired();
        
        builder.Property(e => e.Speed)
            .IsRequired();
        
        builder.Property(e => e.IsAvailable)
            .IsRequired();
        
        builder.HasOne(e => e.ElevatorType)
            .WithMany(et => et.Elevators)
            .HasForeignKey(e => e.ElevatorTypeId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasIndex(e => e.BuildingId);
        builder.HasIndex(e => new { e.BuildingId, e.IsAvailable });
    }
}
