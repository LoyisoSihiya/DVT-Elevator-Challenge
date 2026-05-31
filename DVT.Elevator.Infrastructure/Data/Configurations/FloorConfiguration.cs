using DVT.Elevator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DVT.Elevator.Infrastructure.Data.Configurations;

public class FloorConfiguration : IEntityTypeConfiguration<Floor>
{
    public void Configure(EntityTypeBuilder<Floor> builder)
    {
        builder.HasKey(f => f.Id);
        
        builder.Property(f => f.FloorNumber)
            .IsRequired();
        
        builder.HasIndex(f => new { f.BuildingId, f.FloorNumber })
            .IsUnique();
    }
}
