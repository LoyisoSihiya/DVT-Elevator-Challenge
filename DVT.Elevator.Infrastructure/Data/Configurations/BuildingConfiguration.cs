using DVT.Elevator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DVT.Elevator.Infrastructure.Data.Configurations;

public class BuildingConfiguration : IEntityTypeConfiguration<Building>
{
    public void Configure(EntityTypeBuilder<Building> builder)
    {
        builder.HasKey(b => b.Id);
        
        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(b => b.TotalFloors)
            .IsRequired();
        
        builder.HasMany(b => b.Floors)
            .WithOne(f => f.Building)
            .HasForeignKey(f => f.BuildingId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(b => b.Elevators)
            .WithOne(e => e.Building)
            .HasForeignKey(e => e.BuildingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
