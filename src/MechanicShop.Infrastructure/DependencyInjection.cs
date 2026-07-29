using MechanicShop.Infrastructure.Data;
using MechanicShop.Infrastructure.Data.Interceptors;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MechanicShop.Application.Abstractions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MechanicShop.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Hybrid;
using MechanicShop.Infrastructure.Services;
using MechanicShop.Infrastructure.RealTime;
using MechanicShop.Infrastructure.BackgroundServices;

namespace MechanicShop.Infrastructure;

public static class DependencyInjection
{
  public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddSingleton(TimeProvider.System);

    var constr = configuration.GetConnectionString("DefaultConnection");
    ArgumentNullException.ThrowIfNull(constr);

    services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();

    services.AddDbContext<AppDbContext>((sp,op) =>
    {
      op.AddInterceptors(sp.GetRequiredService<ISaveChangesInterceptor>());
      op.UseSqlServer(constr);
    });

    services.AddScoped<IAppDbContext, AppDbContext>();
    services.AddScoped<ApplicationDbContextInitializer>();

    services.AddAuthentication(options =>
    {
      options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
      options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(options =>{
      var jwtSettings = configuration.GetSection("JwtSettings");

      var tokenValidationParameters = new TokenValidationParameters
      {
        ValidateAudience = true,
        ValidateIssuer = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero,
        ValidAudience = jwtSettings["Audience"],
        ValidIssuer = jwtSettings["Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]!))
      };
    });

    services.AddAuthorizationBuilder()
                      .AddPolicy("ManagerOnly", policy => policy.RequireRole("Manager"));

    services.AddIdentityCore<AppUser>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequiredUniqueChars = 1;
            options.SignIn.RequireConfirmedAccount = false;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<AppDbContext>();

    services.AddHybridCache(options => {
      options.DefaultEntryOptions = new HybridCacheEntryOptions
      {
        LocalCacheExpiration = TimeSpan.FromSeconds(30), //L1
        Expiration = TimeSpan.FromMinutes(10) //L2, L3
      };
    });

    services.AddTransient<IIdentityService, IdentityService>();
    services.AddScoped<IWorkOrderPolicy, WorkOrderPolicy>();
    services.AddScoped<IWorkOrderNotifier, SignalRWorkOrderNotifier>();
    services.AddScoped<IInvoicePdfGenerator, InvoicePdfGenerator>();
    services.AddScoped<ITokenProvider, TokenProvider>();

    services.AddHostedService<OverdueBookingCleanupService>();

    return services;
  }
}