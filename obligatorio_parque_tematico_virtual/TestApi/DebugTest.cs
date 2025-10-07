using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IBusinessLogic;
using BusinessLogic;
using Microsoft.EntityFrameworkCore;
using DataAccess.Context;
using Microsoft.Data.Sqlite;
using IDataAccess;

namespace TestApi
{
    [TestClass]
    public class DebugTest
    {
        [TestMethod]
        public async Task TestTicketsEndpointExists()
        {
            // Reset singleton instance
            DateTimeLogic.ResetInstance();

            // Create shared in-memory connection
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        // Remove SQL Server DbContext
                        var descriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                        if (descriptor != null)
                        {
                            services.Remove(descriptor);
                        }

                        // Remove existing IDateTimeLogic registration
                        var dateTimeDescriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(IDateTimeLogic));
                        if (dateTimeDescriptor != null)
                        {
                            services.Remove(dateTimeDescriptor);
                        }

                        // Add SQLite DbContext with shared connection
                        services.AddDbContext<AppDbContext>(options =>
                            options.UseSqlite(connection));

                        services.AddSingleton<IDateTimeLogic>(provider => DateTimeLogic.GetInstance(provider));
                    });
                });

            // Initialize database schema
            using (var scope = factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                context.Database.EnsureCreated();
            }

            var client = factory.CreateClient();

            // First test that a known endpoint works
            var knownResponse = await client.GetAsync("/api/datetime");
            Assert.AreEqual(System.Net.HttpStatusCode.OK, knownResponse.StatusCode);

            // Test that tickets endpoint exists
            var response = await client.PostAsync("/api/tickets", new StringContent("", System.Text.Encoding.UTF8, "application/json"));

            // Should not be 404 - could be 400 (BadRequest) which is fine
            Assert.AreNotEqual(System.Net.HttpStatusCode.NotFound, response.StatusCode);

            connection.Close();
            connection.Dispose();
        }
    }
}