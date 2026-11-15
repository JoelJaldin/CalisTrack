using CalisTrack.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json").AddJsonFile("appsettings.Local.json", optional: true).AddEnvironmentVariables().Build();
var connection = configuration.GetConnectionString("CalisTrack")!;
var builder = new SqlConnectionStringBuilder(connection);
var context = new CalisTrackContext(new DbContextOptionsBuilder<CalisTrackContext>().UseSqlServer(connection).Options);
if (args.FirstOrDefault() == "drop")
{
    if (!builder.InitialCatalog.StartsWith("CalisTrackPruebas_")) throw new InvalidOperationException("Solo se pueden eliminar las bases temporales de las pruebas.");
    await context.Database.EnsureDeletedAsync();
    Console.WriteLine("Base temporal eliminada.");
}
else
{
    var hashes = await context.Usuarios.Select(u => u.PasswordHash).ToListAsync();
    if (hashes.Any(h => h.Length < 30 || h == "CalisTrack123!")) throw new Exception("Contraseñas sin hash.");
    Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(new {
        Database = builder.InitialCatalog,
        Usuarios = await context.Usuarios.CountAsync(),
        Entrenamientos = await context.Entrenamientos.CountAsync(),
        Registros = await context.Registros.CountAsync(),
        Notificaciones = await context.Notificaciones.CountAsync(),
        Tablas = await context.Database.SqlQueryRaw<int>("SELECT COUNT(*) AS Value FROM sys.tables").SingleAsync(),
        Migraciones = (await context.Database.GetAppliedMigrationsAsync()).Count()
    }));
}
