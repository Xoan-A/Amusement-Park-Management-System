using Domain;

namespace IDataAccess;

public interface IAttractionRepository
{
    void Create(Attraction attraction);
    Attraction GetByName(string name);
    Attraction GetById(Guid id);
    bool IsNameUnique(string name);
    List<Attraction> GetAll();
    void Update(Attraction attraction);
    void Remove(Attraction attraction);
}