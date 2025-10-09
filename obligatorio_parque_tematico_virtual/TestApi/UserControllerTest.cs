using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Text;
using System.Text.Json;
using IBusinessLogic;
using Models.In;
using Models.Out;
using Microsoft.EntityFrameworkCore;
using DataAccess.Context;
using Microsoft.Data.Sqlite;
using Domain;
using BusinessLogic;

namespace ApiTests
{
    [TestClass]
    public class UserControllerTest
    {
        private WebApplicationFactory<Program> _factory = null!;
        private HttpClient _client = null!;
        private HttpClient _adminClient = null!;
        private Mock<IUserLogic> _mockUserLogic = null!;
        private SqliteConnection _connection = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockUserLogic = new Mock<IUserLogic>();

            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddDbContext<AppDbContext>(options =>
                        options.UseSqlite(_connection));

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
            var tokenService = new TokenLogic(jwtSettings);

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
            _connection?.Close();
            _connection?.Dispose();
        }

        [TestMethod]
        public async Task AddRoleToUser_WithAdminRole_ReturnsOk()
        {
            Guid userId = Guid.NewGuid();
            AddRolesRequest request = new AddRolesRequest
            {
                Role = Role.OPERATOR
            };

            _mockUserLogic.Setup(u => u.AddRoleToUser(userId, Role.OPERATOR))
                .Returns(Task.CompletedTask);

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _adminClient.PutAsync($"/api/users/{userId}/roles", content);

            response.EnsureSuccessStatusCode();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();
            MessageResponse messageResponse = JsonSerializer.Deserialize<MessageResponse>(responseContent,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            Assert.IsTrue(messageResponse.Message.Contains(Role.OPERATOR));

            _mockUserLogic.Verify(u => u.AddRoleToUser(userId, Role.OPERATOR), Times.Once);
        }

        [TestMethod]
        public async Task AddRoleToUser_WithoutAuthentication_ReturnsUnauthorized()
        {
            Guid userId = Guid.NewGuid();
            AddRolesRequest request = new AddRolesRequest
            {
                Role = Role.OPERATOR
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PutAsync($"/api/users/{userId}/roles", content);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            _mockUserLogic.Verify(u => u.AddRoleToUser(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task GetUserById_WithAdminRole_ReturnsOk()
        {
            Guid userId = Guid.NewGuid();
            var expected = new UserResponse
            {
                Id = userId,
                Name = "John",
                LastName = "Doe",
                Email = "john@example.com",
                BirthDate = new DateTime(1990, 1, 1),
                MembershipLevel = null,
                UserRoles = new List<string> { Role.VISITOR },
                Score = 0
            };

            _mockUserLogic.Setup(u => u.GetUserResponseById(userId)).ReturnsAsync(expected);

            var response = await _adminClient.GetAsync($"/api/users/{userId}");

            response.EnsureSuccessStatusCode();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            string json = await response.Content.ReadAsStringAsync();
            var userResponse = JsonSerializer.Deserialize<UserResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(userResponse);
            Assert.AreEqual(expected.Id, userResponse.Id);
            Assert.AreEqual(expected.Email, userResponse.Email);
            Assert.AreEqual(expected.Name, userResponse.Name);
            Assert.AreEqual(expected.LastName, userResponse.LastName);
            Assert.IsNotNull(userResponse.UserRoles);
            CollectionAssert.AreEquivalent(expected.UserRoles.ToList(), userResponse.UserRoles.ToList());

            _mockUserLogic.Verify(u => u.GetUserResponseById(userId), Times.Once);
        }

        [TestMethod]
        public async Task GetUserById_WithoutAuthentication_ReturnsUnauthorized()
        {
            Guid userId = Guid.NewGuid();

            var response = await _client.GetAsync($"/api/users/{userId}");

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            _mockUserLogic.Verify(u => u.GetUserResponseById(It.IsAny<Guid>()), Times.Never);
        }

        [TestMethod]
        public async Task CreateUser_WithAdminRole_ReturnsCreated()
        {
            var request = new CreateUserRequest
            {
                Name = "Alice",
                LastName = "Smith",
                Email = "alice@example.com",
                Password = "Password#123",
                BirthDate = new DateTime(1995, 5, 5),
                MembershipLevel = null,
                Roles = new List<string> { Role.OPERATOR }
            };

            var created = new UserResponse
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                LastName = request.LastName,
                Email = request.Email,
                BirthDate = request.BirthDate,
                MembershipLevel = null,
                UserRoles = request.Roles,
                Score = 0
            };

            _mockUserLogic
                .Setup(u => u.CreateUser(It.Is<CreateUserRequest>(r =>
                    r.Name == request.Name &&
                    r.LastName == request.LastName &&
                    r.Email == request.Email)))
                .ReturnsAsync(created);

            string jsonBody = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var response = await _adminClient.PostAsync("/api/users", content);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            StringAssert.Contains(response.Headers.Location.ToString(), $"/api/users/{created.Id}");

            string json = await response.Content.ReadAsStringAsync();
            var userResponse = JsonSerializer.Deserialize<UserResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.AreEqual(created.Id, userResponse.Id);
            Assert.AreEqual(created.Email, userResponse.Email);
            CollectionAssert.AreEquivalent(created.UserRoles.ToList(), userResponse.UserRoles.ToList());

            _mockUserLogic.Verify(u => u.CreateUser(It.IsAny<CreateUserRequest>()), Times.Once);
        }

        [TestMethod]
        public async Task CreateUser_WithoutAuthentication_ReturnsUnauthorized()
        {
            var request = new CreateUserRequest
            {
                Name = "Alice",
                LastName = "Smith",
                Email = "alice@example.com",
                Password = "Password#123",
                BirthDate = new DateTime(1995, 5, 5),
                MembershipLevel = null,
                Roles = new List<string> { Role.OPERATOR }
            };

            string jsonBody = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/users", content);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            _mockUserLogic.Verify(u => u.CreateUser(It.IsAny<CreateUserRequest>()), Times.Never);
        }
    }
}