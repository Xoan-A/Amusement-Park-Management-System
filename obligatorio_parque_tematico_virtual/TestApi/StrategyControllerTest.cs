using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Net;
using System.Text;
using System.Text.Json;
using IBusinessLogic;
using IBusinessLogic.Strategy;
using Models.In;
using Models.Out;
using BusinessLogic;
using Domain;

namespace ApiTests
{
    [TestClass]
    public class StrategyControllerTest
    {
        private WebApplicationFactory<Program> _factory = null!;
        private HttpClient _client = null!;
        private HttpClient _adminClient = null!;
        private Mock<IActiveStrategy> _mockActiveStrategy = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockActiveStrategy = new Mock<IActiveStrategy>();

            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        services.AddSingleton(_mockActiveStrategy.Object);
                    });
                });

            _client = _factory.CreateClient();

            var jwtSettings = Microsoft.Extensions.Options.Options.Create(new Models.JwtSettings
            {
                SecretKey = "MySecretKeyForJWTTokenGeneration1234567890",
                Issuer = "ParqueTematico",
                Audience = "ParqueTematico",
                ExpirationHours = 1
            });
            
            TokenService tokenService = new TokenService(jwtSettings);

            Administrator adminUser = new Administrator
            {
                Id = Guid.NewGuid(),
                Name = "Admin",
                LastName = "User",
                Email = "admin@example.com"
            };
            string adminToken = tokenService.GenerateToken(adminUser);
            _adminClient = _factory.CreateClient();
            _adminClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _client?.Dispose();
            _adminClient?.Dispose();
            _factory?.Dispose();
        }

        [TestMethod]
        public async Task GetStrategy_ShouldReturnCurrentStrategy()
        {
            var mockStrategy = new Mock<IContreteStrategy>();
            mockStrategy.Setup(s => s.Name).Returns("PerAttraction");

            _mockActiveStrategy.Setup(x => x.GetStrategy(It.IsAny<DateTime>()))
                .Returns(mockStrategy.Object);

            var getRequest = new GetStrategyRequest { CurrentDate = new DateTime(2024, 1, 15, 10, 0, 0) };
            var json = JsonSerializer.Serialize(getRequest);
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/strategy")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            var response = await _adminClient.SendAsync(requestMessage);

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var strategyResponse = JsonSerializer.Deserialize<StrategyResponse>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.IsNotNull(strategyResponse);
            Assert.AreEqual("PerAttraction", strategyResponse.Name);
        }

        [TestMethod]
        public async Task GetStrategy_WhenNoStrategySet_ShouldReturnInternalServerError()
        {
            _mockActiveStrategy.Setup(x => x.GetStrategy(It.IsAny<DateTime>()))
                .Throws(new InvalidOperationException("Strategy not set"));

            var getRequest = new GetStrategyRequest { CurrentDate = new DateTime(2024, 1, 15, 10, 0, 0) };
            var json = JsonSerializer.Serialize(getRequest);
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/strategy")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            var response = await _adminClient.SendAsync(requestMessage);

            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [TestMethod]
        public async Task GetStrategy_WithoutAuth_ShouldReturnUnauthorized()
        {
            var getRequest = new GetStrategyRequest { CurrentDate = new DateTime(2024, 1, 15, 10, 0, 0) };
            var json = JsonSerializer.Serialize(getRequest);
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/strategy")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            var response = await _client.SendAsync(requestMessage);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task SetStrategy_WithPerAttraction_ShouldReturnSuccess()
        {
            var request = new SetStrategyRequest
            {
                StrategyName = "PerAttraction"
            };

            _mockActiveStrategy.Setup(x => x.SetStrategy(It.IsAny<SetStrategyRequest>()));

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _adminClient.PutAsync("/api/strategy/set", content);

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var messageResponse = JsonSerializer.Deserialize<MessageResponse>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.IsNotNull(messageResponse);
            Assert.AreEqual("Strategy setted successfully", messageResponse.Message);

            _mockActiveStrategy.Verify(x => x.SetStrategy(It.Is<SetStrategyRequest>(
                r => r.StrategyName == "PerAttraction")), Times.Once);
        }

        [TestMethod]
        public async Task SetStrategy_WithPerEvent_ShouldReturnSuccess()
        {
            var request = new SetStrategyRequest
            {
                StrategyName = "PerEvent"
            };

            _mockActiveStrategy.Setup(x => x.SetStrategy(It.IsAny<SetStrategyRequest>()));

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _adminClient.PutAsync("/api/strategy/set", content);

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var messageResponse = JsonSerializer.Deserialize<MessageResponse>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.IsNotNull(messageResponse);
            Assert.AreEqual("Strategy setted successfully", messageResponse.Message);

            _mockActiveStrategy.Verify(x => x.SetStrategy(It.Is<SetStrategyRequest>(
                r => r.StrategyName == "PerEvent")), Times.Once);
        }

        [TestMethod]
        public async Task SetStrategy_WithCombo_ShouldReturnSuccess()
        {
            var request = new SetStrategyRequest
            {
                StrategyName = "Combo",
                N = 30
            };

            _mockActiveStrategy.Setup(x => x.SetStrategy(It.IsAny<SetStrategyRequest>()));

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _adminClient.PutAsync("/api/strategy/set", content);

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var messageResponse = JsonSerializer.Deserialize<MessageResponse>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.IsNotNull(messageResponse);
            Assert.AreEqual("Strategy setted successfully", messageResponse.Message);

            _mockActiveStrategy.Verify(x => x.SetStrategy(It.Is<SetStrategyRequest>(
                r => r.StrategyName == "Combo" && r.N == 30)), Times.Once);
        }

        [TestMethod]
        public async Task SetStrategy_WithComboWithoutN_ShouldReturnBadRequest()
        {
            var request = new SetStrategyRequest
            {
                StrategyName = "Combo",
                N = null
            };

            _mockActiveStrategy.Setup(x => x.SetStrategy(It.IsAny<SetStrategyRequest>()))
                .Throws(new ArgumentException("N is required for Combo strategy"));

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _adminClient.PutAsync("/api/strategy/set", content);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task SetStrategy_WithInvalidStrategyName_ShouldReturnBadRequest()
        {
            var request = new SetStrategyRequest
            {
                StrategyName = "InvalidStrategy"
            };

            _mockActiveStrategy.Setup(x => x.SetStrategy(It.IsAny<SetStrategyRequest>()))
                .Throws(new ArgumentException("Invalid strategy name: InvalidStrategy"));

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _adminClient.PutAsync("/api/strategy/set", content);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task SetStrategy_WithNullRequest_ShouldReturnBadRequest()
        {
            var json = "null";
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _adminClient.PutAsync("/api/strategy/set", content);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task SetStrategy_WithoutAuth_ShouldReturnUnauthorized()
        {
            var request = new SetStrategyRequest
            {
                StrategyName = "PerAttraction"
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PutAsync("/api/strategy/set", content);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task GetStrategy_AfterSettingStrategy_ShouldReturnNewStrategy()
        {
            var setRequest = new SetStrategyRequest
            {
                StrategyName = "Combo",
                N = 45
            };

            var mockStrategy = new Mock<IContreteStrategy>();
            mockStrategy.Setup(s => s.Name).Returns("Combo");

            _mockActiveStrategy.Setup(x => x.SetStrategy(It.IsAny<SetStrategyRequest>()));
            _mockActiveStrategy.Setup(x => x.GetStrategy(It.IsAny<DateTime>())).Returns(mockStrategy.Object);

            var json = JsonSerializer.Serialize(setRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var setResponse = await _adminClient.PutAsync("/api/strategy/set", content);
            setResponse.EnsureSuccessStatusCode();

            var getRequest = new GetStrategyRequest { CurrentDate = new DateTime(2024, 1, 15, 10, 0, 0) };
            var getJson = JsonSerializer.Serialize(getRequest);
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/strategy")
            {
                Content = new StringContent(getJson, Encoding.UTF8, "application/json")
            };
            var getResponse = await _adminClient.SendAsync(requestMessage);
            getResponse.EnsureSuccessStatusCode();

            var responseContent = await getResponse.Content.ReadAsStringAsync();
            var strategyResponse = JsonSerializer.Deserialize<StrategyResponse>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.IsNotNull(strategyResponse);
            Assert.AreEqual("Combo", strategyResponse.Name);
        }
    }
}
