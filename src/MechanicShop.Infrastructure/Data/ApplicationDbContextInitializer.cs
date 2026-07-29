using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Domain.Employees;
using MechanicShop.Domain.Identity;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Enums;
using MechanicShop.Domain.RepairTasks.Parts;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Infrastructure.Identity;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Infrastructure.Data;

public class ApplicationDbContextInitializer(
    ILogger<ApplicationDbContextInitializer> logger,
    AppDbContext context,
    UserManager<AppUser> userManager,
    RoleManager<IdentityRole> roleManager)
{
    // Fixed ids so re-seeding is idempotent across runs/environments.
    private static readonly SeedUser Manager = new("19a59129-6c20-417a-834d-11a208d32d96", "pm@localhost", "Primary", "Manager", Role.Manager);

    private static readonly SeedUser[] Labors =
    [
        new("b6327240-0aea-46fc-863a-777fc4e42560", "john.labor@localhost", "John", "S.", Role.Labor),
        new("8104ab20-26c2-4651-b1de-c0baf04dbbd9", "peter.labor@localhost", "Peter", "R.", Role.Labor),
        new("e17c83de-1089-4f19-bf79-5f789133d37f", "kevin.labor@localhost", "Kevin", "M.", Role.Labor),
        new("54cd01ba-b9ae-4c14-bab6-f3df0219ba4c", "suzan.labor@localhost", "Suzan", "L.", Role.Labor),
    ];

    private readonly ILogger<ApplicationDbContextInitializer> _logger = logger;
    private readonly AppDbContext _context = context;
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;

    public async Task InitializeAsync()
    {
        try
        {
            await _context.Database.EnsureCreatedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task TrySeedAsync()
    {
        await EnsureRoleAsync(Role.Manager);
        await EnsureRoleAsync(Role.Labor);

        await EnsureUserAsync(Manager);

        foreach (var labor in Labors)
        {
            await EnsureUserAsync(labor);
        }

        SeedEmployees();
        SeedCustomers();
        SeedRepairTasks();

        await _context.SaveChangesAsync();

        if (!_context.WorkOrders.Any())
        {
            await SeedWorkOrdersAsync();
        }
    }

    private async Task EnsureRoleAsync(Role role)
    {
        if (_roleManager.Roles.All(r => r.Name != role.ToString()))
        {
            await _roleManager.CreateAsync(new IdentityRole(role.ToString()));
        }
    }

    private async Task EnsureUserAsync(SeedUser seed)
    {
        if (_userManager.Users.Any(u => u.Email == seed.Email))
        {
            return;
        }

        var user = new AppUser
        {
            Id = seed.Id,
            Email = seed.Email,
            UserName = seed.Email,
            EmailConfirmed = true,
        };

        await _userManager.CreateAsync(user, seed.Email);
        await _userManager.AddToRolesAsync(user, [seed.Role.ToString()]);
    }

    private void SeedEmployees()
    {
        if (_context.Employees.Any())
        {
            return;
        }

        _context.Employees.AddRange(
            new[] { Manager }.Concat(Labors)
                .Select(u => Employee.Create(Guid.Parse(u.Id), u.FirstName, u.LastName, u.Role).Value));
    }

    private void SeedCustomers()
    {
        if (_context.Customers.Any())
        {
            return;
        }

        List<Vehicle> johnsVehicles =
        [
            Vehicle.Create(Guid.Parse("61401e63-007b-4b1c-8914-9eb6e9bd95c5"), "Toyota", "Camry", 2020, "ABC123").Value,
            Vehicle.Create(Guid.Parse("13c80914-41ad-4d46-b7bb-60f6c89ad01e"), "Honda", "Civic", 2018, "XYZ456").Value,
        ];

        List<Vehicle> sarahsVehicles =
        [
            Vehicle.Create(Guid.Parse("a04f329d-0f5a-46a0-beae-699c034ae401"), "Ford", "Focus", 2021, "DEF789").Value,
            Vehicle.Create(Guid.Parse("cf60e95b-5752-4c26-aa07-31a34164606c"), "Chevrolet", "Malibu", 2019, "GHI012").Value,
        ];

        _context.Customers.AddRange(
            Customer.Create(Guid.Parse("f522bbe5-e3b1-4e2c-a8a3-c41550dcf39d"), "John Doe", "123456789", "john.doe@localhost", johnsVehicles).Value,
            Customer.Create(Guid.Parse("73a04dd3-c81a-4a54-9882-ef1017eb192d"), "Sarah Peter", "987654321", "sarah.peter@localhost", sarahsVehicles).Value);
    }

    private void SeedRepairTasks()
    {
        if (_context.RepairTasks.Any())
        {
            return;
        }

        _context.RepairTasks.AddRange(
            RepairTask.Create(Guid.Parse("616aebb1-d515-4b40-8d47-8d5c0b67a313"), "Engine Oil Change", 50.00m, RepairDurationInMinutes.Min60,
                [Part.Create(Guid.Parse("ec65225c-9066-4a1c-974f-f183c39fdd16"), 25.00m, "Engine Oil", 1).Value,
                 Part.Create(Guid.Parse("62ad80e3-2cff-41af-ab40-16fab8db8b38"), 10.00m, "Oil Filter", 1).Value]).Value,

            RepairTask.Create(Guid.Parse("4fa0be55-06f6-4616-b086-e1f0c9354cd8"), "Brake Replacement", 150.00m, RepairDurationInMinutes.Min90,
                [Part.Create(Guid.Parse("86375a12-715e-4aa4-aad9-c0f9ccf44a14"), 40.00m, "Brake Pads", 2).Value,
                 Part.Create(Guid.Parse("526d89c3-a971-4ea7-ba15-de6b50b13c21"), 15.00m, "Brake Fluid", 1).Value]).Value,

            RepairTask.Create(Guid.Parse("a376b5d1-6b2d-4dd8-883e-d3d1721c1316"), "Tire Rotation", 30.00m, RepairDurationInMinutes.Min45,
                [Part.Create(Guid.Parse("a46f974e-a198-4098-8a1f-6be6e68ec743"), 5.00m, "Tire Valve", 4).Value]).Value,

            RepairTask.Create(Guid.Parse("a770cc6e-0c8b-4ac5-9ee6-6928682bd47e"), "Battery Replacement", 70.00m, RepairDurationInMinutes.Min30,
                [Part.Create(Guid.Parse("d4fd3255-29dc-4d45-9d87-f58ab98bc28b"), 120.00m, "Car Battery", 1).Value]).Value,

            RepairTask.Create(Guid.Parse("e4c2b675-4a60-488f-a7b4-61966e7e80e3"), "Wheel Alignment", 80.00m, RepairDurationInMinutes.Min60,
                [Part.Create(Guid.Parse("fa3b9a7e-1c2d-4e3f-9b8a-0c1d2e3f4a5b"), 5.00m, "Alignment Shim Kit (per wheel)", 4).Value]).Value,

            RepairTask.Create(Guid.Parse("1cb1608c-3bc7-4325-99c3-8244c0fb412f"), "Air Conditioning Recharge", 100.00m, RepairDurationInMinutes.Min30,
                [Part.Create(Guid.Parse("526dca0a-d236-47d3-8e8f-c83d555b2de9"), 50.00m, "Refrigerant", 1).Value]).Value,

            RepairTask.Create(Guid.Parse("a8e9b4e0-8581-40df-967d-51a0f4fabc0e"), "Spark Plug Replacement", 90.00m, RepairDurationInMinutes.Min60,
                [Part.Create(Guid.Parse("019f5eab-a8a5-44d4-92b3-1f998e3f10c2"), 10.00m, "Spark Plug", 4).Value]).Value,

            RepairTask.Create(Guid.Parse("90f2f3ef-3357-439e-9689-628aa08200c1"), "Engine Diagnostic", 120.00m, RepairDurationInMinutes.Min120,
                [Part.Create(Guid.Parse("c3d4e5f6-a7b8-9c0d-1e2f-3a4b5c6d7e8f"), 20.00m, "Smoke Leak Detector Fluid Cartridge", 1).Value]).Value,

            RepairTask.Create(Guid.Parse("d124651e-ca72-467e-ba28-81ea4a2080bc"), "Timing Belt Replacement", 200.00m, RepairDurationInMinutes.Min120,
                [Part.Create(Guid.Parse("06b764a0-73a2-4c37-b279-adae3856499c"), 75.00m, "Timing Belt", 1).Value]).Value,

            RepairTask.Create(Guid.Parse("cee9b309-8620-4028-8d38-2532771ab3ea"), "Transmission Fluid Change", 100.00m, RepairDurationInMinutes.Min45,
                [Part.Create(Guid.Parse("0a8b0c19-873a-4da0-811b-45ff85bca0ed"), 60.00m, "Transmission Fluid", 1).Value]).Value);
    }

    private async Task SeedWorkOrdersAsync()
    {
        var repairTasks = _context.RepairTasks.ToList();
        var vehicles = _context.Vehicles.ToList();
        var laborIds = Labors.Select(l => l.Id).ToArray();
        Spot[] spots = [Spot.A, Spot.B, Spot.C, Spot.D];

        var openTime = TimeSpan.FromHours(12);
        var closeTime = TimeSpan.FromHours(23);
        var startDate = DateTimeOffset.Now.Date.AddDays(1);
        var endDate = startDate.AddMonths(1);

        var generatedWorkOrders = new List<WorkOrder>();

        while (startDate < endDate)
        {
            foreach (var spot in spots)
            {
                generatedWorkOrders.AddRange(
                    GenerateWorkOrdersForSpot(startDate, spot, openTime, closeTime, repairTasks, vehicles, laborIds, generatedWorkOrders));
            }

            startDate = startDate.AddDays(1);
        }

        generatedWorkOrders.Add(CreateInProgressOrderStartingNow(repairTasks, vehicles));
        generatedWorkOrders.Add(CreateInProgressOrderEndingSoon(repairTasks, vehicles));

        _context.WorkOrders.AddRange(generatedWorkOrders);

        await _context.SaveChangesAsync();
    }

    private static List<WorkOrder> GenerateWorkOrdersForSpot(
        DateTimeOffset day,
        Spot spot,
        TimeSpan openTime,
        TimeSpan closeTime,
        List<RepairTask> repairTasks,
        List<Vehicle> vehicles,
        string[] laborIds,
        List<WorkOrder> alreadyGenerated)
    {
        var totalMinutes = (int)(closeTime - openTime).TotalMinutes;
        var minOccupancy = (int)(totalMinutes * 0.6); // Minimum 60% usage
        var maxOccupancy = (int)(totalMinutes * 0.8); // Maximum 80% usage

        var random = Random.Shared;
        var occupiedMinutes = 0;
        var currentTime = day.Add(openTime);
        var spotWorkOrders = new List<WorkOrder>();

        while (occupiedMinutes < minOccupancy && currentTime.TimeOfDay < closeTime)
        {
            var distinctTaskIds = repairTasks.Select(t => t.Id).Distinct().Count();

            var selectedTasks = repairTasks
                .DistinctBy(t => t.Id)
                .OrderBy(_ => Guid.NewGuid())
                .Take(random.Next(1, Math.Min(4, distinctTaskIds)))
                .ToList();

            var duration = selectedTasks.Sum(t => (int)t.EstimatedDuration);

            if (occupiedMinutes + duration > maxOccupancy)
            {
                break;
            }

            var startAt = currentTime;
            var endAt = startAt.AddMinutes(duration);

            if (endAt.TimeOfDay > closeTime)
            {
                break;
            }

            var availableVehicle = vehicles
                .Where(v => !alreadyGenerated.Any(w =>
                    w.VehicleId == v.Id &&
                    w.StartAtUtc.Date == startAt.Date &&
                    w.StartAtUtc < endAt &&
                    w.EndAtUtc > startAt))
                .OrderBy(_ => Guid.NewGuid())
                .FirstOrDefault();

            if (availableVehicle is null)
            {
                break;
            }

            var laborId = laborIds[random.Next(laborIds.Length)];

            var workOrder = WorkOrder.Create(
                Guid.NewGuid(), availableVehicle.Id, startAt, endAt, Guid.Parse(laborId), spot, selectedTasks).Value;

            spotWorkOrders.Add(workOrder);
            occupiedMinutes += duration;
            currentTime = day.Add(openTime).AddMinutes(occupiedMinutes);
        }

        return occupiedMinutes >= minOccupancy ? spotWorkOrders : [];
    }

    private WorkOrder CreateInProgressOrderStartingNow(List<RepairTask> repairTasks, List<Vehicle> vehicles)
    {
        var tasks = repairTasks.OrderBy(_ => Guid.NewGuid()).Take(2).ToList();
        var startAt = RoundDownToQuarterHour(DateTimeOffset.UtcNow);
        var endAt = startAt.AddMinutes(tasks.Sum(t => (int)t.EstimatedDuration));

        var workOrder = WorkOrder.Create(
            Guid.NewGuid(),
            vehicles[Random.Shared.Next(vehicles.Count)].Id,
            startAt,
            endAt,
            Guid.Parse(Labors[0].Id),
            Spot.A,
            tasks).Value;

        workOrder.UpdateState(State.InProgress);

        return workOrder;
    }

    private WorkOrder CreateInProgressOrderEndingSoon(List<RepairTask> repairTasks, List<Vehicle> vehicles)
    {
        var task = repairTasks.First(t => t.EstimatedDuration == RepairDurationInMinutes.Min60);
        var startAt = RoundDownToQuarterHour(DateTimeOffset.UtcNow.AddMinutes(-45));
        var endAt = startAt.AddMinutes((int)task.EstimatedDuration);

        var workOrder = WorkOrder.Create(
            Guid.NewGuid(),
            vehicles[Random.Shared.Next(vehicles.Count)].Id,
            startAt,
            endAt,
            Guid.Parse(Labors[1].Id),
            Spot.B,
            [task]).Value;

        workOrder.UpdateState(State.InProgress);

        return workOrder;
    }

    private static DateTimeOffset RoundDownToQuarterHour(DateTimeOffset value) =>
        new(value.Year, value.Month, value.Day, value.Hour, value.Minute - (value.Minute % 15), 0, TimeSpan.Zero);

    private sealed record SeedUser(string Id, string Email, string FirstName, string LastName, Role Role);
}