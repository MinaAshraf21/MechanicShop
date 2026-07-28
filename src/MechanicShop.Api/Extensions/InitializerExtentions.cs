using MechanicShop.Infrastructure.Data;

namespace MechanicShop.Api.Extensions;

public static class InitializerExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var Initializer = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitializer>();

        await Initializer.InitializeAsync();

        await Initializer.SeedAsync();
    }
}