using MechanicShop.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicShop.Infrastructure.Data.Configurations;

public class CustomerConfigurations : IEntityTypeConfiguration<Customer>
{
  public void Configure(EntityTypeBuilder<Customer> builder)
  {
    builder.HasKey(c => c.Id).IsClustered(false);
    builder.ToTable("Customers");

    builder.Property(c => c.Name).HasMaxLength(150).IsRequired();
    builder.Property(c => c.Email).IsRequired().HasMaxLength(150);
    builder.Property(c => c.PhoneNumber).IsRequired().HasMaxLength(20);

/*
  - tells EF Core to use the private backing field for the Vehicles navigation property.
    This is essential for encapsulation, read-only collections, and advanced domain modeling
      where you want to control how your entity's data is accessed and modified.
  
  EF Core needs to populate _vehicles when loading data from the database
  but Vehicles is read-only. By specifying field access, EF Core can write directly to _vehicles.
*/
    builder.Navigation(c => c.Vehicles)
      .UsePropertyAccessMode(PropertyAccessMode.Field);
  }
}