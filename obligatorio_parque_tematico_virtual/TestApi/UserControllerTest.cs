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
                UserId = userId,
                Role = Role.OPERATOR
            };

            _mockUserLogic.Setup(u => u.AddRoleToUser(userId, Role.OPERATOR))
                .Returns(Task.CompletedTask);

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _adminClient.PostAsync($"/api/users/{userId}/roles", content);

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
                UserId = userId,
                Role = Role.OPERATOR
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync($"/api/users/{userId}/roles", content);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            _mockUserLogic.Verify(u => u.AddRoleToUser(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }
    }
}