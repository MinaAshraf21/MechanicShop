namespace MechanicShop.Application.Features.Billings.Dtos;

public sealed class InvoiceLineItemDto
{
  public Guid InvoiceId { get; set; }
  public string? Description { get; set; }
  public decimal LineTotal { get; set; }
  public int Quantity { get; set; }
  public decimal UnitPrice { get; set; }
  public int LineNumber { get; set; }
}