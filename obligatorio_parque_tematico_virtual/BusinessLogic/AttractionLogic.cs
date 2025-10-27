using Domain;
using IBusinessLogic;
using IDataAccess;
using Models.In;
using Models.Out;

namespace BusinessLogic;

public class AttractionLogic : IAttractionLogic, IAttractionLogicEntity
{
    private readonly IAttractionRepository _attractionRepository;
    private readonly IReportRepository _reportRepository;

    private readonly int _nameMaxLength = 100;
    private readonly int _maxDescriptionLength = 500;
    private readonly int _maxMinAge = 25;
    private readonly int _minMinAge = 0;
    private readonly int _maxCapacityLimit = 1000;
    private readonly int _minCapacityLimit = 0;
    private readonly int _minCurrentCapacity = 0;
    private readonly int _noIncidents = 0;

    public AttractionLogic(IAttractionRepository attractionRepository, IReportRepository reportRepository)
    {
        _attractionRepository = attractionRepository;
        _reportRepository = reportRepository;
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
        await ValidateAttractionRequest(newAttraction);
        Attraction attraction = new Attraction()
        {
            Name = newAttraction.Name,
            Description = newAttraction.Description,
            Type = Enum.Parse<AttractionType>(newAttraction.Type),
            MinAge = newAttraction.MinAge,
            MaxCapacity = newAttraction.MaxCapacity,
            CurrentCapacity = 0,
        };
        await _attractionRepository.Create(attraction);

        return attraction.Id;
    }

    public async Task UpdateAttraction(Guid id, AttractionRequest existingAttraction)
    {
        await ValidateAttractionRequest(existingAttraction, true, id);
        Attraction attraction = await _attractionRepository.GetById(id);
        attraction.Name = existingAttraction.Name;
        attraction.Description = existingAttraction.Description;
        attraction.Type = Enum.Parse<AttractionType>(existingAttraction.Type);
        attraction.MinAge = existingAttraction.MinAge;
        attraction.MaxCapacity = existingAttraction.MaxCapacity;
        attraction.CurrentCapacity = existingAttraction.CurrentCapacity ?? attraction.CurrentCapacity;
        await _attractionRepository.Update(attraction);
    }

    public async Task DeleteAttraction(Guid id)
    {
        Attraction attraction = await _attractionRepository.GetById(id);
        await _attractionRepository.Delete(attraction);
    }

    public async Task<Attraction> GetAttractionEntityById(Guid expectedAttractionId)
    {
        return await _attractionRepository.GetById(expectedAttractionId);
    }

    public async Task<CapacityResponse> GetCapacity(Guid id)
    {
        Attraction attraction = await _attractionRepository.GetById(id);
        return new CapacityResponse()
        {
            Id = attraction.Id,
            Capacity = attraction.MaxCapacity,
            CurrentCapacity = attraction.CurrentCapacity
        };
    }

    public async Task<List<string>> GetAttractionIncidents(Guid attractionId)
    {
        Attraction attraction = await _attractionRepository.GetById(attractionId);
        if (attraction == null)
        {
            throw new KeyNotFoundException($"No se encontró la atracción con id {attractionId}");
        }

        if (attraction.Incidents.Count == _noIncidents)
        {
            throw new KeyNotFoundException($"La atracción con id {attractionId} no tiene incidencias");
        }

        return attraction.Incidents;
    }

    public async Task AddIncident(Guid attractionId, string incidence)
    {
        Attraction attraction = await _attractionRepository.GetById(attractionId);
        if (attraction == null)
        {
            throw new KeyNotFoundException($"No se encontró la atracción con id {attractionId}");
        }

        attraction.AddIncident(incidence);
        await _attractionRepository.Update(attraction);
    }

    public async Task RemoveIncident(Guid attractionId, string incidence)
    {
        Attraction attraction = await _attractionRepository.GetById(attractionId);
        if (attraction == null)
        {
            throw new KeyNotFoundException($"No se encontró la atracción con id {attractionId}");
        }

        attraction.RemoveIncident(incidence);
        await _attractionRepository.Update(attraction);
    }

    public async Task<AttractionsVisitResponse> GetAllAttractionsVisits(AttractionsVisitsRequest request)
    {
        DateTime startDate = request.StartDate;
        DateTime endDate = request.EndDate;

        if (startDate > endDate)
        {
            throw new ArgumentException("La fecha de inicio no puede ser posterior a la fecha de fin.");
        }

        List<Report> reports = await _reportRepository.GetAllReports();
        List<Report> filteredReports = reports.Where(r => r.EnterDate >= startDate && r.EnterDate <= endDate).ToList();

        AttractionsVisitResponse attractionsVisits = new AttractionsVisitResponse();
        System.Collections.Generic.IEnumerable<System.Linq.IGrouping<Guid, Report>> groupedReports = filteredReports.GroupBy(r => r.AttractionId);
        foreach (System.Linq.IGrouping<Guid, Report> group in groupedReports)
        {
            Attraction attraction = group.First().Attraction;
            int visitCount = group.Count();
            AttractionResponse attractionRes = new AttractionResponse()
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
            attractionsVisits.AttractionsVisits.Add((attractionRes, visitCount));
        }

        return attractionsVisits;
    }

    private async Task ValidateAttractionRequest(AttractionRequest request, bool checkCurrentCapacity = false, Guid? excludeAttractionId = null)
    {
        if (!await IsValidNameAsync(request.Name, excludeAttractionId))
        {
            throw new ArgumentException("Invalid or duplicate attraction name.");
        }

        if (!IsValidDescription(request.Description))
        {
            throw new ArgumentException("Invalid attraction description.");
        }

        if (!IsValidMinAge(request.MinAge))
        {
            throw new ArgumentException("Invalid minimum age.");
        }

        if (!IsValidMaxCapacity(request.MaxCapacity))
        {
            throw new ArgumentException("Invalid maximum capacity.");
        }

        if (checkCurrentCapacity)
        {
            if (request.CurrentCapacity < _minCurrentCapacity || request.CurrentCapacity > request.MaxCapacity)
            {
                throw new ArgumentException("Invalid current capacity.");
            }
        }
    }

    private async Task<bool> IsAttractionNameUnique(string name, Guid? excludeId = null)
    {
        Attraction existingAttraction = await _attractionRepository.GetByName(name);

        if (existingAttraction == null)
            return true;

        if (excludeId.HasValue && existingAttraction.Id == excludeId.Value)
            return true;

        return false;
    }

    private async Task<bool> IsValidNameAsync(string name, Guid? excludeAttractionId = null)
    {
        return !string.IsNullOrWhiteSpace(name)
               && name.Length <= _nameMaxLength
               && await IsAttractionNameUnique(name, excludeAttractionId);
    }

    private bool IsValidDescription(string description)
    {
        return !string.IsNullOrWhiteSpace(description) && description.Length <= _maxDescriptionLength;
    }

    private bool IsValidMinAge(int minAge)
    {
        return minAge >= _minMinAge && minAge <= _maxMinAge;
    }

    private bool IsValidMaxCapacity(int maxCapacity)
    {
        return maxCapacity > _minCapacityLimit && maxCapacity <= _maxCapacityLimit;
    }
}