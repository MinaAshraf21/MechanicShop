using MechanicShop.Application.Abstractions;
using MechanicShop.Domain.Common;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Domain.Employees;
using MechanicShop.Domain.Identity;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Parts;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Billing;
using MediatR;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options, IMediator mediator) : IdentityDbContext(options), IAppDbContext
{
  public DbSet<Customer> Customers => Set<Customer>();

  public DbSet<Vehicle> Vehicles => Set<Vehicle>();

  public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();

  public DbSet<RepairTask> RepairTasks => Set<RepairTask>();

  public DbSet<Part> Parts => Set<Part>();

  public DbSet<Employee> Employees => Set<Employee>();

  public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

  public DbSet<Invoice> Invoices => Set<Invoice>();

  protected override void OnModelCreating(ModelBuilder builder)
  {
    builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    base.OnModelCreating(builder);
  }

  public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    await DispatchDomainEventsAsync(cancellationToken);
    return await base.SaveChangesAsync(cancellationToken);
  }

  private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
  {
    var domainEntities = ChangeTracker.Entries()
                        .Where(e => e.Entity is Entity entity && entity.DomainEvents.Count != 0)
                        .Select(e => (Entity)e.Entity)
                        .ToList();

    var domainEvents = domainEntities.SelectMany(e => e.DomainEvents).ToList();

    foreach (var e in domainEvents)
    {
      await mediator.Publish(e, cancellationToken);
    }
    foreach (var entity in domainEntities)
    {
      entity.ClearDomainEvents();
    }

  }
}