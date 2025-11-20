using AutoMapper;
using BusinessLogic;
using BusinessLogic.Mapping;
using Moq;
using Domain;
using IBusinessLogic;
using IDataAccess;
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
    private IMapper _mapper;
    private IMaintenanceLogic _maintenanceLogic;

    [TestInitialize]
    public void Setup()
    {
        _mockScheduleRepository = new Mock<IMaintenanceScheduleRepository>();
        _mockAttractionRepository = new Mock<IAttractionRepository>();
        _mockAttractionLogic = new Mock<IAttractionLogic>();
        _mockDateTimeLogic = new Mock<IDateTimeLogic>();

        _mockDateTimeLogic.Setup(d => d.GetCurrentDateTime()).Returns(new DateTime(2025, 11, 7, 12, 0, 0));

        MapperConfiguration configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = configuration.CreateMapper();

        _maintenanceLogic = new MaintenanceLogic(
            _mockScheduleRepository.Object,
            _mockAttractionRepository.Object,
            _mockAttractionLogic.Object,
            _mapper
        );
    }

    #region Schedule Tests

    [TestMethod]
    public void CreateSchedule_ValidRequest_ReturnsScheduleId()
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

        _mockAttractionRepository.Setup(r => r.GetById(attractionId)).Returns(attraction);
        _mockScheduleRepository.Setup(r => r.Create(It.IsAny<MaintenanceSchedule>()));

        Guid result = _maintenanceLogic.CreateSchedule(request);

        Assert.AreNotEqual(Guid.Empty, result);
        _mockScheduleRepository.Verify(r => r.Create(It.Is<MaintenanceSchedule>(
            s => s.EstimatedDuration == 120
        )), Times.Once);
    }

    [TestMethod]
    public void CreateSchedule_NonExistentAttraction_ThrowsKeyNotFoundException()
    {
        MaintenanceScheduleRequest request = new MaintenanceScheduleRequest
        {
            AttractionId = Guid.NewGuid(),
            ScheduledDate = DateTime.Now.AddDays(7),
            Description = "Monthly safety inspection",
            EstimatedDuration = 120
        };

        _mockAttractionRepository.Setup(r => r.GetById(It.IsAny<Guid>())).Returns((Attraction)null);

        Assert.ThrowsException<KeyNotFoundException>(
            () => _maintenanceLogic.CreateSchedule(request)
        );
    }

    [TestMethod]
    public void CreateSchedule_ScheduledDateAtCurrentTime_CreatesSuccessfully()
    {
        Guid attractionId = Guid.NewGuid();
        Attraction attraction = CreateTestAttraction(attractionId);
        DateTime currentDateTime = new DateTime(2025, 11, 7, 12, 0, 0);

        _mockDateTimeLogic.Setup(d => d.GetCurrentDateTime()).Returns(currentDateTime);

        MaintenanceScheduleRequest request = new MaintenanceScheduleRequest
        {
            AttractionId = attractionId,
            ScheduledDate = currentDateTime,
            Description = "Immediate maintenance",
            EstimatedDuration = 120
        };

        _mockAttractionRepository.Setup(r => r.GetById(attractionId)).Returns(attraction);
        _mockScheduleRepository.Setup(r => r.Create(It.IsAny<MaintenanceSchedule>()));

        Guid result = _maintenanceLogic.CreateSchedule(request);

        Assert.AreNotEqual(Guid.Empty, result);
        _mockScheduleRepository.Verify(r => r.Create(It.Is<MaintenanceSchedule>(
            s => s.ScheduledDate == currentDateTime && s.Status == MaintenanceStatus.Pending
        )), Times.Once);
    }

    [TestMethod]
    public void CreateSchedule_ScheduledDateInFuture_CreatesSuccessfully()
    {
        Guid attractionId = Guid.NewGuid();
        Attraction attraction = CreateTestAttraction(attractionId);
        DateTime currentDateTime = new DateTime(2025, 11, 7, 12, 0, 0);
        DateTime futureDateTime = currentDateTime.AddDays(5);

        _mockDateTimeLogic.Setup(d => d.GetCurrentDateTime()).Returns(currentDateTime);

        MaintenanceScheduleRequest request = new MaintenanceScheduleRequest
        {
            AttractionId = attractionId,
            ScheduledDate = futureDateTime,
            Description = "Future maintenance",
            EstimatedDuration = 120
        };

        _mockAttractionRepository.Setup(r => r.GetById(attractionId)).Returns(attraction);
        _mockScheduleRepository.Setup(r => r.Create(It.IsAny<MaintenanceSchedule>()));

        Guid result = _maintenanceLogic.CreateSchedule(request);

        Assert.AreNotEqual(Guid.Empty, result);
        _mockScheduleRepository.Verify(r => r.Create(It.Is<MaintenanceSchedule>(
            s => s.ScheduledDate == futureDateTime && s.Status == MaintenanceStatus.Pending
        )), Times.Once);
    }

    [TestMethod]
    public void GetScheduleById_ExistingSchedule_ReturnsScheduleResponse()
    {
        Guid scheduleId = Guid.NewGuid();
        Attraction attraction = CreateTestAttraction(Guid.NewGuid());
        MaintenanceSchedule schedule = CreateTestSchedule(scheduleId, attraction.Id, attraction);

        _mockScheduleRepository.Setup(r => r.GetById(scheduleId)).Returns(schedule);

        MaintenanceScheduleResponse result = _maintenanceLogic.GetScheduleById(scheduleId);

        Assert.AreEqual(scheduleId, result.Id);
        Assert.AreEqual(schedule.EstimatedDuration, result.EstimatedDuration);
    }

    [TestMethod]
    public void GetScheduleById_NonExistentSchedule_ThrowsKeyNotFoundException()
    {
        _mockScheduleRepository.Setup(r => r.GetById(It.IsAny<Guid>())).Returns((MaintenanceSchedule)null);

        Assert.ThrowsException<KeyNotFoundException>(
            () => _maintenanceLogic.GetScheduleById(Guid.NewGuid())
        );
    }

    [TestMethod]
    public void GetAllSchedules_ReturnsListOfSchedules()
    {
        Attraction attraction = CreateTestAttraction(Guid.NewGuid());
        List<MaintenanceSchedule> schedules = new List<MaintenanceSchedule>
        {
            CreateTestSchedule(Guid.NewGuid(), attraction.Id, attraction),
            CreateTestSchedule(Guid.NewGuid(), attraction.Id, attraction)
        };

        _mockScheduleRepository.Setup(r => r.GetAll()).Returns(schedules);

        List<MaintenanceScheduleResponse> result = _maintenanceLogic.GetAllSchedules();

        Assert.AreEqual(2, result.Count);
        _mockScheduleRepository.Verify(r => r.GetAll(), Times.Once);
    }

    [TestMethod]
    public void GetSchedulesByAttraction_ReturnsFilteredSchedules()
    {
        Guid attractionId = Guid.NewGuid();
        Attraction attraction = CreateTestAttraction(attractionId);
        List<MaintenanceSchedule> schedules = new List<MaintenanceSchedule>
        {
            CreateTestSchedule(Guid.NewGuid(), attractionId, attraction),
            CreateTestSchedule(Guid.NewGuid(), attractionId, attraction)
        };

        _mockScheduleRepository.Setup(r => r.GetByAttractionId(attractionId)).Returns(schedules);

        List<MaintenanceScheduleResponse> result = _maintenanceLogic.GetSchedulesByAttraction(attractionId);

        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.All(s => s.AttractionId == attractionId));
    }

    [TestMethod]
    public void GetOverdueSchedules_ReturnsOverdueSchedules()
    {
        Attraction attraction = CreateTestAttraction(Guid.NewGuid());
        MaintenanceSchedule overdueSchedule = CreateTestSchedule(Guid.NewGuid(), attraction.Id, attraction);
        overdueSchedule.ScheduledDate = DateTime.Now.AddDays(-1);
        overdueSchedule.Status = MaintenanceStatus.Pending;
        overdueSchedule.IsOverdue = true;

        List<MaintenanceSchedule> schedules = new List<MaintenanceSchedule> { overdueSchedule };
        _mockScheduleRepository.Setup(r => r.GetOverdueSchedules()).Returns(schedules);

        List<MaintenanceScheduleResponse> result = _maintenanceLogic.GetOverdueSchedules();

        Assert.AreEqual(1, result.Count);
        Assert.IsTrue(result[0].IsOverdue);
    }

    [TestMethod]
    public void GetUpcomingSchedules_ReturnsUpcomingSchedules()
    {
        Attraction attraction = CreateTestAttraction(Guid.NewGuid());
        MaintenanceSchedule upcomingSchedule = CreateTestSchedule(Guid.NewGuid(), attraction.Id, attraction);
        upcomingSchedule.ScheduledDate = DateTime.Now.AddDays(3);

        List<MaintenanceSchedule> schedules = new List<MaintenanceSchedule> { upcomingSchedule };
        _mockScheduleRepository.Setup(r => r.GetUpcomingSchedules(7)).Returns(schedules);

        List<MaintenanceScheduleResponse> result = _maintenanceLogic.GetUpcomingSchedules(7);

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public void UpdateScheduleStatus_ValidScheduleAndStatus_UpdatesSuccessfully()
    {
        Guid scheduleId = Guid.NewGuid();
        Attraction attraction = CreateTestAttraction(Guid.NewGuid());
        MaintenanceSchedule schedule = CreateTestSchedule(scheduleId, attraction.Id, attraction);

        _mockScheduleRepository.Setup(r => r.GetById(scheduleId)).Returns(schedule);
        _mockScheduleRepository.Setup(r => r.Update(It.IsAny<MaintenanceSchedule>()));

        _maintenanceLogic.UpdateScheduleStatus(scheduleId, "Completed");

        _mockScheduleRepository.Verify(r => r.Update(It.Is<MaintenanceSchedule>(
            s => s.Id == scheduleId && s.Status == MaintenanceStatus.Completed
        )), Times.Once);
    }

    [TestMethod]
    public void UpdateScheduleStatus_InvalidStatus_ThrowsArgumentException()
    {
        Guid scheduleId = Guid.NewGuid();
        Attraction attraction = CreateTestAttraction(Guid.NewGuid());
        MaintenanceSchedule schedule = CreateTestSchedule(scheduleId, attraction.Id, attraction);

        _mockScheduleRepository.Setup(r => r.GetById(scheduleId)).Returns(schedule);

        Assert.ThrowsException<ArgumentException>(
            () => _maintenanceLogic.UpdateScheduleStatus(scheduleId, "InvalidStatus")
        );
    }

    [TestMethod]
    public void UpdateScheduleStatus_NumericStatusNotDefined_ThrowsArgumentException()
    {
        Guid scheduleId = Guid.NewGuid();
        Attraction attraction = CreateTestAttraction(Guid.NewGuid());
        MaintenanceSchedule schedule = CreateTestSchedule(scheduleId, attraction.Id, attraction);

        _mockScheduleRepository.Setup(r => r.GetById(scheduleId)).Returns(schedule);

        Assert.ThrowsException<ArgumentException>(
            () => _maintenanceLogic.UpdateScheduleStatus(scheduleId, "99")
        );
    }

    [TestMethod]
    public void UpdateScheduleStatus_ToCompleted_SetsIsOverdueToFalse()
    {
        Guid scheduleId = Guid.NewGuid();
        Attraction attraction = CreateTestAttraction(Guid.NewGuid());
        MaintenanceSchedule schedule = CreateTestSchedule(scheduleId, attraction.Id, attraction);
        schedule.Status = MaintenanceStatus.InProgress;
        schedule.IsOverdue = true;

        _mockScheduleRepository.Setup(r => r.GetById(scheduleId)).Returns(schedule);
        _mockScheduleRepository.Setup(r => r.Update(It.IsAny<MaintenanceSchedule>()));

        _maintenanceLogic.UpdateScheduleStatus(scheduleId, "Completed");

        _mockScheduleRepository.Verify(r => r.Update(It.Is<MaintenanceSchedule>(
            s => s.Id == scheduleId && s.Status == MaintenanceStatus.Completed && s.IsOverdue == false
        )), Times.Once);
    }

    [TestMethod]
    public void UpdateScheduleStatus_ToCompletedWhenNotOverdue_KeepsIsOverdueFalse()
    {
        Guid scheduleId = Guid.NewGuid();
        Attraction attraction = CreateTestAttraction(Guid.NewGuid());
        MaintenanceSchedule schedule = CreateTestSchedule(scheduleId, attraction.Id, attraction);
        schedule.Status = MaintenanceStatus.InProgress;
        schedule.IsOverdue = false;

        _mockScheduleRepository.Setup(r => r.GetById(scheduleId)).Returns(schedule);
        _mockScheduleRepository.Setup(r => r.Update(It.IsAny<MaintenanceSchedule>()));

        _maintenanceLogic.UpdateScheduleStatus(scheduleId, "Completed");

        _mockScheduleRepository.Verify(r => r.Update(It.Is<MaintenanceSchedule>(
            s => s.Id == scheduleId && s.Status == MaintenanceStatus.Completed && s.IsOverdue == false
        )), Times.Once);
    }

    [TestMethod]
    public void UpdateScheduleStatus_ToInProgress_DoesNotModifyIsOverdue()
    {
        Guid scheduleId = Guid.NewGuid();
        Attraction attraction = CreateTestAttraction(Guid.NewGuid());
        MaintenanceSchedule schedule = CreateTestSchedule(scheduleId, attraction.Id, attraction);
        schedule.Status = MaintenanceStatus.Pending;
        schedule.IsOverdue = true;

        _mockScheduleRepository.Setup(r => r.GetById(scheduleId)).Returns(schedule);
        _mockScheduleRepository.Setup(r => r.Update(It.IsAny<MaintenanceSchedule>()));

        _maintenanceLogic.UpdateScheduleStatus(scheduleId, "InProgress");

        _mockScheduleRepository.Verify(r => r.Update(It.Is<MaintenanceSchedule>(
            s => s.Id == scheduleId && s.Status == MaintenanceStatus.InProgress && s.IsOverdue == true
        )), Times.Once);
    }

    [TestMethod]
    public void UpdateScheduleStatus_NonExistentSchedule_ThrowsKeyNotFoundException()
    {
        Guid scheduleId = Guid.NewGuid();

        _mockScheduleRepository.Setup(r => r.GetById(scheduleId)).Returns((MaintenanceSchedule)null);

        Assert.ThrowsException<KeyNotFoundException>(
            () => _maintenanceLogic.UpdateScheduleStatus(scheduleId, "Completed")
        );

        _mockScheduleRepository.Verify(r => r.Update(It.IsAny<MaintenanceSchedule>()), Times.Never);
    }

    [TestMethod]
    public void DeleteSchedule_ExistingSchedule_DeletesSuccessfully()
    {
        Guid scheduleId = Guid.NewGuid();
        _mockScheduleRepository.Setup(r => r.Delete(scheduleId));

        _maintenanceLogic.DeleteSchedule(scheduleId);

        _mockScheduleRepository.Verify(r => r.Delete(scheduleId), Times.Once);
    }

    #endregion

    #region Business Operations

    [TestMethod]
    public void CompleteMaintenance_ValidScheduleAndRequest_CompletesSchedule()
    {
        Guid scheduleId = Guid.NewGuid();
        Guid attractionId = Guid.NewGuid();
        Guid performedBy = Guid.NewGuid();
        Attraction attraction = CreateTestAttraction(attractionId);
        MaintenanceSchedule schedule = CreateTestSchedule(scheduleId, attractionId, attraction);
        schedule.Status = MaintenanceStatus.Pending;

        _mockScheduleRepository.Setup(r => r.GetById(scheduleId)).Returns(schedule);
        _mockAttractionRepository.Setup(r => r.GetById(attractionId)).Returns(attraction);
        _mockScheduleRepository.Setup(r => r.Update(It.IsAny<MaintenanceSchedule>()));

        Guid result = _maintenanceLogic.CompleteMaintenance(scheduleId, performedBy);

        Assert.AreEqual(scheduleId, result);
        _mockScheduleRepository.Verify(r => r.Update(It.Is<MaintenanceSchedule>(
            s => s.Id == scheduleId && s.Status == MaintenanceStatus.Completed && s.IsOverdue == false
        )), Times.Once);
    }

    [TestMethod]
    public void CompleteMaintenance_NonExistentSchedule_ThrowsKeyNotFoundException()
    {
        _mockScheduleRepository.Setup(r => r.GetById(It.IsAny<Guid>())).Returns((MaintenanceSchedule)null);

        Assert.ThrowsException<KeyNotFoundException>(
            () => _maintenanceLogic.CompleteMaintenance(Guid.NewGuid(), Guid.NewGuid())
        );
    }

    [TestMethod]
    public void CompleteMaintenance_AlreadyCompletedSchedule_ThrowsArgumentException()
    {
        Guid scheduleId = Guid.NewGuid();
        Attraction attraction = CreateTestAttraction(Guid.NewGuid());
        MaintenanceSchedule schedule = CreateTestSchedule(scheduleId, attraction.Id, attraction);
        schedule.Status = MaintenanceStatus.Completed;

        _mockScheduleRepository.Setup(r => r.GetById(scheduleId)).Returns(schedule);

        Assert.ThrowsException<ArgumentException>(
            () => _maintenanceLogic.CompleteMaintenance(scheduleId, Guid.NewGuid())
        );
    }

    [TestMethod]
    public void CompleteMaintenance_AttractionNotFound_ThrowsKeyNotFoundException()
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

        _mockScheduleRepository.Setup(r => r.GetById(scheduleId)).Returns(schedule);
        _mockAttractionRepository.Setup(r => r.GetById(attractionId)).Returns((Attraction)null);

        Assert.ThrowsException<KeyNotFoundException>(
            () => _maintenanceLogic.CompleteMaintenance(scheduleId, performedBy)
        );

        _mockScheduleRepository.Verify(r => r.Update(It.IsAny<MaintenanceSchedule>()), Times.Never);

        _mockAttractionLogic.Verify(x => x.RemoveIncident(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region Observer Pattern Tests

    [TestMethod]
    public void DateUpdated_WithPendingSchedulesInPast_UpdatesStatusToInProgress()
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
        mockSubject.Setup(x => x.GetCurrentDateTime()).Returns(currentDateTime);

        _mockScheduleRepository.Setup(x => x.GetAll()).Returns(allSchedules);
        _mockScheduleRepository.Setup(x => x.Update(It.IsAny<MaintenanceSchedule>()));
        _mockAttractionLogic.Setup(x => x.AddIncident(It.IsAny<Guid>(), It.IsAny<string>()))
        ;

        IDateObserver observer = (IDateObserver)_maintenanceLogic;
        observer.DateUpdated(mockSubject.Object);

        _mockScheduleRepository.Verify(x => x.Update(It.Is<MaintenanceSchedule>(
            s => s.Id == pendingSchedulePast.Id && s.Status == MaintenanceStatus.InProgress
        )), Times.Once);

        _mockAttractionLogic.Verify(x => x.AddIncident(
            attractionId,
            It.Is<string>(msg => msg.Contains("Mantenimiento programado"))
        ), Times.Once);

        _mockScheduleRepository.Verify(x => x.Update(It.Is<MaintenanceSchedule>(
            s => s.Id == pendingScheduleFuture.Id
        )), Times.Never);

        _mockScheduleRepository.Verify(x => x.Update(It.Is<MaintenanceSchedule>(
            s => s.Id == alreadyInProgress.Id
        )), Times.Never);
    }

    [TestMethod]
    public void DateUpdated_WithScheduleAtExactDateTime_UpdatesStatusToInProgress()
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
        mockSubject.Setup(x => x.GetCurrentDateTime()).Returns(currentDateTime);

        _mockScheduleRepository.Setup(x => x.GetAll()).Returns(allSchedules);
        _mockScheduleRepository.Setup(x => x.Update(It.IsAny<MaintenanceSchedule>()));


        IDateObserver observer = (IDateObserver)_maintenanceLogic;
        observer.DateUpdated(mockSubject.Object);

        _mockScheduleRepository.Verify(x => x.Update(It.Is<MaintenanceSchedule>(
            s => s.Id == pendingScheduleNow.Id && s.Status == MaintenanceStatus.InProgress
        )), Times.Once);
    }

    [TestMethod]
    public void DateUpdated_WithNoSchedulesToUpdate_DoesNotUpdateAny()
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
        mockSubject.Setup(x => x.GetCurrentDateTime()).Returns(currentDateTime);

        _mockScheduleRepository.Setup(x => x.GetAll()).Returns(allSchedules);


        IDateObserver observer = (IDateObserver)_maintenanceLogic;
        observer.DateUpdated(mockSubject.Object);

        _mockScheduleRepository.Verify(x => x.Update(It.IsAny<MaintenanceSchedule>()), Times.Never);
    }

    [TestMethod]
    public void DateUpdated_WithMultiplePendingSchedules_UpdatesAllQualified()
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
        mockSubject.Setup(x => x.GetCurrentDateTime()).Returns(currentDateTime);

        _mockScheduleRepository.Setup(x => x.GetAll()).Returns(allSchedules);
        _mockScheduleRepository.Setup(x => x.Update(It.IsAny<MaintenanceSchedule>()));

        IDateObserver observer = (IDateObserver)_maintenanceLogic;
        observer.DateUpdated(mockSubject.Object);

        _mockScheduleRepository.Verify(x => x.Update(It.Is<MaintenanceSchedule>(
            s => s.Status == MaintenanceStatus.InProgress
        )), Times.Exactly(3));
    }

    [TestMethod]
    public void DateUpdated_CreatesIncidentWhenMaintenanceStarts()
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
        mockSubject.Setup(x => x.GetCurrentDateTime()).Returns(currentDateTime);

        _mockScheduleRepository.Setup(x => x.GetAll()).Returns(allSchedules);
        _mockScheduleRepository.Setup(x => x.Update(It.IsAny<MaintenanceSchedule>()));
        _mockAttractionLogic.Setup(x => x.AddIncident(It.IsAny<Guid>(), It.IsAny<string>()))
        ;

        IDateObserver observer = (IDateObserver)_maintenanceLogic;
        observer.DateUpdated(mockSubject.Object);

        _mockAttractionLogic.Verify(x => x.AddIncident(
            attractionId,
            "Mantenimiento programado: Safety inspection"
        ), Times.Once);
    }

    [TestMethod]
    public void CompleteMaintenance_RemovesIncidentWhenCompleted()
    {
        Guid scheduleId = Guid.NewGuid();
        Guid attractionId = Guid.NewGuid();
        Guid performedBy = Guid.NewGuid();

        Attraction attraction = CreateTestAttraction(attractionId);
        MaintenanceSchedule schedule = CreateTestSchedule(scheduleId, attractionId, attraction);
        schedule.Status = MaintenanceStatus.InProgress;
        schedule.Description = "Safety inspection";

        _mockScheduleRepository.Setup(r => r.GetById(scheduleId)).Returns(schedule);
        _mockAttractionRepository.Setup(r => r.GetById(attractionId)).Returns(attraction);
        _mockScheduleRepository.Setup(r => r.Update(It.IsAny<MaintenanceSchedule>()));
        _mockAttractionLogic.Setup(x => x.RemoveIncident(It.IsAny<Guid>(), It.IsAny<string>()))
        ;

        _maintenanceLogic.CompleteMaintenance(scheduleId, performedBy);

        _mockAttractionLogic.Verify(x => x.RemoveIncident(
            attractionId,
            "Mantenimiento programado: Safety inspection"
        ), Times.Once);

        _mockScheduleRepository.Verify(r => r.Update(It.Is<MaintenanceSchedule>(
            s => s.Id == scheduleId && s.Status == MaintenanceStatus.Completed && s.IsOverdue == false
        )), Times.Once);
    }

    [TestMethod]
    public void DateUpdated_UpdatesIsOverdueForInProgressSchedules()
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
        mockSubject.Setup(x => x.GetCurrentDateTime()).Returns(currentDateTime);

        _mockScheduleRepository.Setup(x => x.GetAll()).Returns(allSchedules);
        _mockScheduleRepository.Setup(x => x.Update(It.IsAny<MaintenanceSchedule>()));

        IDateObserver observer = (IDateObserver)_maintenanceLogic;
        observer.DateUpdated(mockSubject.Object);

        _mockScheduleRepository.Verify(x => x.Update(It.Is<MaintenanceSchedule>(
            s => s.Id == overdueSchedule.Id && s.IsOverdue == true
        )), Times.Once);

        _mockScheduleRepository.Verify(x => x.Update(It.Is<MaintenanceSchedule>(
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