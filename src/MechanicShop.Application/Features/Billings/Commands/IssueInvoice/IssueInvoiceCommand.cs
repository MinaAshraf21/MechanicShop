using MechanicShop.Application.Features.Billings.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.Billings.Commands.IssueInvoice;

public sealed record IssueInvoiceCommand(Guid WorkOrderId) : IRequest<Result<InvoiceDto>>;