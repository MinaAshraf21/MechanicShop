using System.ComponentModel.DataAnnotations;

namespace MechanicShop.Contracts.Requests.Identity;

public sealed class LoginRequest
{
  [Required(ErrorMessage = "Email field is required")]
  [EmailAddress(ErrorMessage = "Invalid email format.")]
  public string? Email { get; set; }
  [Required(ErrorMessage = "Password field is required")]
  [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
  public string? Password { get; set; }
}