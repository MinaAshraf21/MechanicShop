using MechanicShop.Api;
using MechanicShop.Application.Abstractions;
using MechanicShop.Infrastructure.BackgroundServices;
using MechanicShop.Infrastructure.Data;
using MechanicShop.Infrastructure.Settings;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Testcontainers.MsSql;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Common;

public class WebAppFactory : WebApplicationFactory<IAssemblyMarker>, IAsyncLifetime
{
  private readonly MsSqlContainer _dbContainer = new MsSqlBuilder().Build();

  public IMediator CreateMediator()
  {
    var scope = Services.CreateScope();
    return scope.ServiceProvider.GetRequiredService<IMediator>();
  }
  public IAppDbContext CreateDbContext()
  {
    var scope = Services.CreateScope();
    return scope.ServiceProvider.GetRequiredService<IAppDbContext>();
  }
public async Task InitializeAsync()
{
    await _dbContainer.StartAsync();

    using var scope = Services.CreateScope();
    var initializer = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitializer>();

    await initializer.InitializeAsync(); // runs migrations

    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.WorkOrders.RemoveRange(context.WorkOrders);
    await context.SaveChangesAsync();
}

  public new Task DisposeAsync() => _dbContainer.StopAsync();

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder.ConfigureTestServices(services =>
    {
      services.RemoveAll<IHostedService>();
      services.RemoveAll<OverdueBookingCleanupService>();
      services.RemoveAll<DbContextOptions<AppDbContext>>();
      services.AddDbContext<AppDbContext>((sp,op) =>
      {
        op.AddInterceptors(sp.GetRequiredService<ISaveChangesInterceptor>());
        op.UseSqlServer(_dbContainer.GetConnectionString());
      });
      services.RemoveAll<AppSettings>();
      //explicit override after configure
      services.PostConfigure<AppSettings>(options =>
      {
        options.OpeningTime = new TimeOnly(9,0);
        options.ClosingTime = new TimeOnly(18,0);
      });
    });
  }
}