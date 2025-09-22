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
    public class AuthControllerTest
    {
        private WebApplicationFactory<Program> _factory;
        private HttpClient _client;
        private Mock<IAuthLogic> _mockAuthLogic;
        private Mock<IUserLogic> _mockUserLogic;

        [TestInitialize]
        public void Setup()
        {
            _mockAuthLogic = new Mock<IAuthLogic>();
            _mockUserLogic = new Mock<IUserLogic>();

            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        services.AddSingleton(_mockAuthLogic.Object);
                        services.AddSingleton(_mockUserLogic.Object);
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
        public async Task Login_ValidCredentials_ReturnsLoginResponse()
        {
            var request = new LoginRequest
            {
                Email = "admin@test.com",
                Password = "password123"
            };
            var expectedResponse = new LoginResponse
            {
                Token = "valid_token",
                Email = "admin@test.com",
                Role = "Administrator",
                Name = "Admin User"
            };

            _mockAuthLogic.Setup(x => x.Login(request.Email, request.Password))
                         .Returns("valid_token");

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/auth/login", content);

            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.AreEqual(expectedResponse.Token, loginResponse.Token);
            Assert.AreEqual(expectedResponse.Email, loginResponse.Email);
            Assert.AreEqual(expectedResponse.Role, loginResponse.Role);
            Assert.AreEqual(expectedResponse.Name, loginResponse.Name);
        }

        [TestMethod]
        public async Task Register_ValidVisitor_ReturnsRegisterResponse()
        {
            var request = new RegisterVisitorRequest
            {
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "password123",
                BirthDate = new DateTime(1990, 1, 1)
            };
            var expectedResponse = new RegisterResponse
            {
                Id = Guid.NewGuid(),
                Email = "john@test.com",
                Message = "Registration successful"
            };

            var visitor = new Domain.Visitor
            {
                Id = expectedResponse.Id,
                Name = request.Name,
                LastName = request.LastName,
                Email = request.Email,
                Password = request.Password,
                BirthDate = request.BirthDate
            };

            _mockUserLogic.Setup(x => x.RegisterVisitor(request.Name, request.LastName, request.Email, request.Password, request.BirthDate))
                         .Returns(visitor);

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/auth/register", content);

            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var registerResponse = JsonSerializer.Deserialize<RegisterResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.AreEqual(expectedResponse.Email, registerResponse.Email);
            Assert.AreEqual(expectedResponse.Message, registerResponse.Message);
        }
    }
}