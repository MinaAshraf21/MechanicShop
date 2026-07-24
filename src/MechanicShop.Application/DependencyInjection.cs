using System.Reflection;
using FluentValidation;
using MechanicShop.Application.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace MechanicShop.Application;

public static class DependencyInjection
{
  public static IServiceCollection AddApplicationServices(this IServiceCollection services)
  {
    services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

    services.AddMediatR(cfg =>
    {
      cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
      cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
      cfg.AddOpenBehavior(typeof(CachingBehavior<,>));
      cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));
      cfg.AddOpenBehavior(typeof(UnhandledExceptionBehavior<,>));
    });
    
    return services;
  }
}