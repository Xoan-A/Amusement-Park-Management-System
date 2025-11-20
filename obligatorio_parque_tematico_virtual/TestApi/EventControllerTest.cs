using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Text;
using System.Text.Json;
using Domain;
using IBusinessLogic;
using Models.In;
using Models.Out;
using Microsoft.EntityFrameworkCore;
using DataAccess.Context;
using Microsoft.Data.Sqlite;
using Api;

namespace ApiTests;

[TestClass]
public class EventControllerTest
{
    private SqliteConnection _connection = null!;
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;
    private HttpClient _adminClient = null!;
    private HttpClient _operatorClient = null!;
    private Mock<IEventLogic> _mockEventService = null!;

    [TestInitialize]
    public void Setup()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _mockEventService = new Mock<IEventLogic>();

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ServiceDescriptor? descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(_connection));

                services.AddSingleton(_mockEventService.Object);
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

        UserResponse operatorUser = new UserResponse
        {
            Id = Guid.NewGuid(),
            Name = "Operator",
            LastName = "User",
            Email = "operator@example.com",
            UserRoles = new List<string> { Role.Operator }
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
    public void GetEvents_ReturnsOkResult_WithListOfEvents()
    {
        List<EventResponse> mockEvents = new List<EventResponse>
        {
            new EventResponse
            {
                Id = Guid.NewGuid(),
                Name = "Event 1",
                Date = DateTime.Now.AddDays(10),
                Hour = 2,
                MaxCapacity = 100,
                CurrentCapacity = 50,
                Cost = 100
            },
            new EventResponse
            {
                Id = Guid.NewGuid(),
                Name = "Event 2",
                Date = DateTime.Now.AddDays(20),
                Hour = 3,
                MaxCapacity = 200,
                CurrentCapacity = 150,
                Cost = 200
            }
        };

        _mockEventService.Setup(service => service.GetAllEvents())
        .Returns(mockEvents);

        HttpResponseMessage response = _ = _adminClient.GetAsync("/api/events").Result;

        response.EnsureSuccessStatusCode();
        string responseString = response.Content.ReadAsStringAsync().Result;
        JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        List<EventResponse>? eventsResponse = JsonSerializer.Deserialize<List<EventResponse>>(responseString, options);

        Assert.AreEqual(2, eventsResponse.Count);
        Assert.AreEqual("Event 1", eventsResponse[0].Name);
        Assert.AreEqual("Event 2", eventsResponse[1].Name);
    }

    [TestMethod]
    public void GetEvents_InvalidAuthentication_ReturnsUnauthorized()
    {
        HttpResponseMessage response = _ = _client.GetAsync("/api/events").Result;

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public void GetEventById_ValidId_ReturnsOkResult_WithEvent()
    {
        Guid eventId = Guid.NewGuid();
        EventResponse mockEvent = new EventResponse
        {
            Id = eventId,
            Name = "Event 1",
            Date = DateTime.Now.AddDays(10),
            Hour = 2,
            MaxCapacity = 100,
            CurrentCapacity = 50,
            Cost = 100
        };

        _mockEventService.Setup(service => service.GetEventById(eventId))
        .Returns(mockEvent);

        HttpResponseMessage response = _ = _adminClient.GetAsync($"/api/events/{eventId}").Result;

        response.EnsureSuccessStatusCode();
        string responseString = response.Content.ReadAsStringAsync().Result;
        JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        EventResponse? eventResponse = JsonSerializer.Deserialize<EventResponse>(responseString, options);

        Assert.AreEqual("Event 1", eventResponse.Name);
    }

    [TestMethod]
    public void GetEventById_InvalidAuthentication_ReturnsUnauthorized()
    {
        Guid eventId = Guid.NewGuid();

        HttpResponseMessage response = _ = _client.GetAsync($"/api/events/{eventId}").Result;

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public void CreateEvent_ValidRequest_ReturnsCreatedResult()
    {
        Guid newEventId = Guid.NewGuid();
        EventRequest newEvent = new EventRequest
        {
            Name = "New Event",
            Date = DateTime.Now.AddDays(30),
            Hour = 4,
            MaxCapacity = 150,
            Cost = 150,
            AttractionIds = new List<Guid>()
        };

        _mockEventService.Setup(service => service.CreateEvent(It.IsAny<EventRequest>()))
        .Returns(newEventId);

        StringContent content =
        new StringContent(JsonSerializer.Serialize(newEvent), Encoding.UTF8, "application/json");
        HttpResponseMessage response = _ = _adminClient.PostAsync("/api/events", content).Result;

        Assert.AreEqual(System.Net.HttpStatusCode.Created, response.StatusCode);
        string responseString = response.Content.ReadAsStringAsync().Result;
        JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        CreateEventResponse? createResponse = JsonSerializer.Deserialize<CreateEventResponse>(responseString, options);

        Assert.AreEqual(newEventId, createResponse.Id);
        Assert.AreEqual("Event created successfully", createResponse.Message);
    }

    [TestMethod]
    public void CreateEvent_InvalidAuthentication_ReturnsUnauthorized()
    {
        Guid newEventId = Guid.NewGuid();
        EventRequest newEvent = new EventRequest
        {
            Name = "New Event",
            Date = DateTime.Now.AddDays(30),
            Hour = 4,
            MaxCapacity = 150,
            Cost = 150,
            AttractionIds = new List<Guid>()
        };
        StringContent content =
        new StringContent(JsonSerializer.Serialize(newEvent), Encoding.UTF8, "application/json");
        HttpResponseMessage response = _ = _client.PostAsync("/api/events", content).Result;

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        _mockEventService.Verify(service => service.CreateEvent(It.IsAny<EventRequest>()), Times.Never);
    }

    [TestMethod]
    public void DeleteEvent_ValidId_ReturnsNoContent()
    {
        Guid eventId = Guid.NewGuid();

        _mockEventService.Setup(service => service.DeleteEvent(eventId))
        ;

        HttpResponseMessage response = _ = _adminClient.DeleteAsync($"/api/events/{eventId}").Result;

        Assert.AreEqual(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        _mockEventService.Verify(service => service.DeleteEvent(eventId), Times.Once);
    }

    [TestMethod]
    public void DeleteEvent_InvalidAuthentication_ReturnsUnauthorized()
    {
        Guid eventId = Guid.NewGuid();

        HttpResponseMessage response = _ = _client.DeleteAsync($"/api/events/{eventId}").Result;

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        _mockEventService.Verify(service => service.DeleteEvent(eventId), Times.Never);
    }

    [TestMethod]
    public void CreateEvent_OperatorRole_ReturnsForbidden()
    {
        EventRequest request = new EventRequest
        {
            Name = "New Event",
            Date = DateTime.Now.AddDays(7),
            Hour = 14,
            MaxCapacity = 500,
            Cost = 100,
            AttractionIds = new List<Guid>()
        };

        string json = JsonSerializer.Serialize(request);
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response = _ = _operatorClient.PostAsync("/api/events", content).Result;

        Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public void DeleteEvent_OperatorRole_ReturnsForbidden()
    {
        Guid eventId = Guid.NewGuid();

        HttpResponseMessage response = _ = _operatorClient.DeleteAsync($"/api/events/{eventId}").Result;

        Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }
}