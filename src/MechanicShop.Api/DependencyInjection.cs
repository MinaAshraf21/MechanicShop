using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Asp.Versioning;
using MechanicShop.Api.Infrastructure;
using MechanicShop.Infrastructure.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

namespace MechanicShop.Api;

public static class DependencyInjection
{
  public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddExceptionHandler<GlobalExceptionHandler>();
    services.AddHttpContextAccessor();
    services.AddAppOutputCache();
    services.AddCustomApiVersioning();
    services.AddCustomProblemDetails();
    services.AddAppOpenTelemetry();
    services.AddControllerWithJsonConfiguration();
    services.AddAppRateLimiting();
    services.AddConfiguredCors(configuration);
    services.AddSignalR();
    return services;
  }

  public static IServiceCollection AddAppOutputCache(this IServiceCollection services)
  {
    services.AddOutputCache(options =>
    {
      options.SizeLimit = 100 * 1024 * 1024;
      options.AddBasePolicy(p => p.Expire(TimeSpan.FromSeconds(60)));
    });
    return services;
  }

  public static IServiceCollection AddAppOpenTelemetry(this IServiceCollection services)
  {
        services.AddOpenTelemetry()
        .ConfigureResource(res => res.AddService("orderservice"))
        .WithTracing(tracing =>
        {
            tracing.AddAspNetCoreInstrumentation().
            AddHttpClientInstrumentation();

            tracing.AddOtlpExporter();
        }).
        WithMetrics(metrics =>
        {
            metrics.AddAspNetCoreInstrumentation().
            AddHttpClientInstrumentation();

            metrics.AddOtlpExporter().
            AddPrometheusExporter(); // /metrics
        });

        return services;
  }

  public static IServiceCollection AddAppRateLimiting(this IServiceCollection services)
  {
    services.AddRateLimiter(options =>
    {
      options.AddSlidingWindowLimiter("SlidingWindowLimiter", options =>
      {
        options.PermitLimit = 100;
        options.QueueLimit = 10;
        options.SegmentsPerWindow = 6;
        options.AutoReplenishment = true;
        options.Window = TimeSpan.FromMinutes(1);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
      }).RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    });
    return services;
  }

  public static IServiceCollection AddCustomProblemDetails(this IServiceCollection services)
  {
    services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
    {
      context.ProblemDetails.Extensions.Add("requestId", context.HttpContext.TraceIdentifier);
      context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
    });
    return services;
  }

  public static IServiceCollection AddCustomApiVersioning(this IServiceCollection services)
  {
    services.AddApiVersioning(options =>
    {
      options.DefaultApiVersion = new ApiVersion(1,0);
      options.AssumeDefaultVersionWhenUnspecified = true;
      options.ReportApiVersions = true;
      options.ApiVersionReader = new UrlSegmentApiVersionReader();
    }).AddMvc()
      .AddApiExplorer(options =>
      {
          options.GroupNameFormat = "'v'VVV";
          options.SubstituteApiVersionInUrl = true;
      });
    return services;
  }

  public static IServiceCollection AddControllerWithJsonConfiguration(this IServiceCollection services)
  {
    services.AddControllers().AddJsonOptions(options => options
        .JsonSerializerOptions
        .DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);
    return services;
  }

  public static IServiceCollection AddConfiguredCors(this IServiceCollection services, IConfiguration configuration)
  {
    var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>();
    services.AddCors(options =>
    {
      options.AddPolicy(appSettings!.CorsPolicyName, policy =>
      {
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();
        policy.AllowCredentials();
        policy.WithOrigins(appSettings.AllowedOrigins);
      });
    });
    return services;
  }

  public static IApplicationBuilder UseCoreMiddlewares(this IApplicationBuilder app, IConfiguration configuration)
  {
      // 1. Exception handling should be FIRST to catch all errors
      app.UseExceptionHandler();

      // 2. Status code pages for handling HTTP status codes
      app.UseStatusCodePages();

      // 3. HTTPS redirection (before any other middleware that might generate URLs)
      app.UseHttpsRedirection();

      // 4. Serilog request logging (early to log all requests)
      app.UseSerilogRequestLogging();

      // 5. CORS (before authentication/authorization)
      app.UseCors(configuration["AppSettings:CorsPolicyName"]!);

      // 6. Rate limiting (before authentication to protect auth endpoints)
      app.UseRateLimiter();

      // 7. Authentication (must come before authorization)
      app.UseAuthentication();

      // 8. Authorization (must come after authentication)
      app.UseAuthorization();

      // 9. Output caching (after auth to cache based on user context)
      app.UseOutputCache();

      return app;
  }
}