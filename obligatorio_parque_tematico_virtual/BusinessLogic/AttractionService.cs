using Domain;
using IBusinessLogic;
using IDataAccess;

namespace BusinessLogic;

public class AttractionService : IAttractionService
{
    private readonly IAttractionRepository _attractionRepository;
    
    public AttractionService(IAttractionRepository attractionRepository)
    {
        _attractionRepository = attractionRepository;
    }
    
    public Attraction GetAttractionById(Guid id)
    {
        return _attractionRepository.GetById(id);
    }
    
    public List<Attraction> GetAllAttractions()
    {
        return _attractionRepository.GetAll();
    }
    
    public Attraction AddAttraction(Attraction newAttraction)
    {
        return _attractionRepository.Create(newAttraction);
    }
    
    public void UpdateAttraction(Attraction existingAttraction)
    {
        _attractionRepository.Update(existingAttraction);
    }
}