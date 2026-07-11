using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.Customers.Vehicles;

public static class VehicleErrors
{
  public static readonly Error IdRequired = Error.Validation("VehicleErrors.IdRequired", "Vehicle Id is required.");
  public static readonly Error YearInvalid = Error.Validation("VehicleErrors.YearInvalid", "Vehicle Year is invalid.");
  public static readonly Error MakeRequired = Error.Validation("VehicleErrors.MakeRequired", "Vehicle Make is required.");
  public static readonly Error ModelRequired = Error.Validation("VehicleErrors.ModelRequired", "Vehicle Model is required.");
  public static readonly Error LicensePlateRequired = Error.Validation("VehicleErrors.LicensePlateRequired", "Vehicle License Plate is required.");
}