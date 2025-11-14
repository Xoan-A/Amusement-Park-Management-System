using DataAccess.Context;
using DataAccess.Repositories;
using Domain;
using IDataAccess;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace TestDataAccess
{
    [TestClass]
    public class MaintenanceScheduleRepositoryTest
    {
        private AppDbContext _context = null!;
        private MaintenanceScheduleRepository _repository = null!;
        private SqliteConnection _connection = null!;
        private Mock<IDateTimeRepository> _mockDateTimeRepository = null!;

        [TestInitialize]
        public void Setup()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();

            _mockDateTimeRepository = new Mock<IDateTimeRepository>();
            _mockDateTimeRepository.Setup(x => x.GetConfiguredDateTime()).Returns(DateTime.Now);

            _repository = new MaintenanceScheduleRepository(_context, _mockDateTimeRepository.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Dispose();
            _connection.Close();
            _connection.Dispose();
        }

        [TestMethod]
        public void Create_ValidSchedule_Success()
        {
            Attraction attraction = CreateTestAttraction();
            _context.Attractions.Add(attraction);
            _context.SaveChanges();

            MaintenanceSchedule schedule = new MaintenanceSchedule
            {
                Id = Guid.NewGuid(),
                AttractionId = attraction.Id,
                ScheduledDate = DateTime.Now.AddDays(7),
                Description = "Monthly inspection",
                Status = MaintenanceStatus.Pending
            };

            _repository.Create(schedule);

            MaintenanceSchedule? result = _context.MaintenanceSchedules.Find(schedule.Id);
            Assert.AreEqual(schedule.Description, result.Description);
            Assert.AreEqual(MaintenanceStatus.Pending, result.Status);
        }

        [TestMethod]
        public void GetById_ExistingSchedule_ReturnsSchedule()
        {
            Attraction attraction = CreateTestAttraction();
            _context.Attractions.Add(attraction);

            MaintenanceSchedule schedule = CreateTestSchedule(attraction.Id);
            _context.MaintenanceSchedules.Add(schedule);
            _context.SaveChanges();

            MaintenanceSchedule? result = _repository.GetById(schedule.Id);

            Assert.AreEqual(schedule.Id, result.Id);
            Assert.AreEqual(schedule.Description, result.Description);
        }

        [TestMethod]
        public void GetById_NonExistingSchedule_ReturnsNull()
        {
            MaintenanceSchedule? result = _repository.GetById(Guid.NewGuid());

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetAll_MultipleSchedules_ReturnsAllSchedules()
        {
            Attraction attraction = CreateTestAttraction();
            _context.Attractions.Add(attraction);

            MaintenanceSchedule schedule1 = CreateTestSchedule(attraction.Id);
            MaintenanceSchedule schedule2 = CreateTestSchedule(attraction.Id);
            schedule2.Description = "Different description";

            _context.MaintenanceSchedules.AddRange(schedule1, schedule2);
            _context.SaveChanges();

            List<MaintenanceSchedule> results = _repository.GetAll();

            Assert.AreEqual(2, results.Count);
        }

        [TestMethod]
        public void GetByAttractionId_ExistingSchedules_ReturnsSchedulesForAttraction()
        {
            Attraction attraction1 = CreateTestAttraction();
            Attraction attraction2 = CreateTestAttraction();
            _context.Attractions.AddRange(attraction1, attraction2);

            MaintenanceSchedule schedule1 = CreateTestSchedule(attraction1.Id);
            MaintenanceSchedule schedule2 = CreateTestSchedule(attraction1.Id);
            MaintenanceSchedule schedule3 = CreateTestSchedule(attraction2.Id);

            _context.MaintenanceSchedules.AddRange(schedule1, schedule2, schedule3);
            _context.SaveChanges();

            List<MaintenanceSchedule> results = _repository.GetByAttractionId(attraction1.Id);

            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(results.All(s => s.AttractionId == attraction1.Id));
        }

        [TestMethod]
        public void GetOverdueSchedules_ReturnsPendingPastDueSchedules()
        {
            Attraction attraction = CreateTestAttraction();
            _context.Attractions.Add(attraction);

            MaintenanceSchedule overdueSchedule = CreateTestSchedule(attraction.Id);
            overdueSchedule.ScheduledDate = DateTime.Now.AddDays(-1);
            overdueSchedule.Status = MaintenanceStatus.Pending;
            overdueSchedule.IsOverdue = true;

            MaintenanceSchedule futureSchedule = CreateTestSchedule(attraction.Id);
            futureSchedule.ScheduledDate = DateTime.Now.AddDays(7);
            futureSchedule.Status = MaintenanceStatus.Pending;
            futureSchedule.IsOverdue = false;

            MaintenanceSchedule completedOverdue = CreateTestSchedule(attraction.Id);
            completedOverdue.ScheduledDate = DateTime.Now.AddDays(-1);
            completedOverdue.Status = MaintenanceStatus.Completed;
            completedOverdue.IsOverdue = false;

            _context.MaintenanceSchedules.AddRange(overdueSchedule, futureSchedule, completedOverdue);
            _context.SaveChanges();

            List<MaintenanceSchedule> results = _repository.GetOverdueSchedules();

            Assert.AreEqual(1, results.Count);
            Assert.IsTrue(results[0].IsOverdue);
            Assert.AreEqual(overdueSchedule.Id, results[0].Id);
        }

        [TestMethod]
        public void GetOverdueSchedules_ReturnsMultipleOverdueSchedules()
        {
            Attraction attraction = CreateTestAttraction();
            _context.Attractions.Add(attraction);

            MaintenanceSchedule overdueInProgress = CreateTestSchedule(attraction.Id);
            overdueInProgress.ScheduledDate = DateTime.Now.AddDays(-3);
            overdueInProgress.Status = MaintenanceStatus.InProgress;
            overdueInProgress.IsOverdue = true;

            MaintenanceSchedule overduePending = CreateTestSchedule(attraction.Id);
            overduePending.ScheduledDate = DateTime.Now.AddDays(-1);
            overduePending.Status = MaintenanceStatus.Pending;
            overduePending.IsOverdue = true;

            MaintenanceSchedule completedNotOverdue = CreateTestSchedule(attraction.Id);
            completedNotOverdue.ScheduledDate = DateTime.Now.AddDays(-2);
            completedNotOverdue.Status = MaintenanceStatus.Completed;
            completedNotOverdue.IsOverdue = false;

            MaintenanceSchedule notOverdueYet = CreateTestSchedule(attraction.Id);
            notOverdueYet.ScheduledDate = DateTime.Now.AddDays(1);
            notOverdueYet.Status = MaintenanceStatus.Pending;
            notOverdueYet.IsOverdue = false;

            _context.MaintenanceSchedules.AddRange(overdueInProgress, overduePending, completedNotOverdue,
                notOverdueYet);
            _context.SaveChanges();

            List<MaintenanceSchedule> results = _repository.GetOverdueSchedules();

            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(results.All(r => r.IsOverdue));
        }

        [TestMethod]
        public void Update_ExistingSchedule_UpdatesSuccessfully()
        {
            Attraction attraction = CreateTestAttraction();
            _context.Attractions.Add(attraction);

            MaintenanceSchedule schedule = CreateTestSchedule(attraction.Id);
            _context.MaintenanceSchedules.Add(schedule);
            _context.SaveChanges();

            schedule.Status = MaintenanceStatus.Completed;
            schedule.Description = "Updated description";
            _repository.Update(schedule);

            MaintenanceSchedule? result = _context.MaintenanceSchedules.Find(schedule.Id);

            Assert.AreEqual(MaintenanceStatus.Completed, result.Status);
            Assert.AreEqual("Updated description", result.Description);
        }

        [TestMethod]
        public void Delete_ExistingSchedule_RemovesSchedule()
        {
            Attraction attraction = CreateTestAttraction();
            _context.Attractions.Add(attraction);

            MaintenanceSchedule schedule = CreateTestSchedule(attraction.Id);
            _context.MaintenanceSchedules.Add(schedule);
            _context.SaveChanges();

            _repository.Delete(schedule.Id);

            MaintenanceSchedule? result = _context.MaintenanceSchedules.Find(schedule.Id);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetUpcomingSchedules_ReturnsSchedulesWithinDays_Success()
        {
            Attraction attraction = CreateTestAttraction();
            _context.Attractions.Add(attraction);

            MaintenanceSchedule schedule1 = CreateTestSchedule(attraction.Id);
            schedule1.ScheduledDate = DateTime.Now.AddDays(3);
            schedule1.Status = MaintenanceStatus.Pending;

            MaintenanceSchedule schedule2 = CreateTestSchedule(attraction.Id);
            schedule2.ScheduledDate = DateTime.Now.AddDays(10);
            schedule2.Status = MaintenanceStatus.Pending;

            _context.MaintenanceSchedules.AddRange(schedule1, schedule2);
            _context.SaveChanges();

            List<MaintenanceSchedule> results = _repository.GetUpcomingSchedules(7);

            Assert.AreEqual(1, results.Count);
            Assert.IsTrue((results[0].ScheduledDate - DateTime.Now).TotalDays <= 7);
        }

        [TestMethod]
        public void GetUpcomingSchedules_WhenDateTimeRepositoryReturnsNull_UsesDateTimeNow()
        {
            _mockDateTimeRepository.Setup(x => x.GetConfiguredDateTime()).Returns((DateTime?)null);

            Attraction attraction = CreateTestAttraction();
            _context.Attractions.Add(attraction);

            MaintenanceSchedule schedule1 = CreateTestSchedule(attraction.Id);
            schedule1.ScheduledDate = DateTime.Now.AddDays(2);
            schedule1.Status = MaintenanceStatus.Pending;

            MaintenanceSchedule schedule2 = CreateTestSchedule(attraction.Id);
            schedule2.ScheduledDate = DateTime.Now.AddDays(8);
            schedule2.Status = MaintenanceStatus.Pending;

            MaintenanceSchedule schedule3 = CreateTestSchedule(attraction.Id);
            schedule3.ScheduledDate = DateTime.Now.AddDays(-1);
            schedule3.Status = MaintenanceStatus.Pending;

            _context.MaintenanceSchedules.AddRange(schedule1, schedule2, schedule3);
            _context.SaveChanges();

            List<MaintenanceSchedule> results = _repository.GetUpcomingSchedules(5);

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(schedule1.Id, results[0].Id);
            Assert.IsTrue(results[0].ScheduledDate >= DateTime.Now);
            Assert.IsTrue((results[0].ScheduledDate - DateTime.Now).TotalDays <= 5);
        }

        [TestMethod]
        public void GetUpcomingSchedules_OnlyReturnsPendingSchedules_ExcludesOtherStatuses()
        {
            Attraction attraction = CreateTestAttraction();
            _context.Attractions.Add(attraction);

            MaintenanceSchedule pendingSchedule = CreateTestSchedule(attraction.Id);
            pendingSchedule.ScheduledDate = DateTime.Now.AddDays(3);
            pendingSchedule.Status = MaintenanceStatus.Pending;

            MaintenanceSchedule inProgressSchedule = CreateTestSchedule(attraction.Id);
            inProgressSchedule.ScheduledDate = DateTime.Now.AddDays(4);
            inProgressSchedule.Status = MaintenanceStatus.InProgress;

            MaintenanceSchedule completedSchedule = CreateTestSchedule(attraction.Id);
            completedSchedule.ScheduledDate = DateTime.Now.AddDays(5);
            completedSchedule.Status = MaintenanceStatus.Completed;

            _context.MaintenanceSchedules.AddRange(pendingSchedule, inProgressSchedule, completedSchedule);
            _context.SaveChanges();

            List<MaintenanceSchedule> results = _repository.GetUpcomingSchedules(7);

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(MaintenanceStatus.Pending, results[0].Status);
            Assert.AreEqual(pendingSchedule.Id, results[0].Id);
        }

        private Attraction CreateTestAttraction()
        {
            return new Attraction
            {
                Id = Guid.NewGuid(),
                Name = $"Test Attraction {Guid.NewGuid()}",
                Description = "Test Description",
                Type = AttractionType.RollerCoaster,
                MinAge = 10,
                MaxCapacity = 100,
                CurrentCapacity = 0
            };
        }

        private MaintenanceSchedule CreateTestSchedule(Guid attractionId)
        {
            return new MaintenanceSchedule
            {
                Id = Guid.NewGuid(),
                AttractionId = attractionId,
                ScheduledDate = DateTime.Now.AddDays(7),
                Description = "Test maintenance schedule",
                Status = MaintenanceStatus.Pending
            };
        }
    }
}