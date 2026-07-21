using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Errors;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Identity.Queries.GetUserById;

public sealed class GetUserByIdQueryHandler(
  IIdentityService identityService,
  ILogger<GetUserByIdQueryHandler> logger
  ) : IRequestHandler<GetUserByIdQuery, Result<AppUserDto>>
{
  public async Task<Result<AppUserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
  {
    var getUserByIdResult = await identityService.GetUserByIdAsync(request.UserId!, cancellationToken);

    if(getUserByIdResult.IsFailure)
    {
      logger.LogError("User with Id: {id} {ErrorDetails}", request.UserId, getUserByIdResult.TopError.Description);
      return getUserByIdResult.Errors!;
    }

    return getUserByIdResult.Value;
  }
}