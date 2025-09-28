using Moq;
using Domain;
using IBusinessLogic;
using IDataAccess;
using BusinessLogic;

namespace TestBusinessLogic;

[TestClass]
public class AttractionServiceTest
{
    private Mock<IAttractionRepository> _mockAttractionRepository;
    private IAttractionService _attractionService;
    
    [TestInitialize]
    public void Setup()
    {
        _mockAttractionRepository = new Mock<IAttractionRepository>();
        _attractionService = new AttractionService(_mockAttractionRepository.Object);
    }
    
    [TestMethod]
    public void GetAttractionById_ShouldReturnAttraction_WhenIdIsValid()
    {
        Attraction expectedAttraction = new Attraction
        {
            Name = "Roller Coaster",
            Description = "A thrilling ride",
            Type = AttractionType.RollerCoaster,
            MinAge = 12,
            MaxCapacity = 20,
            CurrentCapacity = 5,
            IsActive = true
        };
        
        _mockAttractionRepository.Setup(r => r.GetById(expectedAttraction.Id)).Returns(expectedAttraction);
        
        Attraction result = _attractionService.GetAttractionById(expectedAttraction.Id);
        
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedAttraction.Name, result.Name);
        _mockAttractionRepository.Verify(r => r.GetById(expectedAttraction.Id), Times.Once);
    }
}