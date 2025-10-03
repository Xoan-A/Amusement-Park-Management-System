using Domain;
using Models.In;
using Models.Out;

namespace IBusinessLogic;

public interface IAttractionService
{
    Task<AttractionResponse> GetAttractionById(Guid id);
    Task<List<AttractionResponse>> GetAllAttractions();
    Task<Guid> CreateAttraction(AttractionRequest newAttraction);
    Task UpdateAttraction(Guid id, AttractionRequest existingAttraction);
    Task DeleteAttraction(Guid id);
    Task<CapacityResponse> GetCapacity(Guid id);
}