using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Errors;
using MechanicShop.Application.Features.Billings.Dtos;
using MechanicShop.Application.Features.Billings.Mappers;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Billings.Queries.GetInvoicePdf;

public sealed class GetInvoicePdfQueryHandler(
  ILogger<GetInvoicePdfQueryHandler> logger,
  IAppDbContext context,
  IInvoicePdfGenerator invoicePdfGenerator
) : IRequestHandler<GetInvoicePdfQuery, Result<InvoicePdfDto>>
{
  public async Task<Result<InvoicePdfDto>> Handle(GetInvoicePdfQuery request, CancellationToken cancellationToken)
  {
    var invoice = await context.Invoices
                          .AsNoTracking()
                          .Include(i => i.InvoiceLineItems)
                          .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken);

    if (invoice is null)
    {
        logger.LogWarning("Invoice not found. InvoiceId: {InvoiceId}", request.InvoiceId);
        return Error.NotFound("Invoice not found.");
    }
    
    try
    {
      var pdfBytes = invoicePdfGenerator.Generate(invoice);
      var invoicePdf = new InvoicePdfDto
      {
        Content = pdfBytes,
        FileName = $"invoice-{invoice.Id}.pdf"
      };
      return invoicePdf;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Failed to generate PDF for InvoiceId: {InvoiceId}", request.InvoiceId);
      return Error.Failure("An error occurred while generating the invoice PDF.");
    }
  }
}