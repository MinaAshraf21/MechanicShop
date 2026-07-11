using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.Employees;

public static class EmployeeErrors
{
  public static readonly Error IdRequired = Error.Validation("EmployeeErrors.IdRequired", "Employee Id is required.");
  public static readonly Error FirstNameRequired = Error.Validation("EmployeeErrors.FirstNameRequired", "Employee first name is required.");
  public static readonly Error LastNameRequired = Error.Validation("EmployeeErrors.LastNameRequired", "Employee last name is required.");
  public static readonly Error InvalidRole = Error.Validation("EmployeeErrors.InvalidRole", "Employee assigned to an invalid role.");
}