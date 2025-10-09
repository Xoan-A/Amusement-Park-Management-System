using Models.In;
using Models.Out;

namespace IBusinessLogic;

public interface IAttractionLogic
{
    Task<AttractionResponse> GetAttractionById(Guid id);
    Task<List<AttractionResponse>> GetAllAttractions();
    Task<Guid> CreateAttraction(AttractionRequest newAttraction);
    Task UpdateAttraction(Guid id, AttractionRequest existingAttraction);
    Task DeleteAttraction(Guid id);
    Task AddIncident(Guid id, string incident);
    Task RemoveIncident(Guid id, string incident);
    Task<List<string>> GetAttractionIncidents(Guid id);
    Task<CapacityResponse> GetCapacity(Guid id);
    Task<AttractionsVisitResponse> GetAllAttractionsVisits(AttractionsVisitsRequest request);
}