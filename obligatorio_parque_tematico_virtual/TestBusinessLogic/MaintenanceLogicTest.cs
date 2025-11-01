using Moq;
using Domain;
using IBusinessLogic;
using IDataAccess;
using BusinessLogic;
using Models.In;
using Models.Out;

namespace TestBusinessLogic;

[TestClass]
public class MaintenanceLogicTest
{
    private Mock<IMaintenanceScheduleRepository> _mockScheduleRepository;
    private Mock<IMaintenanceRecordRepository> _mockRecordRepository;
    private Mock<IAttractionRepository> _mockAttractionRepository;
    private IMaintenanceLogic _maintenanceLogic;

    [TestInitialize]
    public void Setup()
    {
        _mockScheduleRepository = new Mock<IMaintenanceScheduleRepository>();
        _mockRecordRepository = new Mock<IMaintenanceRecordRepository>();
        _mockAttractionRepository = new Mock<IAttractionRepository>();
        _maintenanceLogic = new MaintenanceLogic(
            _mockScheduleRepository.Object,
            _mockRecordRepository.Object,
            _mockAttractionRepository.Object
        );
    }

    #region Schedule Tests

    [TestMethod]
    public async Task CreateSchedule_ValidRequest_ReturnsScheduleId()
    {
        // Arrange
        var attractionId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();
        var attraction = CreateTestAttraction(attractionId);

        var request = new MaintenanceScheduleRequest
        {
            AttractionId = attractionId,
            ScheduledDate = DateTime.Now.AddDays(7),
            MaintenanceType = "Inspection",
            Description = "Monthly safety inspection"
        };

        _mockAttractionRepository.Setup(r => r.GetById(attractionId)).ReturnsAsync(attraction);
        _mockScheduleRepository.Setup(r => r.Create(It.IsAny<MaintenanceSchedule>()));

        // Act
        var result = await _maintenanceLogic.CreateSchedule(request, createdBy);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreNotEqual(Guid.Empty, result);
        _mockScheduleRepository.Verify(r => r.Create(It.IsAny<MaintenanceSchedule>()), Times.Once);
    }

    [TestMethod]
    public async Task CreateSchedule_NonExistentAttraction_ThrowsKeyNotFoundException()
    {
        // Arrange
        var request = new MaintenanceScheduleRequest
        {
            AttractionId = Guid.NewGuid(),
            ScheduledDate = DateTime.Now.AddDays(7),
            MaintenanceType = "Inspection",
            Description = "Monthly safety inspection"
        };

        _mockAttractionRepository.Setup(r => r.GetById(It.IsAny<Guid>())).ReturnsAsync((Attraction)null);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(
            () => _maintenanceLogic.CreateSchedule(request, Guid.NewGuid())
        );
    }

    [TestMethod]
    public async Task GetScheduleById_ExistingSchedule_ReturnsScheduleResponse()
    {
        // Arrange
        var scheduleId = Guid.NewGuid();
        var attraction = CreateTestAttraction(Guid.NewGuid());
        var schedule = CreateTestSchedule(scheduleId, attraction.Id, attraction);

        _mockScheduleRepository.Setup(r => r.GetById(scheduleId)).Returns(schedule);

        // Act
        var result = await _maintenanceLogic.GetScheduleById(scheduleId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(scheduleId, result.Id);
        Assert.AreEqual(attraction.Name, result.AttractionName);
        Assert.AreEqual(schedule.Description, result.Description);
    }

    [TestMethod]
    public async Task GetScheduleById_NonExistentSchedule_ThrowsKeyNotFoundException()
    {
        // Arrange
        _mockScheduleRepository.Setup(r => r.GetById(It.IsAny<Guid>())).Returns((MaintenanceSchedule)null);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(
            () => _maintenanceLogic.GetScheduleById(Guid.NewGuid())
        );
    }

    [TestMethod]
    public async Task GetAllSchedules_ReturnsListOfSchedules()
    {
        // Arrange
        var attraction = CreateTestAttraction(Guid.NewGuid());
        var schedules = new List<MaintenanceSchedule>
        {
            CreateTestSchedule(Guid.NewGuid(), attraction.Id, attraction),
            CreateTestSchedule(Guid.NewGuid(), attraction.Id, attraction)
        };

        _mockScheduleRepository.Setup(r => r.GetAll()).Returns(schedules);

        // Act
        var result = await _maintenanceLogic.GetAllSchedules();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
        _mockScheduleRepository.Verify(r => r.GetAll(), Times.Once);
    }

    [TestMethod]
    public async Task GetSchedulesByAttraction_ReturnsFilteredSchedules()
    {
        // Arrange
        var attractionId = Guid.NewGuid();
        var attraction = CreateTestAttraction(attractionId);
        var schedules = new List<MaintenanceSchedule>
        {
            CreateTestSchedule(Guid.NewGuid(), attractionId, attraction),
            CreateTestSchedule(Guid.NewGuid(), attractionId, attraction)
        };

        _mockScheduleRepository.Setup(r => r.GetByAttractionId(attractionId)).Returns(schedules);

        // Act
        var result = await _maintenanceLogic.GetSchedulesByAttraction(attractionId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.All(s => s.AttractionId == attractionId));
    }

    [TestMethod]
    public async Task GetOverdueSchedules_ReturnsOverdueSchedules()
    {
        // Arrange
        var attraction = CreateTestAttraction(Guid.NewGuid());
        var overdueSchedule = CreateTestSchedule(Guid.NewGuid(), attraction.Id, attraction);
        overdueSchedule.ScheduledDate = DateTime.Now.AddDays(-1);
        overdueSchedule.Status = MaintenanceStatus.Pending;

        var schedules = new List<MaintenanceSchedule> { overdueSchedule };
        _mockScheduleRepository.Setup(r => r.GetOverdueSchedules()).Returns(schedules);

        // Act
        var result = await _maintenanceLogic.GetOverdueSchedules();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.IsTrue(result[0].IsOverdue);
    }

    [TestMethod]
    public async Task GetUpcomingSchedules_ReturnsUpcomingSchedules()
    {
        // Arrange
        var attraction = CreateTestAttraction(Guid.NewGuid());
        var upcomingSchedule = CreateTestSchedule(Guid.NewGuid(), attraction.Id, attraction);
        upcomingSchedule.ScheduledDate = DateTime.Now.AddDays(3);

        var schedules = new List<MaintenanceSchedule> { upcomingSchedule };
        _mockScheduleRepository.Setup(r => r.GetUpcomingSchedules(7)).Returns(schedules);

        // Act
        var result = await _maintenanceLogic.GetUpcomingSchedules(7);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task UpdateScheduleStatus_ValidScheduleAndStatus_UpdatesSuccessfully()
    {
        // Arrange
        var scheduleId = Guid.NewGuid();
        var attraction = CreateTestAttraction(Guid.NewGuid());
        var schedule = CreateTestSchedule(scheduleId, attraction.Id, attraction);

        _mockScheduleRepository.Setup(r => r.GetById(scheduleId)).Returns(schedule);
        _mockScheduleRepository.Setup(r => r.Update(It.IsAny<MaintenanceSchedule>()));

        // Act
        await _maintenanceLogic.UpdateScheduleStatus(scheduleId, "Completed");

        // Assert
        _mockScheduleRepository.Verify(r => r.Update(It.Is<MaintenanceSchedule>(
            s => s.Id == scheduleId && s.Status == MaintenanceStatus.Completed
        )), Times.Once);
    }

    [TestMethod]
    public async Task UpdateScheduleStatus_InvalidStatus_ThrowsArgumentException()
    {
        // Arrange
        var scheduleId = Guid.NewGuid();
        var attraction = CreateTestAttraction(Guid.NewGuid());
        var schedule = CreateTestSchedule(scheduleId, attraction.Id, attraction);

        _mockScheduleRepository.Setup(r => r.GetById(scheduleId)).Returns(schedule);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _maintenanceLogic.UpdateScheduleStatus(scheduleId, "InvalidStatus")
        );
    }

    [TestMethod]
    public async Task DeleteSchedule_ExistingSchedule_DeletesSuccessfully()
    {
        // Arrange
        var scheduleId = Guid.NewGuid();
        _mockScheduleRepository.Setup(r => r.Delete(scheduleId));

        // Act
        await _maintenanceLogic.DeleteSchedule(scheduleId);

        // Assert
        _mockScheduleRepository.Verify(r => r.Delete(scheduleId), Times.Once);
    }

    #endregion

    #region Record Tests

    [TestMethod]
    public async Task RecordMaintenance_ValidRequest_ReturnsRecordId()
    {
        // Arrange
        var attractionId = Guid.NewGuid();
        var performedBy = Guid.NewGuid();
        var attraction = CreateTestAttraction(attractionId);

        var request = new MaintenanceRecordRequest
        {
            AttractionId = attractionId,
            PerformedDate = DateTime.Now,
            MaintenanceType = "Inspection",
            Description = "Completed safety inspection",
            Duration = TimeSpan.FromHours(2)
        };

        _mockAttractionRepository.Setup(r => r.GetById(attractionId)).ReturnsAsync(attraction);
        _mockRecordRepository.Setup(r => r.Create(It.IsAny<MaintenanceRecord>()));

        // Act
        var result = await _maintenanceLogic.RecordMaintenance(request, performedBy);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreNotEqual(Guid.Empty, result);
        _mockRecordRepository.Verify(r => r.Create(It.IsAny<MaintenanceRecord>()), Times.Once);
    }

    [TestMethod]
    public async Task RecordMaintenance_NonExistentAttraction_ThrowsKeyNotFoundException()
    {
        // Arrange
        var request = new MaintenanceRecordRequest
        {
            AttractionId = Guid.NewGuid(),
            PerformedDate = DateTime.Now,
            MaintenanceType = "Inspection",
            Description = "Test",
            Duration = TimeSpan.FromHours(1)
        };

        _mockAttractionRepository.Setup(r => r.GetById(It.IsAny<Guid>())).ReturnsAsync((Attraction)null);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(
            () => _maintenanceLogic.RecordMaintenance(request, Guid.NewGuid())
        );
    }

    [TestMethod]
    public async Task GetRecordById_ExistingRecord_ReturnsRecordResponse()
    {
        // Arrange
        var recordId = Guid.NewGuid();
        var attraction = CreateTestAttraction(Guid.NewGuid());
        var operatorUser = CreateTestUser();
        var record = CreateTestRecord(recordId, attraction.Id, attraction, operatorUser.Id, operatorUser);

        _mockRecordRepository.Setup(r => r.GetById(recordId)).Returns(record);

        // Act
        var result = await _maintenanceLogic.GetRecordById(recordId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(recordId, result.Id);
        Assert.AreEqual(attraction.Name, result.AttractionName);
    }

    [TestMethod]
    public async Task GetRecordById_NonExistentRecord_ThrowsKeyNotFoundException()
    {
        // Arrange
        _mockRecordRepository.Setup(r => r.GetById(It.IsAny<Guid>())).Returns((MaintenanceRecord)null);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(
            () => _maintenanceLogic.GetRecordById(Guid.NewGuid())
        );
    }

    [TestMethod]
    public async Task GetAllRecords_ReturnsListOfRecords()
    {
        // Arrange
        var attraction = CreateTestAttraction(Guid.NewGuid());
        var operatorUser = CreateTestUser();
        var records = new List<MaintenanceRecord>
        {
            CreateTestRecord(Guid.NewGuid(), attraction.Id, attraction, operatorUser.Id, operatorUser),
            CreateTestRecord(Guid.NewGuid(), attraction.Id, attraction, operatorUser.Id, operatorUser)
        };

        _mockRecordRepository.Setup(r => r.GetAll()).Returns(records);

        // Act
        var result = await _maintenanceLogic.GetAllRecords();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public async Task GetRecordsByAttraction_ReturnsFilteredRecords()
    {
        // Arrange
        var attractionId = Guid.NewGuid();
        var attraction = CreateTestAttraction(attractionId);
        var operatorUser = CreateTestUser();
        var records = new List<MaintenanceRecord>
        {
            CreateTestRecord(Guid.NewGuid(), attractionId, attraction, operatorUser.Id, operatorUser)
        };

        _mockRecordRepository.Setup(r => r.GetByAttractionId(attractionId)).Returns(records);

        // Act
        var result = await _maintenanceLogic.GetRecordsByAttraction(attractionId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(attractionId, result[0].AttractionId);
    }

    [TestMethod]
    public async Task GetRecordsByOperator_ReturnsFilteredRecords()
    {
        // Arrange
        var operatorId = Guid.NewGuid();
        var attraction = CreateTestAttraction(Guid.NewGuid());
        var operatorUser = CreateTestUser();
        var records = new List<MaintenanceRecord>
        {
            CreateTestRecord(Guid.NewGuid(), attraction.Id, attraction, operatorId, operatorUser)
        };

        _mockRecordRepository.Setup(r => r.GetByOperator(operatorId)).Returns(records);

        // Act
        var result = await _maintenanceLogic.GetRecordsByOperator(operatorId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(operatorId, result[0].PerformedBy);
    }

    [TestMethod]
    public async Task GetUnscheduledMaintenance_ReturnsUnscheduledRecords()
    {
        // Arrange
        var attraction = CreateTestAttraction(Guid.NewGuid());
        var operatorUser = CreateTestUser();
        var record = CreateTestRecord(Guid.NewGuid(), attraction.Id, attraction, operatorUser.Id, operatorUser);
        record.MaintenanceScheduleId = null;

        _mockRecordRepository.Setup(r => r.GetUnscheduledMaintenance()).Returns(new List<MaintenanceRecord> { record });

        // Act
        var result = await _maintenanceLogic.GetUnscheduledMaintenance();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.IsNull(result[0].MaintenanceScheduleId);
    }

    [TestMethod]
    public async Task GetMaintenanceHistory_ReturnsRecordsInDateRange()
    {
        // Arrange
        var attractionId = Guid.NewGuid();
        var dateFrom = DateTime.Now.AddDays(-30);
        var dateTo = DateTime.Now;
        var attraction = CreateTestAttraction(attractionId);
        var operatorUser = CreateTestUser();
        var records = new List<MaintenanceRecord>
        {
            CreateTestRecord(Guid.NewGuid(), attractionId, attraction, operatorUser.Id, operatorUser)
        };

        _mockRecordRepository.Setup(r => r.GetByAttractionIdAndDateRange(attractionId, dateFrom, dateTo))
            .Returns(records);

        // Act
        var result = await _maintenanceLogic.GetMaintenanceHistory(attractionId, dateFrom, dateTo);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
    }

    #endregion

    #region Business Operations

    [TestMethod]
    public async Task CompleteMaintenance_ValidScheduleAndRequest_CompletesAndCreatesRecord()
    {
        // Arrange
        var scheduleId = Guid.NewGuid();
        var attractionId = Guid.NewGuid();
        var performedBy = Guid.NewGuid();
        var attraction = CreateTestAttraction(attractionId);
        var schedule = CreateTestSchedule(scheduleId, attractionId, attraction);
        schedule.Status = MaintenanceStatus.Pending;

        var request = new MaintenanceRecordRequest
        {
            MaintenanceScheduleId = scheduleId,
            AttractionId = attractionId,
            PerformedDate = DateTime.Now,
            MaintenanceType = "Inspection",
            Description = "Completed scheduled inspection",
            Duration = TimeSpan.FromHours(2)
        };

        _mockScheduleRepository.Setup(r => r.GetById(scheduleId)).Returns(schedule);
        _mockAttractionRepository.Setup(r => r.GetById(attractionId)).ReturnsAsync(attraction);
        _mockScheduleRepository.Setup(r => r.Update(It.IsAny<MaintenanceSchedule>()));
        _mockRecordRepository.Setup(r => r.Create(It.IsAny<MaintenanceRecord>()));

        // Act
        var result = await _maintenanceLogic.CompleteMaintenance(scheduleId, request, performedBy);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreNotEqual(Guid.Empty, result);
        _mockScheduleRepository.Verify(r => r.Update(It.Is<MaintenanceSchedule>(
            s => s.Id == scheduleId && s.Status == MaintenanceStatus.Completed
        )), Times.Once);
        _mockRecordRepository.Verify(r => r.Create(It.IsAny<MaintenanceRecord>()), Times.Once);
    }

    [TestMethod]
    public async Task CompleteMaintenance_NonExistentSchedule_ThrowsKeyNotFoundException()
    {
        // Arrange
        var request = new MaintenanceRecordRequest
        {
            AttractionId = Guid.NewGuid(),
            PerformedDate = DateTime.Now,
            MaintenanceType = "Inspection",
            Description = "Test",
            Duration = TimeSpan.FromHours(1)
        };

        _mockScheduleRepository.Setup(r => r.GetById(It.IsAny<Guid>())).Returns((MaintenanceSchedule)null);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(
            () => _maintenanceLogic.CompleteMaintenance(Guid.NewGuid(), request, Guid.NewGuid())
        );
    }

    [TestMethod]
    public async Task CompleteMaintenance_AlreadyCompletedSchedule_ThrowsArgumentException()
    {
        // Arrange
        var scheduleId = Guid.NewGuid();
        var attraction = CreateTestAttraction(Guid.NewGuid());
        var schedule = CreateTestSchedule(scheduleId, attraction.Id, attraction);
        schedule.Status = MaintenanceStatus.Completed;

        var request = new MaintenanceRecordRequest
        {
            AttractionId = attraction.Id,
            PerformedDate = DateTime.Now,
            MaintenanceType = "Inspection",
            Description = "Test",
            Duration = TimeSpan.FromHours(1)
        };

        _mockScheduleRepository.Setup(r => r.GetById(scheduleId)).Returns(schedule);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _maintenanceLogic.CompleteMaintenance(scheduleId, request, Guid.NewGuid())
        );
    }

    #endregion

    #region Helper Methods

    private Attraction CreateTestAttraction(Guid id)
    {
        return new Attraction
        {
            Id = id,
            Name = "Test Attraction",
            Description = "Test Description",
            Type = AttractionType.RollerCoaster,
            MinAge = 10,
            MaxCapacity = 100,
            CurrentCapacity = 0
        };
    }

    private MaintenanceSchedule CreateTestSchedule(Guid id, Guid attractionId, Attraction attraction)
    {
        return new MaintenanceSchedule
        {
            Id = id,
            AttractionId = attractionId,
            Attraction = attraction,
            ScheduledDate = DateTime.Now.AddDays(7),
            MaintenanceType = MaintenanceType.Inspection,
            Description = "Test maintenance schedule",
            Status = MaintenanceStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    private User CreateTestUser()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            LastName = "Operator",
            Email = "operator@test.com",
            Password = "hashedpassword",
            BirthDate = new DateTime(1990, 1, 1)
        };
    }

    private MaintenanceRecord CreateTestRecord(Guid id, Guid attractionId, Attraction attraction, Guid performedBy, User operatorUser)
    {
        return new MaintenanceRecord
        {
            Id = id,
            AttractionId = attractionId,
            Attraction = attraction,
            PerformedDate = DateTime.Now,
            PerformedBy = performedBy,
            Operator = operatorUser,
            MaintenanceType = MaintenanceType.Inspection,
            Description = "Test maintenance record",
            Duration = TimeSpan.FromHours(2),
            CreatedAt = DateTime.UtcNow
        };
    }

    #endregion
}
