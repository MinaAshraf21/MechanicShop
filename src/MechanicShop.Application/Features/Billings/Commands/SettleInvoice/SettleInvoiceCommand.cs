using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.Billings.Commands.SettleInvoice;

public sealed record SettleInvoiceCommand(Guid InvoiceId) : IRequest<Result<Success>>;