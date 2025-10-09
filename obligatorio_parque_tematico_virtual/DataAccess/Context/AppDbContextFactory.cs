using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DataAccess.Context
{
    [ExcludeFromCodeCoverage]
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            DbContextOptionsBuilder<AppDbContext> optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            optionsBuilder.UseSqlServer("Server=localhost,1433;Database=ParqueTematicoDB;User ID=SA;Password=Your_password123;TrustServerCertificate=True;Encrypt=False;");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
