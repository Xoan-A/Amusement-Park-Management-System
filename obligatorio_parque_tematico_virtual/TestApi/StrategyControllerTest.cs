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
                        var descriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                        if (descriptor != null) services.Remove(descriptor);

                        services.AddDbContext<AppDbContext>(options =>
                            options.UseSqlite(_connection));

                        services.AddSingleton(_mockActiveStrategy.Object);
                        services.AddSingleton(_mockUserLogic.Object);
                    });
                });

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                context.Database.EnsureCreated();
            }

            _client = _factory.CreateClient();

            var jwtSettings = Microsoft.Extensions.Options.Options.Create(new Models.JwtSettings
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
            var mockStrategy = new Mock<IConcreteStrategy>();
            mockStrategy.Setup(s => s.Name).Returns("PerAttraction");

            _mockActiveStrategy.Setup(x => x.GetStrategy())
                .ReturnsAsync(mockStrategy.Object);

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/strategy");
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
            _mockActiveStrategy.Setup(x => x.GetStrategy())
                .Throws(new InvalidOperationException("Strategy not set"));

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/strategy");
            var response = await _adminClient.SendAsync(requestMessage);

            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [TestMethod]
        public async Task GetStrategy_WithoutAuth_ShouldReturnUnauthorized()
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/strategy");
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

            var response = await _adminClient.PutAsync("/api/strategy", content);

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var messageResponse = JsonSerializer.Deserialize<MessageResponse>(
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
            var request = new SetStrategyRequest
            {
                StrategyName = "PerEvent"
            };

            _mockActiveStrategy.Setup(x => x.SetStrategy(It.IsAny<SetStrategyRequest>()));

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _adminClient.PutAsync("/api/strategy", content);

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var messageResponse = JsonSerializer.Deserialize<MessageResponse>(
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
            var request = new SetStrategyRequest
            {
                StrategyName = "Combo",
                N = 30
            };

            _mockActiveStrategy.Setup(x => x.SetStrategy(It.IsAny<SetStrategyRequest>()));

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _adminClient.PutAsync("/api/strategy", content);

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var messageResponse = JsonSerializer.Deserialize<MessageResponse>(
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
            var request = new SetStrategyRequest
            {
                StrategyName = "Combo",
                N = null
            };

            _mockActiveStrategy.Setup(x => x.SetStrategy(It.IsAny<SetStrategyRequest>()))
                .Throws(new ArgumentException("N is required for Combo strategy"));

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _adminClient.PutAsync("/api/strategy", content);

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

            var response = await _adminClient.PutAsync("/api/strategy", content);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task SetStrategy_WithNullRequest_ShouldReturnBadRequest()
        {
            var json = "null";
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _adminClient.PutAsync("/api/strategy", content);

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

            var response = await _client.PutAsync("/api/strategy", content);

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

            var mockStrategy = new Mock<IConcreteStrategy>();
            mockStrategy.Setup(s => s.Name).Returns("Combo");

            _mockActiveStrategy.Setup(x => x.SetStrategy(It.IsAny<SetStrategyRequest>()));
            _mockActiveStrategy.Setup(x => x.GetStrategy()).ReturnsAsync(mockStrategy.Object);

            var json = JsonSerializer.Serialize(setRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var setResponse = await _adminClient.PutAsync("/api/strategy", content);
            setResponse.EnsureSuccessStatusCode();

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/strategy");
            var getResponse = await _adminClient.SendAsync(requestMessage);
            getResponse.EnsureSuccessStatusCode();

            var responseContent = await getResponse.Content.ReadAsStringAsync();
            var strategyResponse = JsonSerializer.Deserialize<StrategyResponse>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.IsNotNull(strategyResponse);
            Assert.AreEqual("Combo", strategyResponse.Name);
        }

        [TestMethod]
        public async Task GetTopTen_ShouldReturnTopTenUsersOrderedByScore()
        {
            var topTenResponse = new TopTenResponse
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

            var response = await _adminClient.GetAsync("/api/strategy/topTen");

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TopTenResponse>(
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
            var response = await _client.GetAsync("/api/strategy/topTen");

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task GetTopTen_WithNoUsers_ShouldReturnEmptyList()
        {
            var topTenResponse = new TopTenResponse
            {
                TopTenUsers = new List<UserResponse>()
            };

            _mockUserLogic.Setup(x => x.GetTopTenUsers()).ReturnsAsync(topTenResponse);

            var response = await _adminClient.GetAsync("/api/strategy/topTen");

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TopTenResponse>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.TopTenUsers);
            Assert.AreEqual(0, result.TopTenUsers.Count);
        }

        [TestMethod]
        public async Task GetTopTen_WithFewerThanTenUsers_ShouldReturnAllUsers()
        {
            var topTenResponse = new TopTenResponse
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

            var response = await _adminClient.GetAsync("/api/strategy/topTen");

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TopTenResponse>(
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
            var topTenResponse = new TopTenResponse
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

            var response = await _adminClient.GetAsync("/api/strategy/topTen");

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TopTenResponse>(
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