using MechanicShop.Domain.WorkOrders.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicShop.Infrastructure.Data.Configurations;

public class InvoiceConfigurations : IEntityTypeConfiguration<Invoice>
{
  public void Configure(EntityTypeBuilder<Invoice> builder)
  {
    builder.HasKey(i => i.Id).IsClustered(false);
    builder.Property(i => i.Id).ValueGeneratedNever();

    builder.Property(i => i.Status).HasConversion<string>();

    builder.Property(i => i.IssuedAtUtc).IsRequired();
    builder.Property(i => i.DiscountAmount).IsRequired().HasColumnType("decimal(18,2)");
    builder.Property(i => i.TaxAmount).IsRequired().HasColumnType("decimal(18,2)");

    builder.OwnsMany(i => i.InvoiceLineItems, items =>
    {
      items.HasKey(i => new {i.InvoiceId, i.LineNumber});
      items.ToTable("InvoiceLineItems");

      items.Property(i => i.LineNumber).ValueGeneratedNever();

      items.Property(i => i.Description).HasMaxLength(200).IsRequired();

      items.Property(i => i.Quantity).IsRequired();

      items.Property(i => i.UnitPrice).HasPrecision(18, 2).IsRequired();
    });

    builder.Navigation(i => i.InvoiceLineItems).UsePropertyAccessMode(PropertyAccessMode.Field);
  }
}