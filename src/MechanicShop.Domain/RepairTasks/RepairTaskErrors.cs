using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.RepairTasks;

public static class RepairTaskErrors
{
  public static readonly Error NameRequired = Error.Validation("RepairTask.Name.Required", "Repair task name is required.");
  public static readonly Error InvalidLaborCost = Error.Validation("RepairTask.InvalidLaborCost", "Repair task labor cost must be between 1 and 10000.");
  public static readonly Error InvalidEstimatedDuration = Error.Validation("RepairTask.InvalidEstimatedDuration", "Invalid duration selected.");
}