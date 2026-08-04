using System.ComponentModel.DataAnnotations;

namespace MechanicShop.Contracts.Requests.RepairTasks;

public sealed class CreatePartRequest
{
  [Required(ErrorMessage = "Name field is required.")]
  public string? Name { get; set; }
  [Required(ErrorMessage = "Cost field is required.")]
  public decimal Cost { get; set; }
  [Required(ErrorMessage = "Quantity field is required.")]
  public int Quantity { get; set; }
}