using Moq;
using Domain;
using IBusinessLogic;
using IDataAccess;
using BusinessLogic;
using Models.In;
using Models.Out;

namespace TestBusinessLogic;

[TestClass]
public class AttractionLogicTest
{
    private Mock<IAttractionRepository> _mockAttractionRepository;
    private Mock<IReportRepository> _mockReportRepository;
    private IAttractionLogic _attractionLogic;
    private IAttractionLogicEntity _attractionLogicEntity;

    [TestInitialize]
    public void Setup()
    {
        _mockAttractionRepository = new Mock<IAttractionRepository>();
        _mockReportRepository = new Mock<IReportRepository>();
        _attractionLogic = new AttractionLogic(_mockAttractionRepository.Object, _mockReportRepository.Object);
        _attractionLogicEntity = new AttractionLogic(_mockAttractionRepository.Object, _mockReportRepository.Object);
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
        };
        _mockAttractionRepository.Setup(r => r.GetById(expectedAttraction.Id)).ReturnsAsync(expectedAttraction);
        AttractionResponse result = await _attractionLogic.GetAttractionById(expectedAttraction.Id);
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
        _mockAttractionRepository.Setup(r => r.GetAll()).ReturnsAsync(expectedAttractions);
        List<AttractionResponse> result = await _attractionLogic.GetAllAttractions();
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
        };

        _mockAttractionRepository.Setup(r => r.IsNameUnique(newAttraction.Name)).ReturnsAsync(true);

        await _attractionLogic.CreateAttraction(newAttraction);

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
            .ReturnsAsync(existingAttraction);
        _mockAttractionRepository.Setup(r => r.IsNameUnique(attractionRequest.Name)).ReturnsAsync(true);

        await _attractionLogic.UpdateAttraction(existingAttraction.Id, attractionRequest);

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
        };

        _mockAttractionRepository.Setup(r => r.Delete(attractionToDelete));
        _mockAttractionRepository.Setup(r => r.GetById(attractionToDelete.Id)).ReturnsAsync(attractionToDelete);

        _attractionLogic.DeleteAttraction(attractionToDelete.Id);

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
        };
        _mockAttractionRepository.Setup(r => r.GetById(expectedAttraction.Id)).ReturnsAsync(expectedAttraction);
        Attraction result = await _attractionLogicEntity.GetAttractionEntityById(expectedAttraction.Id);
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
        };

        await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
            await _attractionLogic.CreateAttraction(invalidRequest));
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
        };
        Attraction existingAttraction = new Attraction { Id = Guid.NewGuid(), Name = "Duplicado" };
        _mockAttractionRepository.Setup(r => r.GetByName(request.Name)).ReturnsAsync(existingAttraction);

        await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
            await _attractionLogic.CreateAttraction(request));
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
        };
        _mockAttractionRepository.Setup(r => r.IsNameUnique(request.Name)).ReturnsAsync(true);

        await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
            await _attractionLogic.CreateAttraction(request));
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
        };
        _mockAttractionRepository.Setup(r => r.IsNameUnique(request.Name)).ReturnsAsync(true);

        await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
            await _attractionLogic.CreateAttraction(request));
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
        };
        _mockAttractionRepository.Setup(r => r.IsNameUnique(request.Name)).ReturnsAsync(true);

        await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
            await _attractionLogic.CreateAttraction(request));
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
        };
        _mockAttractionRepository.Setup(r => r.IsNameUnique(request.Name)).ReturnsAsync(true);

        await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
            await _attractionLogic.CreateAttraction(request));
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
            CurrentCapacity = 0
        };
        await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
            await _attractionLogic.UpdateAttraction(Guid.NewGuid(), invalidRequest));
    }

    [TestMethod]
    public async Task UpdateAttraction_ShouldThrowException_WhenNameIsNotUnique()
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

        _mockAttractionRepository.Setup(r => r.GetById(attractionId)).ReturnsAsync(currentAttraction);
        _mockAttractionRepository.Setup(r => r.GetByName(request.Name)).ReturnsAsync(duplicateAttraction);

        await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
            await _attractionLogic.UpdateAttraction(attractionId, request));
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
            CurrentCapacity = 0
        };
        _mockAttractionRepository.Setup(r => r.IsNameUnique(request.Name)).ReturnsAsync(true);

        await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
            await _attractionLogic.UpdateAttraction(Guid.NewGuid(), request));
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
            CurrentCapacity = 0
        };
        _mockAttractionRepository.Setup(r => r.IsNameUnique(request.Name)).ReturnsAsync(true);

        await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
            await _attractionLogic.UpdateAttraction(Guid.NewGuid(), request));
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
            CurrentCapacity = 0
        };
        _mockAttractionRepository.Setup(r => r.IsNameUnique(request.Name)).ReturnsAsync(true);

        await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
            await _attractionLogic.UpdateAttraction(Guid.NewGuid(), request));
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
            CurrentCapacity = 20
        };
        _mockAttractionRepository.Setup(r => r.IsNameUnique(request.Name)).ReturnsAsync(true);
        Attraction attraction = new Attraction { Id = Guid.NewGuid(), CurrentCapacity = 0 };
        _mockAttractionRepository.Setup(r => r.GetById(attraction.Id)).ReturnsAsync(attraction);

        await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
            await _attractionLogic.UpdateAttraction(attraction.Id, request));
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
            CurrentCapacity = 0
        };
        _mockAttractionRepository.Setup(r => r.IsNameUnique(request.Name)).ReturnsAsync(true);
        Attraction attraction = new Attraction { Id = Guid.NewGuid(), CurrentCapacity = 0 };
        _mockAttractionRepository.Setup(r => r.GetById(attraction.Id)).ReturnsAsync(attraction);

        await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
            await _attractionLogic.UpdateAttraction(attraction.Id, request));
    }

    [TestMethod]
    public async Task GetAttractionById_ShouldThrowException_WhenIdDoesNotExist()
    {
        Guid newId = Guid.NewGuid();
        _mockAttractionRepository.Setup(r => r.GetById(newId)).ReturnsAsync((Attraction)null);

        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(async () =>
            await _attractionLogic.GetAttractionById(newId));
    }

    [TestMethod]
    public async Task GetAttractionIncidents_ShouldThrowException_WhenAttractionNotFound()
    {
        Guid id = Guid.NewGuid();
        _mockAttractionRepository.Setup(r => r.GetById(id)).ReturnsAsync((Attraction)null);
        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(async () =>
            await _attractionLogic.GetAttractionIncidents(id));
    }

    [TestMethod]
    public async Task GetAttractionIncidents_ShouldThrowException_WhenNoIncidents()
    {
        Attraction attraction = new Attraction { Incidents = new List<string>() };
        _mockAttractionRepository.Setup(r => r.GetById(attraction.Id)).ReturnsAsync(attraction);
        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(async () =>
            await _attractionLogic.GetAttractionIncidents(attraction.Id));
    }

    [TestMethod]
    public async Task GetAttractionIncidents_ShouldReturnIncidents_WhenHasIncidents()
    {
        Attraction attraction = new Attraction { Incidents = new List<string> { "Incidente1" } };
        _mockAttractionRepository.Setup(r => r.GetById(attraction.Id)).ReturnsAsync(attraction);
        List<string> incidents = await _attractionLogic.GetAttractionIncidents(attraction.Id);

        Assert.AreEqual(1, incidents.Count);
        Assert.AreEqual("Incidente1", incidents[0]);
    }

    [TestMethod]
    public async Task AddIncidence_ShouldThrowException_WhenAttractionNotFound()
    {
        Guid id = Guid.NewGuid();
        _mockAttractionRepository.Setup(r => r.GetById(id)).ReturnsAsync((Attraction)null);
        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(async () =>
            await _attractionLogic.AddIncident(id, "Incidente"));
    }

    [TestMethod]
    public async Task AddIncidence_ShouldAddIncident_WhenAttractionExists()
    {
        Attraction attraction = new Attraction { Incidents = new List<string>() };
        _mockAttractionRepository.Setup(r => r.GetById(attraction.Id)).ReturnsAsync(attraction);
        await _attractionLogic.AddIncident(attraction.Id, "Incidente");
        _mockAttractionRepository.Verify(r => r.Update(attraction), Times.Once);
    }

    [TestMethod]
    public async Task RemoveIncidence_ShouldThrowException_WhenAttractionNotFound()
    {
        Guid id = Guid.NewGuid();
        _mockAttractionRepository.Setup(r => r.GetById(id)).ReturnsAsync((Attraction)null);
        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(async () =>
            await _attractionLogic.RemoveIncident(id, "Incidente"));
    }

    [TestMethod]
    public async Task RemoveIncidence_ShouldRemoveIncident_WhenAttractionExists()
    {
        Attraction attraction = new Attraction { Incidents = new List<string> { "Incidente" } };
        _mockAttractionRepository.Setup(r => r.GetById(attraction.Id)).ReturnsAsync(attraction);
        await _attractionLogic.RemoveIncident(attraction.Id, "Incidente");
        _mockAttractionRepository.Verify(r => r.Update(attraction), Times.Once);
    }

    [TestMethod]
    public async Task GetCapacity_ShouldReturnCapacityResponse_WhenIdIsValid()
    {
        Guid attractionId = Guid.NewGuid();
        Attraction expectedAttraction = new Attraction
        {
            Id = attractionId,
            Name = "Carousel",
            Description = "A classic merry-go-round",
            Type = AttractionType.RollerCoaster,
            MinAge = 3,
            MaxCapacity = 50,
            CurrentCapacity = 20,
        };
        _mockAttractionRepository.Setup(r => r.GetById(attractionId)).ReturnsAsync(expectedAttraction);
        CapacityResponse result = await _attractionLogic.GetCapacity(attractionId);
        Assert.IsNotNull(result);
        Assert.AreEqual(attractionId, result.Id);
        Assert.AreEqual(50, result.Capacity);
        Assert.AreEqual(20, result.CurrentCapacity);
        _mockAttractionRepository.Verify(r => r.GetById(attractionId), Times.Once);
    }

    [TestMethod]
    public async Task GetCapacity_ShouldThrowException_WhenAttractionNotFound()
    {
        Guid attractionId = Guid.NewGuid();
        _mockAttractionRepository.Setup(r => r.GetById(attractionId)).ReturnsAsync((Attraction)null);

        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(async () =>
            await _attractionLogic.GetCapacity(attractionId));
    }

    [TestMethod]
    public async Task GetAllAttractionsVisits_ShouldReturnAttractionsWithVisitCounts_WhenReportsExist()
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

        _mockReportRepository.Setup(r => r.GetAllReports()).ReturnsAsync(reports);

        AttractionsVisitResponse result = await _attractionLogic.GetAllAttractionsVisits(request);

        Assert.IsNotNull(result);
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
    public async Task GetAllAttractionsVisits_ShouldThrowException_WhenStartDateIsAfterEndDate()
    {
        DateTime startDate = new DateTime(2025, 10, 7);
        DateTime endDate = new DateTime(2025, 10, 1);

        AttractionsVisitsRequest request = new AttractionsVisitsRequest
        {
            StartDate = startDate,
            EndDate = endDate
        };

        await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
            await _attractionLogic.GetAllAttractionsVisits(request));
    }

    [TestMethod]
    public async Task UpdateAttraction_WithNullCurrentCapacity_PreservesExistingCapacity()
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

        _mockAttractionRepository.Setup(r => r.GetById(attractionId)).ReturnsAsync(existingAttraction);
        _mockAttractionRepository.Setup(r => r.Update(It.IsAny<Attraction>())).Returns(Task.CompletedTask);

        await _attractionLogic.UpdateAttraction(attractionId, request);

        Assert.AreEqual(existingCapacity, existingAttraction.CurrentCapacity,
            "CurrentCapacity should preserve existing value when request has null");
        Assert.AreEqual("Updated Coaster", existingAttraction.Name);
        _mockAttractionRepository.Verify(r => r.Update(existingAttraction), Times.Once);
    }
}