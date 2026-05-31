using DVT.Elevator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DVT.Elevator.Infrastructure.Data.Configurations;

public class ElevatorTypeConfiguration : IEntityTypeConfiguration<ElevatorType>
{
    public void Configure(EntityTypeBuilder<ElevatorType> builder)
    {
        builder.HasKey(et => et.Id);
        
        builder.Property(et => et.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(et => et.Description)
            .HasMaxLength(500);
        
        builder.Property(et => et.MaxSpeed)
            .IsRequired();
        
        builder.Property(et => et.DefaultCapacity)
            .IsRequired();
    }
}
