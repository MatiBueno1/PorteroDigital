using Microsoft.EntityFrameworkCore;
using PorteroDigital.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

var services = new ServiceCollection();
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.Development.json", optional: false)
    .Build();

services.AddDbContext<PorteroDigitalDbContext>(options =>
    options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

var serviceProvider = services.BuildServiceProvider();
using var scope = serviceProvider.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<PorteroDigitalDbContext>();

try 
{
    var connection = context.Database.GetDbConnection();
    await connection.OpenAsync();

    using (var command = connection.CreateCommand())
    {
        command.CommandText = "PRAGMA table_info(VisitorLogs);";
        var hasResidentDecision = false;
        
        using (var reader = await command.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                if (reader["name"].ToString() == "ResidentDecision")
                {
                    hasResidentDecision = true;
                    break;
                }
            }
        }

        if (!hasResidentDecision)
        {
            Console.WriteLine("Agregando columna ResidentDecision a VisitorLogs...");
            using (var alterCmd = connection.CreateCommand())
            {
                alterCmd.CommandText = "ALTER TABLE VisitorLogs ADD COLUMN ResidentDecision TEXT NULL;";
                await alterCmd.ExecuteNonQueryAsync();
            }
            Console.WriteLine("¡Columna agregada con éxito!");
        }
        else
        {
            Console.WriteLine("La columna ResidentDecision ya existe.");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"ERROR: {ex.Message}");
}
