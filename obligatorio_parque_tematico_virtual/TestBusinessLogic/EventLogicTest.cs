using AutoMapper;
using Moq;
using Domain;
using IBusinessLogic;
using IDataAccess;
using BusinessLogic;
using Models.In;
using Models.Out;
using BusinessLogic.Mapping;

namespace TestBusinessLogic;

[TestClass]
public class EventLogicTest
{
    private Mock<IEventRepository> _mockEventRepository;
    private IEventLogic _eventLogic;
    private Mock<IAttractionLogicEntity> _mockAttractionService;
    private Mock<IDateTimeLogic> _mockDateTimeLogic;
    private IMapper _mapper;
    private Event baseEvent;
    private EventRequest baseEventRequest;

    [TestInitialize]
    public void Setup()
    {
        _mockEventRepository = new Mock<IEventRepository>();
        _mockAttractionService = new Mock<IAttractionLogicEntity>();
        _mockDateTimeLogic = new Mock<IDateTimeLogic>();
        _mockDateTimeLogic.Setup(x => x.GetCurrentDateTime()).Returns(DateTime.Now);

        MapperConfiguration configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = configuration.CreateMapper();

        _eventLogic = new EventLogic(_mockEventRepository.Object, _mockAttractionService.Object,
            _mockDateTimeLogic.Object, _mapper);
        baseEvent = new Event
        {
            Name = "Base Event",
            Date = new DateTime(2025, 10, 10),
            Hour = 10,
            MaxCapacity = 1000,
            CurrentCapacity = 100,
            Cost = 50,
            Attractions = new List<EventAttraction>()
        };
        baseEventRequest = new EventRequest
        {
            Name = "Base Event",
            Date = DateTime.Now.AddDays(7),
            Hour = 10,
            MaxCapacity = 1000,
            Cost = 50,
            AttractionIds = new List<Guid>()
        };
    }

    [TestMethod]
    public void GetEventById_ShouldReturnEvent_WhenIdIsValid()
    {
        _mockEventRepository.Setup(r => r.GetById(baseEvent.Id)).Returns(baseEvent);
        EventResponse result = _eventLogic.GetEventById(baseEvent.Id);
        Assert.AreEqual(baseEvent.Name, result.Name);
        _mockEventRepository.Verify(r => r.GetById(baseEvent.Id), Times.Once);
    }

    [TestMethod]
    public void GetEventById_ShouldReturnEventWithAttractions_WhenEventHasAttractions()
    {
        Attraction attraction1 = new Attraction
        {
            Id = Guid.NewGuid(),
            Name = "Roller Coaster",
            Description = "A thrilling ride",
            Type = AttractionType.RollerCoaster,
            MinAge = 12,
            MaxCapacity = 20,
            CurrentCapacity = 5,
        };
        baseEvent.AddAttraction(attraction1);
        _mockEventRepository.Setup(r => r.GetById(baseEvent.Id)).Returns(baseEvent);
        EventResponse result = _eventLogic.GetEventById(baseEvent.Id);

        Assert.AreEqual(baseEvent.Name, result.Name);
        Assert.AreEqual(1, result.Attractions.Count);
        Assert.IsTrue(result.Attractions.Any(a => a.Name == "Roller Coaster"));
    }

    [TestMethod]
    public void GetEventById_ShouldThrowException_WhenEventIsNull()
    {
        Guid eventId = Guid.NewGuid();
        _mockEventRepository.Setup(r => r.GetById(eventId)).Returns((Event)null);

        Assert.ThrowsException<KeyNotFoundException>(() => _eventLogic.GetEventById(eventId));
    }

    [TestMethod]
    public void GetAllEvents_ShouldReturnListOfEvents()
    {
        Event event2 = new Event
        {
            Id = Guid.NewGuid(),
            Name = "Art Expo",
            Date = baseEvent.Date,
            Hour = baseEvent.Hour,
            MaxCapacity = baseEvent.MaxCapacity,
            CurrentCapacity = baseEvent.CurrentCapacity,
            Cost = baseEvent.Cost,
            Attractions = new List<EventAttraction>()
        };
        List<Event> expectedEvents = new List<Event> { baseEvent, event2 };
        _mockEventRepository.Setup(r => r.GetAll()).Returns(expectedEvents);

        List<EventResponse> result = _eventLogic.GetAllEvents();

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(baseEvent.Name, result[0].Name);
        Assert.AreEqual(event2.Name, result[1].Name);
        _mockEventRepository.Verify(r => r.GetAll(), Times.Once);
    }

    [TestMethod]
    public void GetAllEvents_ShouldReturnEventsWithAttractions()
    {
        Attraction attraction = new Attraction
        {
            Id = Guid.NewGuid(),
            Name = "Montaña Rusa",
            Description = "Atracción rápida",
            Type = AttractionType.RollerCoaster,
            MinAge = 10,
            MaxCapacity = 20,
            CurrentCapacity = 5,
        };

        baseEvent.AddAttraction(attraction);

        List<Event> eventsList = new List<Event> { baseEvent };
        _mockEventRepository.Setup(r => r.GetAll()).Returns(eventsList);

        List<EventResponse> result = _eventLogic.GetAllEvents();

        Assert.AreEqual(1, result.Count);
        EventResponse returnedEvent = result[0];
        Assert.AreEqual(baseEvent.Name, returnedEvent.Name);
        AttractionResponse returnedAttraction = returnedEvent.Attractions[0];
        Assert.AreEqual(attraction.Name, returnedAttraction.Name);
    }

    [TestMethod]
    public void CreateEvent_ShouldCreateEvent_WhenDataIsValid()
    {
        _mockEventRepository.Setup(r => r.GetAll()).Returns(new List<Event>());
        _mockEventRepository.Setup(r => r.Create(It.IsAny<Event>())).Callback<Event>(e => e.Id = baseEvent.Id)
        ;

        Guid resultId = _eventLogic.CreateEvent(baseEventRequest);

        Assert.AreEqual(baseEvent.Id, resultId);
    }

    [TestMethod]
    public void CreateEvent_ShouldAddAttractions_WhenEventHasAttractions()
    {
        List<Guid> attractionIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        baseEventRequest.AttractionIds = attractionIds;

        _mockAttractionService.Setup(s => s.GetAttractionEntityById(It.IsAny<Guid>()))
        .Returns((Guid id) => new Attraction { Id = id, Name = $"Attraction-{id}" });


        _mockEventRepository.Setup(r => r.Create(It.IsAny<Event>()))
        .Callback<Event>(e =>
        {
            Assert.AreEqual(attractionIds.Count, e.Attractions.Count);
            Assert.AreEqual(attractionIds[0], e.Attractions[0].AttractionId);
            Assert.AreEqual(attractionIds[1], e.Attractions[1].AttractionId);
        })
        ;

        _eventLogic.CreateEvent(baseEventRequest);
    }

    [TestMethod]
    public void CreateEvent_ShouldThrowException_WhenAttractionNotFound()
    {
        Guid nonExistentAttractionId = Guid.NewGuid();
        baseEventRequest.AttractionIds = new List<Guid> { nonExistentAttractionId };

        _mockEventRepository.Setup(r => r.GetAll()).Returns(new List<Event>());
        _mockAttractionService.Setup(s => s.GetAttractionEntityById(nonExistentAttractionId))
        .Returns((Attraction)null);

        Assert.ThrowsException<KeyNotFoundException>(() =>
        _eventLogic.CreateEvent(baseEventRequest));
    }

    [TestMethod]
    public void CreateEvent_ShouldThrowException_WhenNameIsNotUnique()
    {
        _mockEventRepository.Setup(r => r.GetAll()).Returns(new List<Event> { baseEvent });

        Assert.ThrowsException<ArgumentException>(() =>
        _eventLogic.CreateEvent(baseEventRequest));
    }

    [TestMethod]
    public void CreateEvent_ShouldThrowException_WhenNameIsEmpty()
    {
        _mockEventRepository.Setup(r => r.GetAll()).Returns(new List<Event>());
        baseEventRequest.Name = String.Empty;
        Assert.ThrowsException<ArgumentException>(() =>
        _eventLogic.CreateEvent(baseEventRequest));
    }

    [TestMethod]
    public void CreateEvent_ShouldThrowException_WhenDateIsNotFuture()
    {
        _mockEventRepository.Setup(r => r.GetAll()).Returns(new List<Event>());
        baseEventRequest.Date = DateTime.Now.AddDays(-1);
        Assert.ThrowsException<ArgumentException>(() =>
        _eventLogic.CreateEvent(baseEventRequest));
    }

    [TestMethod]
    public void CreateEvent_ShouldThrowException_WhenHourIsInvalid()
    {
        _mockEventRepository.Setup(r => r.GetAll()).Returns(new List<Event>());
        baseEventRequest.Hour = -1;
        Assert.ThrowsException<ArgumentException>(() =>
        _eventLogic.CreateEvent(baseEventRequest));
    }

    [TestMethod]
    public void CreateEvent_ShouldThrowException_WhenMaxCapacityIsInvalid()
    {
        _mockEventRepository.Setup(r => r.GetAll()).Returns(new List<Event>());
        baseEventRequest.MaxCapacity = -1;
        Assert.ThrowsException<ArgumentException>(() =>
        _eventLogic.CreateEvent(baseEventRequest));
        baseEventRequest.MaxCapacity = 10001;
        Assert.ThrowsException<ArgumentException>(() =>
        _eventLogic.CreateEvent(baseEventRequest));
    }

    [TestMethod]
    public void CreateEvent_ShouldThrowException_WhenCostIsInvalid()
    {
        _mockEventRepository.Setup(r => r.GetAll()).Returns(new List<Event>());
        baseEventRequest.Cost = -1;
        Assert.ThrowsException<ArgumentException>(() =>
        _eventLogic.CreateEvent(baseEventRequest));
    }

    [TestMethod]
    public void DeleteEvent_ShouldCallRepositoryDelete_WhenEventExists()
    {
        _mockEventRepository.Setup(r => r.GetById(baseEvent.Id)).Returns(baseEvent);
        _mockEventRepository.Setup(r => r.Delete(baseEvent));

        _eventLogic.DeleteEvent(baseEvent.Id);

        _mockEventRepository.Verify(r => r.GetById(baseEvent.Id), Times.Once);
        _mockEventRepository.Verify(r => r.Delete(baseEvent), Times.Once);
    }

    [TestMethod]
    public void DeleteEvent_ShouldThrowException_WhenEventDoesNotExist()
    {
        Guid eventId = Guid.NewGuid();
        _mockEventRepository.Setup(r => r.GetById(eventId)).Returns((Event)null);
        Assert.ThrowsException<KeyNotFoundException>(() => _eventLogic.DeleteEvent(eventId));
    }
}