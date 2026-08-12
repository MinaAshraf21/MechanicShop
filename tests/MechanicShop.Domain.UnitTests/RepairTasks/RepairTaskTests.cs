using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Enums;
using MechanicShop.Domain.RepairTasks.Parts;
using MechanicShop.Tests.Common.RepaireTasks;
using MechanicShop.Tests.Common.RepairTasks;
using Xunit;

namespace MechanicShop.Domain.UnitTests.RepairTasks;

public class RepairTaskTests
{
[Fact]
public void Create_ShouldSucceed_WithValidData()
{
  Guid id = Guid.NewGuid();
  const string name = "Task Name";
  const decimal partCost = 20.0m;
  const decimal laborCost = 40.0m;
  const int partQuantity = 1;
  const RepairDurationInMinutes durationInMinutes = RepairDurationInMinutes.Min30;
  List<Part> parts = [PartFactory.CreatePart(cost: partCost, quantity: partQuantity).Value];

  decimal totalCost = (partQuantity * partCost) + laborCost;

  var result = RepairTask.Create(id, name, laborCost, durationInMinutes, parts);
  var task = result.Value;

  Assert.True(result.IsSuccess);
  Assert.Equal(id, task.Id);
  Assert.Equal(name, task.Name);
  Assert.Equal(laborCost, task.LaborCost);
  Assert.Equal(durationInMinutes, task.EstimatedDuration);
  Assert.Single(task.Parts);
  Assert.Equal(totalCost, task.TotalCost);
}

  [Theory]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("  ")]
  public void Create_WithEmptyName_ShouldFail(string? name)
  {

    var result = RepairTask.Create(
                id: Guid.NewGuid(),
                name: name,
                laborCost: 100,
                estimatedDuration: RepairDurationInMinutes.Min30,
                parts: [PartFactory.CreatePart().Value]);

    Assert.True(result.IsFailure);

    Assert.Equal(RepairTaskErrors.NameRequired.Code, result.TopError.Code);
  }

  [Theory]
  [InlineData(0)]
  [InlineData(10001)]
  public void Create_WithInvalidLaborCost_ShouldFail(decimal laborCost)
  {
    var result = RepairTask.Create(
                id: Guid.NewGuid(),
                name: "Brake Inspection",
                laborCost: laborCost,
                estimatedDuration: RepairDurationInMinutes.Min30,
                parts: [PartFactory.CreatePart().Value]);

    Assert.True(result.IsFailure);

    Assert.Equal(RepairTaskErrors.InvalidLaborCost.Code, result.TopError.Code);
  }

  [Fact]
  public void Create_WithInvalidDuration_ShouldFail()
  {
    const RepairDurationInMinutes invalidDurationValue = (RepairDurationInMinutes)999;

    var result = RepairTask.Create(
        id: Guid.NewGuid(),
        name: "Brake Inspection",
        laborCost: 100,
        estimatedDuration: invalidDurationValue,
        parts: [PartFactory.CreatePart().Value]);

    Assert.True(result.IsFailure);

    Assert.Equal(RepairTaskErrors.InvalidEstimatedDuration.Code, result.TopError.Code);
  }

  [Fact]
  public void UpsertParts_AddsNewPart_WhenNotExisting()
  {
    var task = RepairTasksFactory.CreateRepairTask().Value;
    var incoming = PartFactory.CreatePart().Value;

    var result = task.UpdateParts([incoming]);

    Assert.True(result.IsSuccess);
    Assert.Single(task.Parts);
    Assert.Contains(incoming, task.Parts);
  }

  [Fact]
  public void UpsertParts_UpdatesExistingPart_WhenExisting()
  {
    var id = Guid.NewGuid();
    var original = PartFactory.CreatePart(id: id, name: "Old", cost: 10, quantity: 2).Value;
    var task = RepairTasksFactory.CreateRepairTask(parts: [original]).Value;
    var incoming = PartFactory.CreatePart(id: id, name: "New", cost: 20, quantity: 5).Value;

    var result = task.UpdateParts([incoming]);

    Assert.True(result.IsSuccess);
    var updated = task.Parts.First(p => p.Id == id);
    Assert.Equal("New", updated.Name);
    Assert.Equal(20m, updated.Cost);
    Assert.Equal(5, updated.Quantity);
  }

  [Fact]
  public void UpsertParts_RemovesMissingParts()
  {
    var keep = PartFactory.CreatePart().Value;
    var remove = PartFactory.CreatePart().Value;
    var task = RepairTasksFactory.CreateRepairTask(parts: [keep, remove]).Value;

    var result = task.UpdateParts([keep]);

    Assert.True(result.IsSuccess);
    Assert.Single(task.Parts, keep);
  }

  [Fact]
  public void Update_ShouldReturnSuccess_WithValidValues()
  {
    var task = RepairTasksFactory.CreateRepairTask().Value;

    var result = task.Update("Valid", 123m, RepairDurationInMinutes.Min30);

    Assert.True(result.IsSuccess);
    Assert.Equal("Valid", task.Name);
    Assert.Equal(123m, task.LaborCost);
    Assert.Equal(RepairDurationInMinutes.Min30, task.EstimatedDuration);
  }

  [Theory]
  [InlineData("", 1)]
  [InlineData(null, 1)]
  [InlineData("  ", 1)]
  [InlineData("Name", 0)]
  [InlineData("Name", 10001)]
  public void Update_ShouldReturnError_ForInvalidNameOrCost(string? name, decimal cost)
  {
    var task = RepairTasksFactory.CreateRepairTask().Value;

    var result = task.Update(name, cost, RepairDurationInMinutes.Min30);

    Assert.True(result.IsFailure);
  }

  [Fact]
  public void Update_ShouldReturnError_ForInvalidDuration()
  {
    var task = RepairTasksFactory.CreateRepairTask().Value;
    const RepairDurationInMinutes invalid = (RepairDurationInMinutes)999;

    var result = task.Update("Name", 1m, invalid);

    Assert.False(result.IsSuccess);
    Assert.Equal(RepairTaskErrors.InvalidEstimatedDuration.Code, result.TopError.Code);
  }

}
