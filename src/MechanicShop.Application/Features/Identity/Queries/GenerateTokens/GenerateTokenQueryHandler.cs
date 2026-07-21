using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Features.Identity.Models;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Identity.Queries.GenerateTokens;

public sealed class GenerateTokenQueryHandler(
  IIdentityService identityService,
  ILogger<GenerateTokenQueryHandler> logger,
  ITokenProvider tokenProvider
) : IRequestHandler<GenerateTokenQuery, Result<TokenResponse>>
{
  public async Task<Result<TokenResponse>> Handle(GenerateTokenQuery request, CancellationToken cancellationToken)
  {
    var authResult = await identityService.AuthenticateAsync(request.Email, request.Password, cancellationToken);

    if (authResult.IsFailure)
    {
      logger.LogError("Failed login attempt with email: {email}", request.Email);
      return authResult.Errors!;
    }

    var generateJwtTokenResult = await tokenProvider.GenerateJwtTokenAsync(authResult.Value, cancellationToken);
    if(generateJwtTokenResult.IsFailure)
    {
      logger.LogError("Generate token error occurred: {ErrorDescription}", generateJwtTokenResult.TopError.Description);
      return generateJwtTokenResult.Errors!;
    }

    return generateJwtTokenResult.Value;
  }
}