using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Domain.Employees;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Parts;
using MechanicShop.Domain.WorkOrders;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Application.Abstractions;

public interface IDbContext
{
  DbSet<Customer> Customers { get; }
  DbSet<Vehicle> Vehicles { get; }
  DbSet<WorkOrder> WorkOrders { get; }
  DbSet<RepairTask> RepairTasks { get; }
  DbSet<Part> Parts { get; }
  DbSet<Employee> Employees { get; }

  Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}