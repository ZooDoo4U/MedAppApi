
namespace MedAppApi; // Use the EXACT same namespace

using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;


public class AppDbContext : DbContext
{
    // Constructor required for dependency injection
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // This property exposes your table to the rest of the application
    public DbSet<MedTable> MedTable { get; set; }
}
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer("Server=localhost;Database=MedDb;Trusted_Connection=True;TrustServerCertificate=True;");
        return new AppDbContext(optionsBuilder.Options);
    }
}

