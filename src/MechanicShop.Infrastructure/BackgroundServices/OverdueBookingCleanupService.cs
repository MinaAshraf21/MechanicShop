namespace MechanicShop.Infrastructure.BackgroundServices;

using System.Threading;
using System.Threading.Tasks;
using MechanicShop.Application.Abstractions;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public sealed class OverdueBookingCleanupService(
  IServiceScopeFactory serviceScopeFactory,
  ILogger<OverdueBookingCleanupService> logger,
  TimeProvider timeProvider,
  IOptions<AppSettings> options
) : BackgroundService
{
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    using var timer = new PeriodicTimer(TimeSpan.FromMinutes(options.Value.OverdueBookingCleanupFrequencyMinutes));
    while (await timer.WaitForNextTickAsync(stoppingToken))
    {
      logger.LogInformation("Checking overdue work orders at {Now}", timeProvider.GetUtcNow());
      try
      {
        var scope = serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

        var cutoff = timeProvider.GetUtcNow().AddMinutes(options.Value.BookingCancellationThresholdMinutes);
        var overdue = context.WorkOrders
                                .Where(w => w.StartAtUtc <= cutoff && w.State == State.Scheduled)
                                .ToList();

        if(overdue.Count != 0)
        {
          foreach (var order in overdue)
          {
            var cancelResult = order.Cancel();
            if(cancelResult.IsFailure)
              logger.LogWarning("Failed to cancel order {id} : {error}", order.Id, cancelResult.Errors);
          }
          await context.SaveChangesAsync(stoppingToken);
          logger.LogInformation("Cancelled {Count} overdue work orders: {Ids}", overdue.Count, overdue.Select(w => w.Id));
        }
        else
        {
          logger.LogInformation("No overdue work orders found.");
        }

      }
      catch (Exception ex)
      {
        logger.LogError(ex, "An error occurred while cleaning up overdue work orders.");
      }
    }
  }
}