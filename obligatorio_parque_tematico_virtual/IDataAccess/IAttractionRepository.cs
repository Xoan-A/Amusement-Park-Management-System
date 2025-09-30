using Domain;

namespace IDataAccess;

public interface IAttractionRepository
{
    Task Create(Attraction attraction);
    Task<Attraction> GetByName(string name);
    Task<Attraction> GetById(Guid id);
    Task<bool> IsNameUnique(string name);
    Task<List<Attraction>> GetAll();
    Task Update(Attraction attraction);
    Task Delete(Attraction attraction);
}