using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
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
using Microsoft.EntityFrameworkCore;
using DataAccess.Context;
using Microsoft.Data.Sqlite;

namespace ApiTests
{
    [TestClass]
    public class StrategyControllerTest
    {
        private SqliteConnection _connection = null!;
        private WebApplicationFactory<Program> _factory = null!;
        private HttpClient _client = null!;
        private HttpClient _adminClient = null!;
        private Mock<IActiveStrategy> _mockActiveStrategy = null!;
        private Mock<IUserLogic> _mockUserLogic = null!;

        [TestInitialize]
        public void Setup()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            _mockActiveStrategy = new Mock<IActiveStrategy>();
            _mockUserLogic = new Mock<IUserLogic>();

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

                        services.AddSingleton(_mockActiveStrategy.Object);
                        services.AddSingleton(_mockUserLogic.Object);
                    });
                });

            using (IServiceScope scope = _factory.Services.CreateScope())
            {
                AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                context.Database.EnsureCreated();
            }

            _client = _factory.CreateClient();

            Microsoft.Extensions.Options.IOptions<Models.JwtSettings> jwtSettings = Microsoft.Extensions.Options.Options.Create(new Models.JwtSettings
            {
                SecretKey = "MySecretKeyForJWTTokenGeneration1234567890",
                Issuer = "ParqueTematico",
                Audience = "ParqueTematico",
                ExpirationHours = 1
            });

            TokenLogic tokenLogic = new TokenLogic(jwtSettings);

            User adminUser = new User
            {
                Id = Guid.NewGuid(),
                Name = "Admin",
                LastName = "User",
                Email = "admin@example.com"
            };
            adminUser.UserRoles = new List<UserRole>
            {
                new UserRole { Role = new Role { Name = Role.ADMINISTRATOR } }
            };
            string adminToken = tokenLogic.GenerateToken(adminUser);
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
            _connection?.Close();
            _connection?.Dispose();
        }

        [TestMethod]
        public async Task GetStrategy_ShouldReturnCurrentStrategy()
        {
            Mock<IConcreteStrategy> mockStrategy = new Mock<IConcreteStrategy>();
            mockStrategy.Setup(s => s.Name).Returns("PerAttraction");

            _mockActiveStrategy.Setup(x => x.GetStrategy())
                .ReturnsAsync(mockStrategy.Object);

            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/strategy");
            HttpResponseMessage response = await _adminClient.SendAsync(requestMessage);

            response.EnsureSuccessStatusCode();

            string responseContent = await response.Content.ReadAsStringAsync();
            StrategyResponse? strategyResponse = JsonSerializer.Deserialize<StrategyResponse>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.IsNotNull(strategyResponse);
            Assert.AreEqual("PerAttraction", strategyResponse.Name);
        }

        [TestMethod]
        public async Task GetStrategy_WhenNoStrategySet_ShouldReturnInternalServerError()
        {
            _mockActiveStrategy.Setup(x => x.GetStrategy())
                .Throws(new InvalidOperationException("Strategy not set"));

            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/strategy");
            HttpResponseMessage response = await _adminClient.SendAsync(requestMessage);

            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [TestMethod]
        public async Task GetStrategy_WithoutAuth_ShouldReturnUnauthorized()
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/strategy");
            HttpResponseMessage response = await _client.SendAsync(requestMessage);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task SetStrategy_WithPerAttraction_ShouldReturnSuccess()
        {
            SetStrategyRequest request = new SetStrategyRequest
            {
                StrategyName = "PerAttraction"
            };

            _mockActiveStrategy.Setup(x => x.SetStrategy(It.IsAny<SetStrategyRequest>()));

            string json = JsonSerializer.Serialize(request);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _adminClient.PutAsync("/api/strategy", content);

            response.EnsureSuccessStatusCode();

            string responseContent = await response.Content.ReadAsStringAsync();
            MessageResponse? messageResponse = JsonSerializer.Deserialize<MessageResponse>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.IsNotNull(messageResponse);
            Assert.AreEqual("Strategy set successfully", messageResponse.Message);

            _mockActiveStrategy.Verify(x => x.SetStrategy(It.Is<SetStrategyRequest>(
                r => r.StrategyName == "PerAttraction")), Times.Once);
        }

        [TestMethod]
        public async Task SetStrategy_WithPerEvent_ShouldReturnSuccess()
        {
            SetStrategyRequest request = new SetStrategyRequest
            {
                StrategyName = "PerEvent"
            };

            _mockActiveStrategy.Setup(x => x.SetStrategy(It.IsAny<SetStrategyRequest>()));

            string json = JsonSerializer.Serialize(request);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _adminClient.PutAsync("/api/strategy", content);

            response.EnsureSuccessStatusCode();

            string responseContent = await response.Content.ReadAsStringAsync();
            MessageResponse? messageResponse = JsonSerializer.Deserialize<MessageResponse>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.IsNotNull(messageResponse);
            Assert.AreEqual("Strategy set successfully", messageResponse.Message);

            _mockActiveStrategy.Verify(x => x.SetStrategy(It.Is<SetStrategyRequest>(
                r => r.StrategyName == "PerEvent")), Times.Once);
        }

        [TestMethod]
        public async Task SetStrategy_WithCombo_ShouldReturnSuccess()
        {
            SetStrategyRequest request = new SetStrategyRequest
            {
                StrategyName = "Combo",
                N = 30
            };

            _mockActiveStrategy.Setup(x => x.SetStrategy(It.IsAny<SetStrategyRequest>()));

            string json = JsonSerializer.Serialize(request);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _adminClient.PutAsync("/api/strategy", content);

            response.EnsureSuccessStatusCode();

            string responseContent = await response.Content.ReadAsStringAsync();
            MessageResponse? messageResponse = JsonSerializer.Deserialize<MessageResponse>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.IsNotNull(messageResponse);
            Assert.AreEqual("Strategy set successfully", messageResponse.Message);

            _mockActiveStrategy.Verify(x => x.SetStrategy(It.Is<SetStrategyRequest>(
                r => r.StrategyName == "Combo" && r.N == 30)), Times.Once);
        }

        [TestMethod]
        public async Task SetStrategy_WithComboWithoutN_ShouldReturnBadRequest()
        {
            SetStrategyRequest request = new SetStrategyRequest
            {
                StrategyName = "Combo",
                N = null
            };

            _mockActiveStrategy.Setup(x => x.SetStrategy(It.IsAny<SetStrategyRequest>()))
                .Throws(new ArgumentException("N is required for Combo strategy"));

            string json = JsonSerializer.Serialize(request);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _adminClient.PutAsync("/api/strategy", content);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task SetStrategy_WithInvalidStrategyName_ShouldReturnBadRequest()
        {
            SetStrategyRequest request = new SetStrategyRequest
            {
                StrategyName = "InvalidStrategy"
            };

            _mockActiveStrategy.Setup(x => x.SetStrategy(It.IsAny<SetStrategyRequest>()))
                .Throws(new ArgumentException("Invalid strategy name: InvalidStrategy"));

            string json = JsonSerializer.Serialize(request);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _adminClient.PutAsync("/api/strategy", content);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task SetStrategy_WithNullRequest_ShouldReturnBadRequest()
        {
            string json = "null";
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _adminClient.PutAsync("/api/strategy", content);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task SetStrategy_WithoutAuth_ShouldReturnUnauthorized()
        {
            SetStrategyRequest request = new SetStrategyRequest
            {
                StrategyName = "PerAttraction"
            };

            string json = JsonSerializer.Serialize(request);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PutAsync("/api/strategy", content);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task GetStrategy_AfterSettingStrategy_ShouldReturnNewStrategy()
        {
            SetStrategyRequest setRequest = new SetStrategyRequest
            {
                StrategyName = "Combo",
                N = 45
            };

            Mock<IConcreteStrategy> mockStrategy = new Mock<IConcreteStrategy>();
            mockStrategy.Setup(s => s.Name).Returns("Combo");

            _mockActiveStrategy.Setup(x => x.SetStrategy(It.IsAny<SetStrategyRequest>()));
            _mockActiveStrategy.Setup(x => x.GetStrategy()).ReturnsAsync(mockStrategy.Object);

            string json = JsonSerializer.Serialize(setRequest);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage setResponse = await _adminClient.PutAsync("/api/strategy", content);
            setResponse.EnsureSuccessStatusCode();

            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/strategy");
            HttpResponseMessage getResponse = await _adminClient.SendAsync(requestMessage);
            getResponse.EnsureSuccessStatusCode();

            string responseContent = await getResponse.Content.ReadAsStringAsync();
            StrategyResponse? strategyResponse = JsonSerializer.Deserialize<StrategyResponse>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.IsNotNull(strategyResponse);
            Assert.AreEqual("Combo", strategyResponse.Name);
        }

        [TestMethod]
        public async Task GetTopTen_ShouldReturnTopTenUsersOrderedByScore()
        {
            TopTenResponse topTenResponse = new TopTenResponse
            {
                TopTenUsers = new List<UserResponse>
                {
                    new UserResponse
                    {
                        Id = Guid.NewGuid(), Name = "User1", LastName = "Test", Email = "user1@test.com", Score = 100
                    },
                    new UserResponse
                    {
                        Id = Guid.NewGuid(), Name = "User2", LastName = "Test", Email = "user2@test.com", Score = 90
                    },
                    new UserResponse
                    {
                        Id = Guid.NewGuid(), Name = "User3", LastName = "Test", Email = "user3@test.com", Score = 80
                    },
                    new UserResponse
                    {
                        Id = Guid.NewGuid(), Name = "User4", LastName = "Test", Email = "user4@test.com", Score = 70
                    },
                    new UserResponse
                    {
                        Id = Guid.NewGuid(), Name = "User5", LastName = "Test", Email = "user5@test.com", Score = 60
                    },
                    new UserResponse
                    {
                        Id = Guid.NewGuid(), Name = "User6", LastName = "Test", Email = "user6@test.com", Score = 50
                    },
                    new UserResponse
                    {
                        Id = Guid.NewGuid(), Name = "User7", LastName = "Test", Email = "user7@test.com", Score = 40
                    },
                    new UserResponse
                    {
                        Id = Guid.NewGuid(), Name = "User8", LastName = "Test", Email = "user8@test.com", Score = 30
                    },
                    new UserResponse
                    {
                        Id = Guid.NewGuid(), Name = "User9", LastName = "Test", Email = "user9@test.com", Score = 20
                    },
                    new UserResponse
                    {
                        Id = Guid.NewGuid(), Name = "User10", LastName = "Test", Email = "user10@test.com", Score = 10
                    }
                }
            };

            _mockUserLogic.Setup(x => x.GetTopTenUsers()).ReturnsAsync(topTenResponse);

            HttpResponseMessage response = await _adminClient.GetAsync("/api/strategy/topTen");

            response.EnsureSuccessStatusCode();

            string responseContent = await response.Content.ReadAsStringAsync();
            TopTenResponse? result = JsonSerializer.Deserialize<TopTenResponse>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.TopTenUsers);
            Assert.AreEqual(10, result.TopTenUsers.Count);
            Assert.AreEqual(100, result.TopTenUsers[0].Score);
            Assert.AreEqual(10, result.TopTenUsers[9].Score);
            _mockUserLogic.Verify(x => x.GetTopTenUsers(), Times.Once);
        }

        [TestMethod]
        public async Task GetTopTen_WithoutAuth_ShouldReturnUnauthorized()
        {
            HttpResponseMessage response = await _client.GetAsync("/api/strategy/topTen");

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task GetTopTen_WithNoUsers_ShouldReturnEmptyList()
        {
            TopTenResponse topTenResponse = new TopTenResponse
            {
                TopTenUsers = new List<UserResponse>()
            };

            _mockUserLogic.Setup(x => x.GetTopTenUsers()).ReturnsAsync(topTenResponse);

            HttpResponseMessage response = await _adminClient.GetAsync("/api/strategy/topTen");

            response.EnsureSuccessStatusCode();

            string responseContent = await response.Content.ReadAsStringAsync();
            TopTenResponse? result = JsonSerializer.Deserialize<TopTenResponse>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.TopTenUsers);
            Assert.AreEqual(0, result.TopTenUsers.Count);
        }

        [TestMethod]
        public async Task GetTopTen_WithFewerThanTenUsers_ShouldReturnAllUsers()
        {
            TopTenResponse topTenResponse = new TopTenResponse
            {
                TopTenUsers = new List<UserResponse>
                {
                    new UserResponse
                    {
                        Id = Guid.NewGuid(), Name = "User1", LastName = "Test", Email = "user1@test.com", Score = 50
                    },
                    new UserResponse
                    {
                        Id = Guid.NewGuid(), Name = "User2", LastName = "Test", Email = "user2@test.com", Score = 40
                    },
                    new UserResponse
                    {
                        Id = Guid.NewGuid(), Name = "User3", LastName = "Test", Email = "user3@test.com", Score = 30
                    }
                }
            };

            _mockUserLogic.Setup(x => x.GetTopTenUsers()).ReturnsAsync(topTenResponse);

            HttpResponseMessage response = await _adminClient.GetAsync("/api/strategy/topTen");

            response.EnsureSuccessStatusCode();

            string responseContent = await response.Content.ReadAsStringAsync();
            TopTenResponse? result = JsonSerializer.Deserialize<TopTenResponse>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.TopTenUsers);
            Assert.AreEqual(3, result.TopTenUsers.Count);
            Assert.AreEqual(50, result.TopTenUsers[0].Score);
            Assert.AreEqual(30, result.TopTenUsers[2].Score);
        }

        [TestMethod]
        public async Task GetTopTen_ShouldReturnUsersInDescendingOrderByScore()
        {
            TopTenResponse topTenResponse = new TopTenResponse
            {
                TopTenUsers = new List<UserResponse>
                {
                    new UserResponse
                    {
                        Id = Guid.NewGuid(), Name = "User1", LastName = "Test", Email = "user1@test.com", Score = 100
                    },
                    new UserResponse
                    {
                        Id = Guid.NewGuid(), Name = "User2", LastName = "Test", Email = "user2@test.com", Score = 90
                    },
                    new UserResponse
                    {
                        Id = Guid.NewGuid(), Name = "User3", LastName = "Test", Email = "user3@test.com", Score = 80
                    }
                }
            };

            _mockUserLogic.Setup(x => x.GetTopTenUsers()).ReturnsAsync(topTenResponse);

            HttpResponseMessage response = await _adminClient.GetAsync("/api/strategy/topTen");

            response.EnsureSuccessStatusCode();

            string responseContent = await response.Content.ReadAsStringAsync();
            TopTenResponse? result = JsonSerializer.Deserialize<TopTenResponse>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.TopTenUsers);
            for (int i = 0; i < result.TopTenUsers.Count - 1; i++)
            {
                Assert.IsTrue(result.TopTenUsers[i].Score >= result.TopTenUsers[i + 1].Score);
            }
        }
    }
}