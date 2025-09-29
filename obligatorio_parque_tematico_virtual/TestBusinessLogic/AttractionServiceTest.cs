using Moq;
using Domain;
using IBusinessLogic;
using IDataAccess;
using BusinessLogic;
using Models.In;
using Models.Out;

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
        
        AttractionResponse result = _attractionService.GetAttractionById(expectedAttraction.Id);
        
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
        
        List<AttractionResponse> result = _attractionService.GetAllAttractions();
        
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
        _mockAttractionRepository.Verify(r => r.GetAll(), Times.Once);
    }
    
    [TestMethod]
    public void AddAttraction_ShouldCreateAttraction_WhenDataIsValid()
    {
        AttractionRequest newAttraction = new AttractionRequest()
        {
            Name = "Bumper Cars",
            Description = "Fun driving experience",
            Type = AttractionType.Simulator.ToString(),
            MinAge = 5,
            MaxCapacity = 10,
            IsActive = true
        };
        
        AttractionResponse result = _attractionService.AddAttraction(newAttraction);
        
        Assert.AreEqual(newAttraction.Name, result.Name);
        _mockAttractionRepository.Verify(r => r.Create(
            It.Is<Attraction>(a =>
                a.Description == newAttraction.Description &&
                a.Name == newAttraction.Name &&
                a.Type == AttractionType.Simulator &&
                a.MinAge == newAttraction.MinAge &&
                a.MaxCapacity == newAttraction.MaxCapacity &&
                a.CurrentCapacity == 0 &&
                a.IsActive == newAttraction.IsActive
            )), Times.Once);
    }
    
    [TestMethod]
    public void UpdateAttraction_ShouldUpdateAttraction_WhenDataIsValid()
    {
        Attraction existingAttraction = new Attraction
        {
            Name = "Swing Ride",
            Description = "A fun swinging experience",
            Type = AttractionType.RollerCoaster,
            MinAge = 7,
            MaxCapacity = 25,
            CurrentCapacity = 5,
            IsActive = true
        };
        
        
        AttractionRequest attractionRequest = new AttractionRequest
        {
            Name = existingAttraction.Name,
            Description = "An exciting swinging experience",
            Type = existingAttraction.Type.ToString(),
            MinAge = existingAttraction.MinAge,
            MaxCapacity = existingAttraction.MaxCapacity,
            IsActive = existingAttraction.IsActive
        };
        
        _mockAttractionRepository.Setup(r => r.GetById(existingAttraction.Id))
            .Returns(existingAttraction);
       
        _attractionService.UpdateAttraction(existingAttraction.Id, attractionRequest);
        
        _mockAttractionRepository.Verify(r => r.Update(
            It.Is<Attraction>(a =>
                    a.Id == existingAttraction.Id &&
                    a.Description == "An exciting swinging experience" &&
                    a.Name == existingAttraction.Name
            )), Times.Once);
    }
    
    [TestMethod]
    public void DeleteAttraction_ShouldRemoveAttraction_WhenIdIsValid()
    {
        Attraction attractionToDelete = new Attraction
        {
            Name = "Drop Tower",
            Description = "A thrilling drop experience",
            Type = AttractionType.RollerCoaster,
            MinAge = 14,
            MaxCapacity = 15,
            CurrentCapacity = 0,
            IsActive = false
        };
        
        _mockAttractionRepository.Setup(r => r.Remove(attractionToDelete));
        _mockAttractionRepository.Setup(r => r.GetById(attractionToDelete.Id)).Returns(attractionToDelete);
        
        _attractionService.DeleteAttraction(attractionToDelete.Id);
        
        _mockAttractionRepository.Verify(r => r.Remove(attractionToDelete), Times.Once);
    }
}