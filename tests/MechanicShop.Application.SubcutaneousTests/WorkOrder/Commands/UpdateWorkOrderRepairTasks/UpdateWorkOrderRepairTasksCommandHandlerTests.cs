using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Errors;
using MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MechanicShop.Tests.Common.RepairTasks;
using MechanicShop.Tests.Common.WorkOrders;
using MediatR;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.WorkOrder.Commands.UpdateWorkOrderRepairTasks;

[Collection(WebAppFactoryCollection.CollectionName)]
public class UpdateWorkOrderRepairTasksCommandHandlerTests(WebAppFactory factory)
{
  private readonly IMediator _mediator = factory.CreateMediator();
  private readonly IAppDbContext _dbContext = factory.CreateDbContext();

  [Fact]
  public async Task Handle_WhenWorkOrderNotFound_ThenReturnFailure()
  {
    // Given
    var fakeWorkOrderId = Guid.NewGuid();
    // When
    var command = new UpdateWorkOrderRepairTasksCommand(fakeWorkOrderId, [Guid.NewGuid()]);
    var result = await _mediator.Send(command);
    // Then
    Assert.True(result.IsFailure);
    Assert.Equal(ApplicationErrors.WorkOrderNotFound.Code, result.TopError.Code);
  }

  [Fact]
  public async Task Handle_WhenRepairTaskNotFound_ThenReturnsFailure()
  {
    // Given
    var customer = CustomerFactory.CreateCustomer(vehicleLicensePlate: "DSA 357").Value;
    var vehicle = customer.Vehicles.First();
    var labor = EmployeeFactory.CreateLabor().Value;
    var repairTask = RepairTasksFactory.CreateRepairTask().Value;
    var startAt = DateTimeOffset.Parse("2027-01-01T10:00:00+00:00");
    var tasksDuration = (int)repairTask.EstimatedDuration;
    var endAt = startAt.AddMinutes(tasksDuration);
    var workOrder = WorkOrderFactory.CreateWorkOrder(
                id:Guid.NewGuid(),
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
    var command = new UpdateWorkOrderRepairTasksCommand(workOrder.Id, [Guid.Empty]);
    var result = await _mediator.Send(command);

    // Then
    Assert.False(result.IsSuccess);
    Assert.Equal(ApplicationErrors.RepairTaskNotFound.Code, result.TopError.Code);
  }

  // [InlineData("2027-01-01T10:00:00+00:00", 30, true)] // inside hours
  // [InlineData("2027-01-04T09:00:00+00:00", 45, true)] // starts at opening
  // [InlineData("2027-01-05T17:45:00+00:00", 15, true)] // ends exactly at closing (boundary)
  [Theory]
  [InlineData("2027-01-02T08:30:00+00:00", 30, "AAA 444")] // starts before opening
  [InlineData("2027-01-03T17:15:00+00:00", 45, "AAB 444")] // ends after closing
  public async Task Handle_WhenOutsideOperatingHours_ThenReturnsFailure(DateTimeOffset startAt, int duration, string licensePlate)
  {
    // Given
    var customer = CustomerFactory.CreateCustomer(vehicleLicensePlate: licensePlate).Value;
    var vehicle = customer.Vehicles.First();
    var labor = EmployeeFactory.CreateLabor().Value;
    var durationEnum = (Domain.RepairTasks.Enums.RepairDurationInMinutes)duration;
    var newDurationEnum = Domain.RepairTasks.Enums.RepairDurationInMinutes.Min60;
    var taskId = Guid.NewGuid();
    var repairTask = RepairTasksFactory.CreateRepairTask(repairDurationInMinutes: durationEnum).Value;
    var repairTask2 = RepairTasksFactory.CreateRepairTask(id: taskId, repairDurationInMinutes: newDurationEnum).Value;
    var endAt = startAt.AddMinutes(duration);
    var workOrder = WorkOrderFactory.CreateWorkOrder(
                id:Guid.NewGuid(),
                vehicleId: vehicle.Id,
                laborId: labor.Id,
                repairTasks:[repairTask],
                startAt:startAt,
                endAt: endAt
              ).Value;

    await _dbContext.Customers.AddAsync(customer);
    await _dbContext.Employees.AddAsync(labor);
    await _dbContext.RepairTasks.AddAsync(repairTask);
    await _dbContext.RepairTasks.AddAsync(repairTask2);
    await _dbContext.WorkOrders.AddAsync(workOrder);
    await _dbContext.SaveChangesAsync(default);

    // When
    var command = new UpdateWorkOrderRepairTasksCommand(workOrder.Id, [taskId]);
    var result = await _mediator.Send(command);

    // Then
    Assert.False(result.IsSuccess);
    Assert.Equal(ApplicationErrors.WorkOrderOutsideOperatingHours(startAt, startAt.AddMinutes((int)newDurationEnum)).Code, result.TopError.Code);
  }

  [Fact]
  public async Task Handle_WhenSpotUnavailable_ThenReturnsFailure()
  {
    // Given
    var customer1 = CustomerFactory.CreateCustomer(vehicleLicensePlate: "GJR 193").Value;
    var vehicle1 = customer1.Vehicles.First();
    var labor1 = EmployeeFactory.CreateLabor().Value;
    var repairTask = RepairTasksFactory.CreateRepairTask().Value;
    var taskId = Guid.NewGuid();
    var repairTask2 = RepairTasksFactory.CreateRepairTask(id: taskId, repairDurationInMinutes: Domain.RepairTasks.Enums.RepairDurationInMinutes.Min45).Value;
    var startAt = DateTimeOffset.Parse("2027-01-02T11:00:00+00:00");
    var startAt2 = DateTimeOffset.Parse("2027-01-02T10:30:00+00:00");
    var endAt = startAt.AddMinutes((int)repairTask.EstimatedDuration);
    var endAt2 = startAt2.AddMinutes((int)repairTask.EstimatedDuration);
    var workOrder1 = WorkOrderFactory.CreateWorkOrder(
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
                startAt:startAt2,
                endAt: endAt2
              ).Value;
    await _dbContext.Customers.AddAsync(customer1);
    await _dbContext.Employees.AddAsync(labor1);
    await _dbContext.RepairTasks.AddAsync(repairTask);
    await _dbContext.RepairTasks.AddAsync(repairTask2);
    await _dbContext.WorkOrders.AddAsync(workOrder1);
    await _dbContext.WorkOrders.AddAsync(workOrder2);
    await _dbContext.SaveChangesAsync(default);

    // When
    var command = new UpdateWorkOrderRepairTasksCommand(workOrder2.Id, [taskId]);
    var result = await _mediator.Send(command);

    // Then
    Assert.True(result.IsFailure);
    Assert.Equal("WorkOrder_SpotTimeSlot_Unavailable", result.TopError.Code);
  }

  [Fact]
  public async Task Handle_WhenLaborOccupied_ThenReturnsFailure()
  {
    // Given
    var customer1 = CustomerFactory.CreateCustomer(vehicleLicensePlate: "POI 154").Value;
    var vehicle1 = customer1.Vehicles.First();
    var labor1 = EmployeeFactory.CreateLabor().Value;
    var repairTask = RepairTasksFactory.CreateRepairTask().Value;
    var taskId = Guid.NewGuid();
    var repairTask2 = RepairTasksFactory.CreateRepairTask(id: taskId, repairDurationInMinutes: Domain.RepairTasks.Enums.RepairDurationInMinutes.Min45).Value;
    var startAt = DateTimeOffset.Parse("2027-01-02T11:00:00+00:00");
    var startAt2 = DateTimeOffset.Parse("2027-01-02T10:30:00+00:00");
    var endAt = startAt.AddMinutes((int)repairTask.EstimatedDuration);
    var endAt2 = startAt2.AddMinutes((int)repairTask.EstimatedDuration);
    var workOrder1 = WorkOrderFactory.CreateWorkOrder(
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
                startAt:startAt2,
                endAt: endAt2,
                spot: Domain.WorkOrders.Enums.Spot.B
              ).Value;
    await _dbContext.Customers.AddAsync(customer1);
    await _dbContext.Employees.AddAsync(labor1);
    await _dbContext.RepairTasks.AddAsync(repairTask);
    await _dbContext.RepairTasks.AddAsync(repairTask2);
    await _dbContext.WorkOrders.AddAsync(workOrder1);
    await _dbContext.WorkOrders.AddAsync(workOrder2);
    await _dbContext.SaveChangesAsync(default);

    // When
    var command = new UpdateWorkOrderRepairTasksCommand(workOrder2.Id, [taskId]);
    var result = await _mediator.Send(command);

    // Then
    Assert.True(result.IsFailure);
    Assert.Equal(ApplicationErrors.LaborOccupied.Code, result.TopError.Code);
  }

  [Fact]
  public async Task Handle_WhenValidData_ThenReturnsSuccess()
  {
    // Given
    var customer1 = CustomerFactory.CreateCustomer(vehicleLicensePlate: "TYU 123").Value;
    var vehicle1 = customer1.Vehicles.First();
    var labor1 = EmployeeFactory.CreateLabor().Value;
    var repairTask = RepairTasksFactory.CreateRepairTask().Value;
    var taskId = Guid.NewGuid();
    var repairTask2 = RepairTasksFactory.CreateRepairTask(id: taskId, repairDurationInMinutes: Domain.RepairTasks.Enums.RepairDurationInMinutes.Min45).Value;
    var startAt = DateTimeOffset.Parse("2027-01-09T11:00:00+00:00");
    var startAt2 = DateTimeOffset.Parse("2027-01-09T10:15:00+00:00");
    var endAt = startAt.AddMinutes((int)repairTask.EstimatedDuration);
    var endAt2 = startAt2.AddMinutes((int)repairTask.EstimatedDuration);
    var workOrder1 = WorkOrderFactory.CreateWorkOrder(
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
                startAt:startAt2,
                endAt: endAt2
                ).Value;
    await _dbContext.Customers.AddAsync(customer1);
    await _dbContext.Employees.AddAsync(labor1);
    await _dbContext.RepairTasks.AddAsync(repairTask);
    await _dbContext.RepairTasks.AddAsync(repairTask2);
    await _dbContext.WorkOrders.AddAsync(workOrder1);
    await _dbContext.WorkOrders.AddAsync(workOrder2);
    await _dbContext.SaveChangesAsync(default);

    // When
    var command = new UpdateWorkOrderRepairTasksCommand(workOrder2.Id, [taskId]);
    var result = await _mediator.Send(command);

    // Then
    Assert.True(result.IsSuccess);
  }

}