using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Errors;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;

namespace MechanicShop.Infrastructure.Identity;

public sealed class IdentityService(
  UserManager<AppUser> userManager
  ) : IIdentityService
{
  public async Task<Result<AppUserDto>> AuthenticateAsync(string email, string password)
  {
    var user = await userManager.FindByEmailAsync(email);
    if(user is null)
    {
      return Error.NotFound("User_Not_Found", $"User with email {UtilityService.MaskEmail(email)} not found");
    }
    if (!user.EmailConfirmed)
    {
      return Error.Conflict("Email_Not_Confirmed", $"email '{UtilityService.MaskEmail(email)}' not confirmed");
    }
    if(!await userManager.CheckPasswordAsync(user, password))
    {
      return Error.Conflict("Invalid_Login_Attempt", "Email or Password are incorrect");
    }
    
    var roles = await userManager.GetRolesAsync(user);
    var claims = await userManager.GetClaimsAsync(user);

    return new AppUserDto(user.Id, user.Email, roles, claims);
  }

  public async Task<Result<AppUserDto>> GetUserByIdAsync(string userId)
  {
    var user = await userManager.FindByIdAsync(userId);
    if(user is null)
      return ApplicationErrors.UserNotFound;

    var roles = await userManager.GetRolesAsync(user);
    var claims = await userManager.GetClaimsAsync(user);

    return new AppUserDto(userId, user.Email, roles, claims);
  }

  public async Task<string?> GetUserNameAsync(string userId)
  {
    var user = await userManager.FindByIdAsync(userId);
    
    return user?.Email;
  }

  public async Task<bool> IsInRoleAsync(string userId, string role, CancellationToken cancellationToken)
  {
    var user = await userManager.FindByIdAsync(userId);

    return user is not null && await userManager.IsInRoleAsync(user, role);
  }
}