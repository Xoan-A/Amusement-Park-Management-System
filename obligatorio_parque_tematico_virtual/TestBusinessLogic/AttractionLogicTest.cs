using AutoMapper;
using Moq;
using Domain;
using IBusinessLogic;
using IDataAccess;
using BusinessLogic;
using Models.In;
using Models.Out;
using Models.Mapping;

namespace TestBusinessLogic;

[TestClass]
public class AttractionLogicTest
{
    private Mock<IAttractionRepository> _mockAttractionRepository;
    private Mock<IReportRepository> _mockReportRepository;
    private IMapper _mapper;
    private IAttractionLogic _attractionLogic;
    private IAttractionLogicEntity _attractionLogicEntity;

    [TestInitialize]
    public void Setup()
    {
        _mockAttractionRepository = new Mock<IAttractionRepository>();
        _mockReportRepository = new Mock<IReportRepository>();

        MapperConfiguration configuration = new MapperConfiguration(cfg => { cfg.AddProfile<MappingProfile>(); });
        _mapper = configuration.CreateMapper();

        _attractionLogic = new AttractionLogic(_mockAttractionRepository.Object, _mockReportRepository.Object, _mapper);
        _attractionLogicEntity =
        new AttractionLogic(_mockAttractionRepository.Object, _mockReportRepository.Object, _mapper);
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
        };
        _mockAttractionRepository.Setup(r => r.GetById(expectedAttraction.Id)).Returns(expectedAttraction);
        AttractionResponse result = _attractionLogic.GetAttractionById(expectedAttraction.Id);

        Assert.AreEqual(expectedAttraction.Name, result.Name);
        _mockAttractionRepository.Verify(r => r.GetById(expectedAttraction.Id), Times.Once);
    }

    [TestMethod]
    public void GetAttractionById_ShouldReturnAttractionWithIncidents_WhenAttractionHasIncidents()
    {
        Attraction expectedAttraction = new Attraction
        {
            Name = "Interactive Zone",
            Description = "A fun interactive attraction",
            Type = AttractionType.InteractiveZone,
            MinAge = 8,
            MaxCapacity = 30,
            CurrentCapacity = 15,
        };
        expectedAttraction.AddIncident("Equipment failure");

        _mockAttractionRepository.Setup(r => r.GetById(expectedAttraction.Id)).Returns(expectedAttraction);
        AttractionResponse result = _attractionLogic.GetAttractionById(expectedAttraction.Id);

        Assert.AreEqual(expectedAttraction.Name, result.Name);
        Assert.AreEqual("Equipment failure", result.Incidents[0]);
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
            },
            new Attraction
            {
                Name = "Haunted House",
                Description = "A spooky experience",
                Type = AttractionType.Simulator,
                MinAge = 8,
                MaxCapacity = 15,
                CurrentCapacity = 3,
            }
        };
        _mockAttractionRepository.Setup(r => r.GetAll()).Returns(expectedAttractions);
        List<AttractionResponse> result = _attractionLogic.GetAllAttractions();

        Assert.AreEqual(2, result.Count);
        _mockAttractionRepository.Verify(r => r.GetAll(), Times.Once);
    }

    [TestMethod]
    public void GetAllAttractions_ShouldReturnAttractionsWithIncidents_WhenAttractionsHaveIncidents()
    {
        List<Attraction> expectedAttractions = new List<Attraction>
        {
            new Attraction
            {
                Name = "Roller Coaster",
                Description = "A thrilling ride",
                Type = AttractionType.RollerCoaster,
                MinAge = 12,
                MaxCapacity = 20,
                CurrentCapacity = 5,
            }
        };
        expectedAttractions[0].AddIncident("Motor failure");

        _mockAttractionRepository.Setup(r => r.GetAll()).Returns(expectedAttractions);
        List<AttractionResponse> result = _attractionLogic.GetAllAttractions();

        Assert.AreEqual("Motor failure", result[0].Incidents[0]);
        Assert.IsFalse(result[0].IsActive);
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
        };

        _mockAttractionRepository.Setup(r => r.IsNameUnique(newAttraction.Name)).Returns(true);

        _attractionLogic.CreateAttraction(newAttraction);

        _mockAttractionRepository.Verify(r => r.Create(
            It.Is<Attraction>(a =>
            a.Description == newAttraction.Description &&
            a.Name == newAttraction.Name &&
            a.Type == AttractionType.Simulator &&
            a.MinAge == newAttraction.MinAge &&
            a.MaxCapacity == newAttraction.MaxCapacity &&
            a.CurrentCapacity == 0
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
        };


        AttractionRequest attractionRequest = new AttractionRequest
        {
            Name = existingAttraction.Name,
            Description = "An exciting swinging experience",
            Type = existingAttraction.Type.ToString(),
            MinAge = existingAttraction.MinAge,
            MaxCapacity = existingAttraction.MaxCapacity,
        };

        _mockAttractionRepository.Setup(r => r.GetById(existingAttraction.Id))
        .Returns(existingAttraction);
        _mockAttractionRepository.Setup(r => r.IsNameUnique(attractionRequest.Name)).Returns(true);

        _attractionLogic.UpdateAttraction(existingAttraction.Id, attractionRequest);

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
        };

        _mockAttractionRepository.Setup(r => r.Delete(attractionToDelete));
        _mockAttractionRepository.Setup(r => r.GetById(attractionToDelete.Id)).Returns(attractionToDelete);

        _attractionLogic.DeleteAttraction(attractionToDelete.Id);

        _mockAttractionRepository.Verify(r => r.Delete(attractionToDelete), Times.Once);
    }

    [TestMethod]
    public void DeleteAttraction_ShouldThrowException_WhenAttractionNotFound()
    {
        Guid nonExistentId = Guid.NewGuid();
        _mockAttractionRepository.Setup(r => r.GetById(nonExistentId)).Returns((Attraction)null);

        Assert.ThrowsException<KeyNotFoundException>(() =>
        _attractionLogic.DeleteAttraction(nonExistentId));
    }

    [TestMethod]
    public void GetAttractionEntityById_ShouldReturnAttraction_WhenIdIsValid()
    {
        Attraction expectedAttraction = new Attraction
        {
            Name = "Log Flume",
            Description = "A water ride",
            Type = AttractionType.RollerCoaster,
            MinAge = 10,
            MaxCapacity = 30,
            CurrentCapacity = 8,
        };
        _mockAttractionRepository.Setup(r => r.GetById(expectedAttraction.Id)).Returns(expectedAttraction);
        Attraction result = _attractionLogicEntity.GetAttractionEntityById(expectedAttraction.Id);

        Assert.AreEqual(expectedAttraction.Name, result.Name);
        _mockAttractionRepository.Verify(r => r.GetById(expectedAttraction.Id), Times.Once);
    }

    [TestMethod]
    public void CreateAttraction_ShouldThrowException_WhenNameIsInvalid()
    {
        AttractionRequest invalidRequest = new AttractionRequest
        {
            Name = "",
            Description = "desc",
            Type = AttractionType.RollerCoaster.ToString(),
            MinAge = 10,
            MaxCapacity = 10,
        };

        Assert.ThrowsException<ArgumentException>(() =>
        _attractionLogic.CreateAttraction(invalidRequest));
    }

    [TestMethod]
    public void CreateAttraction_ShouldThrowException_WhenNameIsNotUnique()
    {
        AttractionRequest request = new AttractionRequest
        {
            Name = "Duplicado",
            Description = "desc",
            Type = AttractionType.RollerCoaster.ToString(),
            MinAge = 10,
            MaxCapacity = 10,
        };
        Attraction existingAttraction = new Attraction { Id = Guid.NewGuid(), Name = "Duplicado" };
        _mockAttractionRepository.Setup(r => r.GetByName(request.Name)).Returns(existingAttraction);

        Assert.ThrowsException<ArgumentException>(() =>
        _attractionLogic.CreateAttraction(request));
    }

    [TestMethod]
    public void CreateAttraction_ShouldThrowException_WhenDescriptionIsInvalid()
    {
        AttractionRequest request = new AttractionRequest
        {
            Name = "ValidName",
            Description = "",
            Type = AttractionType.RollerCoaster.ToString(),
            MinAge = 10,
            MaxCapacity = 10,
        };
        _mockAttractionRepository.Setup(r => r.IsNameUnique(request.Name)).Returns(true);

        Assert.ThrowsException<ArgumentException>(() =>
        _attractionLogic.CreateAttraction(request));
    }

    [TestMethod]
    public void CreateAttraction_ShouldThrowException_WhenMinAgeIsInvalid()
    {
        AttractionRequest request = new AttractionRequest
        {
            Name = "ValidName",
            Description = "desc",
            Type = AttractionType.RollerCoaster.ToString(),
            MinAge = -1,
            MaxCapacity = 10,
        };
        _mockAttractionRepository.Setup(r => r.IsNameUnique(request.Name)).Returns(true);

        Assert.ThrowsException<ArgumentException>(() =>
        _attractionLogic.CreateAttraction(request));
    }

    [TestMethod]
    public void CreateAttraction_ShouldThrowException_WhenMaxCapacityIsInvalid()
    {
        AttractionRequest request = new AttractionRequest
        {
            Name = "ValidName",
            Description = "desc",
            Type = AttractionType.RollerCoaster.ToString(),
            MinAge = 10,
            MaxCapacity = 0,
        };
        _mockAttractionRepository.Setup(r => r.IsNameUnique(request.Name)).Returns(true);

        Assert.ThrowsException<ArgumentException>(() =>
        _attractionLogic.CreateAttraction(request));
    }

    [TestMethod]
    public void CreateAttraction_ShouldThrowException_WhenTypeIsInvalid()
    {
        AttractionRequest request = new AttractionRequest
        {
            Name = "ValidName",
            Description = "desc",
            Type = "TipoInvalido",
            MinAge = 10,
            MaxCapacity = 10,
        };
        _mockAttractionRepository.Setup(r => r.IsNameUnique(request.Name)).Returns(true);

        ArgumentException exception = Assert.ThrowsException<ArgumentException>(() =>
        _attractionLogic.CreateAttraction(request));

        Assert.AreEqual("Invalid attraction type: TipoInvalido", exception.Message);
    }

    [TestMethod]
    public void CreateAttraction_ShouldThrowException_WhenTypeIsNumericButNotDefined()
    {
        AttractionRequest request = new AttractionRequest
        {
            Name = "ValidName",
            Description = "desc",
            Type = "99",
            MinAge = 10,
            MaxCapacity = 10,
        };
        _mockAttractionRepository.Setup(r => r.IsNameUnique(request.Name)).Returns(true);

        ArgumentException exception = Assert.ThrowsException<ArgumentException>(() =>
        _attractionLogic.CreateAttraction(request));

        Assert.AreEqual("Invalid attraction type: 99", exception.Message);
    }

    [TestMethod]
    public void UpdateAttraction_ShouldThrowException_WhenNameIsInvalid()
    {
        AttractionRequest invalidRequest = new AttractionRequest
        {
            Name = "",
            Description = "desc",
            Type = AttractionType.RollerCoaster.ToString(),
            MinAge = 10,
            MaxCapacity = 10,
            CurrentCapacity = 0
        };
        Assert.ThrowsException<ArgumentException>(() =>
        _attractionLogic.UpdateAttraction(Guid.NewGuid(), invalidRequest));
    }

    [TestMethod]
    public void UpdateAttraction_ShouldThrowException_WhenNameIsNotUnique()
    {
        Guid attractionId = Guid.NewGuid();
        Guid differentAttractionId = Guid.NewGuid();

        AttractionRequest request = new AttractionRequest
        {
            Name = "Duplicado",
            Description = "desc",
            Type = AttractionType.RollerCoaster.ToString(),
            MinAge = 10,
            MaxCapacity = 10,
            CurrentCapacity = 0
        };

        Attraction currentAttraction = new Attraction { Id = attractionId, Name = "Original" };
        Attraction duplicateAttraction = new Attraction { Id = differentAttractionId, Name = "Duplicado" };

        _mockAttractionRepository.Setup(r => r.GetById(attractionId)).Returns(currentAttraction);
        _mockAttractionRepository.Setup(r => r.GetByName(request.Name)).Returns(duplicateAttraction);

        Assert.ThrowsException<ArgumentException>(() =>
        _attractionLogic.UpdateAttraction(attractionId, request));
    }

    [TestMethod]
    public void UpdateAttraction_ShouldThrowException_WhenDescriptionIsInvalid()
    {
        AttractionRequest request = new AttractionRequest
        {
            Name = "ValidName",
            Description = "",
            Type = AttractionType.RollerCoaster.ToString(),
            MinAge = 10,
            MaxCapacity = 10,
            CurrentCapacity = 0
        };
        _mockAttractionRepository.Setup(r => r.IsNameUnique(request.Name)).Returns(true);

        Assert.ThrowsException<ArgumentException>(() =>
        _attractionLogic.UpdateAttraction(Guid.NewGuid(), request));
    }

    [TestMethod]
    public void UpdateAttraction_ShouldThrowException_WhenMinAgeIsInvalid()
    {
        AttractionRequest request = new AttractionRequest
        {
            Name = "ValidName",
            Description = "desc",
            Type = AttractionType.RollerCoaster.ToString(),
            MinAge = -1,
            MaxCapacity = 10,
            CurrentCapacity = 0
        };
        _mockAttractionRepository.Setup(r => r.IsNameUnique(request.Name)).Returns(true);

        Assert.ThrowsException<ArgumentException>(() =>
        _attractionLogic.UpdateAttraction(Guid.NewGuid(), request));
    }

    [TestMethod]
    public void UpdateAttraction_ShouldThrowException_WhenMaxCapacityIsInvalid()
    {
        AttractionRequest request = new AttractionRequest
        {
            Name = "ValidName",
            Description = "desc",
            Type = AttractionType.RollerCoaster.ToString(),
            MinAge = 10,
            MaxCapacity = 0,
            CurrentCapacity = 0
        };
        _mockAttractionRepository.Setup(r => r.IsNameUnique(request.Name)).Returns(true);

        Assert.ThrowsException<ArgumentException>(() =>
        _attractionLogic.UpdateAttraction(Guid.NewGuid(), request));
    }

    [TestMethod]
    public void UpdateAttraction_ShouldThrowException_WhenCurrentCapacityIsInvalid()
    {
        AttractionRequest request = new AttractionRequest
        {
            Name = "ValidName",
            Description = "desc",
            Type = AttractionType.RollerCoaster.ToString(),
            MinAge = 10,
            MaxCapacity = 10,
            CurrentCapacity = 20
        };
        _mockAttractionRepository.Setup(r => r.IsNameUnique(request.Name)).Returns(true);
        Attraction attraction = new Attraction { Id = Guid.NewGuid(), CurrentCapacity = 0 };
        _mockAttractionRepository.Setup(r => r.GetById(attraction.Id)).Returns(attraction);

        Assert.ThrowsException<ArgumentException>(() =>
        _attractionLogic.UpdateAttraction(attraction.Id, request));
    }

    [TestMethod]
    public void UpdateAttraction_ShouldThrowException_WhenTypeIsInvalid()
    {
        AttractionRequest request = new AttractionRequest
        {
            Name = "ValidName",
            Description = "desc",
            Type = "Roller",
            MinAge = 10,
            MaxCapacity = 10,
            CurrentCapacity = 0
        };
        _mockAttractionRepository.Setup(r => r.IsNameUnique(request.Name)).Returns(true);
        Attraction attraction = new Attraction { Id = Guid.NewGuid(), CurrentCapacity = 0 };
        _mockAttractionRepository.Setup(r => r.GetById(attraction.Id)).Returns(attraction);

        ArgumentException exception = Assert.ThrowsException<ArgumentException>(() =>
        _attractionLogic.UpdateAttraction(attraction.Id, request));

        Assert.AreEqual("Invalid attraction type: Roller", exception.Message);
    }

    [TestMethod]
    public void UpdateAttraction_ShouldThrowException_WhenTypeIsNumericButNotDefined()
    {
        AttractionRequest request = new AttractionRequest
        {
            Name = "ValidName",
            Description = "desc",
            Type = "99",
            MinAge = 10,
            MaxCapacity = 10,
            CurrentCapacity = 0
        };
        _mockAttractionRepository.Setup(r => r.IsNameUnique(request.Name)).Returns(true);
        Attraction attraction = new Attraction { Id = Guid.NewGuid(), CurrentCapacity = 0 };
        _mockAttractionRepository.Setup(r => r.GetById(attraction.Id)).Returns(attraction);

        ArgumentException exception = Assert.ThrowsException<ArgumentException>(() =>
        _attractionLogic.UpdateAttraction(attraction.Id, request));

        Assert.AreEqual("Invalid attraction type: 99", exception.Message);
    }

    [TestMethod]
    public void UpdateAttraction_ShouldThrowException_WhenAttractionNotFound()
    {
        Guid attractionId = Guid.NewGuid();
        AttractionRequest request = new AttractionRequest
        {
            Name = "ValidName",
            Description = "Valid description",
            Type = AttractionType.RollerCoaster.ToString(),
            MinAge = 10,
            MaxCapacity = 100,
            CurrentCapacity = 0
        };

        _mockAttractionRepository.Setup(r => r.GetById(attractionId)).Returns((Attraction)null);
        _mockAttractionRepository.Setup(r => r.IsNameUnique(request.Name)).Returns(true);

        Assert.ThrowsException<KeyNotFoundException>(() =>
        _attractionLogic.UpdateAttraction(attractionId, request));
    }

    [TestMethod]
    public void GetAttractionById_ShouldThrowException_WhenIdDoesNotExist()
    {
        Guid newId = Guid.NewGuid();
        _mockAttractionRepository.Setup(r => r.GetById(newId)).Returns((Attraction)null);

        Assert.ThrowsException<KeyNotFoundException>(() =>
        _attractionLogic.GetAttractionById(newId));
    }

    [TestMethod]
    public void AddIncidence_ShouldThrowException_WhenAttractionNotFound()
    {
        Guid id = Guid.NewGuid();
        _mockAttractionRepository.Setup(r => r.GetById(id)).Returns((Attraction)null);
        Assert.ThrowsException<KeyNotFoundException>(() =>
        _attractionLogic.AddIncident(id, "Incidente"));
    }

    [TestMethod]
    public void AddIncidence_ShouldAddIncident_WhenAttractionExists()
    {
        Attraction attraction = new Attraction { Incidents = new List<string>() };
        _mockAttractionRepository.Setup(r => r.GetById(attraction.Id)).Returns(attraction);
        _attractionLogic.AddIncident(attraction.Id, "Incidente");
        _mockAttractionRepository.Verify(r => r.Update(attraction), Times.Once);
    }

    [TestMethod]
    public void RemoveIncidence_ShouldThrowException_WhenAttractionNotFound()
    {
        Guid id = Guid.NewGuid();
        _mockAttractionRepository.Setup(r => r.GetById(id)).Returns((Attraction)null);
        Assert.ThrowsException<KeyNotFoundException>(() =>
        _attractionLogic.RemoveIncident(id, "Incidente"));
    }

    [TestMethod]
    public void RemoveIncidence_ShouldRemoveIncident_WhenAttractionExists()
    {
        Attraction attraction = new Attraction { Incidents = new List<string> { "Incidente" } };
        _mockAttractionRepository.Setup(r => r.GetById(attraction.Id)).Returns(attraction);
        _attractionLogic.RemoveIncident(attraction.Id, "Incidente");
        _mockAttractionRepository.Verify(r => r.Update(attraction), Times.Once);
    }

    [TestMethod]
    public void GetAllAttractionsVisits_ShouldReturnAttractionsWithVisitCounts_WhenReportsExist()
    {
        DateTime startDate = new DateTime(2025, 10, 1);
        DateTime endDate = new DateTime(2025, 10, 7);

        AttractionsVisitsRequest request = new AttractionsVisitsRequest
        {
            StartDate = startDate,
            EndDate = endDate
        };

        Guid attraction1Id = Guid.NewGuid();
        Guid attraction2Id = Guid.NewGuid();

        Attraction attraction1 = new Attraction
        {
            Id = attraction1Id,
            Name = "Montaña Rusa",
            Description = "Una atracción emocionante",
            Type = AttractionType.RollerCoaster,
            MinAge = 12,
            MaxCapacity = 50,
            CurrentCapacity = 0
        };

        Attraction attraction2 = new Attraction
        {
            Id = attraction2Id,
            Name = "Simulador",
            Description = "Experiencia virtual",
            Type = AttractionType.Simulator,
            MinAge = 8,
            MaxCapacity = 30,
            CurrentCapacity = 0
        };

        List<Report> reports = new List<Report>
        {
            new Report
            {
                Id = Guid.NewGuid(),
                AttractionId = attraction1Id,
                Attraction = attraction1,
                EnterDate = new DateTime(2025, 10, 2),
                ExitDate = new DateTime(2025, 10, 2, 1, 0, 0),
                VisitorReportId = Guid.NewGuid()
            },
            new Report
            {
                Id = Guid.NewGuid(),
                AttractionId = attraction1Id,
                Attraction = attraction1,
                EnterDate = new DateTime(2025, 10, 3),
                ExitDate = new DateTime(2025, 10, 3, 1, 0, 0),
                VisitorReportId = Guid.NewGuid()
            },
            new Report
            {
                Id = Guid.NewGuid(),
                AttractionId = attraction1Id,
                Attraction = attraction1,
                EnterDate = new DateTime(2025, 10, 4),
                ExitDate = new DateTime(2025, 10, 4, 1, 0, 0),
                VisitorReportId = Guid.NewGuid()
            },
            new Report
            {
                Id = Guid.NewGuid(),
                AttractionId = attraction2Id,
                Attraction = attraction2,
                EnterDate = new DateTime(2025, 10, 3),
                ExitDate = new DateTime(2025, 10, 3, 1, 0, 0),
                VisitorReportId = Guid.NewGuid()
            },
            new Report
            {
                Id = Guid.NewGuid(),
                AttractionId = attraction2Id,
                Attraction = attraction2,
                EnterDate = new DateTime(2025, 10, 5),
                ExitDate = new DateTime(2025, 10, 5, 1, 0, 0),
                VisitorReportId = Guid.NewGuid()
            }
        };

        _mockReportRepository.Setup(r => r.GetAllReports()).Returns(reports);

        AttractionsVisitResponse result = _attractionLogic.GetAllAttractionsVisits(request);

        Assert.AreEqual(2, result.AttractionsVisits.Count);

        AttractionVisitDetail attraction1Result =
        result.AttractionsVisits.FirstOrDefault(r => r.Attraction.Id == attraction1Id);
        Assert.AreEqual("Montaña Rusa", attraction1Result.Attraction.Name);
        Assert.AreEqual(3, attraction1Result.VisitCount);

        AttractionVisitDetail attraction2Result =
        result.AttractionsVisits.FirstOrDefault(r => r.Attraction.Id == attraction2Id);
        Assert.AreEqual("Simulador", attraction2Result.Attraction.Name);
        Assert.AreEqual(2, attraction2Result.VisitCount);

        _mockReportRepository.Verify(r => r.GetAllReports(), Times.Once);
    }

    [TestMethod]
    public void GetAllAttractionsVisits_ShouldThrowException_WhenStartDateIsAfterEndDate()
    {
        DateTime startDate = new DateTime(2025, 10, 7);
        DateTime endDate = new DateTime(2025, 10, 1);

        AttractionsVisitsRequest request = new AttractionsVisitsRequest
        {
            StartDate = startDate,
            EndDate = endDate
        };

        Assert.ThrowsException<ArgumentException>(() =>
        _attractionLogic.GetAllAttractionsVisits(request));
    }

    [TestMethod]
    public void UpdateAttraction_WithNullCurrentCapacity_PreservesExistingCapacity()
    {
        Guid attractionId = Guid.NewGuid();
        int existingCapacity = 50;

        Attraction existingAttraction = new Attraction
        {
            Id = attractionId,
            Name = "Roller Coaster",
            Description = "Fast ride",
            Type = AttractionType.RollerCoaster,
            MaxCapacity = 100,
            CurrentCapacity = existingCapacity
        };

        AttractionRequest request = new AttractionRequest
        {
            Name = "Updated Coaster",
            Description = "Updated description",
            Type = "RollerCoaster",
            MaxCapacity = 100,
            CurrentCapacity = null
        };

        _mockAttractionRepository.Setup(r => r.GetById(attractionId)).Returns(existingAttraction);
        _mockAttractionRepository.Setup(r => r.Update(It.IsAny<Attraction>()));

        _attractionLogic.UpdateAttraction(attractionId, request);

        Assert.AreEqual(existingCapacity, existingAttraction.CurrentCapacity,
            "CurrentCapacity should preserve existing value when request has null");
        Assert.AreEqual("Updated Coaster", existingAttraction.Name);
        _mockAttractionRepository.Verify(r => r.Update(existingAttraction), Times.Once);
    }
}