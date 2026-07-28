using MechanicShop.Application.Abstractions;
using MechanicShop.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace MechanicShop.Infrastructure.Data.Interceptors;

public sealed class AuditableEntityInterceptor(IUser user, TimeProvider timeProvider) : SaveChangesInterceptor
{
  public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
  {
    UpdateEntities(eventData.Context);
    return base.SavingChanges(eventData, result);
  }
  private void UpdateEntities(DbContext? context)
  {
    if(context is null)
      return;

    foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
    {
      if(entry.State is EntityState.Modified or EntityState.Added || entry.HasChangedOwnedEntities())
      {
        if(entry.State == EntityState.Added)
        {
          entry.Entity.CreatedBy = user.Id;
          entry.Entity.CreatedAtUtc = timeProvider.GetUtcNow();
        }
        entry.Entity.LastModifiedBy = user.Id;
        entry.Entity.LastModifiedUtc = timeProvider.GetUtcNow();

        if(entry.HasChangedOwnedEntities())
        {
          foreach (var ownedEntry in entry.References)
          {
            if(ownedEntry.TargetEntry is { Entity: AuditableEntity ownedEntity } && ownedEntry.TargetEntry.State is EntityState.Modified or EntityState.Added )
            {
              if(ownedEntry.TargetEntry.State == EntityState.Added)
              {
                ownedEntity.CreatedBy = user.Id;
                ownedEntity.CreatedAtUtc = timeProvider.GetUtcNow();
              }
              ownedEntity.LastModifiedBy = user.Id;
              ownedEntity.LastModifiedUtc = timeProvider.GetUtcNow();
            }
          }
        }
      }
    }
  }
}

public static class Extensions
{
    public static bool HasChangedOwnedEntities(this EntityEntry entry) =>
        entry.References.Any(r =>
            r.TargetEntry?.Metadata.IsOwned() == true &&
            (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified));
}