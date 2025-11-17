using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Domain;
using IBusinessLogic;
using IBusinessLogic.Strategy;
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
    private Mock<IPluginLoader> _mockPluginLoader = null!;
    private SqliteConnection _connection = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockPluginLoader = new Mock<IPluginLoader>();

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

        Microsoft.Extensions.Options.IOptions<Models.JwtSettings> jwtSettings =
        Microsoft.Extensions.Options.Options.Create(new Models.JwtSettings
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
            UserRoles = new List<string> { Role.Administrator }
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
    public void GetAvailablePlugins_AsAdministrator_ReturnsOk()
    {
        List<PluginInfoResponse> plugins = new List<PluginInfoResponse>
        {
            new PluginInfoResponse
            {
                Name = "PuntuacionPorHora",
            }
        };

        _mockPluginLoader.Setup(p => p.LoadPlugins()).Returns(plugins);

        HttpResponseMessage response = _ = _adminClient.GetAsync("/api/plugins").Result;

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public void AddPlugin_WithValidDllFile_ReturnsOk()
    {
        byte[] fileContent = new byte[] { 0x4D, 0x5A };
        MultipartFormDataContent content = new MultipartFormDataContent();
        ByteArrayContent fileContentBytes = new ByteArrayContent(fileContent);
        fileContentBytes.Headers.ContentType =
        new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
        content.Add(fileContentBytes, "dllFile", "TestPlugin.dll");

        _mockPluginLoader.Setup(p => p.AddPlugin(It.IsAny<Stream>(), "TestPlugin.dll"));

        HttpResponseMessage response = _ = _adminClient.PostAsync("/api/plugins", content).Result;

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        _mockPluginLoader.Verify(p => p.AddPlugin(It.IsAny<Stream>(), "TestPlugin.dll"), Times.Once);
    }

    [TestMethod]
    public void AddPlugin_WithInvalidFileExtension_ReturnsBadRequest()
    {
        byte[] fileContent = new byte[] { 0x4D, 0x5A };
        MultipartFormDataContent content = new MultipartFormDataContent();
        ByteArrayContent fileContentBytes = new ByteArrayContent(fileContent);
        fileContentBytes.Headers.ContentType =
        new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
        content.Add(fileContentBytes, "dllFile", "TestPlugin.txt");

        _mockPluginLoader.Setup(p => p.AddPlugin(It.IsAny<Stream>(), "TestPlugin.txt"))
        .Throws(new ArgumentException("Only .dll files are allowed"));

        HttpResponseMessage response = _ = _adminClient.PostAsync("/api/plugins", content).Result;

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public void AddPlugin_WithoutAuthentication_ReturnsUnauthorized()
    {
        HttpClient unauthenticatedClient = _factory.CreateClient();
        byte[] fileContent = new byte[] { 0x4D, 0x5A };
        MultipartFormDataContent content = new MultipartFormDataContent();
        ByteArrayContent fileContentBytes = new ByteArrayContent(fileContent);
        fileContentBytes.Headers.ContentType =
        new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
        content.Add(fileContentBytes, "dllFile", "TestPlugin.dll");

        HttpResponseMessage response = _ = unauthenticatedClient.PostAsync("/api/plugins", content).Result;

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}