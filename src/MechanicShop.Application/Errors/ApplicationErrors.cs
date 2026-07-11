using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Errors;

public static class ApplicationErrors
{
  public static Error WorkOrderOutsideOperatingHours(DateTimeOffset startUtc, DateTimeOffset endUtc) 
              => Error.Conflict("ApplicationErrors.WorkOrderOutsideOperatingHours",
                                $"Work order start time {startUtc} and end time {endUtc} are outside of store opening hours.");

  public static readonly Error WorkOrderNotFound = Error.NotFound("WorkOrder.WorkOrderNotFound", "Work order doesn't exist.");
  public static readonly Error UserNotFound = Error.NotFound("Auth.UserNotFound", "User doesn't exist.");
  public static readonly Error LaborNotFound = Error.NotFound("Employee.LaborNotFound", "Labor doesn't exist.");
  public static readonly Error RepairTaskNotFound = Error.NotFound("RepairTask.RepairTaskNotFound", "Repair task doesn't exist.");
  public static readonly Error VehicleNotFound = Error.NotFound("Vehicle.VehicleNotFound", "Vehicle doesn't exist.");
  public static readonly Error CustomerNotFound = Error.NotFound("Customer.CustomerNotFound", "Customer doesn't exist.");
  public static readonly Error InvoiceNotFound = Error.NotFound("Invoice.InvoiceNotFound", "Invoice doesn't exist.");

  public static readonly Error TokenGenerationFailed = Error.Failure("Auth.TokenGenerationFailed", "Failed to generate authentication token.");
  public static readonly Error RefreshTokenExpired = Error.Conflict("Auth.RefreshTokenExpired", "Refresh token is invalid or has expired.");
  public static readonly Error AccessTokenExpiredInvalid = Error.Conflict("Auth.AccessTokenExpiredInvalid", "Access token is invalid.");
  public static readonly Error UserIdClaimInvalid = Error.Conflict("Auth.UserIdClaimInvalid", "Invalid userId claim.");

  public static readonly Error VehicleSchedulingConflict = Error.Conflict("Vehicle_Overlapping_WorkOrder", "Vehicle already has an overlapping work order scheduled.");

  public static readonly Error WorkOrderMustBeCompletedForInvoicing = Error.Conflict("WorkOrder.InvalidIssuance.InvalidState", "Work order must be in a 'Completed' state before invoicing.");

  public static readonly Error LaborOccupied = Error.Conflict("Employee.LaborOccupied", "Labor is already assigned to another work order during the requested time period.");

}