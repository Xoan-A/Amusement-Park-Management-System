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
    private Mock<IAttractionRepository> _mockAttractionRepository;
    private Mock<IAttractionLogic> _mockAttractionLogic;
    private Mock<IDateTimeLogic> _mockDateTimeLogic;
    private IMaintenanceLogic _maintenanceLogic;

    [TestInitialize]
    public void Setup()
    {
        _mockScheduleRepository = new Mock<IMaintenanceScheduleRepository>();
        _mockAttractionRepository = new Mock<IAttractionRepository>();
        _mockAttractionLogic = new Mock<IAttractionLogic>();
        _mockDateTimeLogic = new Mock<IDateTimeLogic>();

        _mockDateTimeLogic.Setup(d => d.GetCurrentDateTime()).ReturnsAsync(new DateTime(2025, 11, 7, 12, 0, 0));

        _maintenanceLogic = new MaintenanceLogic(
            _mockScheduleRepository.Object,
            _mockAttractionRepository.Object,
            _mockAttractionLogic.Object
        );
    }

    #region Schedule Tests

    [TestMethod]
    public async Task CreateSchedule_ValidRequest_ReturnsScheduleId()
    {
        Guid attractionId = Guid.NewGuid();
        Attraction attraction = CreateTestAttraction(attractionId);

        MaintenanceScheduleRequest request = new MaintenanceScheduleRequest
        {
            AttractionId = attractionId,
            ScheduledDate = DateTime.Now.AddDays(7),
            Description = "Monthly safety inspection",
            EstimatedDuration = 120
        };

        _mockAttractionRepository.Setup(r => r.GetById(attractionId)).ReturnsAsync(attraction);
        _mockScheduleRepository.Setup(r => r.CreateAsync(It.IsAny<MaintenanceSchedule>())).Returns(Task.CompletedTask);

        Guid result = await _maintenanceLogic.CreateSchedule(request);

        Assert.AreNotEqual(Guid.Empty, result);
        _mockScheduleRepository.Verify(r => r.CreateAsync(It.Is<MaintenanceSchedule>(
            s => s.EstimatedDuration == 120
        )), Times.Once);
    }

    [TestMethod]
    public async Task CreateSchedule_NonExistentAttraction_ThrowsKeyNotFoundException()
    {
        MaintenanceScheduleRequest request = new MaintenanceScheduleRequest
        {
            AttractionId = Guid.NewGuid(),
            ScheduledDate = DateTime.Now.AddDays(7),
            Description = "Monthly safety inspection",
            EstimatedDuration = 120
        };

        _mockAttractionRepository.Setup(r => r.GetById(It.IsAny<Guid>())).ReturnsAsync((Attraction)null);

        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(
            () => _maintenanceLogic.CreateSchedule(request)
        );
    }

    [TestMethod]
    public async Task CreateSchedule_ScheduledDateAtCurrentTime_CreatesSuccessfully()
    {
        Guid attractionId = Guid.NewGuid();
        Attraction attraction = CreateTestAttraction(attractionId);
        DateTime currentDateTime = new DateTime(2025, 11, 7, 12, 0, 0);

        _mockDateTimeLogic.Setup(d => d.GetCurrentDateTime()).ReturnsAsync(currentDateTime);

        MaintenanceScheduleRequest request = new MaintenanceScheduleRequest
        {
            AttractionId = attractionId,
            ScheduledDate = currentDateTime,
            Description = "Immediate maintenance",
            EstimatedDuration = 120
        };

        _mockAttractionRepository.Setup(r => r.GetById(attractionId)).ReturnsAsync(attraction);
        _mockScheduleRepository.Setup(r => r.CreateAsync(It.IsAny<MaintenanceSchedule>())).Returns(Task.CompletedTask);

        Guid result = await _maintenanceLogic.CreateSchedule(request);

        Assert.AreNotEqual(Guid.Empty, result);
        _mockScheduleRepository.Verify(r => r.CreateAsync(It.Is<MaintenanceSchedule>(
            s => s.ScheduledDate == currentDateTime && s.Status == MaintenanceStatus.Pending
        )), Times.Once);
    }

    [TestMethod]
    public async Task CreateSchedule_ScheduledDateInFuture_CreatesSuccessfully()
    {
        Guid attractionId = Guid.NewGuid();
        Attraction attraction = CreateTestAttraction(attractionId);
        DateTime currentDateTime = new DateTime(2025, 11, 7, 12, 0, 0);
        DateTime futureDateTime = currentDateTime.AddDays(5);

        _mockDateTimeLogic.Setup(d => d.GetCurrentDateTime()).ReturnsAsync(currentDateTime);

        MaintenanceScheduleRequest request = new MaintenanceScheduleRequest
        {
            AttractionId = attractionId,
            ScheduledDate = futureDateTime,
            Description = "Future maintenance",
            EstimatedDuration = 120
        };

        _mockAttractionRepository.Setup(r => r.GetById(attractionId)).ReturnsAsync(attraction);
        _mockScheduleRepository.Setup(r => r.CreateAsync(It.IsAny<MaintenanceSchedule>())).Returns(Task.CompletedTask);

        Guid result = await _maintenanceLogic.CreateSchedule(request);

        Assert.AreNotEqual(Guid.Empty, result);
        _mockScheduleRepository.Verify(r => r.CreateAsync(It.Is<MaintenanceSchedule>(
            s => s.ScheduledDate == futureDateTime && s.Status == MaintenanceStatus.Pending
        )), Times.Once);
    }

    [TestMethod]
    public async Task GetScheduleById_ExistingSchedule_ReturnsScheduleResponse()
    {
        Guid scheduleId = Guid.NewGuid();
        Attraction attraction = CreateTestAttraction(Guid.NewGuid());
        MaintenanceSchedule schedule = CreateTestSchedule(scheduleId, attraction.Id, attraction);

        _mockScheduleRepository.Setup(r => r.GetByIdAsync(scheduleId)).ReturnsAsync(schedule);

        MaintenanceScheduleResponse result = await _maintenanceLogic.GetScheduleById(scheduleId);

        Assert.AreEqual(scheduleId, result.Id);
        Assert.AreEqual(schedule.EstimatedDuration, result.EstimatedDuration);
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
        overdueSchedule.IsOverdue = true;

        List<MaintenanceSchedule> schedules = new List<MaintenanceSchedule> { overdueSchedule };
        _mockScheduleRepository.Setup(r => r.GetOverdueSchedulesAsync()).ReturnsAsync(schedules);

        List<MaintenanceScheduleResponse> result = await _maintenanceLogic.GetOverdueSchedules();

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
    public async Task UpdateScheduleStatus_NumericStatusNotDefined_ThrowsArgumentException()
    {
        Guid scheduleId = Guid.NewGuid();
        Attraction attraction = CreateTestAttraction(Guid.NewGuid());
        MaintenanceSchedule schedule = CreateTestSchedule(scheduleId, attraction.Id, attraction);

        _mockScheduleRepository.Setup(r => r.GetByIdAsync(scheduleId)).ReturnsAsync(schedule);

        await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _maintenanceLogic.UpdateScheduleStatus(scheduleId, "99")
        );
    }

    [TestMethod]
    public async Task UpdateScheduleStatus_ToCompleted_SetsIsOverdueToFalse()
    {
        Guid scheduleId = Guid.NewGuid();
        Attraction attraction = CreateTestAttraction(Guid.NewGuid());
        MaintenanceSchedule schedule = CreateTestSchedule(scheduleId, attraction.Id, attraction);
        schedule.Status = MaintenanceStatus.InProgress;
        schedule.IsOverdue = true;

        _mockScheduleRepository.Setup(r => r.GetByIdAsync(scheduleId)).ReturnsAsync(schedule);
        _mockScheduleRepository.Setup(r => r.UpdateAsync(It.IsAny<MaintenanceSchedule>())).Returns(Task.CompletedTask);

        await _maintenanceLogic.UpdateScheduleStatus(scheduleId, "Completed");

        _mockScheduleRepository.Verify(r => r.UpdateAsync(It.Is<MaintenanceSchedule>(
            s => s.Id == scheduleId && s.Status == MaintenanceStatus.Completed && s.IsOverdue == false
        )), Times.Once);
    }

    [TestMethod]
    public async Task UpdateScheduleStatus_ToCompletedWhenNotOverdue_KeepsIsOverdueFalse()
    {
        Guid scheduleId = Guid.NewGuid();
        Attraction attraction = CreateTestAttraction(Guid.NewGuid());
        MaintenanceSchedule schedule = CreateTestSchedule(scheduleId, attraction.Id, attraction);
        schedule.Status = MaintenanceStatus.InProgress;
        schedule.IsOverdue = false;

        _mockScheduleRepository.Setup(r => r.GetByIdAsync(scheduleId)).ReturnsAsync(schedule);
        _mockScheduleRepository.Setup(r => r.UpdateAsync(It.IsAny<MaintenanceSchedule>())).Returns(Task.CompletedTask);

        await _maintenanceLogic.UpdateScheduleStatus(scheduleId, "Completed");

        _mockScheduleRepository.Verify(r => r.UpdateAsync(It.Is<MaintenanceSchedule>(
            s => s.Id == scheduleId && s.Status == MaintenanceStatus.Completed && s.IsOverdue == false
        )), Times.Once);
    }

    [TestMethod]
    public async Task UpdateScheduleStatus_ToInProgress_DoesNotModifyIsOverdue()
    {
        Guid scheduleId = Guid.NewGuid();
        Attraction attraction = CreateTestAttraction(Guid.NewGuid());
        MaintenanceSchedule schedule = CreateTestSchedule(scheduleId, attraction.Id, attraction);
        schedule.Status = MaintenanceStatus.Pending;
        schedule.IsOverdue = true;

        _mockScheduleRepository.Setup(r => r.GetByIdAsync(scheduleId)).ReturnsAsync(schedule);
        _mockScheduleRepository.Setup(r => r.UpdateAsync(It.IsAny<MaintenanceSchedule>())).Returns(Task.CompletedTask);

        await _maintenanceLogic.UpdateScheduleStatus(scheduleId, "InProgress");

        _mockScheduleRepository.Verify(r => r.UpdateAsync(It.Is<MaintenanceSchedule>(
            s => s.Id == scheduleId && s.Status == MaintenanceStatus.InProgress && s.IsOverdue == true
        )), Times.Once);
    }

    [TestMethod]
    public async Task UpdateScheduleStatus_NonExistentSchedule_ThrowsKeyNotFoundException()
    {
        Guid scheduleId = Guid.NewGuid();

        _mockScheduleRepository.Setup(r => r.GetByIdAsync(scheduleId)).ReturnsAsync((MaintenanceSchedule)null);

        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(
            () => _maintenanceLogic.UpdateScheduleStatus(scheduleId, "Completed")
        );

        _mockScheduleRepository.Verify(r => r.UpdateAsync(It.IsAny<MaintenanceSchedule>()), Times.Never);
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

    #region Business Operations

    [TestMethod]
    public async Task CompleteMaintenance_ValidScheduleAndRequest_CompletesSchedule()
    {
        Guid scheduleId = Guid.NewGuid();
        Guid attractionId = Guid.NewGuid();
        Guid performedBy = Guid.NewGuid();
        Attraction attraction = CreateTestAttraction(attractionId);
        MaintenanceSchedule schedule = CreateTestSchedule(scheduleId, attractionId, attraction);
        schedule.Status = MaintenanceStatus.Pending;

        _mockScheduleRepository.Setup(r => r.GetByIdAsync(scheduleId)).ReturnsAsync(schedule);
        _mockAttractionRepository.Setup(r => r.GetById(attractionId)).ReturnsAsync(attraction);
        _mockScheduleRepository.Setup(r => r.UpdateAsync(It.IsAny<MaintenanceSchedule>())).Returns(Task.CompletedTask);

        Guid result = await _maintenanceLogic.CompleteMaintenance(scheduleId, performedBy);

        Assert.AreEqual(scheduleId, result);
        _mockScheduleRepository.Verify(r => r.UpdateAsync(It.Is<MaintenanceSchedule>(
            s => s.Id == scheduleId && s.Status == MaintenanceStatus.Completed && s.IsOverdue == false
        )), Times.Once);
    }

    [TestMethod]
    public async Task CompleteMaintenance_NonExistentSchedule_ThrowsKeyNotFoundException()
    {
        _mockScheduleRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((MaintenanceSchedule)null);

        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(
            () => _maintenanceLogic.CompleteMaintenance(Guid.NewGuid(), Guid.NewGuid())
        );
    }

    [TestMethod]
    public async Task CompleteMaintenance_AlreadyCompletedSchedule_ThrowsArgumentException()
    {
        Guid scheduleId = Guid.NewGuid();
        Attraction attraction = CreateTestAttraction(Guid.NewGuid());
        MaintenanceSchedule schedule = CreateTestSchedule(scheduleId, attraction.Id, attraction);
        schedule.Status = MaintenanceStatus.Completed;

        _mockScheduleRepository.Setup(r => r.GetByIdAsync(scheduleId)).ReturnsAsync(schedule);

        await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _maintenanceLogic.CompleteMaintenance(scheduleId, Guid.NewGuid())
        );
    }

    [TestMethod]
    public async Task CompleteMaintenance_AttractionNotFound_ThrowsKeyNotFoundException()
    {
        Guid scheduleId = Guid.NewGuid();
        Guid attractionId = Guid.NewGuid();
        Guid performedBy = Guid.NewGuid();

        MaintenanceSchedule schedule = new MaintenanceSchedule
        {
            Id = scheduleId,
            AttractionId = attractionId,
            ScheduledDate = DateTime.Now.AddDays(-1),
            Description = "Test maintenance",
            EstimatedDuration = 2,
            Status = MaintenanceStatus.InProgress
        };

        _mockScheduleRepository.Setup(r => r.GetByIdAsync(scheduleId)).ReturnsAsync(schedule);
        _mockAttractionRepository.Setup(r => r.GetById(attractionId)).ReturnsAsync((Attraction)null);

        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(
            () => _maintenanceLogic.CompleteMaintenance(scheduleId, performedBy)
        );

        _mockScheduleRepository.Verify(r => r.UpdateAsync(It.IsAny<MaintenanceSchedule>()), Times.Never);

        _mockAttractionLogic.Verify(x => x.RemoveIncident(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region Observer Pattern Tests

    [TestMethod]
    public async Task DateUpdated_WithPendingSchedulesInPast_UpdatesStatusToInProgress()
    {
        DateTime currentDateTime = new DateTime(2025, 11, 7, 10, 0, 0);
        Guid attractionId = Guid.NewGuid();
        Attraction attraction = CreateTestAttraction(attractionId);

        MaintenanceSchedule pendingSchedulePast = CreateTestSchedule(Guid.NewGuid(), attractionId, attraction);
        pendingSchedulePast.ScheduledDate = currentDateTime.AddDays(-1);
        pendingSchedulePast.Status = MaintenanceStatus.Pending;
        pendingSchedulePast.IsOverdue = false;

        MaintenanceSchedule pendingScheduleFuture = CreateTestSchedule(Guid.NewGuid(), attractionId, attraction);
        pendingScheduleFuture.ScheduledDate = currentDateTime.AddDays(1);
        pendingScheduleFuture.Status = MaintenanceStatus.Pending;
        pendingScheduleFuture.IsOverdue = false;

        MaintenanceSchedule alreadyInProgress = CreateTestSchedule(Guid.NewGuid(), attractionId, attraction);
        alreadyInProgress.ScheduledDate = currentDateTime.AddDays(-2);
        alreadyInProgress.Status = MaintenanceStatus.InProgress;
        alreadyInProgress.IsOverdue = false;

        List<MaintenanceSchedule> allSchedules = new List<MaintenanceSchedule>
        {
            pendingSchedulePast,
            pendingScheduleFuture,
            alreadyInProgress
        };

        Mock<IDateSubject> mockSubject = new Mock<IDateSubject>();
        mockSubject.Setup(x => x.GetCurrentDateTime()).ReturnsAsync(currentDateTime);

        _mockScheduleRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(allSchedules);
        _mockScheduleRepository.Setup(x => x.UpdateAsync(It.IsAny<MaintenanceSchedule>())).Returns(Task.CompletedTask);
        _mockAttractionLogic.Setup(x => x.AddIncident(It.IsAny<Guid>(), It.IsAny<string>()))
        .Returns(Task.CompletedTask);

        IDateObserver observer = (IDateObserver)_maintenanceLogic;
        await observer.DateUpdated(mockSubject.Object);

        _mockScheduleRepository.Verify(x => x.UpdateAsync(It.Is<MaintenanceSchedule>(
            s => s.Id == pendingSchedulePast.Id && s.Status == MaintenanceStatus.InProgress
        )), Times.Once);

        _mockAttractionLogic.Verify(x => x.AddIncident(
            attractionId,
            It.Is<string>(msg => msg.Contains("Mantenimiento programado"))
        ), Times.Once);

        _mockScheduleRepository.Verify(x => x.UpdateAsync(It.Is<MaintenanceSchedule>(
            s => s.Id == pendingScheduleFuture.Id
        )), Times.Never);

        _mockScheduleRepository.Verify(x => x.UpdateAsync(It.Is<MaintenanceSchedule>(
            s => s.Id == alreadyInProgress.Id
        )), Times.Never);
    }

    [TestMethod]
    public async Task DateUpdated_WithScheduleAtExactDateTime_UpdatesStatusToInProgress()
    {
        DateTime currentDateTime = new DateTime(2025, 11, 7, 10, 0, 0);
        Guid attractionId = Guid.NewGuid();
        Attraction attraction = CreateTestAttraction(attractionId);

        MaintenanceSchedule pendingScheduleNow = CreateTestSchedule(Guid.NewGuid(), attractionId, attraction);
        pendingScheduleNow.ScheduledDate = currentDateTime;
        pendingScheduleNow.Status = MaintenanceStatus.Pending;
        pendingScheduleNow.IsOverdue = false;

        List<MaintenanceSchedule> allSchedules = new List<MaintenanceSchedule> { pendingScheduleNow };

        Mock<IDateSubject> mockSubject = new Mock<IDateSubject>();
        mockSubject.Setup(x => x.GetCurrentDateTime()).ReturnsAsync(currentDateTime);

        _mockScheduleRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(allSchedules);
        _mockScheduleRepository.Setup(x => x.UpdateAsync(It.IsAny<MaintenanceSchedule>())).Returns(Task.CompletedTask);


        IDateObserver observer = (IDateObserver)_maintenanceLogic;
        await observer.DateUpdated(mockSubject.Object);

        _mockScheduleRepository.Verify(x => x.UpdateAsync(It.Is<MaintenanceSchedule>(
            s => s.Id == pendingScheduleNow.Id && s.Status == MaintenanceStatus.InProgress
        )), Times.Once);
    }

    [TestMethod]
    public async Task DateUpdated_WithNoSchedulesToUpdate_DoesNotUpdateAny()
    {
        DateTime currentDateTime = new DateTime(2025, 11, 7, 10, 0, 0);
        Guid attractionId = Guid.NewGuid();
        Attraction attraction = CreateTestAttraction(attractionId);

        MaintenanceSchedule futureSchedule1 = CreateTestSchedule(Guid.NewGuid(), attractionId, attraction);
        futureSchedule1.ScheduledDate = currentDateTime.AddDays(1);
        futureSchedule1.Status = MaintenanceStatus.Pending;
        futureSchedule1.IsOverdue = false;

        MaintenanceSchedule futureSchedule2 = CreateTestSchedule(Guid.NewGuid(), attractionId, attraction);
        futureSchedule2.ScheduledDate = currentDateTime.AddDays(7);
        futureSchedule2.Status = MaintenanceStatus.Pending;
        futureSchedule2.IsOverdue = false;

        List<MaintenanceSchedule> allSchedules = new List<MaintenanceSchedule>
        {
            futureSchedule1,
            futureSchedule2
        };

        Mock<IDateSubject> mockSubject = new Mock<IDateSubject>();
        mockSubject.Setup(x => x.GetCurrentDateTime()).ReturnsAsync(currentDateTime);

        _mockScheduleRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(allSchedules);


        IDateObserver observer = (IDateObserver)_maintenanceLogic;
        await observer.DateUpdated(mockSubject.Object);

        _mockScheduleRepository.Verify(x => x.UpdateAsync(It.IsAny<MaintenanceSchedule>()), Times.Never);
    }

    [TestMethod]
    public async Task DateUpdated_WithMultiplePendingSchedules_UpdatesAllQualified()
    {
        DateTime currentDateTime = new DateTime(2025, 11, 7, 10, 0, 0);
        Guid attractionId = Guid.NewGuid();
        Attraction attraction = CreateTestAttraction(attractionId);

        MaintenanceSchedule pending1 = CreateTestSchedule(Guid.NewGuid(), attractionId, attraction);
        pending1.ScheduledDate = currentDateTime.AddDays(-5);
        pending1.Status = MaintenanceStatus.Pending;
        pending1.IsOverdue = false;

        MaintenanceSchedule pending2 = CreateTestSchedule(Guid.NewGuid(), attractionId, attraction);
        pending2.ScheduledDate = currentDateTime.AddDays(-3);
        pending2.Status = MaintenanceStatus.Pending;
        pending2.IsOverdue = false;

        MaintenanceSchedule pending3 = CreateTestSchedule(Guid.NewGuid(), attractionId, attraction);
        pending3.ScheduledDate = currentDateTime.AddDays(-1);
        pending3.Status = MaintenanceStatus.Pending;
        pending3.IsOverdue = false;

        List<MaintenanceSchedule> allSchedules = new List<MaintenanceSchedule>
        {
            pending1,
            pending2,
            pending3
        };

        Mock<IDateSubject> mockSubject = new Mock<IDateSubject>();
        mockSubject.Setup(x => x.GetCurrentDateTime()).ReturnsAsync(currentDateTime);

        _mockScheduleRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(allSchedules);
        _mockScheduleRepository.Setup(x => x.UpdateAsync(It.IsAny<MaintenanceSchedule>())).Returns(Task.CompletedTask);

        IDateObserver observer = (IDateObserver)_maintenanceLogic;
        await observer.DateUpdated(mockSubject.Object);

        _mockScheduleRepository.Verify(x => x.UpdateAsync(It.Is<MaintenanceSchedule>(
            s => s.Status == MaintenanceStatus.InProgress
        )), Times.Exactly(3));
    }

    [TestMethod]
    public async Task DateUpdated_CreatesIncidentWhenMaintenanceStarts()
    {
        DateTime currentDateTime = new DateTime(2025, 11, 7, 10, 0, 0);
        Guid attractionId = Guid.NewGuid();
        Attraction attraction = CreateTestAttraction(attractionId);

        MaintenanceSchedule pendingSchedule = CreateTestSchedule(Guid.NewGuid(), attractionId, attraction);
        pendingSchedule.ScheduledDate = currentDateTime.AddDays(-1);
        pendingSchedule.Status = MaintenanceStatus.Pending;
        pendingSchedule.IsOverdue = false;
        pendingSchedule.Description = "Safety inspection";

        List<MaintenanceSchedule> allSchedules = new List<MaintenanceSchedule> { pendingSchedule };

        Mock<IDateSubject> mockSubject = new Mock<IDateSubject>();
        mockSubject.Setup(x => x.GetCurrentDateTime()).ReturnsAsync(currentDateTime);

        _mockScheduleRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(allSchedules);
        _mockScheduleRepository.Setup(x => x.UpdateAsync(It.IsAny<MaintenanceSchedule>())).Returns(Task.CompletedTask);
        _mockAttractionLogic.Setup(x => x.AddIncident(It.IsAny<Guid>(), It.IsAny<string>()))
        .Returns(Task.CompletedTask);

        IDateObserver observer = (IDateObserver)_maintenanceLogic;
        await observer.DateUpdated(mockSubject.Object);

        _mockAttractionLogic.Verify(x => x.AddIncident(
            attractionId,
            "Mantenimiento programado: Safety inspection"
        ), Times.Once);
    }

    [TestMethod]
    public async Task CompleteMaintenance_RemovesIncidentWhenCompleted()
    {
        Guid scheduleId = Guid.NewGuid();
        Guid attractionId = Guid.NewGuid();
        Guid performedBy = Guid.NewGuid();

        Attraction attraction = CreateTestAttraction(attractionId);
        MaintenanceSchedule schedule = CreateTestSchedule(scheduleId, attractionId, attraction);
        schedule.Status = MaintenanceStatus.InProgress;
        schedule.Description = "Safety inspection";

        _mockScheduleRepository.Setup(r => r.GetByIdAsync(scheduleId)).ReturnsAsync(schedule);
        _mockAttractionRepository.Setup(r => r.GetById(attractionId)).ReturnsAsync(attraction);
        _mockScheduleRepository.Setup(r => r.UpdateAsync(It.IsAny<MaintenanceSchedule>())).Returns(Task.CompletedTask);
        _mockAttractionLogic.Setup(x => x.RemoveIncident(It.IsAny<Guid>(), It.IsAny<string>()))
        .Returns(Task.CompletedTask);

        await _maintenanceLogic.CompleteMaintenance(scheduleId, performedBy);

        _mockAttractionLogic.Verify(x => x.RemoveIncident(
            attractionId,
            "Mantenimiento programado: Safety inspection"
        ), Times.Once);

        _mockScheduleRepository.Verify(r => r.UpdateAsync(It.Is<MaintenanceSchedule>(
            s => s.Id == scheduleId && s.Status == MaintenanceStatus.Completed && s.IsOverdue == false
        )), Times.Once);
    }

    [TestMethod]
    public async Task DateUpdated_UpdatesIsOverdueForInProgressSchedules()
    {
        DateTime currentDateTime = new DateTime(2025, 11, 7, 10, 0, 0);
        Guid attractionId = Guid.NewGuid();
        Attraction attraction = CreateTestAttraction(attractionId);

        MaintenanceSchedule overdueSchedule = CreateTestSchedule(Guid.NewGuid(), attractionId, attraction);
        overdueSchedule.ScheduledDate = currentDateTime.AddHours(-3);
        overdueSchedule.EstimatedDuration = 2;
        overdueSchedule.Status = MaintenanceStatus.InProgress;
        overdueSchedule.IsOverdue = false;

        MaintenanceSchedule notOverdueSchedule = CreateTestSchedule(Guid.NewGuid(), attractionId, attraction);
        notOverdueSchedule.ScheduledDate = currentDateTime.AddHours(-1);
        notOverdueSchedule.EstimatedDuration = 2;
        notOverdueSchedule.Status = MaintenanceStatus.InProgress;
        notOverdueSchedule.IsOverdue = false;

        List<MaintenanceSchedule> allSchedules = new List<MaintenanceSchedule>
        {
            overdueSchedule,
            notOverdueSchedule
        };

        Mock<IDateSubject> mockSubject = new Mock<IDateSubject>();
        mockSubject.Setup(x => x.GetCurrentDateTime()).ReturnsAsync(currentDateTime);

        _mockScheduleRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(allSchedules);
        _mockScheduleRepository.Setup(x => x.UpdateAsync(It.IsAny<MaintenanceSchedule>())).Returns(Task.CompletedTask);

        IDateObserver observer = (IDateObserver)_maintenanceLogic;
        await observer.DateUpdated(mockSubject.Object);

        _mockScheduleRepository.Verify(x => x.UpdateAsync(It.Is<MaintenanceSchedule>(
            s => s.Id == overdueSchedule.Id && s.IsOverdue == true
        )), Times.Once);

        _mockScheduleRepository.Verify(x => x.UpdateAsync(It.Is<MaintenanceSchedule>(
            s => s.Id == notOverdueSchedule.Id
        )), Times.Never);
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
            EstimatedDuration = 120,
            ScheduledDate = DateTime.Now.AddDays(7),
            Description = "Test maintenance schedule",
            Status = MaintenanceStatus.Pending
        };
    }
    #endregion
}