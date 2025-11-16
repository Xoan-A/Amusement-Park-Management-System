using AutoMapper;
using IBusinessLogic;
using IDataAccess;
using Models.Out;
using Domain;
using Models.In;

namespace BusinessLogic;

public class EventLogic : IEventLogic
{
    private IEventRepository _eventRepository;
    private IAttractionLogicEntity _attractionLogic;
    private readonly IDateTimeLogic _dateTimeLogic;
    private readonly IMapper _mapper;
    private const int MinCapacityLimit = 1;
    private const int MaxCapacityLimit = 10000;
    private const int MinHour = 0;
    private const int MaxHour = 23;
    private const int MinCost = 1;

    public EventLogic(IEventRepository eventRepository, IAttractionLogicEntity attractionLogic,
        IDateTimeLogic dateTimeLogic, IMapper mapper)
    {
        _eventRepository = eventRepository;
        _attractionLogic = attractionLogic;
        _dateTimeLogic = dateTimeLogic;
        _mapper = mapper;
    }

    public EventResponse GetEventById(Guid expectedEventId)
    {
        Event eventEntity = _eventRepository.GetById(expectedEventId);
        if (eventEntity == null)
        {
            throw new KeyNotFoundException($"Event with id {expectedEventId} not found");
        }

        return _mapper.Map<EventResponse>(eventEntity);
    }

    public List<EventResponse> GetAllEvents()
    {
        List<Event> events = _eventRepository.GetAll();
        return _mapper.Map<List<EventResponse>>(events);
    }

    public Guid CreateEvent(EventRequest newEvent)
    {
        ValidateEventRequest(newEvent);
        Event eventEntity = new Event()
        {
            Name = newEvent.Name,
            Date = newEvent.Date,
            Hour = newEvent.Hour,
            MaxCapacity = newEvent.MaxCapacity,
            CurrentCapacity = 0,
            Cost = newEvent.Cost,
            Attractions = new List<EventAttraction>()
        };
        if (newEvent.AttractionIds != null)
        {
            foreach (Guid attractionId in newEvent.AttractionIds)
            {
                Attraction attraction = _attractionLogic.GetAttractionEntityById(attractionId);
                if (attraction == null)
                {
                    throw new KeyNotFoundException($"Attraction with id {attractionId} not found");
                }

                eventEntity.AddAttraction(attraction);
            }
        }

        _eventRepository.Create(eventEntity);
        return eventEntity.Id;
    }

    public void DeleteEvent(Guid eventId)
    {
        Event eventEntity = _eventRepository.GetById(eventId);
        if (eventEntity == null)
        {
            throw new KeyNotFoundException($"Event with id {eventId} not found");
        }

        _eventRepository.Delete(eventEntity);
    }

    private void ValidateEventRequest(EventRequest newEvent)
    {
        if (string.IsNullOrWhiteSpace(newEvent.Name))
            throw new ArgumentException("Event name cannot be empty.");
        if (!IsEventNameUnique(newEvent.Name))
            throw new ArgumentException("Event name already exists.");

        DateTime currentDateTime = _dateTimeLogic.GetCurrentDateTime();
        if (newEvent.Date <= currentDateTime)
            throw new ArgumentException("Event date must be in the future.");

        if (newEvent.Hour < MinHour || newEvent.Hour > MaxHour)
            throw new ArgumentException("Hour must be between 0 and 23.");
        if (newEvent.MaxCapacity <= MinCapacityLimit || newEvent.MaxCapacity > MaxCapacityLimit)
            throw new ArgumentException(
                $"Max capacity must be greater than 1 and less than or equal to {MaxCapacityLimit}.");
        if (newEvent.Cost <= MinCost)
            throw new ArgumentException("Cost must be greater than 0.");
    }

    private bool IsEventNameUnique(string name)
    {
        List<Event> events = _eventRepository.GetAll() ?? new List<Event>();
        return !events.Any(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}