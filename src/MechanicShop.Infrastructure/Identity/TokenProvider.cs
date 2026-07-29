using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Application.Features.Identity.Models;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Identity;

namespace MechanicShop.Infrastructure.Identity;

public sealed class TokenProvider(IAppDbContext context, IConfiguration configuration) : ITokenProvider
{
  public ClaimsPrincipal? ExtractUserFromExpiredToken(string expiredToken)
  {
    var tokenValidationParameters = new TokenValidationParameters
    {
      ValidateAudience = true,
      ValidateIssuer = true,
      ValidateIssuerSigningKey = true,
      ValidateLifetime = false,
      ClockSkew = TimeSpan.Zero,
      ValidIssuer = configuration["JwtSettings:Issuer"],
      ValidAudience = configuration["JwtSettings:Audience"],
      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Secret"]!))
    };

    var tokenHandler = new JwtSecurityTokenHandler();
    var principal = tokenHandler.ValidateToken(expiredToken, tokenValidationParameters, out SecurityToken securityToken);

    if(
        securityToken is not JwtSecurityToken jwtSecurityToken || 
        !jwtSecurityToken.SignatureAlgorithm.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase)
      )
        throw new SecurityTokenException("Invalid access token");

    return principal;
  }

  public async Task<Result<TokenResponse>> GenerateJwtTokenAsync(AppUserDto user, CancellationToken cancellationToken)
  {
    var tokenResponse = await CreateAsync(user, cancellationToken);
    if (tokenResponse.IsFailure)
    {
      return tokenResponse.Errors!;
    }
    return tokenResponse.Value;
  }

  private async Task<Result<TokenResponse>> CreateAsync(AppUserDto user, CancellationToken cancellationToken)
  {
    var jwtSettings = configuration.GetSection("JwtSettings");

    var audience = jwtSettings["Audience"];
    var issuer = jwtSettings["Issuer"];
    var key = jwtSettings["Secret"];

    var expiration = DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["TokenExpirationInMinutes"]!));

    var claims = new List<Claim>
    {
      new (System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, user.UserId!),
      new (System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email, user.Email!)
    };

    foreach (var role in user.Roles)
    {
      claims.Add(new(ClaimTypes.Role, role));
    }

    var descriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(claims),
        Expires = expiration,
        Issuer = issuer,
        Audience = audience,
        SigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!)),
            SecurityAlgorithms.HmacSha256Signature),
    };
    var tokenHandler = new JwtSecurityTokenHandler();

    var securityToken = tokenHandler.CreateToken(descriptor);

    var deletedRefreshTokens = await context.RefreshTokens
                                      .Where(r => r.UserId == user.UserId)
                                      .ExecuteDeleteAsync();

    var refreshTokenResult = RefreshToken.Create(Guid.NewGuid(), GenerateRefreshToken(), user.UserId, DateTime.UtcNow.AddDays(7));

    if (refreshTokenResult.IsFailure)
    {
      return refreshTokenResult.Errors!;
    }

    context.RefreshTokens.Add(refreshTokenResult.Value);
    await context.SaveChangesAsync(cancellationToken);

    return new TokenResponse
    {
      AccessToken = tokenHandler.WriteToken(securityToken),
      ExpiresOnUtc = expiration,
      RefreshToken = refreshTokenResult.Value.Token,
    };
  }

  private static string GenerateRefreshToken()
  {
      return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
  }
}