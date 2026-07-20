using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.WorkOrders.Commands.AssignLabor;


public sealed record AssignLaborCommand(Guid LaborId, Guid WorkOrderId) : IRequest<Result<Updated>>;