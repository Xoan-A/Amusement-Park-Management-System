using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Domain;
using BusinessLogic.Plugins;
using Microsoft.EntityFrameworkCore;
using DataAccess.Context;
using Microsoft.Data.Sqlite;
using Models.Out;
using System.Net.Http.Json;

namespace ApiTests;

[TestClass]
public class PluginControllerTest
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _adminClient = null!;
    private Mock<PluginLoader> _mockPluginLoader = null!;
    private SqliteConnection _connection = null!;

    [TestInitialize]
    public void Setup()
    {
        string testPluginsPath = Path.Combine(Path.GetTempPath(), "TestPlugins");
        _mockPluginLoader = new Mock<PluginLoader>(testPluginsPath);

        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ServiceDescriptor? descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlite(_connection));

                services.AddSingleton(_mockPluginLoader.Object);
            });
        });

        using (IServiceScope scope = _factory.Services.CreateScope())
        {
            AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.EnsureCreated();
        }

        Microsoft.Extensions.Options.IOptions<Models.JwtSettings> jwtSettings = Microsoft.Extensions.Options.Options.Create(new Models.JwtSettings
        {
            SecretKey = "MySecretKeyForJWTTokenGeneration1234567890",
            Issuer = "ParqueTematico",
            Audience = "ParqueTematico",
            ExpirationHours = 1
        });
        BusinessLogic.TokenLogic tokenService = new BusinessLogic.TokenLogic(jwtSettings);

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
    }

    [TestCleanup]
    public void Cleanup()
    {
        _adminClient?.Dispose();
        _factory?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }

    [TestMethod]
    public async Task GetAvailablePlugins_AsAdministrator_ReturnsOk()
    {
        // Arrange
        var plugins = new List<PluginInfo>
        {
            new PluginInfo
            {
                Name = "PuntuacionPorHora",
                Description = "Test plugin",
                Author = "Test Author",
                Version = "1.0.0"
            }
        };

        _mockPluginLoader.Setup(p => p.LoadPlugins()).Returns(plugins);

        // Act
        var response = await _adminClient.GetAsync("/api/plugins");

        // Assert
        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetPluginByName_AsAdministrator_ReturnsOk()
    {
        // Arrange
        var plugin = new PluginInfo
        {
            Name = "PuntuacionPorHora",
            Description = "Test plugin",
            Author = "Test Author",
            Version = "1.0.0"
        };

        _mockPluginLoader.Setup(p => p.GetPluginByName("PuntuacionPorHora")).Returns(plugin);

        // Act
        var response = await _adminClient.GetAsync("/api/plugins/PuntuacionPorHora");

        // Assert
        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetPluginByName_NonExistent_ReturnsNotFound()
    {
        // Arrange
        _mockPluginLoader.Setup(p => p.GetPluginByName("NonExistent")).Returns((PluginInfo?)null);

        // Act
        var response = await _adminClient.GetAsync("/api/plugins/NonExistent");

        // Assert
        Assert.AreEqual(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }
}
