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

    [TestMethod]
    public void GetAllAttractions_ShouldReturnListOfAttractions()
    {
        List<Attraction> expectedAttractions = new List<Attraction>
        {
            new Attraction
            {
                Name = "Ferris Wheel",
                Description = "A giant wheel with cabins",
                Type = AttractionType.RollerCoaster,
                MinAge = 0,
                MaxCapacity = 40,
                CurrentCapacity = 10,
                IsActive = true
            },
            new Attraction
            {
                Name = "Haunted House",
                Description = "A spooky experience",
                Type = AttractionType.Simulator,
                MinAge = 8,
                MaxCapacity = 15,
                CurrentCapacity = 3,
                IsActive = false
            }
        };
        
        _mockAttractionRepository.Setup(r => r.GetAll()).Returns(expectedAttractions);
        
        List<Attraction> result = _attractionService.GetAllAttractions();
        
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
        _mockAttractionRepository.Verify(r => r.GetAll(), Times.Once);
    }
    
    [TestMethod]
    public void AddAttraction_ShouldCreateAttraction_WhenDataIsValid()
    {
        Attraction newAttraction = new Attraction
        {
            Name = "Bumper Cars",
            Description = "Fun driving experience",
            Type = AttractionType.Simulator,
            MinAge = 5,
            MaxCapacity = 10,
            CurrentCapacity = 0,
            IsActive = true
        };
        
        _attractionService.AddAttraction(newAttraction);
        
        Attraction result = _attractionService.GetAttractionById(newAttraction.Id);
        Assert.AreEqual(newAttraction.Name, result.Name);
    }
}