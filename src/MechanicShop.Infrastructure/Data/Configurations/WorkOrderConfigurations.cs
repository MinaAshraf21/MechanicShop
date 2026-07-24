using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicShop.Infrastructure.Data.Configurations;

public class WorkOrderConfigurations : IEntityTypeConfiguration<WorkOrder>
{
  public void Configure(EntityTypeBuilder<WorkOrder> builder)
  {
    builder.HasKey(w => w.Id).IsClustered(false);
    builder.Property(w => w.Id).ValueGeneratedNever();

    builder.HasIndex(w => w.LaborId);
    builder.HasIndex(w => w.VehicleId);
    builder.HasIndex(a => new { a.StartAtUtc, a.EndAtUtc });

    builder.Property(w => w.Spot).HasConversion<string>().IsRequired();
    builder.Property(w => w.State).HasConversion<string>().IsRequired();

    builder.Property(w => w.StartAtUtc).IsRequired();
    builder.Property(w => w.EndAtUtc).IsRequired();

    builder.Property(w => w.Tax).HasPrecision(18, 2);
    builder.Property(w => w.Discount).HasPrecision(18, 2);

    builder.Ignore(w => w.TotalCost);
    builder.Ignore(w => w.TotalLaborCost);
    builder.Ignore(w => w.TotalPartsCost);

    builder.HasMany(w => w.Tasks).WithMany().UsingEntity(j => j.ToTable("WorkOrderRepairTasks"));
    builder.HasOne(w => w.Labor).WithMany().HasForeignKey(w => w.LaborId);
    builder.HasOne(w => w.Vehicle).WithMany().HasForeignKey(w => w.VehicleId);
    builder.HasOne(w => w.Invoice).WithOne().HasForeignKey<Invoice>(i => i.WorkOrderId).OnDelete(DeleteBehavior.Restrict);

    builder.Navigation(w => w.Tasks).UsePropertyAccessMode( PropertyAccessMode.Field);
  }
}