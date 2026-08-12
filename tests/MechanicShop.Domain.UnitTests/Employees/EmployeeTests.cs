
using MechanicShop.Domain.Employees;
using MechanicShop.Domain.Identity;
using MechanicShop.Tests.Common.Employees;
using Xunit;

namespace MechanicShop.Domain.UnitTests.Employees;

public class EmployeeTests
{
  [Fact]
  public void Create_WithValidData_ShouldSucceed()
  {
    var id = Guid.NewGuid();
    const string firstName = "John";
    const string lastName = "Doe";
    const Role role = Role.Labor;

    var result = EmployeeFactory.CreateEmployee(id: id, firstName: firstName, lastName: lastName, role: role);

    Assert.True(result.IsSuccess);
    var employee = result.Value;
    Assert.Equal(id, employee.Id);
    Assert.Equal(firstName, employee.FirstName);
    Assert.Equal(lastName, employee.LastName);
    Assert.Equal(role, employee.Role);
    Assert.Equal("John Doe", employee.FullName);
  }

  [Fact]
  public void Create_WithEmptyId_ShouldFail()
  {
    var result = Employee.Create(Guid.Empty, "John", "Doe", Role.Manager);

    Assert.True(result.IsFailure);
    Assert.Equal(EmployeeErrors.IdRequired.Code, result.TopError.Code);
    Assert.Equal(EmployeeErrors.IdRequired.Description, result.TopError.Description);
  }

  [Theory]
  [InlineData("")]
  [InlineData(" ")]
  [InlineData(null)]
  public void Create_WithEmptyOrNullFirstName_ShouldFail(string? fname)
  {
    var result = Employee.Create(Guid.NewGuid(), fname, "Doe", Role.Manager);

    Assert.True(result.IsFailure);
    Assert.Equal(EmployeeErrors.FirstNameRequired.Code, result.TopError.Code);
    Assert.Equal(EmployeeErrors.FirstNameRequired.Description, result.TopError.Description);
  }

  [Theory]
  [InlineData("")]
  [InlineData(" ")]
  [InlineData(null)]
  public void Create_WithEmptyOrNullLastName_ShouldFail(string? lname)
  {
    var result = Employee.Create(Guid.NewGuid(), "John", lname, Role.Manager);

    Assert.True(result.IsFailure);
    Assert.Equal(EmployeeErrors.LastNameRequired.Code, result.TopError.Code);
    Assert.Equal(EmployeeErrors.LastNameRequired.Description, result.TopError.Description);
  }

  [Fact]
  public void Create_WithInvalidRole_ShouldFail()
  {
    var result = Employee.Create(Guid.NewGuid(), "John", "Doe", (Role)999);

    Assert.True(result.IsFailure);
    Assert.Equal(EmployeeErrors.InvalidRole.Code, result.TopError.Code);
    Assert.Equal(EmployeeErrors.InvalidRole.Description, result.TopError.Description);
  }
}