using Domain;
using IBusinessLogic;
using IDataAccess;
using Models.In;
using Models.Out;

namespace BusinessLogic;

public class AttractionService : IAttractionService, IAttractionServiceEntity
{
    private readonly IAttractionRepository _attractionRepository;
    private readonly int nameMaxLength = 100;
    private readonly int maxDescriptionLength = 500;
    private readonly int maxMinAge = 25;
    private readonly int maxCapacityLimit = 1000;

    public AttractionService(IAttractionRepository attractionRepository)
    {
        _attractionRepository = attractionRepository;
    }

    public async Task<AttractionResponse> GetAttractionById(Guid id)
    {
        Attraction attraction = await _attractionRepository.GetById(id);
        if (attraction == null)
        {
            throw new KeyNotFoundException($"No se encontró la atracción con id {id}");
        }
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
        if (!await IsValidNameAsync(newAttraction.Name))
        {
            throw new ArgumentException("Invalid or duplicate attraction name.");
        }
        if (!IsValidDescription(newAttraction.Description))
        {
            throw new ArgumentException("Invalid attraction description.");
        }
        if (!IsValidMinAge(newAttraction.MinAge))
        {
            throw new ArgumentException("Invalid minimum age.");
        }
        if (!IsValidMaxCapacity(newAttraction.MaxCapacity))
        {
            throw new ArgumentException("Invalid maximum capacity.");
        }
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
        if (!await IsValidNameAsync(existingAttraction.Name))
        {
            throw new ArgumentException("Invalid or duplicate attraction name.");
        }
        if (!IsValidDescription(existingAttraction.Description))
        {
            throw new ArgumentException("Invalid attraction description.");
        }
        if (!IsValidMinAge(existingAttraction.MinAge))
        {
            throw new ArgumentException("Invalid minimum age.");
        }
        if (!IsValidMaxCapacity(existingAttraction.MaxCapacity))
        {
            throw new ArgumentException("Invalid maximum capacity.");
        }
        if (existingAttraction.CurrentCapacity < 0 || existingAttraction.CurrentCapacity > existingAttraction.MaxCapacity)
        {
            throw new ArgumentException("Invalid current capacity.");
        }
        
        Attraction attraction = await _attractionRepository.GetById(id);
        Attraction updatedAttraction = new Attraction()
        {
            Id = id,
            Name = existingAttraction.Name,
            Description = existingAttraction.Description,
            Type = Enum.Parse<AttractionType>(existingAttraction.Type),
            MinAge = existingAttraction.MinAge,
            MaxCapacity = existingAttraction.MaxCapacity,
            CurrentCapacity = existingAttraction.CurrentCapacity ?? attraction.CurrentCapacity,
            IsActive = existingAttraction.IsActive
        };
        await _attractionRepository.Update(updatedAttraction);
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
    
    private async Task<bool> IsAttractionNameUnique(string name)
    {
        return await _attractionRepository.IsNameUnique(name);
    }
    
    private async Task<bool> IsValidNameAsync(string name)
    {
        return !string.IsNullOrWhiteSpace(name) 
               && name.Length <= nameMaxLength 
               && await IsAttractionNameUnique(name);
    }
    
    private bool IsValidDescription(string description)
    {
        return !string.IsNullOrWhiteSpace(description) && description.Length <= maxDescriptionLength;
    }
    
    private bool IsValidMinAge(int minAge)
    {
        return minAge >= 0 && minAge <= maxMinAge;
    }
    
    private bool IsValidMaxCapacity(int maxCapacity)
    {
        return maxCapacity > 0 && maxCapacity <= maxCapacityLimit;
    }
}