using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Errors;
using MechanicShop.Application.Features.WorkOrders.Commands.RelocateWorkOrder;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MechanicShop.Tests.Common.RepairTasks;
using MechanicShop.Tests.Common.WorkOrders;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.WorkOrder.Commands.RelocateWorkOrder;

[Collection(WebAppFactoryCollection.CollectionName)]
public class RelocateWorkOrderCommandHandlerTests(WebAppFactory factory)
{
  private readonly IMediator _mediator = factory.CreateMediator();
  private readonly IAppDbContext _dbContext = factory.CreateDbContext();

  [Fact]
  public async Task Handle_WhenWorkOrderNotFound_ThenReturnFailure()
  {
    // Given
    var fakeWorkOrderId = Guid.NewGuid();
    // When
    var command = new RelocateWorkOrderCommand(fakeWorkOrderId, DateTimeOffset.Now.AddMinutes(15), Domain.WorkOrders.Enums.Spot.A);
    var result = await _mediator.Send(command);
    // Then
    Assert.True(result.IsFailure);
    Assert.Equal(ApplicationErrors.WorkOrderNotFound.Code, result.TopError.Code);
  }

  // [InlineData("2027-01-01T10:00:00+00:00", 30, true)] // inside hours
  // [InlineData("2027-01-04T09:00:00+00:00", 45, true)] // starts at opening
  // [InlineData("2027-01-05T17:45:00+00:00", 15, true)] // ends exactly at closing (boundary)
  [Theory]
  [InlineData("2027-01-02T08:30:00+00:00", 30, "ABC 123")] // starts before opening
  [InlineData("2027-01-03T17:45:00+00:00", 30, "DEF 345")] // ends after closing
  public async Task Handle_WhenOutsideOperatingHours_ThenReturnsFailure(DateTimeOffset newStartAt, int duration, string licensePlate)
  {
    // Given
    var customer = CustomerFactory.CreateCustomer(vehicleLicensePlate:licensePlate).Value;
    var vehicle = customer.Vehicles.First();
    var labor = EmployeeFactory.CreateLabor().Value;
    var durationEnum = (Domain.RepairTasks.Enums.RepairDurationInMinutes)duration;
    var repairTask = RepairTasksFactory.CreateRepairTask(repairDurationInMinutes: durationEnum).Value;
    var fakeWorkOrderId = Guid.NewGuid();
    var tasksDuration = (int)repairTask.EstimatedDuration;
    var startAt = DateTimeOffset.UtcNow;
    var endAt = startAt.AddMinutes(tasksDuration);
    var workOrder = WorkOrderFactory.CreateWorkOrder(
                id:fakeWorkOrderId,
                vehicleId: vehicle.Id,
                laborId: labor.Id,
                repairTasks:[repairTask],
                startAt:startAt,
                endAt: endAt
              ).Value;
    await _dbContext.Customers.AddAsync(customer);
    await _dbContext.Employees.AddAsync(labor);
    await _dbContext.RepairTasks.AddAsync(repairTask);
    await _dbContext.WorkOrders.AddAsync(workOrder);
    await _dbContext.SaveChangesAsync(default);

    // When
    var command = new RelocateWorkOrderCommand(workOrder.Id, newStartAt, Domain.WorkOrders.Enums.Spot.A);
    var result = await _mediator.Send(command);

    // Then
    Assert.True(result.IsFailure);
    Assert.Equal(ApplicationErrors.WorkOrderOutsideOperatingHours(newStartAt, newStartAt.AddMinutes(duration)).Code, result.TopError.Code);
  }

  [Fact]
  public async Task Handle_WhenSpotUnavailable_ThenReturnsFailure()
  {
    // Given
    var customer1 = CustomerFactory.CreateCustomer(vehicleLicensePlate: "BBB 123").Value;
    var vehicle1 = customer1.Vehicles.First();
    var labor1 = EmployeeFactory.CreateLabor().Value;
    var repairTask = RepairTasksFactory.CreateRepairTask().Value;

    var tasksDuration = (int)repairTask.EstimatedDuration;
    var startAt = DateTimeOffset.Parse("2027-01-01T10:00:00+00:00");
    var endAt = startAt.AddMinutes(tasksDuration);
    var workOrder = WorkOrderFactory.CreateWorkOrder(
                id:Guid.NewGuid(),
                vehicleId: vehicle1.Id,
                laborId: labor1.Id,
                repairTasks:[repairTask],
                startAt:startAt,
                endAt: endAt
              ).Value;
    var workOrder2 = WorkOrderFactory.CreateWorkOrder(
                id:Guid.NewGuid(),
                vehicleId: vehicle1.Id,
                laborId: labor1.Id,
                repairTasks:[repairTask],
                startAt:startAt,
                endAt: endAt
              ).Value;
    await _dbContext.Customers.AddAsync(customer1);
    await _dbContext.Employees.AddAsync(labor1);
    await _dbContext.RepairTasks.AddAsync(repairTask);
    await _dbContext.WorkOrders.AddAsync(workOrder);
    await _dbContext.WorkOrders.AddAsync(workOrder2);
    await _dbContext.SaveChangesAsync(default);

    // When
    var command = new RelocateWorkOrderCommand(workOrder2.Id, workOrder.StartAtUtc, workOrder.Spot);
    var result = await _mediator.Send(command);

    // Then
    Assert.True(result.IsFailure);
    Assert.Equal("WorkOrder_SpotTimeSlot_Unavailable", result.TopError.Code);
  }

  [Fact]
  public async Task Handle_WhenVehicleAlreadyScheduled_ThenReturnsFailure()
  {
    // Given
    var customer1 = CustomerFactory.CreateCustomer(vehicleLicensePlate: "MMM 147").Value;
    var vehicle1 = customer1.Vehicles.First();
    var labor1 = EmployeeFactory.CreateLabor().Value;
    var repairTask = RepairTasksFactory.CreateRepairTask().Value;

    var tasksDuration = (int)repairTask.EstimatedDuration;
    var startAt = DateTimeOffset.Parse("2027-01-01T10:00:00+00:00");
    var endAt = startAt.AddMinutes(tasksDuration);
    var workOrder = WorkOrderFactory.CreateWorkOrder(
                id:Guid.NewGuid(),
                vehicleId: vehicle1.Id,
                laborId: labor1.Id,
                repairTasks:[repairTask],
                startAt:startAt,
                endAt: endAt,
                spot: Domain.WorkOrders.Enums.Spot.A
              ).Value;
    var workOrder2 = WorkOrderFactory.CreateWorkOrder(
                id:Guid.NewGuid(),
                vehicleId: vehicle1.Id,
                laborId: labor1.Id,
                repairTasks:[repairTask],
                startAt:startAt,
                endAt: endAt,
                spot: Domain.WorkOrders.Enums.Spot.B
              ).Value;

    await _dbContext.Customers.AddAsync(customer1);
    await _dbContext.Employees.AddAsync(labor1);
    await _dbContext.RepairTasks.AddAsync(repairTask);
    await _dbContext.WorkOrders.AddAsync(workOrder);
    await _dbContext.WorkOrders.AddAsync(workOrder2);
    await _dbContext.SaveChangesAsync(default);

    // When
    var command = new RelocateWorkOrderCommand(workOrder2.Id, workOrder.StartAtUtc, workOrder2.Spot);
    var result = await _mediator.Send(command);

    // Then
    Assert.True(result.IsFailure);
    Assert.Equal(ApplicationErrors.VehicleSchedulingConflict.Code, result.TopError.Code);
  }

  [Fact]
  public async Task Handle_WhenLaborOccupied_ThenReturnsFailure()
  {
    // Given
    var customer1 = CustomerFactory.CreateCustomer(vehicleLicensePlate: "BCZ 123").Value;
    var vehicle1 = customer1.Vehicles.First();
    var customer2 = CustomerFactory.CreateCustomer(vehicleLicensePlate: "BCO 123").Value;
    var vehicle2 = customer2.Vehicles.First();
    var labor1 = EmployeeFactory.CreateLabor().Value;
    var repairTask = RepairTasksFactory.CreateRepairTask().Value;

    var tasksDuration = (int)repairTask.EstimatedDuration;
    var startAt = DateTimeOffset.Parse("2027-01-01T10:00:00+00:00");
    var endAt = startAt.AddMinutes(tasksDuration);
    var startAt2 = DateTimeOffset.Parse("2027-01-01T12:00:00+00:00");
    var endAt2 = startAt2.AddMinutes(tasksDuration);
    var workOrder = WorkOrderFactory.CreateWorkOrder(
                id:Guid.NewGuid(),
                vehicleId: vehicle1.Id,
                laborId: labor1.Id,
                repairTasks:[repairTask],
                startAt:startAt,
                endAt: endAt,
                spot: Domain.WorkOrders.Enums.Spot.A
              ).Value;
    var workOrder2 = WorkOrderFactory.CreateWorkOrder(
                id:Guid.NewGuid(),
                vehicleId: vehicle2.Id,
                laborId: labor1.Id,
                repairTasks:[repairTask],
                startAt:startAt2,
                endAt: endAt2,
                spot: Domain.WorkOrders.Enums.Spot.B
              ).Value;

    await _dbContext.Customers.AddAsync(customer1);
    await _dbContext.Customers.AddAsync(customer2);
    await _dbContext.Employees.AddAsync(labor1);
    await _dbContext.RepairTasks.AddAsync(repairTask);
    await _dbContext.WorkOrders.AddAsync(workOrder);
    await _dbContext.WorkOrders.AddAsync(workOrder2);
    await _dbContext.SaveChangesAsync(default);

    // When
    var command = new RelocateWorkOrderCommand(workOrder2.Id, workOrder.StartAtUtc, Domain.WorkOrders.Enums.Spot.B);
    var result = await _mediator.Send(command);

    // Then
    Assert.True(result.IsFailure);
    Assert.Equal(ApplicationErrors.LaborOccupied.Code, result.TopError.Code);
  }

  [Fact]
  public async Task Handle_WhenValidData_ThenReturnsSuccess()
  {
    // Given
    var customer1 = CustomerFactory.CreateCustomer(vehicleLicensePlate: "III 147").Value;
    var vehicle1 = customer1.Vehicles.First();
    var labor1 = EmployeeFactory.CreateLabor().Value;
    var repairTask = RepairTasksFactory.CreateRepairTask().Value;

    var tasksDuration = (int)repairTask.EstimatedDuration;
    var startAt = DateTimeOffset.Parse("2027-01-01T10:00:00+00:00");
    var endAt = startAt.AddMinutes(tasksDuration);
    var workOrder = WorkOrderFactory.CreateWorkOrder(
                id:Guid.NewGuid(),
                vehicleId: vehicle1.Id,
                laborId: labor1.Id,
                repairTasks:[repairTask],
                startAt:startAt,
                endAt: endAt,
                spot: Domain.WorkOrders.Enums.Spot.A
              ).Value;

    await _dbContext.Customers.AddAsync(customer1);
    await _dbContext.Employees.AddAsync(labor1);
    await _dbContext.RepairTasks.AddAsync(repairTask);
    await _dbContext.WorkOrders.AddAsync(workOrder);
    await _dbContext.SaveChangesAsync(default);

    // When
    var newStartAt = DateTimeOffset.Parse("2027-01-01T12:00:00+00:00");
    var command = new RelocateWorkOrderCommand(workOrder.Id, newStartAt, Domain.WorkOrders.Enums.Spot.C);
    var result = await _mediator.Send(command);

    // Then
    var updated = await _dbContext.WorkOrders.AsNoTracking().FirstAsync(w => w.Id == workOrder.Id);
    Assert.True(result.IsSuccess);
    Assert.Equal(Domain.WorkOrders.Enums.Spot.C, updated.Spot);
    Assert.Equal(newStartAt, updated.StartAtUtc);
  }

}