using MechanicShop.Domain.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks.Enums;
using MechanicShop.Domain.RepairTasks.Parts;

namespace MechanicShop.Domain.RepairTasks;

public sealed class RepairTask : AuditableEntity
{
  public string Name { get; private set; }
  public decimal LaborCost { get; private set; }
  public RepairDurationInMinutes EstimatedDuration { get; private set; }
  private readonly List<Part> _parts = new();
  public IEnumerable<Part> Parts => _parts.AsReadOnly();
  public decimal TotalCost => _parts.Sum(p => p.Cost * p.Quantity) + LaborCost;

  private RepairTask()
  {
    
  }
  private RepairTask(Guid id, string name, decimal laborCost, RepairDurationInMinutes estimatedDuration, List<Part> parts) : base(id)
  {
    Name = name;
    LaborCost = laborCost;
    EstimatedDuration = estimatedDuration;
    _parts = parts;
  }

  public static Result<RepairTask> Create(Guid id, string name, decimal laborCost, RepairDurationInMinutes estimatedDuration, List<Part> parts)
  {
    if (string.IsNullOrWhiteSpace(name))
    {
      return RepairTaskErrors.NameRequired;
    }
    if (laborCost <= 0 || laborCost > 10000)
    {
      return RepairTaskErrors.InvalidLaborCost;
    }
    if (!Enum.IsDefined(estimatedDuration))
    {
      return RepairTaskErrors.InvalidEstimatedDuration;
    }
    return new RepairTask(id, name, laborCost, estimatedDuration, parts);
  }
  public Result<Updated> Update(string name, decimal laborCost, RepairDurationInMinutes estimatedDurationInMins)
  {
    if (string.IsNullOrWhiteSpace(name))
    {
        return RepairTaskErrors.NameRequired;
    }

    if (laborCost <= 0 || laborCost > 10000)
    {
        return RepairTaskErrors.InvalidLaborCost;
    }

    if (!Enum.IsDefined(estimatedDurationInMins))
    {
        return RepairTaskErrors.InvalidEstimatedDuration;
    }

    Name = name.Trim();
    LaborCost = laborCost;
    EstimatedDuration = estimatedDurationInMins;

    return Result.Updated;
  }

  public Result<Updated> UpdateParts(List<Part> incomingParts)
  {
    _parts.RemoveAll(p => !incomingParts.Any(np => np.Id == p.Id));

    foreach (var p in incomingParts)
    {
      var part = _parts.FirstOrDefault(x => x.Id == p.Id);
      if(part is null)
        _parts.Add(p);
      else
      {
        var updatePartResult = part.Update(p.Cost, p.Name!, p.Quantity);
        if(updatePartResult.IsFailure)
          return updatePartResult.Errors!;
      }
    }
    return Result.Updated;
  }

}