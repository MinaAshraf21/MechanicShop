using MechanicShop.Domain.RepairTasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicShop.Infrastructure.Data.Configurations;

public class RepairTaskConfigurations : IEntityTypeConfiguration<RepairTask>
{
  public void Configure(EntityTypeBuilder<RepairTask> builder)
  {
    builder.HasKey(t => t.Id).IsClustered(false);
    builder.Property(t => t.Id).ValueGeneratedNever();

    builder.Property(t => t.Name).IsRequired().HasMaxLength(150);
    builder.Property(t => t.LaborCost).IsRequired().HasColumnType("decimal(18,2)");
    builder.Property(t => t.EstimatedDuration).IsRequired().HasConversion<string>();
    builder.Ignore(t => t.TotalCost);

    builder.HasMany(t => t.Parts).WithOne().HasForeignKey("RepairTaskId").OnDelete(DeleteBehavior.Cascade);

    builder.Navigation(c => c.Parts).UsePropertyAccessMode(PropertyAccessMode.Field);

  }
}