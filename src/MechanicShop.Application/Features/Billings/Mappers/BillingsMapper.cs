using MechanicShop.Application.Features.Billings.Dtos;
using MechanicShop.Application.Features.Customers.Mappers;
using MechanicShop.Domain.WorkOrders.Billing;

namespace MechanicShop.Application.Features.Billings.Mappers;

public static class BillingsMapper
{
  public static InvoiceLineItemDto ToDto(this InvoiceLineItem invoiceLineItem)
  {
    ArgumentNullException.ThrowIfNull(invoiceLineItem, nameof(invoiceLineItem));

    return new InvoiceLineItemDto
    {
      InvoiceId = invoiceLineItem.InvoiceId,
      Description = invoiceLineItem.Description,
      LineTotal = invoiceLineItem.LineTotal,
      UnitPrice = invoiceLineItem.UnitPrice,
      Quantity = invoiceLineItem.Quantity,
      LineNumber = invoiceLineItem.LineNumber
    };
  }
  public static List<InvoiceLineItemDto> ToDtos(this IEnumerable<InvoiceLineItem> invoiceLineItems)
  {
    return invoiceLineItems.Select(i => i.ToDto()).ToList();
  }

  public static InvoiceDto ToDto(this Invoice invoice)
  {
    return new InvoiceDto
    {
      InvoiceId = invoice.Id,
      IssuedAtUtc = invoice.IssuedAtUtc,
      PaymentStatus = invoice.Status.ToString(),
      TaxAmount = invoice.TaxAmount,
      Subtotal = invoice.SubTotal,
      Total = invoice.Total,
      WorkOrderId = invoice.WorkOrderId,
      DiscountAmount = invoice.DiscountAmount,
      Items = invoice.InvoiceLineItems.ToDtos(),
      Vehicle = invoice.WorkOrder!.Vehicle!.ToDto(),
      Customer = invoice.WorkOrder!.Vehicle!.Customer.ToDto()
    };
  }
  public static List<InvoiceDto> ToDtos(this IEnumerable<Invoice> invoices)
  {
    return invoices.Select(i => i.ToDto()).ToList();
  }

}