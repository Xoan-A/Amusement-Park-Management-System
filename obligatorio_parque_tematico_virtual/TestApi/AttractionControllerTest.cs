using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
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
    private Mock<IAttractionService> _mockAttractionService = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockAttractionService = new Mock<IAttractionService>();

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services => { services.AddSingleton(_mockAttractionService.Object); });
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

        var response = await _client.GetAsync("/api/attractions");

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var attractionsResponse = JsonSerializer.Deserialize<AllAttractionsResponse>(content, new JsonSerializerOptions
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

        _mockAttractionService.Setup(s => s.GetAttractionById(id)).Returns(expectedAttraction);

        var response = await _client.GetAsync($"/api/attractions/{id}");

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var attractionResponse = JsonSerializer.Deserialize<AttractionResponse>(content, new JsonSerializerOptions
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
            IsActive = true
        };
        
        _mockAttractionService.Setup(s => s.CreateAttraction(It.IsAny<AttractionRequest>())).Returns(expectedId);

        var content = new StringContent(JsonSerializer.Serialize(newAttraction), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/attractions", content);

        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        CreateAttractionResponse? createdResponse = JsonSerializer.Deserialize<CreateAttractionResponse>(responseContent, new JsonSerializerOptions
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
            IsActive = true
        };
        
        var content = new StringContent(JsonSerializer.Serialize(newAttraction), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync($"/api/attractions/{id}", content);
        
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        MessageResponse? updatedResponse = JsonSerializer.Deserialize<MessageResponse>(responseContent, new JsonSerializerOptions
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
        
        var response = await _client.DeleteAsync($"/api/attractions/{id}");
        
        response.EnsureSuccessStatusCode();
        Assert.AreEqual(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        _mockAttractionService.Verify(s => s.DeleteAttraction(id), Times.Once);
    }
}