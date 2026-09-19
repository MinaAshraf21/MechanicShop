using System.Threading.Tasks;
using MechanicShop.Application.Abstractions;
using MechanicShop.Application.Errors;
using MechanicShop.Application.Features.WorkOrders.Commands.AssignLabor;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MechanicShop.Tests.Common.RepairTasks;
using MechanicShop.Tests.Common.WorkOrders;
using MediatR;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.WorkOrder.Commands.AssignLabor;

[Collection(WebAppFactoryCollection.CollectionName)]
public class AssignLaborCommandHandlerTests(WebAppFactory webAppFactory)
{
  private readonly IMediator _mediator = webAppFactory.CreateMediator();
  private readonly IAppDbContext _dbContext = webAppFactory.CreateDbContext();

  [Fact]
  public async Task Handle_WhenValidData_ThenReturnsSuccess()
  {
    // Given
    var customer = CustomerFactory.CreateCustomer(vehicleLicensePlate: "ASD 125").Value;
    var vehicle = customer.Vehicles.First();
    var labor1 = EmployeeFactory.CreateLabor().Value;
    var labor2 = EmployeeFactory.CreateLabor().Value;
    var repairTask = RepairTasksFactory.CreateRepairTask().Value;
  
    var fakeWorkOrderId = Guid.NewGuid();
    var startAt = DateTimeOffset.Parse("2027-01-01T10:00:00+00:00");
    var tasksDuration = (int)repairTask.EstimatedDuration;
    var endAt = startAt.AddMinutes(tasksDuration);
    var workOrder = WorkOrderFactory.CreateWorkOrder(
                id:fakeWorkOrderId,
                vehicleId: vehicle.Id,
                laborId: labor1.Id,
                repairTasks:[repairTask],
                startAt:startAt,
                endAt: endAt
              ).Value;

    await _dbContext.Customers.AddAsync(customer);
    await _dbContext.Employees.AddAsync(labor1);
    await _dbContext.Employees.AddAsync(labor2);
    await _dbContext.RepairTasks.AddAsync(repairTask);
    await _dbContext.WorkOrders.AddAsync(workOrder);
    await _dbContext.SaveChangesAsync(default);
    // When
    var command = new AssignLaborCommand(labor2.Id, workOrder.Id);
    var result = await _mediator.Send(command);
    // Then
    Assert.True(result.IsSuccess);
  }

  [Fact]
  public async Task Handle_WhenLaborNotFound_ThenReturnsFailure()
  {
    // Given
    // No seeding needed — handler checks labor existence before work order,
    // so a nonexistent labor short-circuits regardless of work order validity.
    var laborFakeId = Guid.NewGuid();
    var fakeWorkOrderId = Guid.NewGuid();
    // When
    var command = new AssignLaborCommand(laborFakeId, fakeWorkOrderId);
    var result = await _mediator.Send(command);
    // Then
    Assert.True(result.IsFailure);
    Assert.Equal(ApplicationErrors.LaborNotFound.Code, result.TopError.Code);
  }

  [Fact]
  public async Task Handle_WhenWorkOrderNotFound_ThenReturnsFailure()
  {
    // Given
    var labor1 = EmployeeFactory.CreateLabor().Value;
    var fakeWorkOrderId = Guid.NewGuid();
    await _dbContext.Employees.AddAsync(labor1);
    await _dbContext.SaveChangesAsync(default);
    // When
    var command = new AssignLaborCommand(labor1.Id, fakeWorkOrderId);
    var result = await _mediator.Send(command);
    // Then
    Assert.True(result.IsFailure);
    Assert.Equal(ApplicationErrors.WorkOrderNotFound.Code, result.TopError.Code);
  }

  [Fact]
  public async Task Handle_WhenLaborOccupied_ThenReturnsFailure()
  {
    // Given
    var customer1 = CustomerFactory.CreateCustomer(vehicleLicensePlate: "BCA 852").Value;
    var vehicle1 = customer1.Vehicles.First();
    var labor1 = EmployeeFactory.CreateLabor().Value;
    var labor2 = EmployeeFactory.CreateLabor().Value;
    var repairTask = RepairTasksFactory.CreateRepairTask().Value;
    var fakeWorkOrderId1 = Guid.NewGuid();
    var startAt = DateTimeOffset.Parse("2027-01-01T10:00:00+00:00");
    var tasksDuration = (int)repairTask.EstimatedDuration;
    var endAt = startAt.AddMinutes(tasksDuration);
    var workOrder1 = WorkOrderFactory.CreateWorkOrder(
                id:fakeWorkOrderId1,
                vehicleId: vehicle1.Id,
                laborId: labor1.Id,
                repairTasks:[repairTask],
                startAt:startAt,
                endAt: endAt
              ).Value;
    var fakeWorkOrderId2 = Guid.NewGuid();
    var workOrder2 = WorkOrderFactory.CreateWorkOrder(
                id:fakeWorkOrderId2,
                vehicleId: vehicle1.Id,
                laborId: labor2.Id,
                startAt:startAt,
                endAt:endAt,
                repairTasks:[repairTask]
              ).Value;
    await _dbContext.Customers.AddAsync(customer1);
    await _dbContext.Employees.AddAsync(labor1);
    await _dbContext.Employees.AddAsync(labor2);
    await _dbContext.RepairTasks.AddAsync(repairTask);
    await _dbContext.WorkOrders.AddAsync(workOrder1);
    await _dbContext.WorkOrders.AddAsync(workOrder2);
    await _dbContext.SaveChangesAsync(default);

    // When
    var command = new AssignLaborCommand(labor1.Id, workOrder2.Id);
    var result = await _mediator.Send(command);
    // Then
    Assert.True(result.IsFailure);
    Assert.Equal(ApplicationErrors.LaborOccupied.Code, result.TopError.Code);
  }

  [Fact]
  public async Task Handle_WhenWorkOrderStateNotScheduled_ThenReturnsFailure()
  {
    // Given
    var customer1 = CustomerFactory.CreateCustomer(vehicleLicensePlate: "BCQ 123").Value;
    var vehicle1 = customer1.Vehicles.First();
    var labor1 = EmployeeFactory.CreateLabor().Value;
    var labor2 = EmployeeFactory.CreateLabor().Value;
    var repairTask = RepairTasksFactory.CreateRepairTask().Value;

    var fakeWorkOrderId1 = Guid.NewGuid();
    var startAt = DateTimeOffset.Parse("2027-01-01T10:00:00+00:00");
    var tasksDuration = (int)repairTask.EstimatedDuration;
    var endAt = startAt.AddMinutes(tasksDuration);
    var workOrder1 = WorkOrderFactory.CreateWorkOrder(
                id:fakeWorkOrderId1,
                vehicleId: vehicle1.Id,
                laborId: labor1.Id,
                repairTasks:[repairTask],
                startAt:startAt,
                endAt: endAt
              ).Value;
    workOrder1.UpdateState(Domain.WorkOrders.Enums.State.InProgress);
    await _dbContext.Customers.AddAsync(customer1);
    await _dbContext.Employees.AddAsync(labor1);
    await _dbContext.Employees.AddAsync(labor2);
    await _dbContext.RepairTasks.AddAsync(repairTask);
    await _dbContext.WorkOrders.AddAsync(workOrder1);
    await _dbContext.SaveChangesAsync(default);

    // When
    var command = new AssignLaborCommand(labor2.Id, workOrder1.Id);
    var result = await _mediator.Send(command);
    // Then
    Assert.True(result.IsFailure);
    Assert.Equal(WorkOrderErrors.Readonly.Code, result.TopError.Code);
  }

}