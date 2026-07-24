using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Errors;
using MechanicShop.Application.Features.Billings.Dtos;
using MechanicShop.Application.Features.Billings.Mappers;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Domain.WorkOrders.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Billings.Commands.IssueInvoice;

public sealed class IssueInvoiceCommandHandler(
  ILogger<IssueInvoiceCommandHandler> logger,
  IAppDbContext context,
  TimeProvider timeProvider,
  HybridCache cache
) : IRequestHandler<IssueInvoiceCommand, Result<InvoiceDto>>
{
  public async Task<Result<InvoiceDto>> Handle(IssueInvoiceCommand request, CancellationToken cancellationToken)
  {
    var workOrder = await context.WorkOrders
                                    .Include(w => w.Vehicle)
                                      .ThenInclude(v => v!.Customer)
                                    .Include(w => w.Tasks)
                                      .ThenInclude(t => t.Parts)
                                    .Where(w => w.Id == request.WorkOrderId)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(cancellationToken);
  
    if(workOrder is null)
    {
      logger.LogError("Work order with Id: {id} was not found for issuing invoice.", request.WorkOrderId);
      return ApplicationErrors.WorkOrderNotFound;
    }
    if(workOrder.State != State.Completed)
    {
      logger.LogWarning("Invoice issuance rejected. WorkOrder {WorkOrderId} is not in completed.", request.WorkOrderId);
      return ApplicationErrors.WorkOrderMustBeCompletedForInvoicing;
    }

    var lineItems = new List<InvoiceLineItem>();
    var invoiceId = Guid.NewGuid();
    int line = 1;

    foreach (var (task, taskIndex) in workOrder.Tasks.Select((t, i) => (t, i + 1)))
    {
      var partsSummary = task.Parts.Any()
        ? string.Join(Environment.NewLine, task.Parts.Select(p => $"    • {p.Name} x{p.Quantity} @ {p.Cost:C}"))
        : "    • No parts";
      
      var lineDescription =
          $"{taskIndex}: {task.Name}{Environment.NewLine}" +
          $"  Labor = {task.LaborCost:C}{Environment.NewLine}" +
          $"  Parts:{Environment.NewLine}{partsSummary}";

      var totalPartsCost = task.Parts.Sum(p => p.Cost * p.Quantity);
      var totalTaskCost = task.LaborCost + totalPartsCost;

      var invoiceLineItemResult = InvoiceLineItem.Create(invoiceId, line++, lineDescription, 1, totalTaskCost);
      if (invoiceLineItemResult.IsFailure)
      {
        return invoiceLineItemResult.Errors!;
      }
      lineItems.Add(invoiceLineItemResult.Value);
    }

    var invoiceResult = Invoice.Create(Guid.NewGuid(), request.WorkOrderId,lineItems,workOrder.Discount,workOrder.Tax, timeProvider);
    if (invoiceResult.IsFailure)
    {
      logger.LogWarning(
            "Invoice creation failed for WorkOrderId: {WorkOrderId}. Errors: {@Errors}",
            request.WorkOrderId,
            invoiceResult.Errors);
      return invoiceResult.Errors!;
    }
    var invoice = invoiceResult.Value;
    await context.Invoices.AddAsync(invoice ,cancellationToken);
    await context.SaveChangesAsync(cancellationToken);
    await cache.RemoveByTagAsync("invoice", cancellationToken);

    logger.LogInformation("Invoice {InvoiceId} issued for WorkOrder {WorkOrderId}.", invoice.Id, workOrder.Id);

    return invoice.ToDto();
  }
}