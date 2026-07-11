using MechanicShop.Domain.Common;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.Identity;

public sealed class RefreshToken : AuditableEntity
{
  public string? Token { get; }
  public Guid UserId { get; }
  public DateTimeOffset ExpiresOnUtc { get; }

  private RefreshToken(){}
  private RefreshToken(Guid id, string? token, Guid userId, DateTimeOffset expiresOnUtc) : base(id)
  {
    Token = token;
    UserId = userId;
    ExpiresOnUtc = expiresOnUtc;
  }

  public static Result<RefreshToken> Create(Guid id, string? token, Guid userId, DateTimeOffset expiresOnUtc)
  {
    if (id == Guid.Empty)
    {
      return RefreshTokenErrors.IdRequired;
    }
    if (string.IsNullOrEmpty(token))
    {
      return RefreshTokenErrors.TokenRequired;
    }
    if (userId == Guid.Empty)
    {
      return RefreshTokenErrors.UserIdRequired;
    }
    if (expiresOnUtc <= DateTime.UtcNow)
    {
      return RefreshTokenErrors.ExpiryInvalid;
    }
    return new RefreshToken(id, token, userId, expiresOnUtc);
  }
}