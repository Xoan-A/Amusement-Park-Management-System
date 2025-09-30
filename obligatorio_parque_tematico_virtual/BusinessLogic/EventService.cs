using IBusinessLogic;
using IDataAccess;
using Models.Out;
using Domain;

namespace BusinessLogic;

public class EventService : IEventService
{
    IEventRepository _eventRepository;
    public EventService(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
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
}