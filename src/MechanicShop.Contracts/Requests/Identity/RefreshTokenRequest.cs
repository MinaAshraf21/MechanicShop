using System.ComponentModel.DataAnnotations;

namespace MechanicShop.Contracts.Requests.Identity;

public sealed class RefreshTokenRequest
{
  [Required(ErrorMessage = "RefreshToken is required.")]
  public string? RefreshToken { get; set; }
  [Required(ErrorMessage = "ExpiredAccessToken is required.")]
  public string? ExpiredAccessToken { get; set; }
}