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
}