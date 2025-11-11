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
using Domain;
using Microsoft.Data.Sqlite;

namespace ApiTests
{
    [TestClass]
    public class AuthControllerTest
    {
        private WebApplicationFactory<Program> _factory = null!;
        private HttpClient _client = null!;
        private Mock<IAuthLogic> _mockAuthLogic = null!;
        private Mock<IUserLogic> _mockUserLogic = null!;
        private SqliteConnection _connection = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockAuthLogic = new Mock<IAuthLogic>();
            _mockUserLogic = new Mock<IUserLogic>();

            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

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

                        services.AddSingleton(_mockAuthLogic.Object);
                        services.AddSingleton(_mockUserLogic.Object);
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
        public async Task Login_ValidCredentials_ReturnsLoginResponse()
        {
            LoginRequest request = new LoginRequest
            {
                Email = "admin@test.com",
                Password = "password123"
            };

            UserResponse userResponse = new UserResponse
            {
                Id = Guid.NewGuid(),
                Name = "Admin",
                LastName = "User",
                Email = "admin@test.com",
                UserRoles = new List<string> { Domain.Role.ADMINISTRATOR }
            };

            _mockAuthLogic.Setup(x => x.Login(request.Email, request.Password))
                         .ReturnsAsync(userResponse);

            string json = JsonSerializer.Serialize(request);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync("/api/auth/login", content);

            response.EnsureSuccessStatusCode();
            string responseContent = await response.Content.ReadAsStringAsync();
            LoginResponse? loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.AreEqual("admin@test.com", loginResponse.Email);
            Assert.AreEqual(1, loginResponse.Roles.Length);
            Assert.AreEqual(Domain.Role.ADMINISTRATOR, loginResponse.Roles[0]);
        }

        [TestMethod]
        public async Task Register_ValidVisitor_ReturnsRegisterResponse()
        {
            RegisterVisitorRequest request = new RegisterVisitorRequest
            {
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "password123",
                BirthDate = new DateTime(1990, 1, 1)
            };
            RegisterResponse expectedResponse = new RegisterResponse
            {
                Id = Guid.NewGuid(),
                Email = "john@test.com",
                Message = "Registration successful"
            };

            UserResponse userResponse = new UserResponse
            {
                Id = expectedResponse.Id,
                Name = request.Name,
                LastName = request.LastName,
                Email = request.Email,
                BirthDate = request.BirthDate,
                UserRoles = new List<string>()
            };

            _mockUserLogic.Setup(x => x.RegisterVisitor(It.IsAny<RegisterVisitorRequest>()))
                         .ReturnsAsync(userResponse);

            string json = JsonSerializer.Serialize(request);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync("/api/auth/register", content);

            response.EnsureSuccessStatusCode();
            string responseContent = await response.Content.ReadAsStringAsync();
            RegisterResponse? registerResponse = JsonSerializer.Deserialize<RegisterResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.AreEqual(expectedResponse.Email, registerResponse.Email);
            Assert.AreEqual(expectedResponse.Message, registerResponse.Message);
        }

        [TestMethod]
        public async Task Login_InvalidCredentials_Returns400()
        {
            LoginRequest request = new LoginRequest
            {
                Email = "invalid@test.com",
                Password = "wrongpassword"
            };

            _mockAuthLogic.Setup(x => x.Login(request.Email, request.Password))
                         .ThrowsAsync(new ArgumentException("Invalid email or password."));

            string json = JsonSerializer.Serialize(request);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync("/api/auth/login", content);

            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task Login_WithUserHavingNoRoles_ReturnsEmptyRolesArray()
        {
            LoginRequest request = new LoginRequest
            {
                Email = "user@test.com",
                Password = "password123"
            };

            UserResponse userResponse = new UserResponse
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                LastName = "User",
                Email = "user@test.com",
                UserRoles = new List<string>()
            };

            _mockAuthLogic.Setup(x => x.Login(request.Email, request.Password))
                         .ReturnsAsync(userResponse);

            string json = JsonSerializer.Serialize(request);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync("/api/auth/login", content);

            response.EnsureSuccessStatusCode();
            string responseContent = await response.Content.ReadAsStringAsync();
            LoginResponse? loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.AreEqual(0, loginResponse.Roles.Length);
        }

        [TestMethod]
        public async Task Register_FailedRegistration_Returns400()
        {
            RegisterVisitorRequest request = new RegisterVisitorRequest
            {
                Name = "John",
                LastName = "Doe",
                Email = "existing@test.com",
                Password = "password123",
                BirthDate = new DateTime(1990, 1, 1)
            };

            _mockUserLogic.Setup(x => x.RegisterVisitor(It.IsAny<RegisterVisitorRequest>()))
                         .ThrowsAsync(new ArgumentException("Email already exists."));

            string json = JsonSerializer.Serialize(request);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync("/api/auth/register", content);

            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task Login_WithUserHavingNullRoles_ReturnsEmptyRolesArray()
        {
            LoginRequest request = new LoginRequest
            {
                Email = "user@test.com",
                Password = "password123"
            };

            UserResponse userResponse = new UserResponse
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                LastName = "User",
                Email = "user@test.com",
                UserRoles = null
            };

            _mockAuthLogic.Setup(x => x.Login(request.Email, request.Password))
                         .ReturnsAsync(userResponse);

            string json = JsonSerializer.Serialize(request);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync("/api/auth/login", content);

            response.EnsureSuccessStatusCode();
            string responseContent = await response.Content.ReadAsStringAsync();
            LoginResponse? loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.AreEqual(0, loginResponse.Roles.Length);
        }
    }
}