using IBusinessLogic;
using IDataAccess;
using Models.Out;
using Domain;
using Models.In;

namespace BusinessLogic;

public class EventService : IEventService
{
    IEventRepository _eventRepository;
    IAttractionServiceEntitys _attractionService;
    public EventService(IEventRepository eventRepository, IAttractionServiceEntitys attractionService)
    {
        _eventRepository = eventRepository;
        _attractionService = attractionService;
    }

    public async Task<EventResponse> GetEventById(Guid expectedEventId)
    {
        Event eventEntity = await _eventRepository.GetById(expectedEventId);
    
        var eventResponse = new EventResponse
        {
            Id = eventEntity.Id,
            Name = eventEntity.Name,
            Date = eventEntity.Date,
            Hour = eventEntity.Hour,
            MaxCapacity = eventEntity.MaxCapacity,
            CurrentCapacity = eventEntity.CurrentCapacity,
            Cost = eventEntity.Cost,
            Attractions = eventEntity.Attractions
                .Select(ea => new AttractionResponse
                {
                    Id = ea.Attraction.Id,
                    Name = ea.Attraction.Name,
                    Description = ea.Attraction.Description,
                    Type = ea.Attraction.Type.ToString(),
                    MinAge = ea.Attraction.MinAge,
                    MaxCapacity = ea.Attraction.MaxCapacity,
                    CurrentCapacity = ea.Attraction.CurrentCapacity,
                    IsActive = ea.Attraction.IsActive
                })
                .ToList()
        };
    
        return eventResponse;
    }

    public async Task<List<EventResponse>> GetAllEvents()
    {
        var events = await _eventRepository.GetAll();
    
        var eventResponses = events.Select(eventEntity => new EventResponse
        {
            Id = eventEntity.Id,
            Name = eventEntity.Name,
            Date = eventEntity.Date,
            Hour = eventEntity.Hour,
            MaxCapacity = eventEntity.MaxCapacity,
            CurrentCapacity = eventEntity.CurrentCapacity,
            Cost = eventEntity.Cost,
            Attractions = eventEntity.Attractions
                .Select(ea => new AttractionResponse
                {
                    Id = ea.Attraction.Id,
                    Name = ea.Attraction.Name,
                    Description = ea.Attraction.Description,
                    Type = ea.Attraction.Type.ToString(),
                    MinAge = ea.Attraction.MinAge,
                    MaxCapacity = ea.Attraction.MaxCapacity,
                    CurrentCapacity = ea.Attraction.CurrentCapacity,
                    IsActive = ea.Attraction.IsActive
                })
                .ToList()
        }).ToList();
    
        return eventResponses;
    }

    public async Task<Guid> CreateEvent(EventRequest newEvent)
    {
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
            foreach (var attractionId in newEvent.AttractionIds)
            {
                Attraction attraction = await _attractionService.GetAttractionEntityById(attractionId);
                eventEntity.AddAttraction(attraction);
            }
        }
        
        await _eventRepository.Create(eventEntity);
        
        return eventEntity.Id;
    }
}