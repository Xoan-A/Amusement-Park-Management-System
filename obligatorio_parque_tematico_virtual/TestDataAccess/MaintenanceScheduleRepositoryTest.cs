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
            _mockDateTimeRepository.Setup(x => x.GetConfiguredDateTime()).ReturnsAsync(DateTime.Now);

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
                MaintenanceType = MaintenanceType.Inspection,
                Description = "Monthly inspection",
                Status = MaintenanceStatus.Pending
            };

            _repository.Create(schedule);

            MaintenanceSchedule? result = _context.MaintenanceSchedules.Find(schedule.Id);
            Assert.IsNotNull(result);
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

            Assert.IsNotNull(result);
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
        public void GetByStatus_FiltersByStatus_ReturnsMatchingSchedules()
        {
            Attraction attraction = CreateTestAttraction();
            _context.Attractions.Add(attraction);

            MaintenanceSchedule pendingSchedule = CreateTestSchedule(attraction.Id);
            pendingSchedule.Status = MaintenanceStatus.Pending;

            MaintenanceSchedule completedSchedule = CreateTestSchedule(attraction.Id);
            completedSchedule.Status = MaintenanceStatus.Completed;

            _context.MaintenanceSchedules.AddRange(pendingSchedule, completedSchedule);
            _context.SaveChanges();

            List<MaintenanceSchedule> results = _repository.GetByStatus(MaintenanceStatus.Pending);

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(MaintenanceStatus.Pending, results[0].Status);
        }

        [TestMethod]
        public void GetOverdueSchedules_ReturnsPendingPastDueSchedules()
        {
            Attraction attraction = CreateTestAttraction();
            _context.Attractions.Add(attraction);

            MaintenanceSchedule overdueSchedule = CreateTestSchedule(attraction.Id);
            overdueSchedule.ScheduledDate = DateTime.Now.AddDays(-1);
            overdueSchedule.Status = MaintenanceStatus.Pending;

            MaintenanceSchedule futureSchedule = CreateTestSchedule(attraction.Id);
            futureSchedule.ScheduledDate = DateTime.Now.AddDays(7);
            futureSchedule.Status = MaintenanceStatus.Pending;

            MaintenanceSchedule completedOverdue = CreateTestSchedule(attraction.Id);
            completedOverdue.ScheduledDate = DateTime.Now.AddDays(-1);
            completedOverdue.Status = MaintenanceStatus.Completed;

            _context.MaintenanceSchedules.AddRange(overdueSchedule, futureSchedule, completedOverdue);
            _context.SaveChanges();

            List<MaintenanceSchedule> results = _repository.GetOverdueSchedules();

            Assert.AreEqual(1, results.Count);
            Assert.IsTrue(results[0].ScheduledDate < DateTime.Now);
            Assert.AreEqual(MaintenanceStatus.Pending, results[0].Status);
        }

        [TestMethod]
        public void GetByDateRange_FiltersCorrectly_ReturnsSchedulesInRange()
        {
            Attraction attraction = CreateTestAttraction();
            _context.Attractions.Add(attraction);

            DateTime dateFrom = DateTime.Now;
            DateTime dateTo = DateTime.Now.AddDays(14);

            MaintenanceSchedule scheduleInRange = CreateTestSchedule(attraction.Id);
            scheduleInRange.ScheduledDate = DateTime.Now.AddDays(7);

            MaintenanceSchedule scheduleBeforeRange = CreateTestSchedule(attraction.Id);
            scheduleBeforeRange.ScheduledDate = DateTime.Now.AddDays(-1);

            MaintenanceSchedule scheduleAfterRange = CreateTestSchedule(attraction.Id);
            scheduleAfterRange.ScheduledDate = DateTime.Now.AddDays(15);

            _context.MaintenanceSchedules.AddRange(scheduleInRange, scheduleBeforeRange, scheduleAfterRange);
            _context.SaveChanges();

            List<MaintenanceSchedule> results = _repository.GetByDateRange(dateFrom, dateTo);

            Assert.AreEqual(1, results.Count);
            Assert.IsTrue(results[0].ScheduledDate >= dateFrom);
            Assert.IsTrue(results[0].ScheduledDate <= dateTo);
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
            Assert.IsNotNull(result);
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
        public void GetByAttractionIdAndDateRange_CombinesFilters_Success()
        {
            Attraction attraction1 = CreateTestAttraction();
            Attraction attraction2 = CreateTestAttraction();
            _context.Attractions.AddRange(attraction1, attraction2);

            DateTime dateFrom = DateTime.Now;
            DateTime dateTo = DateTime.Now.AddDays(14);

            MaintenanceSchedule targetSchedule = CreateTestSchedule(attraction1.Id);
            targetSchedule.ScheduledDate = DateTime.Now.AddDays(7);

            MaintenanceSchedule wrongAttraction = CreateTestSchedule(attraction2.Id);
            wrongAttraction.ScheduledDate = DateTime.Now.AddDays(7);

            MaintenanceSchedule wrongDate = CreateTestSchedule(attraction1.Id);
            wrongDate.ScheduledDate = DateTime.Now.AddDays(20);

            _context.MaintenanceSchedules.AddRange(targetSchedule, wrongAttraction, wrongDate);
            _context.SaveChanges();

            List<MaintenanceSchedule> results =
                _repository.GetByAttractionIdAndDateRange(attraction1.Id, dateFrom, dateTo);

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(targetSchedule.Id, results[0].Id);
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
                MaintenanceType = MaintenanceType.Inspection,
                Description = "Test maintenance schedule",
                Status = MaintenanceStatus.Pending
            };
        }
    }
}