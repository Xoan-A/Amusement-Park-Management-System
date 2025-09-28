using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IBusinessLogic;
using BusinessLogic;

namespace TestApi
{
    [TestClass]
    public class DebugTest
    {
        [TestMethod]
        public async Task TestTicketsEndpointExists()
        {
            var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        services.AddSingleton<IDateTimeLogic>(provider => DateTimeLogic.Instance);
                    });
                });

            var client = factory.CreateClient();

            // First test that a known endpoint works
            var knownResponse = await client.GetAsync("/api/datetime");
            Assert.AreEqual(System.Net.HttpStatusCode.OK, knownResponse.StatusCode);

            // Test that tickets endpoint exists
            var response = await client.PostAsync("/api/tickets", new StringContent("", System.Text.Encoding.UTF8, "application/json"));

            // Should not be 404 - could be 400 (BadRequest) which is fine
            Assert.AreNotEqual(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}