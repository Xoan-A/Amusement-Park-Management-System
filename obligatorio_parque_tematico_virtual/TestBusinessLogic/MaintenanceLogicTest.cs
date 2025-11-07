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
    private Mock<IDateTimeLogic> _mockDateTimeLogic;
    private IMaintenanceLogic _maintenanceLogic;

    [TestInitialize]
    public void Setup()
    {
        _mockScheduleRepository = new Mock<IMaintenanceScheduleRepository>();
        _mockRecordRepository = new Mock<IMaintenanceRecordRepository>();
        _mockAttractionRepository = new Mock<IAttractionRepository>();
        _mockDateTimeLogic = new Mock<IDateTimeLogic>();
        _mockDateTimeLogic.Setup(x => x.GetCurrentDateTime()).ReturnsAsync(DateTime.Now);

        _maintenanceLogic = new MaintenanceLogic(
            _mockScheduleRepository.Object,
            _mockRecordRepository.Object,
            _mockAttractionRepository.Object,
            _mockDateTimeLogic.Object
        );
    }

    #region Schedule Tests

    [TestMethod]
    public async Task CreateSchedule_ValidRequest_ReturnsScheduleId()
    {
        Guid attractionId = Guid.NewGuid();
        Guid createdBy = Guid.NewGuid();
        Attraction attraction = CreateTestAttraction(attractionId);

        MaintenanceScheduleRequest request = new MaintenanceScheduleRequest
        {
            AttractionId = attractionId,
            ScheduledDate = DateTime.Now.AddDays(7),
            MaintenanceType = "Inspection",
            Description = "Monthly safety inspection"
        };

        _mockAttractionRepository.Setup(r => r.GetById(attractionId)).ReturnsAsync(attraction);
        _mockScheduleRepository.Setup(r => r.CreateAsync(It.IsAny<MaintenanceSchedule>())).Returns(Task.CompletedTask);

        Guid result = await _maintenanceLogic.CreateSchedule(request, createdBy);

        Assert.IsNotNull(result);
        Assert.AreNotEqual(Guid.Empty, result);
        _mockScheduleRepository.Verify(r => r.CreateAsync(It.IsAny<MaintenanceSchedule>()), Times.Once);
    }

    [TestMethod]
    public async Task CreateSchedule_NonExistentAttraction_ThrowsKeyNotFoundException()
    {
        MaintenanceScheduleRequest request = new MaintenanceScheduleRequest
        {
            AttractionId = Guid.NewGuid(),
            ScheduledDate = DateTime.Now.AddDays(7),
            MaintenanceType = "Inspection",
            Description = "Monthly safety inspection"
        };

        _mockAttractionRepository.Setup(r => r.GetById(It.IsAny<Guid>())).ReturnsAsync((Attraction)null);

        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(
            () => _maintenanceLogic.CreateSchedule(request, Guid.NewGuid())
        );
    }

    [TestMethod]
    public async Task GetScheduleById_ExistingSchedule_ReturnsScheduleResponse()
    {
        Guid scheduleId = Guid.NewGuid();
        Attraction attraction = CreateTestAttraction(Guid.NewGuid());
        MaintenanceSchedule schedule = CreateTestSchedule(scheduleId, attraction.Id, attraction);

        _mockScheduleRepository.Setup(r => r.GetByIdAsync(scheduleId)).ReturnsAsync(schedule);

        MaintenanceScheduleResponse result = await _maintenanceLogic.GetScheduleById(scheduleId);

        Assert.IsNotNull(result);
        Assert.AreEqual(scheduleId, result.Id);
        Assert.AreEqual(attraction.Name, result.AttractionName);
        Assert.AreEqual(schedule.Description, result.Description);
    }

    [TestMethod]
    public async Task GetScheduleById_NonExistentSchedule_ThrowsKeyNotFoundException()
    {
        _mockScheduleRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((MaintenanceSchedule)null);

        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(
            () => _maintenanceLogic.GetScheduleById(Guid.NewGuid())
        );
    }

    [TestMethod]
    public async Task GetAllSchedules_ReturnsListOfSchedules()
    {
        Attraction attraction = CreateTestAttraction(Guid.NewGuid());
        List<MaintenanceSchedule> schedules = new List<MaintenanceSchedule>
        {
            CreateTestSchedule(Guid.NewGuid(), attraction.Id, attraction),
            CreateTestSchedule(Guid.NewGuid(), attraction.Id, attraction)
        };

        _mockScheduleRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(schedules);

        List<MaintenanceScheduleResponse> result = await _maintenanceLogic.GetAllSchedules();

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
        _mockScheduleRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [TestMethod]
    public async Task GetSchedulesByAttraction_ReturnsFilteredSchedules()
    {
        Guid attractionId = Guid.NewGuid();
        Attraction attraction = CreateTestAttraction(attractionId);
        List<MaintenanceSchedule> schedules = new List<MaintenanceSchedule>
        {
            CreateTestSchedule(Guid.NewGuid(), attractionId, attraction),
            CreateTestSchedule(Guid.NewGuid(), attractionId, attraction)
        };

        _mockScheduleRepository.Setup(r => r.GetByAttractionIdAsync(attractionId)).ReturnsAsync(schedules);

        List<MaintenanceScheduleResponse> result = await _maintenanceLogic.GetSchedulesByAttraction(attractionId);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.All(s => s.AttractionId == attractionId));
    }

    [TestMethod]
    public async Task GetOverdueSchedules_ReturnsOverdueSchedules()
    {
        Attraction attraction = CreateTestAttraction(Guid.NewGuid());
        MaintenanceSchedule overdueSchedule = CreateTestSchedule(Guid.NewGuid(), attraction.Id, attraction);
        overdueSchedule.ScheduledDate = DateTime.Now.AddDays(-1);
        overdueSchedule.Status = MaintenanceStatus.Pending;

        List<MaintenanceSchedule> schedules = new List<MaintenanceSchedule> { overdueSchedule };
        _mockScheduleRepository.Setup(r => r.GetOverdueSchedulesAsync()).ReturnsAsync(schedules);

        List<MaintenanceScheduleResponse> result = await _maintenanceLogic.GetOverdueSchedules();

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.IsTrue(result[0].IsOverdue);
    }

    [TestMethod]
    public async Task GetUpcomingSchedules_ReturnsUpcomingSchedules()
    {
        Attraction attraction = CreateTestAttraction(Guid.NewGuid());
        MaintenanceSchedule upcomingSchedule = CreateTestSchedule(Guid.NewGuid(), attraction.Id, attraction);
        upcomingSchedule.ScheduledDate = DateTime.Now.AddDays(3);

        List<MaintenanceSchedule> schedules = new List<MaintenanceSchedule> { upcomingSchedule };
        _mockScheduleRepository.Setup(r => r.GetUpcomingSchedulesAsync(7)).ReturnsAsync(schedules);

        List<MaintenanceScheduleResponse> result = await _maintenanceLogic.GetUpcomingSchedules(7);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task UpdateScheduleStatus_ValidScheduleAndStatus_UpdatesSuccessfully()
    {
        Guid scheduleId = Guid.NewGuid();
        Attraction attraction = CreateTestAttraction(Guid.NewGuid());
        MaintenanceSchedule schedule = CreateTestSchedule(scheduleId, attraction.Id, attraction);

        _mockScheduleRepository.Setup(r => r.GetByIdAsync(scheduleId)).ReturnsAsync(schedule);
        _mockScheduleRepository.Setup(r => r.UpdateAsync(It.IsAny<MaintenanceSchedule>())).Returns(Task.CompletedTask);

        await _maintenanceLogic.UpdateScheduleStatus(scheduleId, "Completed");

        _mockScheduleRepository.Verify(r => r.UpdateAsync(It.Is<MaintenanceSchedule>(
            s => s.Id == scheduleId && s.Status == MaintenanceStatus.Completed
        )), Times.Once);
    }

    [TestMethod]
    public async Task UpdateScheduleStatus_InvalidStatus_ThrowsArgumentException()
    {
        Guid scheduleId = Guid.NewGuid();
        Attraction attraction = CreateTestAttraction(Guid.NewGuid());
        MaintenanceSchedule schedule = CreateTestSchedule(scheduleId, attraction.Id, attraction);

        _mockScheduleRepository.Setup(r => r.GetByIdAsync(scheduleId)).ReturnsAsync(schedule);

        await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _maintenanceLogic.UpdateScheduleStatus(scheduleId, "InvalidStatus")
        );
    }

    [TestMethod]
    public async Task DeleteSchedule_ExistingSchedule_DeletesSuccessfully()
    {
        Guid scheduleId = Guid.NewGuid();
        _mockScheduleRepository.Setup(r => r.DeleteAsync(scheduleId)).Returns(Task.CompletedTask);

        await _maintenanceLogic.DeleteSchedule(scheduleId);

        _mockScheduleRepository.Verify(r => r.DeleteAsync(scheduleId), Times.Once);
    }

    #endregion

    #region Record Tests

    [TestMethod]
    public async Task RecordMaintenance_ValidRequest_ReturnsRecordId()
    {
        Guid attractionId = Guid.NewGuid();
        Guid performedBy = Guid.NewGuid();
        Attraction attraction = CreateTestAttraction(attractionId);

        MaintenanceRecordRequest request = new MaintenanceRecordRequest
        {
            AttractionId = attractionId,
            PerformedDate = DateTime.Now,
            MaintenanceType = "Inspection",
            Description = "Completed safety inspection",
            Duration = TimeSpan.FromHours(2)
        };

        _mockAttractionRepository.Setup(r => r.GetById(attractionId)).ReturnsAsync(attraction);
        _mockRecordRepository.Setup(r => r.CreateAsync(It.IsAny<MaintenanceRecord>())).Returns(Task.CompletedTask);

        Guid result = await _maintenanceLogic.RecordMaintenance(request, performedBy);

        Assert.IsNotNull(result);
        Assert.AreNotEqual(Guid.Empty, result);
        _mockRecordRepository.Verify(r => r.CreateAsync(It.IsAny<MaintenanceRecord>()), Times.Once);
    }

    [TestMethod]
    public async Task RecordMaintenance_NonExistentAttraction_ThrowsKeyNotFoundException()
    {
        MaintenanceRecordRequest request = new MaintenanceRecordRequest
        {
            AttractionId = Guid.NewGuid(),
            PerformedDate = DateTime.Now,
            MaintenanceType = "Inspection",
            Description = "Test",
            Duration = TimeSpan.FromHours(1)
        };

        _mockAttractionRepository.Setup(r => r.GetById(It.IsAny<Guid>())).ReturnsAsync((Attraction)null);

        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(
            () => _maintenanceLogic.RecordMaintenance(request, Guid.NewGuid())
        );
    }

    [TestMethod]
    public async Task GetRecordById_ExistingRecord_ReturnsRecordResponse()
    {
        Guid recordId = Guid.NewGuid();
        Attraction attraction = CreateTestAttraction(Guid.NewGuid());
        User operatorUser = CreateTestUser();
        MaintenanceRecord record = CreateTestRecord(recordId, attraction.Id, attraction, operatorUser.Id, operatorUser);

        _mockRecordRepository.Setup(r => r.GetByIdAsync(recordId)).ReturnsAsync(record);

        MaintenanceRecordResponse result = await _maintenanceLogic.GetRecordById(recordId);

        Assert.IsNotNull(result);
        Assert.AreEqual(recordId, result.Id);
        Assert.AreEqual(attraction.Name, result.AttractionName);
    }

    [TestMethod]
    public async Task GetRecordById_NonExistentRecord_ThrowsKeyNotFoundException()
    {
        _mockRecordRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((MaintenanceRecord)null);

        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(
            () => _maintenanceLogic.GetRecordById(Guid.NewGuid())
        );
    }

    [TestMethod]
    public async Task GetAllRecords_ReturnsListOfRecords()
    {
        Attraction attraction = CreateTestAttraction(Guid.NewGuid());
        User operatorUser = CreateTestUser();
        List<MaintenanceRecord> records = new List<MaintenanceRecord>
        {
            CreateTestRecord(Guid.NewGuid(), attraction.Id, attraction, operatorUser.Id, operatorUser),
            CreateTestRecord(Guid.NewGuid(), attraction.Id, attraction, operatorUser.Id, operatorUser)
        };

        _mockRecordRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(records);

        List<MaintenanceRecordResponse> result = await _maintenanceLogic.GetAllRecords();

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public async Task GetRecordsByAttraction_ReturnsFilteredRecords()
    {
        Guid attractionId = Guid.NewGuid();
        Attraction attraction = CreateTestAttraction(attractionId);
        User operatorUser = CreateTestUser();
        List<MaintenanceRecord> records = new List<MaintenanceRecord>
        {
            CreateTestRecord(Guid.NewGuid(), attractionId, attraction, operatorUser.Id, operatorUser)
        };

        _mockRecordRepository.Setup(r => r.GetByAttractionIdAsync(attractionId)).ReturnsAsync(records);

        List<MaintenanceRecordResponse> result = await _maintenanceLogic.GetRecordsByAttraction(attractionId);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(attractionId, result[0].AttractionId);
    }

    [TestMethod]
    public async Task GetRecordsByOperator_ReturnsFilteredRecords()
    {
        Guid operatorId = Guid.NewGuid();
        Attraction attraction = CreateTestAttraction(Guid.NewGuid());
        User operatorUser = CreateTestUser();
        List<MaintenanceRecord> records = new List<MaintenanceRecord>
        {
            CreateTestRecord(Guid.NewGuid(), attraction.Id, attraction, operatorId, operatorUser)
        };

        _mockRecordRepository.Setup(r => r.GetByOperatorAsync(operatorId)).ReturnsAsync(records);

        List<MaintenanceRecordResponse> result = await _maintenanceLogic.GetRecordsByOperator(operatorId);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(operatorId, result[0].PerformedBy);
    }

    [TestMethod]
    public async Task GetUnscheduledMaintenance_ReturnsUnscheduledRecords()
    {
        Attraction attraction = CreateTestAttraction(Guid.NewGuid());
        User operatorUser = CreateTestUser();
        MaintenanceRecord record =
            CreateTestRecord(Guid.NewGuid(), attraction.Id, attraction, operatorUser.Id, operatorUser);
        record.MaintenanceScheduleId = null;

        _mockRecordRepository.Setup(r => r.GetUnscheduledMaintenanceAsync()).ReturnsAsync(new List<MaintenanceRecord> { record });

        List<MaintenanceRecordResponse> result = await _maintenanceLogic.GetUnscheduledMaintenance();

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.IsNull(result[0].MaintenanceScheduleId);
    }

    [TestMethod]
    public async Task GetMaintenanceHistory_ReturnsRecordsInDateRange()
    {
        Guid attractionId = Guid.NewGuid();
        DateTime dateFrom = DateTime.Now.AddDays(-30);
        DateTime dateTo = DateTime.Now;
        Attraction attraction = CreateTestAttraction(attractionId);
        User operatorUser = CreateTestUser();
        List<MaintenanceRecord> records = new List<MaintenanceRecord>
        {
            CreateTestRecord(Guid.NewGuid(), attractionId, attraction, operatorUser.Id, operatorUser)
        };

        _mockRecordRepository.Setup(r => r.GetByAttractionIdAndDateRangeAsync(attractionId, dateFrom, dateTo))
            .ReturnsAsync(records);

        List<MaintenanceRecordResponse> result =
            await _maintenanceLogic.GetMaintenanceHistory(attractionId, dateFrom, dateTo);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
    }

    #endregion

    #region Business Operations

    [TestMethod]
    public async Task CompleteMaintenance_ValidScheduleAndRequest_CompletesAndCreatesRecord()
    {
        Guid scheduleId = Guid.NewGuid();
        Guid attractionId = Guid.NewGuid();
        Guid performedBy = Guid.NewGuid();
        Attraction attraction = CreateTestAttraction(attractionId);
        MaintenanceSchedule schedule = CreateTestSchedule(scheduleId, attractionId, attraction);
        schedule.Status = MaintenanceStatus.Pending;

        MaintenanceRecordRequest request = new MaintenanceRecordRequest
        {
            MaintenanceScheduleId = scheduleId,
            AttractionId = attractionId,
            PerformedDate = DateTime.Now,
            MaintenanceType = "Inspection",
            Description = "Completed scheduled inspection",
            Duration = TimeSpan.FromHours(2)
        };

        _mockScheduleRepository.Setup(r => r.GetByIdAsync(scheduleId)).ReturnsAsync(schedule);
        _mockAttractionRepository.Setup(r => r.GetById(attractionId)).ReturnsAsync(attraction);
        _mockScheduleRepository.Setup(r => r.UpdateAsync(It.IsAny<MaintenanceSchedule>())).Returns(Task.CompletedTask);
        _mockRecordRepository.Setup(r => r.CreateAsync(It.IsAny<MaintenanceRecord>())).Returns(Task.CompletedTask);

        Guid result = await _maintenanceLogic.CompleteMaintenance(scheduleId, request, performedBy);

        Assert.IsNotNull(result);
        Assert.AreNotEqual(Guid.Empty, result);
        _mockScheduleRepository.Verify(r => r.UpdateAsync(It.Is<MaintenanceSchedule>(
            s => s.Id == scheduleId && s.Status == MaintenanceStatus.Completed
        )), Times.Once);
        _mockRecordRepository.Verify(r => r.CreateAsync(It.IsAny<MaintenanceRecord>()), Times.Once);
    }

    [TestMethod]
    public async Task CompleteMaintenance_NonExistentSchedule_ThrowsKeyNotFoundException()
    {
        MaintenanceRecordRequest request = new MaintenanceRecordRequest
        {
            AttractionId = Guid.NewGuid(),
            PerformedDate = DateTime.Now,
            MaintenanceType = "Inspection",
            Description = "Test",
            Duration = TimeSpan.FromHours(1)
        };

        _mockScheduleRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((MaintenanceSchedule)null);

        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(
            () => _maintenanceLogic.CompleteMaintenance(Guid.NewGuid(), request, Guid.NewGuid())
        );
    }

    [TestMethod]
    public async Task CompleteMaintenance_AlreadyCompletedSchedule_ThrowsArgumentException()
    {
        Guid scheduleId = Guid.NewGuid();
        Attraction attraction = CreateTestAttraction(Guid.NewGuid());
        MaintenanceSchedule schedule = CreateTestSchedule(scheduleId, attraction.Id, attraction);
        schedule.Status = MaintenanceStatus.Completed;

        MaintenanceRecordRequest request = new MaintenanceRecordRequest
        {
            AttractionId = attraction.Id,
            PerformedDate = DateTime.Now,
            MaintenanceType = "Inspection",
            Description = "Test",
            Duration = TimeSpan.FromHours(1)
        };

        _mockScheduleRepository.Setup(r => r.GetByIdAsync(scheduleId)).ReturnsAsync(schedule);

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

    private MaintenanceRecord CreateTestRecord(Guid id, Guid attractionId, Attraction attraction, Guid performedBy,
        User operatorUser)
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

    [TestMethod]
    public async Task CreateSchedule_WithInvalidMaintenanceType_ThrowsArgumentException()
    {
        Guid attractionId = Guid.NewGuid();
        Attraction attraction = new Attraction
        {
            Id = attractionId,
            Name = "Test Attraction",
            Description = "Test",
            Type = AttractionType.RollerCoaster,
            MaxCapacity = 100
        };

        MaintenanceScheduleRequest request = new MaintenanceScheduleRequest
        {
            AttractionId = attractionId,
            ScheduledDate = DateTime.UtcNow.AddDays(5),
            MaintenanceType = "InvalidTypeValue",
            Description = "Test maintenance"
        };

        Guid userId = Guid.NewGuid();
        _mockAttractionRepository.Setup(r => r.GetById(attractionId)).ReturnsAsync(attraction);

        await Assert.ThrowsExceptionAsync<ArgumentException>(
            async () => await _maintenanceLogic.CreateSchedule(request, userId),
            "Should throw ArgumentException for invalid MaintenanceType");
    }

    [TestMethod]
    public async Task RecordMaintenance_WithInvalidMaintenanceType_ThrowsArgumentException()
    {
        Guid attractionId = Guid.NewGuid();
        Attraction attraction = new Attraction
        {
            Id = attractionId,
            Name = "Test Attraction",
            Description = "Test",
            Type = AttractionType.RollerCoaster,
            MaxCapacity = 100
        };

        MaintenanceRecordRequest request = new MaintenanceRecordRequest
        {
            AttractionId = attractionId,
            MaintenanceScheduleId = null,
            MaintenanceType = "BadEnumValue",
            Description = "Test record",
            Duration = TimeSpan.FromHours(1)
        };

        Guid userId = Guid.NewGuid();
        _mockAttractionRepository.Setup(r => r.GetById(attractionId)).ReturnsAsync(attraction);

        await Assert.ThrowsExceptionAsync<ArgumentException>(
            async () => await _maintenanceLogic.RecordMaintenance(request, userId),
            "Should throw ArgumentException for invalid MaintenanceType");
    }

    #endregion
}