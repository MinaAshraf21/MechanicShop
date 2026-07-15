using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.Customers;

public static class CustomerErrors
{
  public static readonly Error IdRequired = Error.Validation("CustomerErrors.IdRequired", "Customer Id is required.");
  public static Error NameRequired => Error.Validation("CustomerErrors.NameRequired", "Customer name is required.");
  public static Error InvalidPhoneNumber => Error.Validation("CustomerErrors.InvalidPhoneNumber", "Phone number must be 7–15 digits and may start with '+'.");
  public static Error EmailRequired => Error.Validation("CustomerErrors.EmailRequired", "Customer email is required.");
  public static Error InvalidEmail => Error.Validation("CustomerErrors.InvalidEmail", "Customer email is invalid.");
  public static Error CannotDeleteCustomerWithWorkOrders => Error.Failure("CustomerErrors.CannotDeleteCustomerWithWorkOrders", "Cannot delete a customer with associated work orders.");
}