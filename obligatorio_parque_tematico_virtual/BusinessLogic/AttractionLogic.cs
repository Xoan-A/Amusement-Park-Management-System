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

    public AttractionResponse GetAttractionById(Guid id)
    {
        Attraction attraction = _attractionRepository.GetById(id);
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
            IsActive = attraction.IsActive,
            Incidents = attraction.Incidents
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
            IsActive = attraction.IsActive,
            Incidents = attraction.Incidents
        }).ToList();
    }

    public Guid CreateAttraction(AttractionRequest newAttraction)
    {
        ValidateAttractionRequest(newAttraction);

        if (!Enum.TryParse<AttractionType>(newAttraction.Type, out AttractionType attractionType) ||
            !Enum.IsDefined(typeof(AttractionType), attractionType))
        {
            throw new ArgumentException($"Invalid attraction type: {newAttraction.Type}");
        }

        Attraction attraction = new Attraction()
        {
            Name = newAttraction.Name,
            Description = newAttraction.Description,
            Type = attractionType,
            MinAge = newAttraction.MinAge,
            MaxCapacity = newAttraction.MaxCapacity,
            CurrentCapacity = 0,
        };
        _attractionRepository.Create(attraction);

        return attraction.Id;
    }

    public void UpdateAttraction(Guid id, AttractionRequest existingAttraction)
    {
        ValidateAttractionRequest(existingAttraction, true, id);
        Attraction attraction = _attractionRepository.GetById(id);
        if (attraction == null)
        {
            throw new KeyNotFoundException($"No se encontró la atracción con id {id}");
        }

        if (!Enum.TryParse<AttractionType>(existingAttraction.Type, out AttractionType attractionType) ||
            !Enum.IsDefined(typeof(AttractionType), attractionType))
        {
            throw new ArgumentException($"Invalid attraction type: {existingAttraction.Type}");
        }

        attraction.Name = existingAttraction.Name;
        attraction.Description = existingAttraction.Description;
        attraction.Type = attractionType;
        attraction.MinAge = existingAttraction.MinAge;
        attraction.MaxCapacity = existingAttraction.MaxCapacity;
        attraction.CurrentCapacity = existingAttraction.CurrentCapacity ?? attraction.CurrentCapacity;
        _attractionRepository.Update(attraction);
    }

    public void DeleteAttraction(Guid id)
    {
        Attraction attraction = _attractionRepository.GetById(id);
        if (attraction == null)
        {
            throw new KeyNotFoundException($"No se encontró la atracción con id {id}");
        }

        _attractionRepository.Delete(attraction);
    }

    public Attraction GetAttractionEntityById(Guid expectedAttractionId)
    {
        return _attractionRepository.GetById(expectedAttractionId);
    }

    public void AddIncident(Guid attractionId, string incidence)
    {
        Attraction attraction = _attractionRepository.GetById(attractionId);
        if (attraction == null)
        {
            throw new KeyNotFoundException($"No se encontró la atracción con id {attractionId}");
        }

        attraction.AddIncident(incidence);
        _attractionRepository.Update(attraction);
    }

    public void RemoveIncident(Guid attractionId, string incidence)
    {
        Attraction attraction = _attractionRepository.GetById(attractionId);
        if (attraction == null)
        {
            throw new KeyNotFoundException($"No se encontró la atracción con id {attractionId}");
        }

        attraction.RemoveIncident(incidence);
        _attractionRepository.Update(attraction);
    }

    public AttractionsVisitResponse GetAllAttractionsVisits(AttractionsVisitsRequest request)
    {
        DateTime startDate = request.StartDate;
        DateTime endDate = request.EndDate;

        if (startDate > endDate)
        {
            throw new ArgumentException("La fecha de inicio no puede ser posterior a la fecha de fin.");
        }

        List<Report> reports = _reportRepository.GetAllReports();
        List<Report> filteredReports =
        reports.Where(r => r.EnterDate.Date >= startDate.Date && r.EnterDate.Date <= endDate.Date).ToList();

        AttractionsVisitResponse attractionsVisits = new AttractionsVisitResponse();
        System.Collections.Generic.IEnumerable<System.Linq.IGrouping<Guid, Report>> groupedReports =
        filteredReports.GroupBy(r => r.AttractionId);
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
            attractionsVisits.AttractionsVisits.Add(new AttractionVisitDetail
            {
                Attraction = attractionRes,
                VisitCount = visitCount
            });
        }

        return attractionsVisits;
    }

    private void ValidateAttractionRequest(AttractionRequest request, bool checkCurrentCapacity = false,
        Guid? excludeAttractionId = null)
    {
        if (!IsValidName(request.Name, excludeAttractionId))
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

    private bool IsAttractionNameUnique(string name, Guid? excludeId = null)
    {
        Attraction existingAttraction = _attractionRepository.GetByName(name);

        if (existingAttraction == null)
            return true;

        if (excludeId.HasValue && existingAttraction.Id == excludeId.Value)
            return true;

        return false;
    }

    private bool IsValidName(string name, Guid? excludeAttractionId = null)
    {
        return !string.IsNullOrWhiteSpace(name)
               && name.Length <= _nameMaxLength
               && IsAttractionNameUnique(name, excludeAttractionId);
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