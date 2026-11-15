using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CalisTrack.Data;

public class CalisTrackContextFactory : IDesignTimeDbContextFactory<CalisTrackContext>
{
    public CalisTrackContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Local.json", optional: true)
            .AddEnvironmentVariables().Build();
        var options = new DbContextOptionsBuilder<CalisTrackContext>().UseSqlServer(configuration.GetConnectionString("CalisTrack")).Options;
        return new CalisTrackContext(options);
    }
}
