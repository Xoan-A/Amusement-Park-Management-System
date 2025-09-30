using Domain;

namespace IBusinessLogic;

public interface IAttractionServiceEntity
{
    Task<Attraction> GetAttractionEntityById(Guid expectedAttractionId);
}