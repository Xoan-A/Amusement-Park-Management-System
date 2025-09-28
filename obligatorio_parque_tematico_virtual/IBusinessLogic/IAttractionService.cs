using Domain;

namespace IBusinessLogic;

public interface IAttractionService
{
    Attraction GetAttractionById(Guid Id);
}