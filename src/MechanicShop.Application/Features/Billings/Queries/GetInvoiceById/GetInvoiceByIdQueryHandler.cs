
using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Features.Billings.Dtos;
using MechanicShop.Application.Features.Billings.Mappers;
using MechanicShop.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Billing.Queries.GetInvoiceById;

public class GetInvoiceByIdQueryHandler(
    ILogger<GetInvoiceByIdQueryHandler> logger,
    IAppDbContext context
    )
    : IRequestHandler<GetInvoiceByIdQuery, Result<InvoiceDto>>
{
    public async Task<Result<InvoiceDto>> Handle(GetInvoiceByIdQuery query, CancellationToken ct)
    {
        var invoice = await context.Invoices.AsNoTracking()
            .Include(i => i.InvoiceLineItems)
            .Include(i => i.WorkOrder!)
                .ThenInclude(w => w.Vehicle!)
                    .ThenInclude(v => v.Customer)
            .FirstOrDefaultAsync(i => i.Id == query.InvoiceId, ct);

        if (invoice is null)
        {
            logger.LogWarning("Invoice not found. InvoiceId: {InvoiceId}", query.InvoiceId);
            return Error.NotFound("Invoice not found.");
        }

        return invoice.ToDto();
    }
}