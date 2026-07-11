namespace MechanicShop.Domain.Common;

public abstract class AuditableEntity : Entity
{
  protected AuditableEntity()
  {
    
  }
  protected AuditableEntity(Guid id) : base(id)
  {
    
  }

  public string? CreatedBy { get; set; }
  public string? LastModifiedBy { get; set; }
  public DateTimeOffset LastModifiedUtc { get; set; }
  public DateTimeOffset CreatedAtUtc { get; set; }
}