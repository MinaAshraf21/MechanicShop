using MechanicShop.Application.Abstractions;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Domain.Employees;
using MechanicShop.Domain.Identity;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Parts;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Billing;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext(options), IAppDbContext
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
}