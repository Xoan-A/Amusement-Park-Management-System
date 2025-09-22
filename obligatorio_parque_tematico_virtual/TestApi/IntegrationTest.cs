using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Models.In;
using Models.Out;
using IBusinessLogic;
using BusinessLogic;

namespace ApiTests
{
    [TestClass]
    public class IntegrationTest
    {
        private WebApplicationFactory<Program> _factory;
        private HttpClient _client;

        [TestInitialize]
        public void Setup()
        {
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        // Override with simpler registration for tests
                        services.AddSingleton<IDateTimeLogic>(provider => DateTimeLogic.Instance);
                    });
                });

            _client = _factory.CreateClient();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _client?.Dispose();
            _factory?.Dispose();
        }

        [TestMethod]
        public async Task LoginEndpoint_RequiresNoAuthentication()
        {
            var request = new LoginRequest
            {
                Email = "admin@test.com",
                Password = "password123"
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/auth/login", content);

            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task RegisterEndpoint_RequiresNoAuthentication()
        {
            var request = new RegisterVisitorRequest
            {
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "password123",
                BirthDate = new DateTime(1990, 1, 1)
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/auth/register", content);

            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task DateTimeEndpoints_RequireNoAuthentication()
        {
            var getResponse = await _client.GetAsync("/api/datetime");
            Assert.AreEqual(System.Net.HttpStatusCode.OK, getResponse.StatusCode);

            var setRequest = new SetDateTimeRequest
            {
                DateTime = "2024-01-01T12:00:00"
            };

            var json = JsonSerializer.Serialize(setRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var postResponse = await _client.PostAsync("/api/datetime", content);
            Assert.AreEqual(System.Net.HttpStatusCode.OK, postResponse.StatusCode);
        }
    }
}