using Domain;

namespace IBusinessLogic;

public interface IAttractionLogicEntity
{
    Attraction GetAttractionEntityById(Guid expectedAttractionId);
}