using Xunit;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.UnitTests.Customers;

public class CustomerTests
{
  [Fact]
  public void Create_ShouldSucceed_WithValidData()
  {
    var id = Guid.NewGuid();
    const string name = "Customer #1";
    const string phoneNumber = "5555555555";
    const string email = "customer01@localhost";
    List<Vehicle> vehicles = [VehicleFactory.CreateVehicle().Value];
    var result = Customer.Create(id, name, email, phoneNumber, vehicles);
    var customer = result.Value;

    Assert.True(result.IsSuccess);
    Assert.NotNull(customer);
    Assert.IsType<Customer>(customer);
    Assert.Equal(id, customer.Id);
    Assert.Equal(name, customer.Name);
    Assert.Equal(email, customer.Email);
    Assert.Equal(phoneNumber, customer.PhoneNumber);
    Assert.Single(customer.Vehicles);
  }

  [Fact]
  public void Create_ShouldFail_WithInvalidId()
  {
    var id = Guid.Empty;
    var result = CustomerFactory.CreateCustomer(id);
    Assert.True(result.IsFailure);
  }

  [Theory]
  [InlineData("")]
  [InlineData("  ")]
  public void Create_ShouldFail_WithInvalidName(string invalidName)
  {
    var result = CustomerFactory.CreateCustomer(name: invalidName);
    Assert.True(result.IsFailure);
  }

  [Theory]
  [InlineData("")]
  [InlineData("  ")]
  [InlineData("55555")]
  [InlineData("+555555")]
  [InlineData("55555555555555555")]
  public void Create_ShouldFail_WithInvalidPhone(string invalidPhone)
  {
    var result = CustomerFactory.CreateCustomer(phoneNumber: invalidPhone);
    Assert.True(result.IsFailure);
  }

  [Theory]
  [InlineData("")]
  [InlineData("  ")]
  public void Create_ShouldFail_WithEmailEmptyOrNull(string invalidEmail)
  {
    var result = CustomerFactory.CreateCustomer(email: invalidEmail);
    Assert.True(result.IsFailure);
  }

  [Theory]
  [InlineData("abc@")]
  [InlineData("abc")]
  [InlineData("abc.@")]
  [InlineData("abc@@localhost")]
  public void Create_ShouldFail_WithInvalidEmailFormat(string? invalidEmail)
  {
    var result = CustomerFactory.CreateCustomer(email: invalidEmail);
    Assert.True(result.IsFailure);
  }

  [Fact]
  public void Update_ShouldSucceed_WithValidData()
  {
    var customer = CustomerFactory.CreateCustomer().Value;

    var result = customer.Update("Updated Name", "updated@email.com", "1234567890");
    Assert.True(result.IsSuccess);
    Assert.Equal(Result.Updated, result.Value);
  }

  [Theory]
  [InlineData("")]
  [InlineData("  ")]
  [InlineData(null)]
  public void Update_ShouldFail_WithInvalidName(string? invalidName)
  {
    var customer = CustomerFactory.CreateCustomer().Value;
    var result = customer.Update(invalidName, customer.Email, customer.PhoneNumber);
    Assert.True(result.IsFailure);
  }

  [Theory]
  [InlineData("")]
  [InlineData("  ")]
  [InlineData(null)]
  [InlineData("55555")]
  [InlineData("+555555")]
  [InlineData("55555555555555555")]
  public void UpdateCustomer_ShouldFail_WhenInvalidPhoneNumber(string? invalidPhone)
  {
    var customer = CustomerFactory.CreateCustomer().Value;

    var result = customer.Update("New name", "newEmail@localhost", invalidPhone);

    Assert.True(result.IsFailure);
  }

  [Theory]
  [InlineData("")]
  [InlineData("  ")]
  [InlineData(null)]
  [InlineData("abc@")]
  [InlineData("abc")]
  [InlineData("abc.@")]
  [InlineData("abc@@localhost")]
  public void UpdateCustomer_ShouldFail_WhenInvalidEmail(string? invalidEmail)
  {
    var customer = CustomerFactory.CreateCustomer().Value;

    var result = customer.Update("New name", invalidEmail, "123-1232");

    Assert.True(result.IsFailure);
  }

  [Fact]
  public void UpdateVehicles_ShouldAddNewVehiclesAndUpdateExisting()
  {
    var originalVehicle = VehicleFactory.CreateVehicle(make: "Ford").Value;
    var customer = CustomerFactory.CreateCustomer(vehicles: [originalVehicle]).Value;

    var updatedVehicle = VehicleFactory.CreateVehicle(id: originalVehicle.Id, make: "UpdatedFord").Value;
    var newVehicle = VehicleFactory.CreateVehicle(make: "NewBrand").Value;

    var result = customer.UpdateVehicles([updatedVehicle, newVehicle]);

    Assert.True(result.IsSuccess);
    Assert.Equal(2, customer.Vehicles.Count());
    Assert.Equal(Result.Updated, result.Value);

    Assert.Contains(customer.Vehicles, v => v.Id == originalVehicle.Id && v.Make == "UpdatedFord");
    Assert.Contains(customer.Vehicles, v => v.Id == newVehicle.Id && v.Make == "NewBrand");

  }

  [Fact]
  public void UpsertParts_ShouldRemoveVehiclesAndUpdateExisting()
  {
    var existing1 = VehicleFactory.CreateVehicle().Value;
    var existing2 = VehicleFactory.CreateVehicle(make: "Ford").Value;
    var customer = CustomerFactory.CreateCustomer(vehicles: [existing1, existing2]).Value;

    var incoming = VehicleFactory.CreateVehicle(id: existing2.Id, make: "UpdatedFord").Value;

    var result = customer.UpdateVehicles([incoming]);

    Assert.Equal(Result.Updated, result.Value);
    Assert.True(result.IsSuccess);
    Assert.Single(customer.Vehicles);
    // Assert.Equal(existing2.Id, customer.Vehicles.Single().Id);
    Assert.Contains(customer.Vehicles, v => v.Make == "UpdatedFord" && v.Id == existing2.Id);
  }
}