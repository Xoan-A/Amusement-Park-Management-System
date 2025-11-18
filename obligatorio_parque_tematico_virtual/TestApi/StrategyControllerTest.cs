using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Text;
using System.Text.Json;
using IBusinessLogic;
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
        private HttpClient _operatorClient = null!;
        private Mock<IActiveStrategy> _mockActiveStrategy = null!;
        private Mock<IUserManagementLogic> _mockUserManagementLogic = null!;

        [TestInitialize]
        public void Setup()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            _mockActiveStrategy = new Mock<IActiveStrategy>();
            _mockUserManagementLogic = new Mock<IUserManagementLogic>();

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
                    services.AddSingleton(_mockUserManagementLogic.Object);
                });
            });

            using (IServiceScope scope = _factory.Services.CreateScope())
            {
                AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                context.Database.EnsureCreated();
            }

            _client = _factory.CreateClient();

            Microsoft.Extensions.Options.IOptions<Models.JwtSettings> jwtSettings =
            Microsoft.Extensions.Options.Options.Create(new Models.JwtSettings
            {
                SecretKey = "MySecretKeyForJWTTokenGeneration1234567890",
                Issuer = "ParqueTematico",
                Audience = "ParqueTematico",
                ExpirationHours = 1
            });

            TokenLogic tokenLogic = new TokenLogic(jwtSettings);

            UserResponse adminUser = new UserResponse
            {
                Id = Guid.NewGuid(),
                Name = "Admin",
                LastName = "User",
                Email = "admin@example.com",
                UserRoles = new List<string> { Role.Administrator }
            };
            string adminToken = tokenLogic.GenerateToken(adminUser);
            _adminClient = _factory.CreateClient();
            _adminClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            UserResponse operatorUser = new UserResponse
            {
                Id = Guid.NewGuid(),
                Name = "Operator",
                LastName = "User",
                Email = "operator@example.com",
                UserRoles = new List<string> { Role.Operator }
            };
            string operatorToken = tokenLogic.GenerateToken(operatorUser);
            _operatorClient = _factory.CreateClient();
            _operatorClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", operatorToken);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _client?.Dispose();
            _adminClient?.Dispose();
            _operatorClient?.Dispose();
            _factory?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }

        [TestMethod]
        public void GetStrategy_ShouldReturnCurrentStrategy()
        {
            Mock<IConcreteStrategy> mockStrategy = new Mock<IConcreteStrategy>();
            mockStrategy.Setup(s => s.Name).Returns("PerAttraction");

            _mockActiveStrategy.Setup(x => x.GetStrategy())
            .Returns(mockStrategy.Object);

            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/strategy");
            HttpResponseMessage response = _adminClient.SendAsync(requestMessage).Result;

            response.EnsureSuccessStatusCode();

            string responseContent = response.Content.ReadAsStringAsync().Result;
            StrategyResponse? strategyResponse = JsonSerializer.Deserialize<StrategyResponse>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.AreEqual("PerAttraction", strategyResponse.Name);
        }

        [TestMethod]
        public void GetStrategy_WhenNoStrategySet_ShouldReturnBadRequest()
        {
            _mockActiveStrategy.Setup(x => x.GetStrategy())
            .Throws(new InvalidOperationException("Strategy not set"));

            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/strategy");
            HttpResponseMessage response = _adminClient.SendAsync(requestMessage).Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public void GetStrategy_WithoutAuth_ShouldReturnUnauthorized()
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/strategy");
            HttpResponseMessage response = _client.SendAsync(requestMessage).Result;

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public void SetStrategy_WithPerAttraction_ShouldReturnSuccess()
        {
            SetStrategyRequest request = new SetStrategyRequest
            {
                StrategyName = "PerAttraction"
            };

            _mockActiveStrategy.Setup(x => x.SetStrategy(It.IsAny<SetStrategyRequest>()));

            string json = JsonSerializer.Serialize(request);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = _ = _adminClient.PutAsync("/api/strategy", content).Result;

            response.EnsureSuccessStatusCode();

            string responseContent = response.Content.ReadAsStringAsync().Result;
            MessageResponse? messageResponse = JsonSerializer.Deserialize<MessageResponse>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.AreEqual("Strategy set successfully", messageResponse.Message);

            _mockActiveStrategy.Verify(x => x.SetStrategy(It.Is<SetStrategyRequest>(
                r => r.StrategyName == "PerAttraction")), Times.Once);
        }

        [TestMethod]
        public void SetStrategy_WithPerEvent_ShouldReturnSuccess()
        {
            SetStrategyRequest request = new SetStrategyRequest
            {
                StrategyName = "PerEvent"
            };

            _mockActiveStrategy.Setup(x => x.SetStrategy(It.IsAny<SetStrategyRequest>()));

            string json = JsonSerializer.Serialize(request);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = _ = _adminClient.PutAsync("/api/strategy", content).Result;

            response.EnsureSuccessStatusCode();

            string responseContent = response.Content.ReadAsStringAsync().Result;
            MessageResponse? messageResponse = JsonSerializer.Deserialize<MessageResponse>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.AreEqual("Strategy set successfully", messageResponse.Message);

            _mockActiveStrategy.Verify(x => x.SetStrategy(It.Is<SetStrategyRequest>(
                r => r.StrategyName == "PerEvent")), Times.Once);
        }

        [TestMethod]
        public void SetStrategy_WithCombo_ShouldReturnSuccess()
        {
            SetStrategyRequest request = new SetStrategyRequest
            {
                StrategyName = "Combo",
                N = 30
            };

            _mockActiveStrategy.Setup(x => x.SetStrategy(It.IsAny<SetStrategyRequest>()));

            string json = JsonSerializer.Serialize(request);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = _ = _adminClient.PutAsync("/api/strategy", content).Result;

            response.EnsureSuccessStatusCode();

            string responseContent = response.Content.ReadAsStringAsync().Result;
            MessageResponse? messageResponse = JsonSerializer.Deserialize<MessageResponse>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.AreEqual("Strategy set successfully", messageResponse.Message);

            _mockActiveStrategy.Verify(x => x.SetStrategy(It.Is<SetStrategyRequest>(
                r => r.StrategyName == "Combo" && r.N == 30)), Times.Once);
        }

        [TestMethod]
        public void SetStrategy_WithComboWithoutN_ShouldReturnBadRequest()
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

            HttpResponseMessage response = _ = _adminClient.PutAsync("/api/strategy", content).Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public void SetStrategy_WithInvalidStrategyName_ShouldReturnBadRequest()
        {
            SetStrategyRequest request = new SetStrategyRequest
            {
                StrategyName = "InvalidStrategy"
            };

            _mockActiveStrategy.Setup(x => x.SetStrategy(It.IsAny<SetStrategyRequest>()))
            .Throws(new ArgumentException("Invalid strategy name: InvalidStrategy"));

            string json = JsonSerializer.Serialize(request);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = _ = _adminClient.PutAsync("/api/strategy", content).Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public void SetStrategy_WithNullRequest_ShouldReturnBadRequest()
        {
            string json = "null";
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = _ = _adminClient.PutAsync("/api/strategy", content).Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public void SetStrategy_WithoutAuth_ShouldReturnUnauthorized()
        {
            SetStrategyRequest request = new SetStrategyRequest
            {
                StrategyName = "PerAttraction"
            };

            string json = JsonSerializer.Serialize(request);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = _ = _client.PutAsync("/api/strategy", content).Result;

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public void GetStrategy_AfterSettingStrategy_ShouldReturnNewStrategy()
        {
            SetStrategyRequest setRequest = new SetStrategyRequest
            {
                StrategyName = "Combo",
                N = 45
            };

            Mock<IConcreteStrategy> mockStrategy = new Mock<IConcreteStrategy>();
            mockStrategy.Setup(s => s.Name).Returns("Combo");

            _mockActiveStrategy.Setup(x => x.SetStrategy(It.IsAny<SetStrategyRequest>()));
            _mockActiveStrategy.Setup(x => x.GetStrategy()).Returns(mockStrategy.Object);

            string json = JsonSerializer.Serialize(setRequest);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage setResponse = _ = _adminClient.PutAsync("/api/strategy", content).Result;
            setResponse.EnsureSuccessStatusCode();

            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/strategy");
            HttpResponseMessage getResponse = _adminClient.SendAsync(requestMessage).Result;
            getResponse.EnsureSuccessStatusCode();

            string responseContent = getResponse.Content.ReadAsStringAsync().Result;
            StrategyResponse? strategyResponse = JsonSerializer.Deserialize<StrategyResponse>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.AreEqual("Combo", strategyResponse.Name);
        }

        [TestMethod]
        public void GetTopTen_ShouldReturnTopTenUsersOrderedByScore()
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

            _mockUserManagementLogic.Setup(x => x.GetTopTenUsers()).Returns(topTenResponse);

            HttpResponseMessage response = _ = _adminClient.GetAsync("/api/strategy/topTen").Result;

            response.EnsureSuccessStatusCode();

            string responseContent = response.Content.ReadAsStringAsync().Result;
            TopTenResponse? result = JsonSerializer.Deserialize<TopTenResponse>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.AreEqual(10, result.TopTenUsers.Count);
            Assert.AreEqual(100, result.TopTenUsers[0].Score);
            Assert.AreEqual(10, result.TopTenUsers[9].Score);
            _mockUserManagementLogic.Verify(x => x.GetTopTenUsers(), Times.Once);
        }

        [TestMethod]
        public void GetTopTen_WithoutAuth_ShouldReturnUnauthorized()
        {
            HttpResponseMessage response = _ = _client.GetAsync("/api/strategy/topTen").Result;

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public void GetTopTen_WithNoUsers_ShouldReturnEmptyList()
        {
            TopTenResponse topTenResponse = new TopTenResponse
            {
                TopTenUsers = new List<UserResponse>()
            };

            _mockUserManagementLogic.Setup(x => x.GetTopTenUsers()).Returns(topTenResponse);

            HttpResponseMessage response = _ = _adminClient.GetAsync("/api/strategy/topTen").Result;

            response.EnsureSuccessStatusCode();

            string responseContent = response.Content.ReadAsStringAsync().Result;
            TopTenResponse? result = JsonSerializer.Deserialize<TopTenResponse>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.AreEqual(0, result.TopTenUsers.Count);
        }

        [TestMethod]
        public void GetTopTen_WithFewerThanTenUsers_ShouldReturnAllUsers()
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

            _mockUserManagementLogic.Setup(x => x.GetTopTenUsers()).Returns(topTenResponse);

            HttpResponseMessage response = _ = _adminClient.GetAsync("/api/strategy/topTen").Result;

            response.EnsureSuccessStatusCode();

            string responseContent = response.Content.ReadAsStringAsync().Result;
            TopTenResponse? result = JsonSerializer.Deserialize<TopTenResponse>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.AreEqual(3, result.TopTenUsers.Count);
            Assert.AreEqual(50, result.TopTenUsers[0].Score);
            Assert.AreEqual(30, result.TopTenUsers[2].Score);
        }

        [TestMethod]
        public void GetTopTen_ShouldReturnUsersInDescendingOrderByScore()
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

            _mockUserManagementLogic.Setup(x => x.GetTopTenUsers()).Returns(topTenResponse);

            HttpResponseMessage response = _ = _adminClient.GetAsync("/api/strategy/topTen").Result;

            response.EnsureSuccessStatusCode();

            string responseContent = response.Content.ReadAsStringAsync().Result;
            TopTenResponse? result = JsonSerializer.Deserialize<TopTenResponse>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            for (int i = 0; i < result.TopTenUsers.Count - 1; i++)
            {
                Assert.IsTrue(result.TopTenUsers[i].Score >= result.TopTenUsers[i + 1].Score);
            }
        }

        [TestMethod]
        public void SetStrategy_OperatorRole_ReturnsForbidden()
        {
            SetStrategyRequest request = new SetStrategyRequest { StrategyName = "PerVisitor", N = 5 };
            string json = JsonSerializer.Serialize(request);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = _ = _operatorClient.PutAsync("/api/strategy", content).Result;

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}