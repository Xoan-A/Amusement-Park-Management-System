using Models.In;
using Models.Out;

namespace IBusinessLogic;

public interface IAttractionLogic
{
    AttractionResponse GetAttractionById(Guid id);
    List<AttractionResponse> GetAllAttractions();
    Guid CreateAttraction(AttractionRequest newAttraction);
    void UpdateAttraction(Guid id, AttractionRequest existingAttraction);
    void DeleteAttraction(Guid id);
    void AddIncident(Guid id, string incident);
    void RemoveIncident(Guid id, string incident);
    AttractionsVisitResponse GetAllAttractionsVisits(AttractionsVisitsRequest request);
}