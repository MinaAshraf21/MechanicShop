namespace MechanicShop.Application.Features.Billings.Dtos;

public sealed class InvoicePdfDto
{
  public byte[]? Content { get; set; }
  public string? ContentType { get; set; } = "application/json";
  public string? FileName { get; set; }
}