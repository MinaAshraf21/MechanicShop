using MechanicShop.Domain.WorkOrders.Billing;

namespace MechanicShop.Application.Abstractions;

public interface IInvoicePdfGenerator
{
  byte[] Generate(Invoice invoice);
}