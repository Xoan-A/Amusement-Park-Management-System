using Domain;

namespace IBusinessLogic;

public interface IAttractionLogicEntity
{
    Task<Attraction> GetAttractionEntityById(Guid expectedAttractionId);
}