using System.ComponentModel.DataAnnotations;
using MechanicShop.Contracts.Common;

namespace MechanicShop.Contracts.Requests.RepairTasks;

public sealed class CreateRepairTaskRequest
{
  [Required(ErrorMessage = "Name field is required.")]
  public string Name { get; set; }
  [Required(ErrorMessage = "LaborCost field is required.")]
  public decimal LaborCost { get; set; }
  [Required(ErrorMessage = "EstimatedDuration field is required.")]
  public RepairDurationInMinutes EstimatedDuration { get; set; }
  [MinLength(1, ErrorMessage = "At least one part is required for a repair task.")]
  [ValidateComplexType]
  public List<CreatePartRequest> Parts {get; set;} = [];
}