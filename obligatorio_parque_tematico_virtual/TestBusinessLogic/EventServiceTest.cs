using Moq;
using Domain;
using IBusinessLogic;
using IDataAccess;
using BusinessLogic;
using Models.In;
using Models.Out;

namespace TestBusinessLogic;

[TestClass]
public class EventServiceTest
{
    private Mock<IEventRepository> _mockEventRepository;
    private IEventService _eventService;
    
    [TestInitialize]
    public void Setup()
    {
        _mockEventRepository = new Mock<IEventRepository>();
        _eventService = new EventService(_mockEventRepository.Object);
    }

    [TestMethod]
    public async Task GetEventById_ShouldReturnEvent_WhenIdIsValid()
    {
        Event expectedEvent = new Event
        {
            Id = Guid.NewGuid(),
            Name = "Music Festival",
            Date = new DateTime(2024, 8, 15),
            Hour = 10,
            MaxCapacity = 5000,
            CurrentCapacity = 0,
            Cost = 100,
            Attractions = []
        };
        _mockEventRepository.Setup(r => r.GetById(expectedEvent.Id)).ReturnsAsync(expectedEvent);
        EventResponse result = await _eventService.GetEventById(expectedEvent.Id);
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedEvent.Name, result.Name);
        _mockEventRepository.Verify(r => r.GetById(expectedEvent.Id), Times.Once);
    }
}