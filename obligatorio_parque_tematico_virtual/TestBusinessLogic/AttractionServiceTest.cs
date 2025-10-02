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
    private IAttractionServiceEntity _attractionServiceEntity;

    [TestInitialize]
    public void Setup()
    {
        _mockAttractionRepository = new Mock<IAttractionRepository>();
        _attractionService = new AttractionService(_mockAttractionRepository.Object);
        _attractionServiceEntity = new AttractionService(_mockAttractionRepository.Object);
    }

    [TestMethod]
    public async Task GetAttractionById_ShouldReturnAttraction_WhenIdIsValid()
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
        _mockAttractionRepository.Setup(r => r.GetById(expectedAttraction.Id)).ReturnsAsync(expectedAttraction);
        AttractionResponse result = await _attractionService.GetAttractionById(expectedAttraction.Id);
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedAttraction.Name, result.Name);
        _mockAttractionRepository.Verify(r => r.GetById(expectedAttraction.Id), Times.Once);
    }

    [TestMethod]
    public async Task GetAllAttractions_ShouldReturnListOfAttractions()
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
        _mockAttractionRepository.Setup(r => r.GetAll()).ReturnsAsync(expectedAttractions);
        List<AttractionResponse> result = await _attractionService.GetAllAttractions();
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
        _mockAttractionRepository.Verify(r => r.GetAll(), Times.Once);
    }

    [TestMethod]
    public async Task AddAttraction_ShouldCreateAttraction_WhenDataIsValid()
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

        _mockAttractionRepository.Setup(r => r.IsNameUnique(newAttraction.Name)).ReturnsAsync(true);
        
        await _attractionService.CreateAttraction(newAttraction);

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
    public async Task UpdateAttraction_ShouldUpdateAttraction_WhenDataIsValid()
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
            .ReturnsAsync(existingAttraction);
        _mockAttractionRepository.Setup(r => r.IsNameUnique(attractionRequest.Name)).ReturnsAsync(true);

        await _attractionService.UpdateAttraction(existingAttraction.Id, attractionRequest);

        _mockAttractionRepository.Verify(r => r.Update(
            It.Is<Attraction>(a =>
                a.Id == existingAttraction.Id &&
                a.Description == "An exciting swinging experience" &&
                a.Name == existingAttraction.Name
            )), Times.Once);
    }

    [TestMethod]
    public async Task DeleteAttraction_ShouldRemoveAttraction_WhenIdIsValid()
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

        _mockAttractionRepository.Setup(r => r.Delete(attractionToDelete));
        _mockAttractionRepository.Setup(r => r.GetById(attractionToDelete.Id)).ReturnsAsync(attractionToDelete);

        _attractionService.DeleteAttraction(attractionToDelete.Id);

        _mockAttractionRepository.Verify(r => r.Delete(attractionToDelete), Times.Once);
    }

    [TestMethod]
    public async Task GetAttractionEntityById_ShouldReturnAttraction_WhenIdIsValid()
    {
        Attraction expectedAttraction = new Attraction
        {
            Name = "Log Flume",
            Description = "A water ride",
            Type = AttractionType.RollerCoaster,
            MinAge = 10,
            MaxCapacity = 30,
            CurrentCapacity = 8,
            IsActive = true
        };
        _mockAttractionRepository.Setup(r => r.GetById(expectedAttraction.Id)).ReturnsAsync(expectedAttraction);
        Attraction result = await _attractionServiceEntity.GetAttractionEntityById(expectedAttraction.Id);
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedAttraction.Name, result.Name);
        _mockAttractionRepository.Verify(r => r.GetById(expectedAttraction.Id), Times.Once);
    }

    [TestMethod]
    public async Task CreateAttraction_ShouldThrowException_WhenNameIsInvalid()
    {
        AttractionRequest invalidRequest = new AttractionRequest
        {
            Name = "",
            Description = "desc",
            Type = AttractionType.RollerCoaster.ToString(),
            MinAge = 10,
            MaxCapacity = 10,
            IsActive = true
        };
        
        await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await _attractionService.CreateAttraction(invalidRequest));
    }

    [TestMethod]
    public async Task CreateAttraction_ShouldThrowException_WhenNameIsNotUnique()
    {
        AttractionRequest request = new AttractionRequest
        {
            Name = "Duplicado",
            Description = "desc",
            Type = AttractionType.RollerCoaster.ToString(),
            MinAge = 10,
            MaxCapacity = 10,
            IsActive = true
        };
        _mockAttractionRepository.Setup(r => r.IsNameUnique(request.Name)).ReturnsAsync(false);
        
        await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await _attractionService.CreateAttraction(request));
    }

    [TestMethod]
    public async Task CreateAttraction_ShouldThrowException_WhenDescriptionIsInvalid()
    {
        AttractionRequest request = new AttractionRequest
        {
            Name = "ValidName",
            Description = "", 
            Type = AttractionType.RollerCoaster.ToString(),
            MinAge = 10,
            MaxCapacity = 10,
            IsActive = true
        };
        _mockAttractionRepository.Setup(r => r.IsNameUnique(request.Name)).ReturnsAsync(true);
        
        await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await _attractionService.CreateAttraction(request));
    }

    [TestMethod]
    public async Task CreateAttraction_ShouldThrowException_WhenMinAgeIsInvalid()
    {
        AttractionRequest request = new AttractionRequest
        {
            Name = "ValidName",
            Description = "desc",
            Type = AttractionType.RollerCoaster.ToString(),
            MinAge = -1,
            MaxCapacity = 10,
            IsActive = true
        };
        _mockAttractionRepository.Setup(r => r.IsNameUnique(request.Name)).ReturnsAsync(true);
        
        await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await _attractionService.CreateAttraction(request));
    }

    [TestMethod]
    public async Task CreateAttraction_ShouldThrowException_WhenMaxCapacityIsInvalid()
    {
        AttractionRequest request = new AttractionRequest
        {
            Name = "ValidName",
            Description = "desc",
            Type = AttractionType.RollerCoaster.ToString(),
            MinAge = 10,
            MaxCapacity = 0,
            IsActive = true
        };
        _mockAttractionRepository.Setup(r => r.IsNameUnique(request.Name)).ReturnsAsync(true);
        
        await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await _attractionService.CreateAttraction(request));
    }

    [TestMethod]
    public async Task CreateAttraction_ShouldThrowException_WhenTypeIsInvalid()
    {
        AttractionRequest request = new AttractionRequest
        {
            Name = "ValidName",
            Description = "desc",
            Type = "TipoInvalido",
            MinAge = 10,
            MaxCapacity = 10,
            IsActive = true
        };
        _mockAttractionRepository.Setup(r => r.IsNameUnique(request.Name)).ReturnsAsync(true);
        
        await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await _attractionService.CreateAttraction(request));
    }

    [TestMethod]
    public async Task UpdateAttraction_ShouldThrowException_WhenNameIsInvalid()
    {
        AttractionRequest invalidRequest = new AttractionRequest
        {
            Name = "",
            Description = "desc",
            Type = AttractionType.RollerCoaster.ToString(),
            MinAge = 10,
            MaxCapacity = 10,
            IsActive = true,
            CurrentCapacity = 0
        };
        await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await _attractionService.UpdateAttraction(Guid.NewGuid(), invalidRequest));
    }

    [TestMethod]
    public async Task UpdateAttraction_ShouldThrowException_WhenNameIsNotUnique()
    {
        AttractionRequest request = new AttractionRequest
        {
            Name = "Duplicado",
            Description = "desc",
            Type = AttractionType.RollerCoaster.ToString(),
            MinAge = 10,
            MaxCapacity = 10,
            IsActive = true,
            CurrentCapacity = 0
        };
        _mockAttractionRepository.Setup(r => r.IsNameUnique(request.Name)).ReturnsAsync(false);
        await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await _attractionService.UpdateAttraction(Guid.NewGuid(), request));
    }

    [TestMethod]
    public async Task UpdateAttraction_ShouldThrowException_WhenDescriptionIsInvalid()
    {
        AttractionRequest request = new AttractionRequest
        {
            Name = "ValidName",
            Description = "",
            Type = AttractionType.RollerCoaster.ToString(),
            MinAge = 10,
            MaxCapacity = 10,
            IsActive = true,
            CurrentCapacity = 0
        };
        _mockAttractionRepository.Setup(r => r.IsNameUnique(request.Name)).ReturnsAsync(true);
        
        await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await _attractionService.UpdateAttraction(Guid.NewGuid(), request));
    }

    [TestMethod]
    public async Task UpdateAttraction_ShouldThrowException_WhenMinAgeIsInvalid()
    {
        AttractionRequest request = new AttractionRequest
        {
            Name = "ValidName",
            Description = "desc",
            Type = AttractionType.RollerCoaster.ToString(),
            MinAge = -1,
            MaxCapacity = 10,
            IsActive = true,
            CurrentCapacity = 0
        };
        _mockAttractionRepository.Setup(r => r.IsNameUnique(request.Name)).ReturnsAsync(true);
        
        await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await _attractionService.UpdateAttraction(Guid.NewGuid(), request));
    }

    [TestMethod]
    public async Task UpdateAttraction_ShouldThrowException_WhenMaxCapacityIsInvalid()
    {
        AttractionRequest request = new AttractionRequest
        {
            Name = "ValidName",
            Description = "desc",
            Type = AttractionType.RollerCoaster.ToString(),
            MinAge = 10,
            MaxCapacity = 0,
            IsActive = true,
            CurrentCapacity = 0
        };
        _mockAttractionRepository.Setup(r => r.IsNameUnique(request.Name)).ReturnsAsync(true);
        
        await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await _attractionService.UpdateAttraction(Guid.NewGuid(), request));
    }

    [TestMethod]
    public async Task UpdateAttraction_ShouldThrowException_WhenCurrentCapacityIsInvalid()
    {
        AttractionRequest request = new AttractionRequest
        {
            Name = "ValidName",
            Description = "desc",
            Type = AttractionType.RollerCoaster.ToString(),
            MinAge = 10,
            MaxCapacity = 10,
            IsActive = true,
            CurrentCapacity = 20
        };
        _mockAttractionRepository.Setup(r => r.IsNameUnique(request.Name)).ReturnsAsync(true);
        Attraction attraction = new Attraction { Id = Guid.NewGuid(), CurrentCapacity = 0 };
        _mockAttractionRepository.Setup(r => r.GetById(attraction.Id)).ReturnsAsync(attraction);
        
        await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await _attractionService.UpdateAttraction(attraction.Id, request));
    }

    [TestMethod]
    public async Task UpdateAttraction_ShouldThrowException_WhenTypeIsInvalid()
    {
        AttractionRequest request = new AttractionRequest
        {
            Name = "ValidName",
            Description = "desc",
            Type = "TipoInvalido",
            MinAge = 10,
            MaxCapacity = 10,
            IsActive = true,
            CurrentCapacity = 0
        };
        _mockAttractionRepository.Setup(r => r.IsNameUnique(request.Name)).ReturnsAsync(true);
        Attraction attraction = new Attraction { Id = Guid.NewGuid(), CurrentCapacity = 0 };
        _mockAttractionRepository.Setup(r => r.GetById(attraction.Id)).ReturnsAsync(attraction);
        
        await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await _attractionService.UpdateAttraction(attraction.Id, request));
    }

    [TestMethod]
    public async Task GetAttractionById_ShouldThrowException_WhenIdDoesNotExist()
    {
        Guid newId = Guid.NewGuid();
        _mockAttractionRepository.Setup(r => r.GetById(newId)).ReturnsAsync((Attraction)null);
        
        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(async () => await _attractionService.GetAttractionById(newId));
    }
}