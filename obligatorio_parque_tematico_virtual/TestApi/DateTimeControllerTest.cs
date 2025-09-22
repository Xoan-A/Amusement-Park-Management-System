using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BusinessLogic;
using IBusinessLogic;
using Models.In;
using Models.Out;

namespace ApiTests
{
    [TestClass]
    public class DateTimeControllerTest
    {
        private WebApplicationFactory<Program> _factory;
        private HttpClient _client;
        private Mock<IDateTimeLogic> _mockDateTimeLogic;

        [TestInitialize]
        public void Setup()
        {
            _mockDateTimeLogic = new Mock<IDateTimeLogic>();

            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        services.AddSingleton(_mockDateTimeLogic.Object);
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
        public async Task GetDateTime_ReturnsCurrentDateTime()
        {
            var expectedDateTime = new DateTime(2024, 1, 1, 12, 0, 0);
            var expectedResponse = new DateTimeResponse
            {
                CurrentDateTime = expectedDateTime
            };

            _mockDateTimeLogic.Setup(x => x.GetCurrentDateTime())
                             .Returns(expectedDateTime);

            var response = await _client.GetAsync("/api/datetime");

            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var dateTimeResponse = JsonSerializer.Deserialize<DateTimeResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.AreEqual(expectedResponse.CurrentDateTime, dateTimeResponse.CurrentDateTime);
        }

        [TestMethod]
        public async Task SetDateTime_ValidDateTime_ReturnsSuccess()
        {
            var request = new SetDateTimeRequest
            {
                DateTime = "2024-01-01T12:00:00"
            };
            var expectedDateTime = DateTime.Parse(request.DateTime);

            _mockDateTimeLogic.Setup(x => x.SetDateTime(expectedDateTime));

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/datetime", content);

            response.EnsureSuccessStatusCode();
            _mockDateTimeLogic.Verify(x => x.SetDateTime(expectedDateTime), Times.Once);
        }
    }
}