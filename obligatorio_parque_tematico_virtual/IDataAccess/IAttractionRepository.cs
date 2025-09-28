using Domain;

namespace IDataAccess;

public interface IAttractionRepository
{
    Attraction Create(Attraction attraction);
    Attraction GetByName(string name);
    Attraction GetById(Guid id);
    bool IsNameUnique(string name);
    List<Attraction> GetAll();
}