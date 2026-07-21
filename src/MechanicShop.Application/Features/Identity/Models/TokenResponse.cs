namespace MechanicShop.Application.Features.Identity.Models;

public sealed class TokenResponse
{
  public string? RefreshToken { get; set; }
  public string? AccessToken { get; set; }
  public DateTime ExpiresOnUtc { get; set; }
}