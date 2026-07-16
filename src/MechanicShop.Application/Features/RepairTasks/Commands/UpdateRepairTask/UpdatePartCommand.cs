using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.RepairTasks.Commands.UpdateRepairTask;


public sealed record UpdatePartCommand(Guid? Id, string Name, decimal Cost, int Quantity) : IRequest<Result<PartDto>>;