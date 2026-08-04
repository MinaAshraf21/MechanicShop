using System.ComponentModel.DataAnnotations;

namespace MechanicShop.Contracts.Requests.Customers;

public sealed class CreateCustomerRequest
{
  [Required(ErrorMessage = "Name is Required.")]
  public string Name { get; set; }
  [Required(ErrorMessage = "PhoneNumber is Required.")]
  [RegularExpression(@"^\+?\d{7,15}$", ErrorMessage = "Phone number must be 7–15 digits and may start with '+'.")]
  public string PhoneNumber { get; set; }
  [Required(ErrorMessage = "Email is Required.")]
  [EmailAddress(ErrorMessage = "Email is invalid.")]
  public string Email { get; set; }
  [ValidateComplexType] // validate nested objects
  [MinLength(1, ErrorMessage = "At least one vehicle is required.")]
  public List<CreateVehicleRequest> Vehicles { get; set; } = [];
}
