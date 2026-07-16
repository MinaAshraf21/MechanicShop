using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.RepairTasks.Commands.CreateRepairTask;


public sealed record CreatePartCommand(string Name, decimal Cost, int Quantity) : IRequest<Result<PartDto>>;