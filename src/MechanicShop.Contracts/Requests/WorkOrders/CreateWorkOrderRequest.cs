using System.ComponentModel.DataAnnotations;
using MechanicShop.Contracts.Common;

namespace MechanicShop.Contracts.Requests.WorkOrders;

public sealed class CreateWorkOrderRequest
{
  [Required(ErrorMessage = "Vehicle id is required.")]
  public Guid VehicleId { get; set; }
  [Required(ErrorMessage = "Labor id is required.")]
  public Guid LaborId { get; set; }
  [Required(ErrorMessage = "Start date is required.")]
  public DateTimeOffset StartAt { get; set; }
  [Required(ErrorMessage = "Spot is required.")]
  public Spot Spot { get; set; }
  [Required(ErrorMessage = "Repair tasks is required.")]
  [MinLength(1 , ErrorMessage = "At least one repair task is required.")]
  public List<Guid> RepairTasksIds { get; set; }
}