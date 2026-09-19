using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Errors;
using MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Contracts.Common;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MechanicShop.Tests.Common.RepairTasks;
using MediatR;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.WorkOrder.Commands.CreateWorkOrder;

[Collection(WebAppFactoryCollection.CollectionName)]
public class CreateWorkOrderCommandHandlerTests(WebAppFactory webAppFactory)
{
  private readonly IMediator _mediator = webAppFactory.CreateMediator();
  private readonly IAppDbContext _dbContext = webAppFactory.CreateDbContext();

  [Fact]
  public async Task Handle_WhenValidData_ThenCreatesWorkOrder()
  {
    // Given
    var customer = CustomerFactory.CreateCustomer(vehicleLicensePlate: "HFG 147").Value;
    var vehicle = customer.Vehicles.First();
    var labor = EmployeeFactory.CreateLabor().Value;
    var repairTask = RepairTasksFactory.CreateRepairTask().Value;

    await _dbContext.Customers.AddAsync(customer);
    await _dbContext.Employees.AddAsync(labor);
    await _dbContext.RepairTasks.AddAsync(repairTask);
    await _dbContext.SaveChangesAsync(default);

    var command  = new CreateWorkOrderCommand(
      vehicle.Id,
      DateTimeOffset.Parse("2027-01-07T11:00:00+00:00"),
      labor.Id,
      Domain.WorkOrders.Enums.Spot.A,
      [repairTask.Id]
    );

    // When
    var result = await _mediator.Send(command);

    // Then
    Assert.True(result.IsSuccess);
  }

  [Fact]
  public async Task Handle_WhenVehicleNotFound_ThenReturnsFailure()
  {
    // Given
    var labor = EmployeeFactory.CreateLabor().Value;
    var repairTask = RepairTasksFactory.CreateRepairTask().Value;
    var guid = Guid.NewGuid();

    await _dbContext.Employees.AddAsync(labor);
    await _dbContext.RepairTasks.AddAsync(repairTask);
    await _dbContext.SaveChangesAsync(default);

    var command  = new CreateWorkOrderCommand(
      guid, // Non-existing vehicle Id
      DateTimeOffset.UtcNow.AddDays(1),
      labor.Id,
      Domain.WorkOrders.Enums.Spot.A,
      [repairTask.Id]
    );

    // When
    var result = await _mediator.Send(command);

    // Then
    Assert.False(result.IsSuccess);
    Assert.Equal(ApplicationErrors.VehicleNotFound.Code, result.TopError.Code);
  }

  [Fact]
  public async Task Handle_WhenLaborNotFound_ThenReturnsFailure()
  {
    // Given
    var customer = CustomerFactory.CreateCustomer(vehicleLicensePlate: "DQG 147").Value;
    var vehicle = customer.Vehicles.First();
    var repairTask = RepairTasksFactory.CreateRepairTask().Value;
    var laborId = Guid.NewGuid();

    await _dbContext.Customers.AddAsync(customer);
    await _dbContext.RepairTasks.AddAsync(repairTask);
    await _dbContext.SaveChangesAsync(default);

    var command  = new CreateWorkOrderCommand(
      vehicle.Id,
      DateTimeOffset.UtcNow.AddDays(1),
      laborId,
      Domain.WorkOrders.Enums.Spot.A,
      [repairTask.Id]
    );

    // When
    var result = await _mediator.Send(command);

    // Then
    Assert.False(result.IsSuccess);
    Assert.Equal(ApplicationErrors.LaborNotFound.Code, result.TopError.Code);
  }

  [Fact]
  public async Task Handle_WhenRepairTaskNotFound_ThenReturnsFailure()
  {
    // Given
    var customer = CustomerFactory.CreateCustomer(vehicleLicensePlate: "DFH 147").Value;
    var vehicle = customer.Vehicles.First();
    var labor = EmployeeFactory.CreateLabor().Value;
    var repairTaskId = Guid.NewGuid();

    await _dbContext.Customers.AddAsync(customer);
    await _dbContext.Employees.AddAsync(labor);
    await _dbContext.SaveChangesAsync(default);

    var command  = new CreateWorkOrderCommand(
      vehicle.Id,
      DateTimeOffset.Parse("2027-01-01T10:00:00+00:00"),
      labor.Id,
      Domain.WorkOrders.Enums.Spot.A,
      [repairTaskId]
    );

    // When
    var result = await _mediator.Send(command);

    // Then
    Assert.False(result.IsSuccess);
    Assert.Equal(ApplicationErrors.RepairTaskNotFound.Code, result.TopError.Code);
  }

  [Theory]
  [InlineData("2027-01-11T12:00:00+00:00", 30, true, Domain.WorkOrders.Enums.Spot.A, "XBH 177")] // inside hours
  [InlineData("2027-01-04T08:30:00+00:00", 30, false, Domain.WorkOrders.Enums.Spot.B, "HBG 447")] // starts before opening
  [InlineData("2027-01-03T17:45:00+00:00", 30, false, Domain.WorkOrders.Enums.Spot.C, "XHG 947")] // ends after closing
  [InlineData("2027-01-04T09:00:00+00:00", 45, true, Domain.WorkOrders.Enums.Spot.D, "MBG 137")] // starts at opening
  [InlineData("2027-01-06T17:45:00+00:00", 15, true, Domain.WorkOrders.Enums.Spot.A, "UBG 141")] // ends exactly at closing (boundary)
  public async Task Handle_WhenOutsideOperatingHours_ThenReturnsFailure(DateTimeOffset startAt, int duration, bool expected, Domain.WorkOrders.Enums.Spot spot, string licensePlate)
  {
    // Given
    var customer = CustomerFactory.CreateCustomer(vehicleLicensePlate: licensePlate).Value;
    var vehicle = customer.Vehicles.First();
    var labor = EmployeeFactory.CreateLabor().Value;
    var durationEnum = (Domain.RepairTasks.Enums.RepairDurationInMinutes)duration;
    var repairTask = RepairTasksFactory.CreateRepairTask(repairDurationInMinutes: durationEnum).Value;

    await _dbContext.Customers.AddAsync(customer);
    await _dbContext.Employees.AddAsync(labor);
    await _dbContext.RepairTasks.AddAsync(repairTask);
    await _dbContext.SaveChangesAsync(default);

    var command  = new CreateWorkOrderCommand(
      vehicle.Id,
      startAt,
      labor.Id,
      spot,
      [repairTask.Id]
    );

    // When
    var result = await _mediator.Send(command);

    // Then
    Assert.Equal(expected, result.IsSuccess);
    if(result.IsFailure)
      Assert.Equal(ApplicationErrors.WorkOrderOutsideOperatingHours(startAt, startAt.AddMinutes(duration)).Code, result.TopError.Code);
  }

  [Fact]
  public async Task Handle_WhenSpotUnavailable_ThenReturnsFailure()
  {
    // Given
    var customer1 = CustomerFactory.CreateCustomer(vehicleLicensePlate: "BCA 123").Value;
    var vehicle1 = customer1.Vehicles.First();
    var customer2 = CustomerFactory.CreateCustomer(vehicleLicensePlate: "BCE 123").Value;
    var vehicle2 = customer2.Vehicles.First();
    var labor1 = EmployeeFactory.CreateLabor().Value;
    var labor2 = EmployeeFactory.CreateLabor().Value;
    var repairTask = RepairTasksFactory.CreateRepairTask().Value;

    await _dbContext.Customers.AddAsync(customer1);
    await _dbContext.Customers.AddAsync(customer2);
    await _dbContext.Employees.AddAsync(labor1);
    await _dbContext.Employees.AddAsync(labor2);
    await _dbContext.RepairTasks.AddAsync(repairTask);
    await _dbContext.SaveChangesAsync(default);

    var command1  = new CreateWorkOrderCommand(
      vehicle1.Id,
      DateTimeOffset.Parse("2027-01-01T10:00:00+00:00"),
      labor1.Id,
      Domain.WorkOrders.Enums.Spot.A,
      [repairTask.Id]
    );
    var command2  = new CreateWorkOrderCommand(
      vehicle2.Id,
      DateTimeOffset.Parse("2027-01-01T10:00:00+00:00"),
      labor2.Id,
      Domain.WorkOrders.Enums.Spot.A,
      [repairTask.Id]
    );

    // When
    await _mediator.Send(command1);
    var result = await _mediator.Send(command2);

    // Then
    Assert.True(result.IsFailure);
    Assert.Equal("WorkOrder_SpotTimeSlot_Unavailable", result.TopError.Code);
  }

  [Fact]
  public async Task Handle_WhenLaborOccupied_ThenReturnsFailure()
  {
    // Given
    var customer1 = CustomerFactory.CreateCustomer(vehicleLicensePlate: "BCB 123").Value;
    var vehicle1 = customer1.Vehicles.First();
    var customer2 = CustomerFactory.CreateCustomer(vehicleLicensePlate: "BCC 123").Value;
    var vehicle2 = customer2.Vehicles.First();
    var labor1 = EmployeeFactory.CreateLabor().Value;
    var repairTask = RepairTasksFactory.CreateRepairTask().Value;

    await _dbContext.Customers.AddAsync(customer1);
    await _dbContext.Customers.AddAsync(customer2);
    await _dbContext.Employees.AddAsync(labor1);
    await _dbContext.RepairTasks.AddAsync(repairTask);
    await _dbContext.SaveChangesAsync(default);

    var command1  = new CreateWorkOrderCommand(
      vehicle1.Id,
      DateTimeOffset.Parse("2027-01-01T15:00:00+00:00"),
      labor1.Id,
      Domain.WorkOrders.Enums.Spot.A,
      [repairTask.Id]
    );
    var command2  = new CreateWorkOrderCommand(
      vehicle2.Id,
      DateTimeOffset.Parse("2027-01-01T15:00:00+00:00"),
      labor1.Id,
      Domain.WorkOrders.Enums.Spot.B,
      [repairTask.Id]
    );

    // When
    await _mediator.Send(command1);
    var result = await _mediator.Send(command2);

    // Then
    Assert.True(result.IsFailure);
    Assert.Equal(ApplicationErrors.LaborOccupied.Code, result.TopError.Code);
  }

  [Fact]
  public async Task Handle_WhenVehicleAlreadyScheduled_ThenReturnsFailure()
  {
    // Given
    var customer1 = CustomerFactory.CreateCustomer(vehicleLicensePlate: "DFG 887").Value;
    var vehicle1 = customer1.Vehicles.First();
    var labor1 = EmployeeFactory.CreateLabor().Value;
    var labor2 = EmployeeFactory.CreateLabor().Value;
    var repairTask = RepairTasksFactory.CreateRepairTask().Value;

    await _dbContext.Customers.AddAsync(customer1);
    await _dbContext.Employees.AddAsync(labor1);
    await _dbContext.Employees.AddAsync(labor2);
    await _dbContext.RepairTasks.AddAsync(repairTask);
    await _dbContext.SaveChangesAsync(default);

    var command1  = new CreateWorkOrderCommand(
      vehicle1.Id,
      DateTimeOffset.Parse("2027-01-08T10:00:00+00:00"),
      labor1.Id,
      Domain.WorkOrders.Enums.Spot.A,
      [repairTask.Id]
    );
    var command2  = new CreateWorkOrderCommand(
      vehicle1.Id,
      DateTimeOffset.Parse("2027-01-08T10:00:00+00:00"),
      labor2.Id,
      Domain.WorkOrders.Enums.Spot.D,
      [repairTask.Id]
    );

    // When
    await _mediator.Send(command1);
    var result = await _mediator.Send(command2);

    // Then
    Assert.True(result.IsFailure);
    Assert.Equal(ApplicationErrors.VehicleSchedulingConflict.Code, result.TopError.Code);
  }

}