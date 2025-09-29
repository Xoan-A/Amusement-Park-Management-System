using Domain;
using IBusinessLogic;
using IDataAccess;
using Models.In;
using Models.Out;

namespace BusinessLogic;

public class AttractionService : IAttractionService
{
    private readonly IAttractionRepository _attractionRepository;
    
    public AttractionService(IAttractionRepository attractionRepository)
    {
        _attractionRepository = attractionRepository;
    }
    
    public AttractionResponse GetAttractionById(Guid id)
    {
        Attraction attraction = _attractionRepository.GetById(id);
        return new AttractionResponse(){
            Id = attraction.Id,
            Name = attraction.Name,
            Description = attraction.Description,
            Type = attraction.Type.ToString(),
            MinAge = attraction.MinAge,
            MaxCapacity = attraction.MaxCapacity,
            CurrentCapacity = attraction.CurrentCapacity,
            IsActive = attraction.IsActive
        };
    }
    
    public List<AttractionResponse> GetAllAttractions()
    {
        List<Attraction> attractions = _attractionRepository.GetAll();
        return attractions.Select(attraction => new AttractionResponse()
        {
            Id = attraction.Id,
            Name = attraction.Name,
            Description = attraction.Description,
            Type = attraction.Type.ToString(),
            MinAge = attraction.MinAge,
            MaxCapacity = attraction.MaxCapacity,
            CurrentCapacity = attraction.CurrentCapacity,
            IsActive = attraction.IsActive
        }).ToList();
    }
    
    public Guid CreateAttraction(AttractionRequest newAttraction)
    {
        Attraction attraction = new Attraction()
        {
            Name = newAttraction.Name,
            Description = newAttraction.Description,
            Type = Enum.Parse<AttractionType>(newAttraction.Type),
            MinAge = newAttraction.MinAge,
            MaxCapacity = newAttraction.MaxCapacity,
            CurrentCapacity = 0,
            IsActive = newAttraction.IsActive
        };
        _attractionRepository.Create(attraction);

        return attraction.Id;
    }
    
    public void UpdateAttraction(Guid id, AttractionRequest existingAttraction)
    {
        Attraction attraction = _attractionRepository.GetById(id);
        var UpdatedAttraction = new Attraction()
        {
            Id = id,
            Name = existingAttraction.Name,
            Description = existingAttraction.Description,
            Type = Enum.Parse<AttractionType>(existingAttraction.Type),
            MinAge = existingAttraction.MinAge,
            MaxCapacity = existingAttraction.MaxCapacity,
            CurrentCapacity = attraction.CurrentCapacity,
            IsActive = existingAttraction.IsActive
        };
        _attractionRepository.Update(UpdatedAttraction);
    }
    
    public void RemoveAttraction(Guid id)
    {
        var attraction = _attractionRepository.GetById(id);
        _attractionRepository.Remove(attraction);
    }
}