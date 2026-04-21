using Microsoft.EntityFrameworkCore;
using PorteroDigital.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

var services = new ServiceCollection();
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.Development.json")
    .Build();

services.AddDbContext<PorteroDigitalDbContext>(options =>
    options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

var serviceProvider = services.BuildServiceProvider();
using var scope = serviceProvider.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<PorteroDigitalDbContext>();

try 
{
    Console.WriteLine("Probando consulta de historial...");
    var firstLog = await context.VisitorLogs.OrderByDescending(v => v.RequestedAtUtc).FirstOrDefaultAsync();
    if (firstLog != null) {
        Console.WriteLine($"Encontrado Log: {firstLog.VisitorName} - {firstLog.Status}");
    } else {
        Console.WriteLine("No hay logs en la base de datos.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"ERROR: {ex.Message}");
    if (ex.InnerException != null) Console.WriteLine($"INNER: {ex.InnerException.Message}");
}
