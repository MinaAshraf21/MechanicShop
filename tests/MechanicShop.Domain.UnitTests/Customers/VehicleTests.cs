using MechanicShop.Tests.Common.Customers;
using Xunit;

namespace MechanicShop.Domain.UnitTests.Customers;

public class VehicleTests
{
    [Fact]
  public void CreateVehicle_ShouldSucceed_WithValidData()
  {
    var id = Guid.NewGuid();
    const string make = "Honda";
    const string model = "Accord";
    const int year = 2024;
    const string licensePlate = "ABC 123";

    var result = VehicleFactory.CreateVehicle(id: id, make: make, model: model, year: year, licensePlate: licensePlate);
    var vehicle = result.Value;

    Assert.True(result.IsSuccess);
    Assert.Equal(make, vehicle.Make);
    Assert.Equal(model, vehicle.Model);
    Assert.Equal(year, vehicle.Year);
    Assert.Equal(licensePlate, vehicle.LicensePlate);
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  public void CreateVehicle_ShouldFail_WhenMakeInvalid(string make)
  {
    var result = VehicleFactory.CreateVehicle(make: make);
    Assert.True(result.IsFailure);
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  public void CreateVehicle_ShouldFail_WhenModelInvalid(string model)
  {
    var result = VehicleFactory.CreateVehicle(model: model);
    Assert.True(result.IsFailure);
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  public void CreateVehicle_ShouldFail_WhenLicensePlateInvalid(string licensePlate)
  {
    var result = VehicleFactory.CreateVehicle(licensePlate: licensePlate);
    Assert.True(result.IsFailure);
  }

  [Theory]
  [InlineData(1700)]
  [InlineData(3000)]
  public void CreateVehicle_ShouldFail_WhenYearInvalid(int year)
  {
    var result = VehicleFactory.CreateVehicle(year: year);
    Assert.True(result.IsFailure);
  }

  [Fact]
  public void UpdateVehicle_ShouldSucceed_WithValidData()
  {
    var vehicle = VehicleFactory.CreateVehicle().Value;

    var result = vehicle.Update("Toyota", "Camry", 2022, "XYZ 789");

    Assert.True(result.IsSuccess);
    Assert.Equal("Toyota", vehicle.Make);
    Assert.Equal("Camry", vehicle.Model);
    Assert.Equal(2022, vehicle.Year);
    Assert.Equal("XYZ 789", vehicle.LicensePlate);
  }

  [Theory]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("   ")]
  public void UpdateVehicle_ShouldFail_WhenMakeIsInvalid(string? make)
  {
    var vehicle = VehicleFactory.CreateVehicle().Value;

    var result = vehicle.Update(make, "Model", 2022, "XYZ123");

    Assert.True(result.IsFailure);
  }

  [Theory]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("   ")]
  public void UpdateVehicle_ShouldFail_WhenModelIsInvalid(string? model)
  {
    var vehicle = VehicleFactory.CreateVehicle().Value;

    var result = vehicle.Update("Make", model, 2022, "XYZ123");

    Assert.True(result.IsFailure);
  }

  [Theory]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("   ")]
  public void UpdateVehicle_ShouldFail_WhenLicensePlateIsInvalid(string? licensePlate)
  {
    var vehicle = VehicleFactory.CreateVehicle().Value;

    var result = vehicle.Update("Make", "Model", 2022, licensePlate);

    Assert.True(result.IsFailure);
  }

  [Theory]
  [InlineData(1800)]
  [InlineData(5000)]
  public void UpdateVehicle_ShouldFail_WhenYearInvalid(int year)
  {
    var vehicle = VehicleFactory.CreateVehicle().Value;

    var result = vehicle.Update("Make", "Model", year, "XYZ123");

    Assert.True(result.IsFailure);
  }


}