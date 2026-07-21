using System.Security.Claims;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Application.Features.Identity.Models;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Abstractions;

public interface ITokenProvider
{
  Task<Result<TokenResponse>> GenerateJwtTokenAsync(AppUserDto user, CancellationToken cancellationToken);
  ClaimsPrincipal? ExtractUserFromExpiredToken(string expiredToken);
}