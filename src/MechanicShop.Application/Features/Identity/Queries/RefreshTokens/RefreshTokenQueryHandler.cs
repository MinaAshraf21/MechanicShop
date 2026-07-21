using System.Security.Claims;
using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Errors;
using MechanicShop.Application.Features.Identity.Models;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace MechanicShop.Application.Features.Identity.Queries.RefreshTokens;

public sealed class RefreshTokenQueryHandler(
  IIdentityService identityService,
  ILogger<RefreshTokenQueryHandler> logger,
  ITokenProvider tokenProvider,
  IAppDbContext context
) : IRequestHandler<RefreshTokenQuery, Result<TokenResponse>>
{
  public async Task<Result<TokenResponse>> Handle(RefreshTokenQuery request, CancellationToken cancellationToken)
  {
    var principal = tokenProvider.ExtractUserFromExpiredToken(request.ExpiredAccessToken);

    if(principal is null)
    {
      logger.LogError("Expired access token is not valid");
      return ApplicationErrors.ExpiredAccessTokenInvalid;
    }

    var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if(userId is null)
    {
      logger.LogError("Invalid userId claim");
      return ApplicationErrors.UserIdClaimInvalid;
    }

    var userResult = await identityService.GetUserByIdAsync(userId, cancellationToken);
    if(userResult.IsFailure)
    {
      logger.LogError("Get user by id error occurred {errorDesc}", userResult.TopError.Description);
      return userResult.Errors!;
    }

    var refreshToken = await context.RefreshTokens
                                        .FirstOrDefaultAsync(r => r.Token == request.RefreshToken && r.UserId == userId);

    if(refreshToken is null)
    {
      logger.LogError("Invalid refresh token for userId: {userId}.", userId);
      return ApplicationErrors.InvalidRefreshToken;
    }
    if(refreshToken.ExpiresOnUtc < DateTime.UtcNow)
    {
      logger.LogError("Refresh token has expired.");
      return ApplicationErrors.RefreshTokenExpired;
    }

    var generateJwtTokenResult = await tokenProvider.GenerateJwtTokenAsync(userResult.Value, cancellationToken);

    if (generateJwtTokenResult.IsFailure)
    {
      logger.LogError("Generate token error occurred: {errorDesc}", generateJwtTokenResult.TopError.Description);
      return generateJwtTokenResult.Errors!;
    }

    return generateJwtTokenResult.Value;
  }
}