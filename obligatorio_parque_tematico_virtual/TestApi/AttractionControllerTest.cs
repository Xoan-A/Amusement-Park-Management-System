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
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(_mockAttractionService.Object);
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
    public async Task GetAttractions_ValidRequest_ReturnsAttractionResponse()
    {
        Guid id = Guid.NewGuid();
        var expectedResponse = new AllAttractionsResponse
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
        
        _mockAttractionService.Setup(s => s.GetAllAttractions())
            .Returns(new List<Attraction> { new Attraction
            {
                Id = id,
                Name = "Eiffel Tower",
                Description = "Paris",
                Type = AttractionType.InteractiveZone,
                MinAge = 10,
                MaxCapacity = 100,
                CurrentCapacity = 50,
                IsActive = true
            }});
        
        var response = await _client.GetAsync("/api/attractions");
        
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var attractionsResponse = JsonSerializer.Deserialize<AllAttractionsResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        Assert.IsNotNull(attractionsResponse);
        Assert.AreEqual(id, attractionsResponse[0].Id);
        Assert.AreEqual("Eiffel Tower", attractionsResponse[0].Name);
        Assert.AreEqual("Paris", attractionsResponse[0].Description);
        Assert.AreEqual("InteractiveZone", attractionsResponse[0].Type);
        Assert.AreEqual(10, attractionsResponse[0].MinAge);
        Assert.AreEqual(100, attractionsResponse[0].MaxCapacity);
        Assert.AreEqual(50, attractionsResponse[0].CurrentCapacity);
        Assert.AreEqual(true, attractionsResponse[0].IsActive);
    }
}