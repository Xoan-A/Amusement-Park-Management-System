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

namespace ApiTests;

[TestClass]
public class AttractionControllerTest
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;
    private HttpClient _adminClient = null!;
    private HttpClient _operatorClient = null!;
    private HttpClient _visitorClient = null!;
    private Mock<IAttractionLogic> _mockAttractionService = null!;
    private Mock<IUserManagementLogic> _mockUserManagementLogic = null!;
    private SqliteConnection _connection = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockAttractionService = new Mock<IAttractionLogic>();
        _mockUserManagementLogic = new Mock<IUserManagementLogic>();

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

                services.AddSingleton(_mockAttractionService.Object);
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

        UserResponse visitorUser = new UserResponse
        {
            Id = Guid.NewGuid(),
            Name = "Visitor",
            LastName = "User",
            Email = "visitor@example.com",
            UserRoles = new List<string> { Role.Visitor }
        };
        string visitorToken = tokenService.GenerateToken(visitorUser);
        _visitorClient = _factory.CreateClient();
        _visitorClient.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", visitorToken);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _client?.Dispose();
        _adminClient?.Dispose();
        _operatorClient?.Dispose();
        _visitorClient?.Dispose();
        _factory?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }

    [TestMethod]
    public void GetAttractions_ValidRequest_ReturnsAttractionResponse()
    {
        Guid id = Guid.NewGuid();
        AllAttractionsResponse expectedResponse = new AllAttractionsResponse
        {
            Attractions =
            {
                new AttractionResponse
                {
                    Id = id,
                    Name = "Eiffel Tower",
                    Description = "Paris",
                    Type = "InteractiveZone",
                    MinAge = 10,
                    MaxCapacity = 100,
                    CurrentCapacity = 50,
                    IsActive = true
                }
            }
        };

        _mockAttractionService.Setup(s => s.GetAllAttractions())
        .Returns(new List<AttractionResponse>
        {
            new AttractionResponse()
            {
                Id = id,
                Name = "Eiffel Tower",
                Description = "Paris",
                Type = AttractionType.InteractiveZone.ToString(),
                MinAge = 10,
                MaxCapacity = 100,
                CurrentCapacity = 50,
                IsActive = true
            }
        });

        HttpResponseMessage response = _ = _adminClient.GetAsync("/api/attractions").Result;
        response.EnsureSuccessStatusCode();
        string content = response.Content.ReadAsStringAsync().Result;
        AllAttractionsResponse attractionsResponse = JsonSerializer.Deserialize<AllAttractionsResponse>(content,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        Assert.AreEqual(id, attractionsResponse.Attractions[0].Id);
        Assert.AreEqual("Eiffel Tower", attractionsResponse.Attractions[0].Name);
        Assert.AreEqual("InteractiveZone", attractionsResponse.Attractions[0].Type);
        Assert.AreEqual(true, attractionsResponse.Attractions[0].IsActive);
    }

    [TestMethod]
    public void GetAttractionById_ValidId_ReturnsAttractionResponse()
    {
        Guid id = Guid.NewGuid();
        AttractionResponse expectedAttraction = new AttractionResponse
        {
            Id = id,
            Name = "Eiffel Tower",
            Description = "Paris",
            Type = "InteractiveZone",
            MinAge = 10,
            MaxCapacity = 100,
            CurrentCapacity = 50,
            IsActive = true
        };

        _mockAttractionService.Setup(s => s.GetAttractionById(id)).Returns(expectedAttraction);

        HttpResponseMessage response = _ = _adminClient.GetAsync($"/api/attractions/{id}").Result;
        response.EnsureSuccessStatusCode();
        string content = response.Content.ReadAsStringAsync().Result;
        AttractionResponse attractionResponse = JsonSerializer.Deserialize<AttractionResponse>(content,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        Assert.AreEqual(id, attractionResponse.Id);
        Assert.AreEqual("Eiffel Tower", attractionResponse.Name);
        Assert.AreEqual("InteractiveZone", attractionResponse.Type);
    }

    [TestMethod]
    public void CreateAttraction_ValidRequest_ReturnsCreatedAttractionResponse()
    {
        Guid expectedId = Guid.NewGuid();
        AttractionRequest newAttraction = new AttractionRequest
        {
            Name = "Eiffel Tower",
            Description = "Paris",
            Type = "InteractiveZone",
            MinAge = 10,
            MaxCapacity = 100,
        };

        _mockAttractionService.Setup(s => s.CreateAttraction(It.IsAny<AttractionRequest>())).Returns(expectedId);

        StringContent content =
        new StringContent(JsonSerializer.Serialize(newAttraction), Encoding.UTF8, "application/json");
        HttpResponseMessage response = _ = _adminClient.PostAsync("/api/attractions", content).Result;

        response.EnsureSuccessStatusCode();
        string responseContent = response.Content.ReadAsStringAsync().Result;
        CreateAttractionResponse? createdResponse = JsonSerializer.Deserialize<CreateAttractionResponse>(
            responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        Assert.AreEqual(expectedId, createdResponse.Id);
        Assert.AreEqual("Attraction created successfully", createdResponse.Message);
        _mockAttractionService.Verify(s => s.CreateAttraction(It.IsAny<AttractionRequest>()), Times.Once);
    }

    [TestMethod]
    public void UpdateAttraction_ValidRequest_ReturnsUpdatedAttractionResponse()
    {
        Guid id = Guid.NewGuid();
        AttractionRequest newAttraction = new AttractionRequest
        {
            Name = "Eiffel Tower",
            Description = "Paris",
            Type = "InteractiveZone",
            MinAge = 10,
            MaxCapacity = 100,
        };

        _mockAttractionService.Setup(s => s.UpdateAttraction(id, It.IsAny<AttractionRequest>()))
        ;
        StringContent content =
        new StringContent(JsonSerializer.Serialize(newAttraction), Encoding.UTF8, "application/json");
        HttpResponseMessage response = _ = _adminClient.PutAsync($"/api/attractions/{id}", content).Result;

        response.EnsureSuccessStatusCode();
        string responseContent = response.Content.ReadAsStringAsync().Result;
        MessageResponse? updatedResponse = JsonSerializer.Deserialize<MessageResponse>(responseContent,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        Assert.AreEqual("Attraction updated successfully", updatedResponse.Message);
        _mockAttractionService.Verify(s => s.UpdateAttraction(id, It.IsAny<AttractionRequest>()), Times.Once);
    }

    [TestMethod]
    public void DeleteAttraction_ValidId_ReturnsNoContent()
    {
        Guid id = Guid.NewGuid();
        _mockAttractionService.Setup(s => s.DeleteAttraction(id));
        HttpResponseMessage response = _ = _adminClient.DeleteAsync($"/api/attractions/{id}").Result;
        response.EnsureSuccessStatusCode();
        Assert.AreEqual(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        _mockAttractionService.Verify(s => s.DeleteAttraction(id), Times.Once);
    }

    [TestMethod]
    public void CreateAttraction_InvalidAuthentication_ReturnsUnauthorized()
    {
        AttractionRequest newAttraction = new AttractionRequest
        {
            Name = "Eiffel Tower",
            Description = "Paris",
            Type = "InteractiveZone",
            MinAge = 10,
            MaxCapacity = 100,
        };

        StringContent content =
        new StringContent(JsonSerializer.Serialize(newAttraction), Encoding.UTF8, "application/json");
        HttpResponseMessage response = _ = _client.PostAsync("/api/attractions", content).Result;

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        _mockAttractionService.Verify(s => s.CreateAttraction(It.IsAny<AttractionRequest>()), Times.Never);
    }

    [TestMethod]
    public void GetAttractions_InvalidAuthentication_ReturnsUnauthorized()
    {
        HttpResponseMessage response = _ = _client.GetAsync("/api/attractions").Result;
        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        _mockAttractionService.Verify(s => s.GetAllAttractions(), Times.Never);
    }

    [TestMethod]
    public void GetAttractionById_InvalidAuthentication_ReturnsUnauthorized()
    {
        Guid id = Guid.NewGuid();
        HttpResponseMessage response = _ = _client.GetAsync($"/api/attractions/{id}").Result;
        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        _mockAttractionService.Verify(s => s.GetAttractionById(id), Times.Never);
    }

    [TestMethod]
    public void UpdateAttraction_InvalidAuthentication_ReturnsUnauthorized()
    {
        Guid id = Guid.NewGuid();
        AttractionRequest newAttraction = new AttractionRequest
        {
            Name = "Eiffel Tower",
            Description = "Paris",
            Type = "InteractiveZone",
            MinAge = 10,
            MaxCapacity = 100,
        };
        StringContent content =
        new StringContent(JsonSerializer.Serialize(newAttraction), Encoding.UTF8, "application/json");
        HttpResponseMessage response = _ = _client.PutAsync($"/api/attractions/{id}", content).Result;
        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        _mockAttractionService.Verify(s => s.UpdateAttraction(id, It.IsAny<AttractionRequest>()), Times.Never);
    }

    [TestMethod]
    public void DeleteAttraction_InvalidAuthentication_ReturnsUnauthorized()
    {
        Guid id = Guid.NewGuid();
        HttpResponseMessage response = _ = _client.DeleteAsync($"/api/attractions/{id}").Result;
        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        _mockAttractionService.Verify(s => s.DeleteAttraction(id), Times.Never);
    }

    [TestMethod]
    public void RegisterEntry_ValidRequest_ReturnsSuccessMessage()
    {
        Guid attractionId = Guid.NewGuid();

        RegisterEntryRequest requestBody = new RegisterEntryRequest
        {
        };

        _mockUserManagementLogic.Setup(s =>
        s.RegisterEntry(attractionId, requestBody))
        ;

        StringContent content =
        new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        HttpResponseMessage response =
        _ = _operatorClient.PutAsync($"/api/attractions/entry/{attractionId}", content).Result;

        response.EnsureSuccessStatusCode();
        string responseContent = response.Content.ReadAsStringAsync().Result;
        MessageResponse? messageResponse = JsonSerializer.Deserialize<MessageResponse>(responseContent,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        Assert.AreEqual("Entry registered successfully", messageResponse.Message);
        _mockUserManagementLogic.Verify(
            s => s.RegisterEntry(attractionId, It.IsAny<RegisterEntryRequest>()), Times.Once);
    }

    [TestMethod]
    public void RegisterEntry_InvalidAuthentication_ReturnsUnauthorized()
    {
        Guid attractionId = Guid.NewGuid();

        RegisterEntryRequest requestBody = new RegisterEntryRequest
        {
        };

        StringContent content =
        new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        HttpResponseMessage response = _ = _client.PutAsync($"/api/attractions/entry/{attractionId}", content).Result;

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        _mockUserManagementLogic.Verify(
            s => s.RegisterEntry(It.IsAny<Guid>(), It.IsAny<RegisterEntryRequest>()), Times.Never);
    }

    [TestMethod]
    public void RegisterEntry_WithQrCode_ReturnsSuccessMessage()
    {
        Guid attractionId = Guid.NewGuid();
        Guid qrCode = Guid.NewGuid();

        RegisterEntryRequest requestBody = new RegisterEntryRequest
        {
            Qr = qrCode
        };

        _mockUserManagementLogic.Setup(s => s.RegisterEntry(attractionId, requestBody))
        ;

        StringContent content =
        new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        HttpResponseMessage response =
        _ = _operatorClient.PutAsync($"/api/attractions/entry/{attractionId}", content).Result;

        response.EnsureSuccessStatusCode();
        string responseContent = response.Content.ReadAsStringAsync().Result;
        MessageResponse? messageResponse = JsonSerializer.Deserialize<MessageResponse>(responseContent,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        Assert.AreEqual("Entry registered successfully", messageResponse.Message);
        _mockUserManagementLogic.Verify(s => s.RegisterEntry(attractionId, It.IsAny<RegisterEntryRequest>()),
            Times.Once);
    }

    [TestMethod]
    public void RegisterEntry_WithNfc_ReturnsSuccessMessage()
    {
        Guid attractionId = Guid.NewGuid();
        Guid nfcId = Guid.NewGuid();

        RegisterEntryRequest requestBody = new RegisterEntryRequest
        {
            NFC = nfcId
        };

        _mockUserManagementLogic.Setup(s => s.RegisterEntry(attractionId, requestBody))
        ;

        StringContent content =
        new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        HttpResponseMessage response =
        _ = _operatorClient.PutAsync($"/api/attractions/entry/{attractionId}", content).Result;

        response.EnsureSuccessStatusCode();
        string responseContent = response.Content.ReadAsStringAsync().Result;
        MessageResponse? messageResponse = JsonSerializer.Deserialize<MessageResponse>(responseContent,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        Assert.AreEqual("Entry registered successfully", messageResponse.Message);
        _mockUserManagementLogic.Verify(s => s.RegisterEntry(attractionId, It.IsAny<RegisterEntryRequest>()),
            Times.Once);
    }

    [TestMethod]
    public void RegisterEntry_WithEventId_ReturnsSuccessMessage()
    {
        Guid attractionId = Guid.NewGuid();
        Guid qrCode = Guid.NewGuid();
        Guid eventId = Guid.NewGuid();

        RegisterEntryRequest requestBody = new RegisterEntryRequest
        {
            Qr = qrCode,
            EventId = eventId
        };

        _mockUserManagementLogic.Setup(s => s.RegisterEntry(attractionId, requestBody))
        ;

        StringContent content =
        new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        HttpResponseMessage response =
        _ = _operatorClient.PutAsync($"/api/attractions/entry/{attractionId}", content).Result;

        response.EnsureSuccessStatusCode();
        string responseContent = response.Content.ReadAsStringAsync().Result;
        MessageResponse? messageResponse = JsonSerializer.Deserialize<MessageResponse>(responseContent,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        Assert.AreEqual("Entry registered successfully", messageResponse.Message);
        _mockUserManagementLogic.Verify(s => s.RegisterEntry(attractionId, It.IsAny<RegisterEntryRequest>()),
            Times.Once);
    }

    [TestMethod]
    public void RegisterEntry_WithNfcAndEventId_ReturnsSuccessMessage()
    {
        Guid attractionId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        Guid eventId = Guid.NewGuid();

        RegisterEntryRequest requestBody = new RegisterEntryRequest
        {
            NFC = userId,
            EventId = eventId
        };

        _mockUserManagementLogic.Setup(s => s.RegisterEntry(attractionId, requestBody))
        ;

        StringContent content =
        new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        HttpResponseMessage response =
        _ = _operatorClient.PutAsync($"/api/attractions/entry/{attractionId}", content).Result;

        response.EnsureSuccessStatusCode();
        string responseContent = response.Content.ReadAsStringAsync().Result;
        MessageResponse? messageResponse = JsonSerializer.Deserialize<MessageResponse>(responseContent,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        Assert.AreEqual("Entry registered successfully", messageResponse.Message);
        _mockUserManagementLogic.Verify(s => s.RegisterEntry(attractionId, It.IsAny<RegisterEntryRequest>()),
            Times.Once);
    }

    [TestMethod]
    public void RegisterEntry_WhenUserLogicThrowsException_ReturnsBadRequest()
    {
        Guid attractionId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        Guid qrCode = Guid.NewGuid();

        RegisterEntryRequest requestBody = new RegisterEntryRequest
        {
            Qr = qrCode
        };

        _mockUserManagementLogic.Setup(s => s.RegisterEntry(attractionId, It.IsAny<RegisterEntryRequest>()))
        .Throws(new ArgumentException("User does not have a valid ticket."));

        StringContent content =
        new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        HttpResponseMessage response =
        _ = _operatorClient.PutAsync($"/api/attractions/entry/{attractionId}", content).Result;

        _mockUserManagementLogic.Verify(s => s.RegisterEntry(attractionId, It.IsAny<RegisterEntryRequest>()),
            Times.Once);
        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public void RegisterExit_ValidRequest_ReturnsSuccessMessage()
    {
        Guid attractionId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();

        RegisterExitRequest requestBody = new RegisterExitRequest
        {
            userId = userId
        };

        _mockUserManagementLogic.Setup(s => s.RegisterExit(attractionId, requestBody))
        ;

        StringContent content =
        new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        HttpResponseMessage response =
        _ = _operatorClient.PutAsync($"/api/attractions/exit/{attractionId}", content).Result;

        response.EnsureSuccessStatusCode();
        string responseContent = response.Content.ReadAsStringAsync().Result;
        MessageResponse? messageResponse = JsonSerializer.Deserialize<MessageResponse>(responseContent,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        Assert.AreEqual("Exit registered successfully", messageResponse.Message);
        _mockUserManagementLogic.Verify(s => s.RegisterExit(attractionId, It.IsAny<RegisterExitRequest>()), Times.Once);
    }

    [TestMethod]
    public void RegisterExit_InvalidAuthentication_ReturnsUnauthorized()
    {
        Guid attractionId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();

        RegisterExitRequest requestBody = new RegisterExitRequest
        {
            userId = userId
        };

        StringContent content =
        new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        HttpResponseMessage response = _ = _client.PutAsync($"/api/attractions/exit/{attractionId}", content).Result;

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        _mockUserManagementLogic.Verify(s => s.RegisterExit(It.IsAny<Guid>(), It.IsAny<RegisterExitRequest>()),
            Times.Never);
    }

    [TestMethod]
    public void GetAttractionVisits_AsAdmin_ReturnsVisitsSuccessfully()
    {
        DateTime startDate = new DateTime(2025, 10, 1);
        DateTime endDate = new DateTime(2025, 10, 7);

        Guid attraction1Id = Guid.NewGuid();
        Guid attraction2Id = Guid.NewGuid();

        Attraction attraction1 = new Attraction
        {
            Id = attraction1Id,
            Name = "Monta�a Rusa",
            Description = "Una atracci�n emocionante",
            Type = AttractionType.RollerCoaster,
            MinAge = 12,
            MaxCapacity = 50,
            CurrentCapacity = 0
        };

        Attraction attraction2 = new Attraction
        {
            Id = attraction2Id,
            Name = "Simulador",
            Description = "Experiencia virtual",
            Type = AttractionType.Simulator,
            MinAge = 8,
            MaxCapacity = 30,
            CurrentCapacity = 0
        };

        AttractionsVisitResponse expectedResponse = new AttractionsVisitResponse();
        AttractionResponse attractionRes1 = new AttractionResponse()
        {
            Id = attraction1.Id,
            Name = attraction1.Name,
            Description = attraction1.Description,
            Type = attraction1.Type.ToString(),
            MinAge = attraction1.MinAge,
            MaxCapacity = attraction1.MaxCapacity,
            CurrentCapacity = attraction1.CurrentCapacity,
            IsActive = attraction1.IsActive
        };

        AttractionResponse attractionRes2 = new AttractionResponse()
        {
            Id = attraction2.Id,
            Name = attraction2.Name,
            Description = attraction2.Description,
            Type = attraction2.Type.ToString(),
            MinAge = attraction2.MinAge,
            MaxCapacity = attraction2.MaxCapacity,
            CurrentCapacity = attraction2.CurrentCapacity,
            IsActive = attraction2.IsActive
        };
        expectedResponse.AttractionsVisits.Add(new AttractionVisitDetail
        {
            Attraction = attractionRes1,
            VisitCount = 3
        });
        expectedResponse.AttractionsVisits.Add(new AttractionVisitDetail
        {
            Attraction = attractionRes2,
            VisitCount = 2
        });

        _mockAttractionService.Setup(s => s.GetAllAttractionsVisits(It.IsAny<AttractionsVisitsRequest>()))
        .Returns(expectedResponse);

        HttpResponseMessage response =
        _ = _adminClient.GetAsync(
            $"/api/attractions/visits?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}").Result;

        response.EnsureSuccessStatusCode();
        string responseContent = response.Content.ReadAsStringAsync().Result;
        AttractionsVisitResponse? visitResponse = JsonSerializer.Deserialize<AttractionsVisitResponse>(responseContent,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        Assert.AreEqual(2, visitResponse.AttractionsVisits.Count);
        _mockAttractionService.Verify(s => s.GetAllAttractionsVisits(It.IsAny<AttractionsVisitsRequest>()), Times.Once);
    }

    [TestMethod]
    public void GetAttractionVisits_AsNonAdmin_ReturnsUnauthorized()
    {
        DateTime startDate = new DateTime(2025, 10, 1);
        DateTime endDate = new DateTime(2025, 10, 7);

        HttpResponseMessage response =
        _ = _operatorClient.GetAsync(
            $"/api/attractions/visits?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}").Result;

        Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
        _mockAttractionService.Verify(s => s.GetAllAttractionsVisits(It.IsAny<AttractionsVisitsRequest>()),
            Times.Never);
    }

    [TestMethod]
    public void UpdateAttraction_OperatorRole_ReturnsForbidden()
    {
        Guid attractionId = Guid.NewGuid();
        AttractionRequest request = new AttractionRequest
        {
            Name = "Updated Attraction",
            Description = "Updated Description",
            Type = "RollerCoaster",
            MinAge = 10,
            MaxCapacity = 100
        };

        string json = JsonSerializer.Serialize(request);
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response = _ = _operatorClient.PutAsync($"/api/attractions/{attractionId}", content).Result;

        Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public void DeleteAttraction_OperatorRole_ReturnsForbidden()
    {
        Guid attractionId = Guid.NewGuid();

        HttpResponseMessage response = _ = _operatorClient.DeleteAsync($"/api/attractions/{attractionId}").Result;

        Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public void RegisterEntry_AdminRole_ReturnsForbidden()
    {
        Guid attractionId = Guid.NewGuid();
        RegisterEntryRequest request = new RegisterEntryRequest
        {
            Qr = Guid.NewGuid()
        };

        string json = JsonSerializer.Serialize(request);
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response =
        _ = _adminClient.PutAsync($"/api/attractions/entry/{attractionId}", content).Result;

        Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public void RegisterExit_AdminRole_ReturnsForbidden()
    {
        Guid attractionId = Guid.NewGuid();
        RegisterExitRequest request = new RegisterExitRequest
        {
            userId = Guid.NewGuid()
        };

        string json = JsonSerializer.Serialize(request);
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response =
        _ = _adminClient.PutAsync($"/api/attractions/exit/{attractionId}", content).Result;

        Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public void CreateAttraction_VisitorRole_ReturnsForbidden()
    {
        AttractionRequest request = new AttractionRequest
        {
            Name = "New Attraction",
            Description = "Description",
            Type = "RollerCoaster",
            MinAge = 10,
            MaxCapacity = 100
        };

        string json = JsonSerializer.Serialize(request);
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response = _ = _visitorClient.PostAsync("/api/attractions", content).Result;

        Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public void GetAttractions_EmptyList_ReturnsEmptyResponse()
    {
        _mockAttractionService.Setup(s => s.GetAllAttractions())
        .Returns(new List<AttractionResponse>());

        HttpResponseMessage response = _ = _adminClient.GetAsync("/api/attractions").Result;

        response.EnsureSuccessStatusCode();
        string content = response.Content.ReadAsStringAsync().Result;
        AllAttractionsResponse? result = JsonSerializer.Deserialize<AllAttractionsResponse>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.AreEqual(0, result.Attractions.Count);
    }
}