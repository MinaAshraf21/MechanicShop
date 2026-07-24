using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Errors;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Billings.Commands.SettleInvoice;

public sealed class SettleInvoiceCommandHandler(
  IAppDbContext context,
  ILogger<SettleInvoiceCommandHandler> logger,
  HybridCache cache,
  TimeProvider timeProvider
) : IRequestHandler<SettleInvoiceCommand, Result<Success>>
{
  public async Task<Result<Success>> Handle(SettleInvoiceCommand request, CancellationToken cancellationToken)
  {
    var invoice = await context.Invoices
                              .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken);
  
    if(invoice is null)
    {
      logger.LogError("Settle invoice error. Invoice with Id: {id} was not found", request.InvoiceId);
      return ApplicationErrors.InvoiceNotFound;
    }

    var result = invoice.MarkAsPaid(timeProvider);
    if (result.IsFailure)
    {
      logger.LogWarning(
          "Invoice payment failed for InvoiceId: {InvoiceId}. Errors: {Errors}",
          invoice.Id,
          result.TopError.Description);
      return result.Errors!;
    }

        await context.SaveChangesAsync(cancellationToken);

        await cache.RemoveByTagAsync("invoice", cancellationToken);

        logger.LogInformation("Invoice {InvoiceId} successfully paid.", invoice.Id);

        return Result.Success;
  }
}