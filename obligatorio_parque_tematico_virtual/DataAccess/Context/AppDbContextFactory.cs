using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DataAccess.Context
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // Use a connection string for design-time operations
            optionsBuilder.UseSqlServer("Server=localhost,1433;Database=ParqueTematicoDB;User ID=SA;Password=Your_password123;TrustServerCertificate=True;Encrypt=False;");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
