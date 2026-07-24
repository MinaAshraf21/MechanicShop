using MechanicShop.Domain.Customers.Vehicles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicShop.Infrastructure.Data.Configurations;

public class VehicleConfigurations : IEntityTypeConfiguration<Vehicle>
{
  public void Configure(EntityTypeBuilder<Vehicle> builder)
  {
    builder.HasKey(v => v.Id).IsClustered(false);
    builder.ToTable("Vehicles");

    builder.Property(v => v.Make).IsRequired().HasMaxLength(50);
    builder.Property(v => v.Model).IsRequired().HasMaxLength(50);
    builder.Property(v => v.LicensePlate).IsRequired();
    builder.Property(v => v.Year).IsRequired();

    builder.HasOne(v => v.Customer).WithMany(c => c.Vehicles).HasForeignKey(v => v.CustomerId);

  }
}