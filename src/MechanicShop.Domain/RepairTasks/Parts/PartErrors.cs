using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.RepairTasks.Parts;


public static class PartErrors
{
  public static readonly Error NameRequired = Error.Validation("PartErrors.NameRequired", "Part name is required.");
  public static readonly Error InvalidCost = Error.Validation("PartErrors.InvalidCost", "Part cost must be greater than 0 and less than or equal to 10,000.");
  public static readonly Error InvalidQuantity = Error.Validation("PartErrors.InvalidQuantity", "Part quantity must be greater than 0 and less than or equal to 10.");
}