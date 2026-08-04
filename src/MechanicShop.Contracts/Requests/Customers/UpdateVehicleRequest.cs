using System.ComponentModel.DataAnnotations;

namespace MechanicShop.Contracts.Requests.Customers;

public sealed class UpdateVehicleRequest
{
  public Guid? VehicleId { get; set; }
  [Required(ErrorMessage = "Make is required.")]
  public string Make { get; set; }
  [Required(ErrorMessage = "Model is required.")]
  public string Model { get; set; }
  [Required(ErrorMessage = "LicensePlate is required.")]
  public string LicensePlate { get; set; }
  [Required(ErrorMessage = "Year is required.")]
  public int Year { get; set; }
}