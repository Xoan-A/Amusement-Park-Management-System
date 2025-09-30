using Domain;
using IBusinessLogic;
using IDataAccess;
using Models.In;
using Models.Out;

namespace BusinessLogic;

public class AttractionService : IAttractionService, IAttractionServiceEntity
{
    private readonly IAttractionRepository _attractionRepository;

    public AttractionService(IAttractionRepository attractionRepository)
    {
        _attractionRepository = attractionRepository;
    }

    public async Task<AttractionResponse> GetAttractionById(Guid id)
    {
        Attraction attraction = await _attractionRepository.GetById(id);
        return new AttractionResponse()
        {
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

    public async Task<List<AttractionResponse>> GetAllAttractions()
    {
        List<Attraction> attractions = await _attractionRepository.GetAll();
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

    public async Task<Guid> CreateAttraction(AttractionRequest newAttraction)
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
        await _attractionRepository.Create(attraction);

        return attraction.Id;
    }

    public async Task UpdateAttraction(Guid id, AttractionRequest existingAttraction)
    {
        Attraction attraction = await _attractionRepository.GetById(id);
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
        await _attractionRepository.Update(UpdatedAttraction);
    }

    public async Task DeleteAttraction(Guid id)
    {
        var attraction = await _attractionRepository.GetById(id);
        await _attractionRepository.Delete(attraction);
    }

    public async Task<Attraction> GetAttractionEntityById(Guid expectedAttractionId)
    {
        return await _attractionRepository.GetById(expectedAttractionId);
    }
}