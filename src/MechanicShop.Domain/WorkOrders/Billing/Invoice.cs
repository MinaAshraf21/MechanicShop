using MechanicShop.Domain.Common;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.WorkOrders.Billing;

public sealed class Invoice : AuditableEntity
{
  public decimal DiscountAmount { get; private set; }
  public decimal TaxAmount { get; }
  public decimal SubTotal => _invoiceLineItems.Sum(i => i.LineTotal);
  public decimal Total => SubTotal - DiscountAmount + TaxAmount;
  public DateTimeOffset IssuedAtUtc { get; }
  public DateTimeOffset PaidAtUtc { get; private set; }
  public Guid WorkOrderId { get; }
  public WorkOrder? WorkOrder { get; set; }
  public InvoiceStatus Status { get; private set; }

  private readonly List<InvoiceLineItem> _invoiceLineItems = [];
  public IReadOnlyList<InvoiceLineItem> InvoiceLineItems => _invoiceLineItems;

    private Invoice()
    { }

    private Invoice(
        Guid id,
        Guid workOrderId,
        DateTimeOffset issuedAt,
        List<InvoiceLineItem> lineItems,
        decimal discountAmount,
        decimal taxAmount)
        : base(id)
    {
        WorkOrderId = workOrderId;
        IssuedAtUtc = issuedAt;
        DiscountAmount = discountAmount;
        Status = InvoiceStatus.Unpaid;
        TaxAmount = taxAmount;
        _invoiceLineItems = lineItems;
    }

  public static Result<Invoice> Create(Guid id,
        Guid workOrderId,
        List<InvoiceLineItem> lineItems,
        decimal discountAmount,
        decimal taxAmount,
        TimeProvider timeProvider)
  {
    if(workOrderId == Guid.Empty)
    {
      return InvoiceErrors.WorkOrderIdInvalid;
    }
    if(lineItems is null || lineItems.Count == 0)
    {
      return InvoiceErrors.LineItemsEmpty;
    }

    return new Invoice(id, workOrderId, timeProvider.GetUtcNow(), lineItems, discountAmount, taxAmount);
  }

  public Result<Updated> ApplyDiscount(decimal discountAmount)
  {
    if(Status != InvoiceStatus.Unpaid)
    {
      return InvoiceErrors.InvoiceLocked;
    }
    if(discountAmount < 0)
    {
      return InvoiceErrors.DiscountNegative;
    }
    if(discountAmount > SubTotal)
    {
      return InvoiceErrors.DiscountExceedsSubtotal;
    }

    DiscountAmount = discountAmount;
    return Result.Updated;
  }

  public Result<Updated> MarkAsPaid(TimeProvider timeProvider)
  {
    if(Status != InvoiceStatus.Unpaid)
    {
      return InvoiceErrors.InvoiceLocked;
    }
    PaidAtUtc = timeProvider.GetUtcNow();
    Status = InvoiceStatus.Paid;
    return Result.Updated;
  }
}
