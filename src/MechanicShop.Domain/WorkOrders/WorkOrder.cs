using MechanicShop.Domain.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Domain.Employees;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Domain.WorkOrders;

public sealed class WorkOrder : AuditableEntity
{
  public bool IsEditable => State == State.Scheduled ? true : false;
  public decimal Tax { get; private set; }
  public decimal Discount { get; private set; }
  public DateTimeOffset StartAtUtc { get; private set; }
  public DateTimeOffset EndAtUtc { get; private set; }
  public State State { get; private set; }
  public Spot Spot { get; private set; }
  public Guid LaborId { get; private set; }
  public Guid VehicleId { get; }
  public Guid InvoiceId { get; }
  public Employee? Labor { get; set; }
  public Invoice? Invoice { get; set; }
  public Vehicle? Vehicle { get; set; }
  private readonly List<RepairTask> _tasks = new();
  public IEnumerable<RepairTask> Tasks => _tasks.AsReadOnly();

  public decimal? TotalLaborCost => _tasks.Sum(t => t.LaborCost);
  public decimal? TotalPartsCost => _tasks.SelectMany(t => t.Parts).Sum(p => p.Cost * p.Quantity);
  public decimal TotalCost => (TotalLaborCost ?? 0) + (TotalPartsCost ?? 0) ;

  private WorkOrder(){ }
  private WorkOrder(Guid id, Guid vehicleId, DateTimeOffset startAt, DateTimeOffset endAt, Guid laborId, Spot spot, State state, List<RepairTask> repairTasks)
    : base(id)
{
    VehicleId = vehicleId;
    StartAtUtc = startAt;
    EndAtUtc = endAt;
    LaborId = laborId;
    Spot = spot;
    State = state;
    _tasks = repairTasks;
}

  public static Result<WorkOrder> Create(Guid id, Guid vehicleId, DateTimeOffset startAt, DateTimeOffset endAt, Guid laborId, Spot spot, List<RepairTask> repairTasks)
  {
    if(id == Guid.Empty)
    {
      return WorkOrderErrors.WorkOrderIdRequired;
    }
    if(vehicleId == Guid.Empty)
    {
      return WorkOrderErrors.VehicleIdRequired;
    }
    if(laborId == Guid.Empty)
    {
      return WorkOrderErrors.LaborIdRequired;
    }
    if(repairTasks == null || repairTasks.Count == 0)
    {
      return WorkOrderErrors.RepairTasksRequired;
    }
    if(endAt <= startAt)
    {
      return WorkOrderErrors.InvalidTiming;
    }
    if (!Enum.IsDefined(spot))
    {
      return WorkOrderErrors.SpotInvalid;
    }
    return new WorkOrder(id, vehicleId, startAt, endAt, laborId, spot, State.Scheduled, repairTasks);
  }

  public Result<Updated> AddRepairTask(RepairTask repairTask)
  {
    if (!IsEditable)
    {
      return WorkOrderErrors.Readonly;
    }
    if(_tasks.Any(t => t.Id == repairTask.Id))
    {
      return WorkOrderErrors.RepairTaskAlreadyAdded;
    }
    _tasks.Add(repairTask);
    return Result.Updated;
  }

  public Result<Updated> UpdateTiming(DateTimeOffset startAt, DateTimeOffset endAt)
  {
    if (!IsEditable)
    {
      return WorkOrderErrors.TimingReadonly(Id.ToString(), State);
    }
    if(endAt <= startAt)
    {
      return WorkOrderErrors.InvalidTiming;
    }
    StartAtUtc = startAt;
    EndAtUtc = endAt;
    return Result.Updated;
  }

  public Result<Updated> UpdateLabor(Guid laborId)
  {
    if (!IsEditable)
    {
      return WorkOrderErrors.Readonly;
    }
    if(laborId == Guid.Empty)
    {
      return WorkOrderErrors.LaborIdEmpty(Id.ToString());
    }
    LaborId = laborId;
    return Result.Updated;
  }

  public Result<Updated> UpdateState(State state)
  {
    if (!IsEditable)
    {
      return WorkOrderErrors.Readonly;
    }
    if(!CanTransitionTo(state))
    {
      return WorkOrderErrors.InvalidStateTransition(this.State, state);
    }
    State = state;
    return Result.Updated;
  }
  public bool CanTransitionTo(State newStatus)
  {
    return (this.State, newStatus) switch
    {
        (State.Scheduled, State.InProgress) => true,
        (State.InProgress, State.Completed) => true,
        (_, State.Cancelled) when State != State.Completed => true,
        _ => false
    };
  }

  public Result<Updated> UpdateSpot(Spot spot)
  {
    if (!IsEditable)
    {
      return WorkOrderErrors.Readonly;
    }
    if (Enum.IsDefined(spot))
    {
      return WorkOrderErrors.SpotInvalid; 
    }
    Spot = spot;
    return Result.Updated;
  }

  public Result<Updated> ClearRepairTasks()
  {
    if (!IsEditable)
    {
        return WorkOrderErrors.Readonly;
    }

    _tasks.Clear();

    return Result.Updated;
  }

}