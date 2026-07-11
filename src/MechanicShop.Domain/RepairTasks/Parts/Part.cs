using MechanicShop.Domain.Common;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.RepairTasks.Parts;

public sealed class Part : AuditableEntity
{
  public decimal Cost { get; private set; }
  public string? Name { get; private set; }
  public int Quantity { get; private set; }

  private Part()
  {
    
  }
  private Part(Guid id, decimal cost, string name, int quantity) : base(id)
  {
    Cost = cost;
    Name = name;
    Quantity = quantity;
  }

  public static Result<Part> Create(Guid id, decimal cost, string name, int quantity)
  {
    if(string.IsNullOrWhiteSpace(name))
    {
      return PartErrors.NameRequired;
    }
    if(cost <= 0 || cost > 10000)
    {
      return PartErrors.InvalidCost;
    }
    if(quantity <= 0 || quantity > 10)
    {
      return PartErrors.InvalidQuantity;
    }
    return new Part(Guid.NewGuid(), cost, name, quantity);
  }

  public Result<Updated> Update(decimal cost, string name, int quantity)
  {
    if(string.IsNullOrWhiteSpace(name))
    {
      return PartErrors.NameRequired;
    }
    if(cost <= 0 || cost > 10000)
    {
      return PartErrors.InvalidCost;
    }
    if(quantity <= 0 || quantity > 10)
    {
      return PartErrors.InvalidQuantity;
    }

    Cost = cost;
    Name = name.Trim();
    Quantity = quantity;

    return Result.Updated;
  }

}