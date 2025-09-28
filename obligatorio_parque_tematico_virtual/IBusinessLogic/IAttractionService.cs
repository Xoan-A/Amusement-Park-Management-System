using Domain;

namespace IBusinessLogic;

public interface IAttractionService
{
    Attraction GetAttractionById(Guid Id);
    List<Attraction> GetAllAttractions();
    Attraction AddAttraction(Attraction newAttraction);
}