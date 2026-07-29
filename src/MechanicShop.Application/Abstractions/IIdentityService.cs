using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Abstractions;

public interface IIdentityService
{
  Task<Result<AppUserDto>> GetUserByIdAsync(string userId);

  Task<Result<AppUserDto>> AuthenticateAsync(string email, string password);

  Task<bool> IsInRoleAsync(string userId, string role, CancellationToken cancellationToken);

  Task<string?> GetUserNameAsync(string userId);

}