using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Text;
using System.Text.Json;
using IBusinessLogic;
using Models.In;
using Models.Out;
using Microsoft.EntityFrameworkCore;
using DataAccess.Context;
using Microsoft.Data.Sqlite;
using Api;

namespace ApiTests
{
    [TestClass]
    public class DateTimeControllerTest
    {
        private WebApplicationFactory<Program> _factory;
        private HttpClient _client;
        private Mock<IDateTimeLogic> _mockDateTimeLogic;
        private SqliteConnection _connection = null!;

        [TestInitialize]
        public void Setup()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            _mockDateTimeLogic = new Mock<IDateTimeLogic>();

            _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    ServiceDescriptor? descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (descriptor != null) services.Remove(descriptor);

                    services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlite(_connection));

                    services.AddSingleton(_mockDateTimeLogic.Object);
                });
            });

            using (IServiceScope scope = _factory.Services.CreateScope())
            {
                AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                context.Database.EnsureCreated();
            }

            _client = _factory.CreateClient();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _client?.Dispose();
            _factory?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }

        [TestMethod]
        public void GetDateTime_ReturnsCurrentDateTime()
        {
            DateTime expectedDateTime = new DateTime(2024, 1, 1, 12, 0, 0);
            DateTimeResponse expectedResponse = new DateTimeResponse
            {
                CurrentDateTime = expectedDateTime
            };

            _mockDateTimeLogic.Setup(x => x.GetCurrentDateTime())
            .Returns(expectedDateTime);

            HttpResponseMessage response = _ = _client.GetAsync("/api/datetime").Result;

            response.EnsureSuccessStatusCode();
            string responseContent = response.Content.ReadAsStringAsync().Result;
            DateTimeResponse? dateTimeResponse = JsonSerializer.Deserialize<DateTimeResponse>(responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.AreEqual(expectedResponse.CurrentDateTime, dateTimeResponse.CurrentDateTime);
        }

        [TestMethod]
        public void SetDateTime_ValidDateTime_ReturnsSuccess()
        {
            SetDateTimeRequest request = new SetDateTimeRequest
            {
                DateTime = "2024-01-01T12:00:00"
            };
            DateTime expectedDateTime = DateTime.Parse(request.DateTime);

            _mockDateTimeLogic.Setup(x => x.SetDateTime(expectedDateTime));

            string json = JsonSerializer.Serialize(request);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = _ = _client.PutAsync("/api/datetime", content).Result;

            response.EnsureSuccessStatusCode();
            _mockDateTimeLogic.Verify(x => x.SetDateTime(expectedDateTime), Times.Once);
        }

        [TestMethod]
        public void SetDateTime_InvalidFormat_Returns500()
        {
            SetDateTimeRequest request = new SetDateTimeRequest
            {
                DateTime = "invalid-date-format"
            };

            string json = JsonSerializer.Serialize(request);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = _ = _client.PutAsync("/api/datetime", content).Result;

            Assert.AreEqual(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
        }
    }
}