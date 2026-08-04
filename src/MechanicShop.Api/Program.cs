using MechanicShop.Api;
using MechanicShop.Api.Extensions;
using MechanicShop.Application;
using MechanicShop.Infrastructure;
using MechanicShop.Infrastructure.RealTime;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices()
                .AddInfrastructure(builder.Configuration)
                .AddPresentation(builder.Configuration);

builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig.ReadFrom.Configuration(context.Configuration));


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "MechanicShop API V1");

        options.EnableDeepLinking();
        options.DisplayRequestDuration();
        options.EnableFilter();
    });

    app.MapScalarApiReference();

    await app.InitializeDatabaseAsync();
  }
else
{
  app.UseHsts();
}


app.UseCoreMiddlewares(builder.Configuration);
app.MapControllers();
app.UseAntiforgery();

app.MapStaticAssets();

app.MapHub<WorkOrderHub>("/hub/workorders");



app.Run();
