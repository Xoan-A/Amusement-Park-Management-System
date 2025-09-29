using Domain;
using Models.In;
using Models.Out;

namespace IBusinessLogic;

public interface IAttractionService
{
    AttractionResponse GetAttractionById(Guid id);
    List<AttractionResponse> GetAllAttractions();
    AttractionResponse AddAttraction(AttractionRequest newAttraction);
    void UpdateAttraction(Guid id, AttractionRequest existingAttraction);
    void DeleteAttraction(Guid id);
}