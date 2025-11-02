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
using Domain.Exceptions;

namespace ApiTests
{
    [TestClass]
    public class UserControllerTest
    {
        private WebApplicationFactory<Program> _factory = null!;
        private HttpClient _client = null!;
        private HttpClient _adminClient = null!;
        private HttpClient _operatorClient = null!;
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
                    ServiceDescriptor? descriptor =
                        services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddDbContext<AppDbContext>(options =>
                        options.UseSqlite(_connection));

                    services.AddSingleton(_mockUserLogic.Object);
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
            TokenLogic tokenService = new TokenLogic(jwtSettings);

            UserResponse adminUser = new UserResponse
            {
                Id = Guid.NewGuid(),
                Name = "Admin",
                LastName = "User",
                Email = "admin@example.com",
                UserRoles = new List<string> { Role.ADMINISTRATOR }
            };
            string adminToken = tokenService.GenerateToken(adminUser);

            _adminClient = _factory.CreateClient();
            _adminClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            UserResponse operatorUser = new UserResponse
            {
                Id = Guid.NewGuid(),
                Name = "Operator",
                LastName = "User",
                Email = "operator@example.com",
                UserRoles = new List<string> { Role.OPERATOR }
            };
            string operatorToken = tokenService.GenerateToken(operatorUser);
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
        public async Task AddRoleToUser_WithAdminRole_ReturnsOk()
        {
            Guid userId = Guid.NewGuid();
            AddRolesRequest request = new AddRolesRequest
            {
                Role = Role.OPERATOR
            };

            _mockUserLogic.Setup(u => u.AddRoleToUser(userId, Role.OPERATOR))
                .Returns(Task.CompletedTask);

            string json = JsonSerializer.Serialize(request);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _adminClient.PutAsync($"/api/users/{userId}/roles", content);

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

            string json = JsonSerializer.Serialize(request);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PutAsync($"/api/users/{userId}/roles", content);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            _mockUserLogic.Verify(u => u.AddRoleToUser(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task GetUserById_WithAdminRole_ReturnsOk()
        {
            Guid userId = Guid.NewGuid();
            UserResponse expected = new UserResponse
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

            HttpResponseMessage response = await _adminClient.GetAsync($"/api/users/{userId}");

            response.EnsureSuccessStatusCode();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            string json = await response.Content.ReadAsStringAsync();
            UserResponse? userResponse = JsonSerializer.Deserialize<UserResponse>(json, new JsonSerializerOptions
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

            HttpResponseMessage response = await _client.GetAsync($"/api/users/{userId}");

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            _mockUserLogic.Verify(u => u.GetUserResponseById(It.IsAny<Guid>()), Times.Never);
        }

        [TestMethod]
        public async Task CreateUser_WithAdminRole_ReturnsCreated()
        {
            CreateUserRequest request = new CreateUserRequest
            {
                Name = "Alice",
                LastName = "Smith",
                Email = "alice@example.com",
                Password = "Password#123",
                BirthDate = new DateTime(1995, 5, 5),
                MembershipLevel = null,
                Roles = new List<string> { Role.OPERATOR }
            };

            UserResponse created = new UserResponse
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
            StringContent content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _adminClient.PostAsync("/api/users", content);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            StringAssert.Contains(response.Headers.Location.ToString(), $"/api/users/{created.Id}");

            string json = await response.Content.ReadAsStringAsync();
            UserResponse? userResponse = JsonSerializer.Deserialize<UserResponse>(json, new JsonSerializerOptions
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
            CreateUserRequest request = new CreateUserRequest
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
            StringContent content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync("/api/users", content);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            _mockUserLogic.Verify(u => u.CreateUser(It.IsAny<CreateUserRequest>()), Times.Never);
        }

        [TestMethod]
        public async Task ModifyUser_WithAuthenticatedUser_ReturnsOk()
        {
            Microsoft.Extensions.Options.IOptions<Models.JwtSettings> jwtSettings =
                Microsoft.Extensions.Options.Options.Create(new Models.JwtSettings
                {
                    SecretKey = "MySecretKeyForJWTTokenGeneration1234567890",
                    Issuer = "ParqueTematico",
                    Audience = "ParqueTematico",
                    ExpirationHours = 1
                });
            TokenLogic tokenService = new TokenLogic(jwtSettings);

            Guid userId = Guid.NewGuid();
            UserResponse user = new UserResponse
            {
                Id = userId,
                Name = "John",
                LastName = "Doe",
                Email = "john@example.com",
                UserRoles = new List<string>()
            };
            string token = tokenService.GenerateToken(user);
            HttpClient authedClient = _factory.CreateClient();
            authedClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            ModifyUserRequest request = new ModifyUserRequest
            {
                Name = "New",
                LastName = "Surname",
                Email = "new@example.com",
                Password = "New#Pass1",
                BirthDate = new DateTime(1992, 2, 2)
            };

            UserResponse expected = new UserResponse
            {
                Id = userId,
                Name = request.Name,
                LastName = request.LastName,
                Email = request.Email,
                BirthDate = request.BirthDate,
                MembershipLevel = null,
                UserRoles = new List<string> { Role.VISITOR },
                Score = 5
            };

            _mockUserLogic
                .Setup(l => l.ModifyUser(
                    It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<ModifyUserRequest>()))
                .ReturnsAsync(expected);

            string json = JsonSerializer.Serialize(request);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await authedClient.PutAsync($"/api/users/{userId}", content);

            response.EnsureSuccessStatusCode();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            string body = await response.Content.ReadAsStringAsync();
            UserResponse? parsed = JsonSerializer.Deserialize<UserResponse>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.AreEqual(expected.Id, parsed.Id);
            Assert.AreEqual(expected.Email, parsed.Email);
            _mockUserLogic.Verify(
                l => l.ModifyUser(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<ModifyUserRequest>()), Times.Once);
        }

        [TestMethod]
        public async Task ModifyUser_WithoutAuthentication_ReturnsUnauthorized()
        {
            Guid userId = Guid.NewGuid();
            ModifyUserRequest request = new ModifyUserRequest
            {
                Name = "A",
                LastName = "B",
                Email = "a@b.com",
                Password = "p"
            };
            string json = JsonSerializer.Serialize(request);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PutAsync($"/api/users/{userId}", content);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            _mockUserLogic.Verify(
                l => l.ModifyUser(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<ModifyUserRequest>()), Times.Never);
        }

        [TestMethod]
        public async Task ModifyUser_WithDifferentUserToken_ReturnsForbidden()
        {
            Microsoft.Extensions.Options.IOptions<Models.JwtSettings> jwtSettings =
                Microsoft.Extensions.Options.Options.Create(new Models.JwtSettings
                {
                    SecretKey = "MySecretKeyForJWTTokenGeneration1234567890",
                    Issuer = "ParqueTematico",
                    Audience = "ParqueTematico",
                    ExpirationHours = 1
                });
            TokenLogic tokenService = new TokenLogic(jwtSettings);

            Guid routeUserId = Guid.NewGuid();
            UserResponse tokenUser = new UserResponse
            {
                Id = Guid.NewGuid(),
                Name = "Other",
                LastName = "User",
                Email = "o@u.com",
                UserRoles = new List<string>()
            };
            string token = tokenService.GenerateToken(tokenUser);
            HttpClient authedClient = _factory.CreateClient();
            authedClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            ModifyUserRequest request = new ModifyUserRequest
            {
                Name = "A",
                LastName = "B",
                Email = "a@b.com",
                Password = "p"
            };

            _mockUserLogic
                .Setup(l => l.ModifyUser(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<ModifyUserRequest>()))
                .ThrowsAsync(new ForbiddenException("You cannot modify another user"));

            string json = JsonSerializer.Serialize(request);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await authedClient.PutAsync($"/api/users/{routeUserId}", content);

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            _mockUserLogic.Verify(
                l => l.ModifyUser(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<ModifyUserRequest>()), Times.Once);
        }

        [TestMethod]
        public async Task ModifyUser_WhenLogicThrowsUnauthorized_Returns401()
        {
            Microsoft.Extensions.Options.IOptions<Models.JwtSettings> jwtSettings =
                Microsoft.Extensions.Options.Options.Create(new Models.JwtSettings
                {
                    SecretKey = "MySecretKeyForJWTTokenGeneration1234567890",
                    Issuer = "ParqueTematico",
                    Audience = "ParqueTematico",
                    ExpirationHours = 1
                });
            TokenLogic tokenService = new TokenLogic(jwtSettings);

            Guid userId = Guid.NewGuid();
            UserResponse user = new UserResponse
            {
                Id = userId,
                Name = "John",
                LastName = "Doe",
                Email = "john@example.com",
                UserRoles = new List<string>()
            };
            string token = tokenService.GenerateToken(user);
            HttpClient authedClient = _factory.CreateClient();
            authedClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            ModifyUserRequest request = new ModifyUserRequest
            {
                Name = "A",
                LastName = "B",
                Email = "a@b.com",
                Password = "p"
            };

            _mockUserLogic
                .Setup(l => l.ModifyUser(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<ModifyUserRequest>()))
                .ThrowsAsync(new UnauthorizedException("Invalid token"));

            string json = JsonSerializer.Serialize(request);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await authedClient.PutAsync($"/api/users/{userId}", content);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task ModifyUser_WhenLogicThrowsNotFound_Returns404()
        {
            Microsoft.Extensions.Options.IOptions<Models.JwtSettings> jwtSettings =
                Microsoft.Extensions.Options.Options.Create(new Models.JwtSettings
                {
                    SecretKey = "MySecretKeyForJWTTokenGeneration1234567890",
                    Issuer = "ParqueTematico",
                    Audience = "ParqueTematico",
                    ExpirationHours = 1
                });
            TokenLogic tokenService = new TokenLogic(jwtSettings);

            Guid userId = Guid.NewGuid();
            UserResponse user = new UserResponse
            {
                Id = userId,
                Name = "John",
                LastName = "Doe",
                Email = "john@example.com",
                UserRoles = new List<string>()
            };
            string token = tokenService.GenerateToken(user);
            HttpClient authedClient = _factory.CreateClient();
            authedClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            ModifyUserRequest request = new ModifyUserRequest
            {
                Name = "A",
                LastName = "B",
                Email = "a@b.com",
                Password = "p"
            };

            _mockUserLogic
                .Setup(l => l.ModifyUser(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<ModifyUserRequest>()))
                .ThrowsAsync(new KeyNotFoundException("User not found"));

            string json = JsonSerializer.Serialize(request);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await authedClient.PutAsync($"/api/users/{userId}", content);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task ModifyUser_WhenLogicThrowsArgument_Returns400()
        {
            Microsoft.Extensions.Options.IOptions<Models.JwtSettings> jwtSettings =
                Microsoft.Extensions.Options.Options.Create(new Models.JwtSettings
                {
                    SecretKey = "MySecretKeyForJWTTokenGeneration1234567890",
                    Issuer = "ParqueTematico",
                    Audience = "ParqueTematico",
                    ExpirationHours = 1
                });
            TokenLogic tokenService = new TokenLogic(jwtSettings);

            Guid userId = Guid.NewGuid();
            UserResponse user = new UserResponse
            {
                Id = userId,
                Name = "John",
                LastName = "Doe",
                Email = "john@example.com",
                UserRoles = new List<string>()
            };
            string token = tokenService.GenerateToken(user);
            HttpClient authedClient = _factory.CreateClient();
            authedClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            ModifyUserRequest request = new ModifyUserRequest
            {
                Name = "",
                LastName = "B",
                Email = "a@b.com",
                Password = "p"
            };

            _mockUserLogic
                .Setup(l => l.ModifyUser(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<ModifyUserRequest>()))
                .ThrowsAsync(new ArgumentException("Name cannot be empty"));

            string json = JsonSerializer.Serialize(request);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await authedClient.PutAsync($"/api/users/{userId}", content);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task ModifyUser_WithTokenWithoutSubClaim_PassesNullToLogic()
        {
            var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("MySecretKeyForJWTTokenGeneration1234567890"));
            var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                securityKey, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email,
                    "test@example.com"),
                new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString())
            };

            var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
                issuer: "ParqueTematico",
                audience: "ParqueTematico",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            string tokenString = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);

            HttpClient authedClient = _factory.CreateClient();
            authedClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenString);

            Guid userId = Guid.NewGuid();
            ModifyUserRequest request = new ModifyUserRequest
            {
                Name = "A",
                LastName = "B",
                Email = "a@b.com",
                Password = "p"
            };

            _mockUserLogic
                .Setup(l => l.ModifyUser(It.IsAny<Guid>(), null, It.IsAny<ModifyUserRequest>()))
                .ThrowsAsync(new UnauthorizedException("Invalid token: missing user identifier"));

            string json = JsonSerializer.Serialize(request);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await authedClient.PutAsync($"/api/users/{userId}", content);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            _mockUserLogic.Verify(l => l.ModifyUser(It.IsAny<Guid>(), null, It.IsAny<ModifyUserRequest>()), Times.Once);
        }

        [TestMethod]
        public async Task AddRoleToUser_OperatorRole_ReturnsForbidden()
        {
            Guid userId = Guid.NewGuid();
            AddRolesRequest request = new AddRolesRequest { Role = "Administrator" };

            string json = JsonSerializer.Serialize(request);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _operatorClient.PutAsync($"/api/users/{userId}/roles", content);

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}