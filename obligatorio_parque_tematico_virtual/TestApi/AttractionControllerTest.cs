using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Text;
using System.Text.Json;
using BusinessLogic;
using Domain;
using IBusinessLogic;
using Models.In;
using Models.Out;

namespace ApiTests;

[TestClass]
public class AttractionControllerTest
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;
    private HttpClient _adminClient = null!;
    private HttpClient _operatorClient = null!;
    private Mock<IAttractionService> _mockAttractionService = null!;
    private Mock<IUserLogic> _mockUserLogic = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockAttractionService = new Mock<IAttractionService>();
        _mockUserLogic = new Mock<IUserLogic>();

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(_mockAttractionService.Object);
                services.AddSingleton(_mockUserLogic.Object);
            });
        });

        _client = _factory.CreateClient();

        TokenService tokenService = new TokenService();

        Administrator adminUser = new Administrator
        {
            Id = Guid.NewGuid(),
            Name = "Admin",
            LastName = "User",
            Email = "admin@example.com"
        };
        string adminToken = tokenService.GenerateToken(adminUser);
        _adminClient = _factory.CreateClient();
        _adminClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        Operator operatorUser = new Operator
        {
            Id = Guid.NewGuid(),
            Name = "Operator",
            LastName = "User",
            Email = "operator@example.com"
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
    }

    [TestMethod]
    public async Task GetAttractions_ValidRequest_ReturnsAttractionResponse()
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
            .ReturnsAsync(new List<AttractionResponse>
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

        var response = await _adminClient.GetAsync("/api/attractions");
        response.EnsureSuccessStatusCode();
        string content = await response.Content.ReadAsStringAsync();
        AllAttractionsResponse attractionsResponse = JsonSerializer.Deserialize<AllAttractionsResponse>(content,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        Assert.IsNotNull(attractionsResponse);
        Assert.AreEqual(id, attractionsResponse.Attractions[0].Id);
        Assert.AreEqual("Eiffel Tower", attractionsResponse.Attractions[0].Name);
        Assert.AreEqual("Paris", attractionsResponse.Attractions[0].Description);
        Assert.AreEqual("InteractiveZone", attractionsResponse.Attractions[0].Type);
        Assert.AreEqual(10, attractionsResponse.Attractions[0].MinAge);
        Assert.AreEqual(100, attractionsResponse.Attractions[0].MaxCapacity);
        Assert.AreEqual(50, attractionsResponse.Attractions[0].CurrentCapacity);
        Assert.AreEqual(true, attractionsResponse.Attractions[0].IsActive);
    }

    [TestMethod]
    public async Task GetAttractionById_ValidId_ReturnsAttractionResponse()
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

        _mockAttractionService.Setup(s => s.GetAttractionById(id)).ReturnsAsync(expectedAttraction);

        var response = await _adminClient.GetAsync($"/api/attractions/{id}");
        response.EnsureSuccessStatusCode();
        string content = await response.Content.ReadAsStringAsync();
        AttractionResponse attractionResponse = JsonSerializer.Deserialize<AttractionResponse>(content,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        Assert.IsNotNull(attractionResponse);
        Assert.AreEqual(id, attractionResponse.Id);
        Assert.AreEqual("Eiffel Tower", attractionResponse.Name);
        Assert.AreEqual("Paris", attractionResponse.Description);
        Assert.AreEqual("InteractiveZone", attractionResponse.Type);
        Assert.AreEqual(10, attractionResponse.MinAge);
        Assert.AreEqual(100, attractionResponse.MaxCapacity);
        Assert.AreEqual(50, attractionResponse.CurrentCapacity);
        Assert.AreEqual(true, attractionResponse.IsActive);
    }

    [TestMethod]
    public async Task CreateAttraction_ValidRequest_ReturnsCreatedAttractionResponse()
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

        _mockAttractionService.Setup(s => s.CreateAttraction(It.IsAny<AttractionRequest>())).ReturnsAsync(expectedId);

        var content = new StringContent(JsonSerializer.Serialize(newAttraction), Encoding.UTF8, "application/json");
        var response = await _adminClient.PostAsync("/api/attractions", content);

        response.EnsureSuccessStatusCode();
        string responseContent = await response.Content.ReadAsStringAsync();
        CreateAttractionResponse? createdResponse = JsonSerializer.Deserialize<CreateAttractionResponse>(
            responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        Assert.IsNotNull(createdResponse);
        Assert.AreEqual(expectedId, createdResponse.Id);
        Assert.AreEqual("Attraction created successfully", createdResponse.Message);
        _mockAttractionService.Verify(s => s.CreateAttraction(It.IsAny<AttractionRequest>()), Times.Once);
    }

    [TestMethod]
    public async Task UpdateAttraction_ValidRequest_ReturnsUpdatedAttractionResponse()
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
            .Returns(Task.CompletedTask);
        var content = new StringContent(JsonSerializer.Serialize(newAttraction), Encoding.UTF8, "application/json");
        var response = await _adminClient.PutAsync($"/api/attractions/{id}", content);

        response.EnsureSuccessStatusCode();
        string responseContent = await response.Content.ReadAsStringAsync();
        MessageResponse? updatedResponse = JsonSerializer.Deserialize<MessageResponse>(responseContent,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        Assert.IsNotNull(updatedResponse);
        Assert.AreEqual("Attraction updated successfully", updatedResponse.Message);
        _mockAttractionService.Verify(s => s.UpdateAttraction(id, It.IsAny<AttractionRequest>()), Times.Once);
    }

    [TestMethod]
    public async Task DeleteAttraction_ValidId_ReturnsNoContent()
    {
        Guid id = Guid.NewGuid();
        _mockAttractionService.Setup(s => s.DeleteAttraction(id)).Returns(Task.CompletedTask);
        var response = await _adminClient.DeleteAsync($"/api/attractions/{id}");
        response.EnsureSuccessStatusCode();
        Assert.AreEqual(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        _mockAttractionService.Verify(s => s.DeleteAttraction(id), Times.Once);
    }

    [TestMethod]
    public async Task CreateAttraction_InvalidAuthentication_ReturnsUnauthorized()
    {
        AttractionRequest newAttraction = new AttractionRequest
        {
            Name = "Eiffel Tower",
            Description = "Paris",
            Type = "InteractiveZone",
            MinAge = 10,
            MaxCapacity = 100,
        };

        var content = new StringContent(JsonSerializer.Serialize(newAttraction), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/attractions", content);

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        _mockAttractionService.Verify(s => s.CreateAttraction(It.IsAny<AttractionRequest>()), Times.Never);
    }

    [TestMethod]
    public async Task GetAttractions_InvalidAuthentication_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/attractions");
        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        _mockAttractionService.Verify(s => s.GetAllAttractions(), Times.Never);
    }

    [TestMethod]
    public async Task GetAttractionById_InvalidAuthentication_ReturnsUnauthorized()
    {
        Guid id = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/attractions/{id}");
        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        _mockAttractionService.Verify(s => s.GetAttractionById(id), Times.Never);
    }

    [TestMethod]
    public async Task UpdateAttraction_InvalidAuthentication_ReturnsUnauthorized()
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
        var content = new StringContent(JsonSerializer.Serialize(newAttraction), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync($"/api/attractions/{id}", content);
        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        _mockAttractionService.Verify(s => s.UpdateAttraction(id, It.IsAny<AttractionRequest>()), Times.Never);
    }

    [TestMethod]
    public async Task DeleteAttraction_InvalidAuthentication_ReturnsUnauthorized()
    {
        Guid id = Guid.NewGuid();
        var response = await _client.DeleteAsync($"/api/attractions/{id}");
        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        _mockAttractionService.Verify(s => s.DeleteAttraction(id), Times.Never);
    }

    [TestMethod]
    public async Task RegisterEntry_ValidRequest_ReturnsSuccessMessage()
    {
        Guid attractionId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);

        _mockUserLogic.Setup(s => s.RegisterEntry(userId, attractionId, enterDate, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<int?>()))
            .Returns(Task.CompletedTask);

        RegisterEntryRequest requestBody = new RegisterEntryRequest
        {
            EnterDate = enterDate,
            UserId = userId
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await _operatorClient.PostAsync($"/api/attractions/registerEntry/{attractionId}", content);

        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        MessageResponse? messageResponse = JsonSerializer.Deserialize<MessageResponse>(responseContent,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        Assert.IsNotNull(messageResponse);
        Assert.AreEqual("Entry registered successfully", messageResponse.Message);
        _mockUserLogic.Verify(s => s.RegisterEntry(userId, attractionId, enterDate, It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<int?>()), Times.Once);
    }

    [TestMethod]
    public async Task RegisterEntry_InvalidAuthentication_ReturnsUnauthorized()
    {
        Guid attractionId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);

        RegisterEntryRequest requestBody = new RegisterEntryRequest
        {
            EnterDate = enterDate,
            UserId = userId
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"/api/attractions/registerEntry/{attractionId}", content);

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        _mockUserLogic.Verify(s => s.RegisterEntry(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<int?>()), Times.Never);
    }

    [TestMethod]
    public async Task RegisterEntry_WithQrCode_ReturnsSuccessMessage()
    {
        Guid attractionId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        Guid qrCode = Guid.NewGuid();
        DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);

        _mockUserLogic.Setup(s => s.RegisterEntry(userId, attractionId, enterDate, qrCode, null, null))
            .Returns(Task.CompletedTask);

        RegisterEntryRequest requestBody = new RegisterEntryRequest
        {
            EnterDate = enterDate,
            UserId = userId,
            Qr = qrCode
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await _operatorClient.PostAsync($"/api/attractions/registerEntry/{attractionId}", content);

        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        MessageResponse? messageResponse = JsonSerializer.Deserialize<MessageResponse>(responseContent,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        Assert.IsNotNull(messageResponse);
        Assert.AreEqual("Entry registered successfully", messageResponse.Message);
        _mockUserLogic.Verify(s => s.RegisterEntry(userId, attractionId, enterDate, qrCode, null, null), Times.Once);
    }

    [TestMethod]
    public async Task RegisterEntry_WithNfc_ReturnsSuccessMessage()
    {
        Guid attractionId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);

        _mockUserLogic.Setup(s => s.RegisterEntry(userId, attractionId, enterDate, null, userId, null))
            .Returns(Task.CompletedTask);

        RegisterEntryRequest requestBody = new RegisterEntryRequest
        {
            EnterDate = enterDate,
            UserId = userId,
            NFC = userId
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await _operatorClient.PostAsync($"/api/attractions/registerEntry/{attractionId}", content);

        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        MessageResponse? messageResponse = JsonSerializer.Deserialize<MessageResponse>(responseContent,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        Assert.IsNotNull(messageResponse);
        Assert.AreEqual("Entry registered successfully", messageResponse.Message);
        _mockUserLogic.Verify(s => s.RegisterEntry(userId, attractionId, enterDate, null, userId, null), Times.Once);
    }

    [TestMethod]
    public async Task RegisterEntry_WithEventId_ReturnsSuccessMessage()
    {
        Guid attractionId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        Guid qrCode = Guid.NewGuid();
        int eventId = 5;
        DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);

        _mockUserLogic.Setup(s => s.RegisterEntry(userId, attractionId, enterDate, qrCode, null, eventId))
            .Returns(Task.CompletedTask);

        RegisterEntryRequest requestBody = new RegisterEntryRequest
        {
            EnterDate = enterDate,
            UserId = userId,
            Qr = qrCode,
            EventId = eventId
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await _operatorClient.PostAsync($"/api/attractions/registerEntry/{attractionId}", content);

        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        MessageResponse? messageResponse = JsonSerializer.Deserialize<MessageResponse>(responseContent,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        Assert.IsNotNull(messageResponse);
        Assert.AreEqual("Entry registered successfully", messageResponse.Message);
        _mockUserLogic.Verify(s => s.RegisterEntry(userId, attractionId, enterDate, qrCode, null, eventId), Times.Once);
    }

    [TestMethod]
    public async Task RegisterEntry_WithNfcAndEventId_ReturnsSuccessMessage()
    {
        Guid attractionId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        int eventId = 10;
        DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);

        _mockUserLogic.Setup(s => s.RegisterEntry(userId, attractionId, enterDate, null, userId, eventId))
            .Returns(Task.CompletedTask);

        RegisterEntryRequest requestBody = new RegisterEntryRequest
        {
            EnterDate = enterDate,
            UserId = userId,
            NFC = userId,
            EventId = eventId
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await _operatorClient.PostAsync($"/api/attractions/registerEntry/{attractionId}", content);

        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        MessageResponse? messageResponse = JsonSerializer.Deserialize<MessageResponse>(responseContent,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        Assert.IsNotNull(messageResponse);
        Assert.AreEqual("Entry registered successfully", messageResponse.Message);
        _mockUserLogic.Verify(s => s.RegisterEntry(userId, attractionId, enterDate, null, userId, eventId), Times.Once);
    }

    [TestMethod]
    public async Task RegisterEntry_WhenUserLogicThrowsException_ReturnsBadRequest()
    {
        Guid attractionId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        Guid qrCode = Guid.NewGuid();
        DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);

        _mockUserLogic.Setup(s => s.RegisterEntry(userId, attractionId, enterDate, qrCode, null, null))
            .ThrowsAsync(new ArgumentException("User does not have a valid ticket."));

        RegisterEntryRequest requestBody = new RegisterEntryRequest
        {
            EnterDate = enterDate,
            UserId = userId,
            Qr = qrCode
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await _operatorClient.PostAsync($"/api/attractions/registerEntry/{attractionId}", content);

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        _mockUserLogic.Verify(s => s.RegisterEntry(userId, attractionId, enterDate, qrCode, null, null), Times.Once);
    }

    [TestMethod]
    public async Task RegisterExit_ValidRequest_ReturnsSuccessMessage()
    {
        Guid attractionId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        DateTime exitDate = new DateTime(2025, 10, 1, 12, 0, 0);

        _mockUserLogic.Setup(s => s.RegisterExit(userId, attractionId, exitDate))
            .Returns(Task.CompletedTask);

        RegisterExitRequest requestBody = new RegisterExitRequest
        {
            exitDate = exitDate,
            userId = userId
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await _operatorClient.PutAsync($"/api/attractions/registerExit/{attractionId}", content);

        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        MessageResponse? messageResponse = JsonSerializer.Deserialize<MessageResponse>(responseContent,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        Assert.IsNotNull(messageResponse);
        Assert.AreEqual("Exit registered successfully", messageResponse.Message);
        _mockUserLogic.Verify(s => s.RegisterExit(userId, attractionId, exitDate), Times.Once);
    }

    [TestMethod]
    public async Task RegisterExit_InvalidAuthentication_ReturnsUnauthorized()
    {
        Guid attractionId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        DateTime exitDate = new DateTime(2025, 10, 1, 12, 0, 0);

        RegisterExitRequest requestBody = new RegisterExitRequest
        {
            exitDate = exitDate,
            userId = userId
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync($"/api/attractions/registerExit/{attractionId}", content);

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        _mockUserLogic.Verify(s => s.RegisterExit(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTime>()), Times.Never);
    }

    [TestMethod]
    public async Task GetCapacity_ValidRequest_ReturnsCapacityResponse()
    {
        Guid attractionId = Guid.NewGuid();
        CapacityResponse expectedCapacity = new CapacityResponse
        {
            Id = attractionId,
            Capacity = 100,
            CurrentCapacity = 45
        };

        _mockAttractionService.Setup(s => s.GetCapacity(attractionId)).ReturnsAsync(expectedCapacity);

        var response = await _adminClient.GetAsync($"/api/attractions/capacity/{attractionId}");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var capacityResponse = JsonSerializer.Deserialize<CapacityResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.IsNotNull(capacityResponse);
        Assert.AreEqual(attractionId, capacityResponse.Id);
        Assert.AreEqual(100, capacityResponse.Capacity);
        Assert.AreEqual(45, capacityResponse.CurrentCapacity);
        _mockAttractionService.Verify(s => s.GetCapacity(attractionId), Times.Once);
    }

    [TestMethod]
    public async Task GetCapacity_InvalidAuthentication_ReturnsUnauthorized()
    {
        Guid attractionId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/attractions/capacity/{attractionId}");
        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        _mockAttractionService.Verify(s => s.GetCapacity(It.IsAny<Guid>()), Times.Never);
    }
}