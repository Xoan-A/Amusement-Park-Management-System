using DataAccess.Context;
using DataAccess.Repositories;
using Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace TestDataAccess
{
    [TestClass]
    public class MaintenanceScheduleRepositoryTest
    {
        private AppDbContext _context = null!;
        private MaintenanceScheduleRepository _repository = null!;
        private SqliteConnection _connection = null!;

        [TestInitialize]
        public void Setup()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();

            _repository = new MaintenanceScheduleRepository(_context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }

        [TestMethod]
        public void Create_ValidSchedule_Success()
        {
            // Arrange
            var attraction = CreateTestAttraction();
            _context.Attractions.Add(attraction);
            _context.SaveChanges();

            var schedule = new MaintenanceSchedule
            {
                Id = Guid.NewGuid(),
                AttractionId = attraction.Id,
                ScheduledDate = DateTime.Now.AddDays(7),
                MaintenanceType = MaintenanceType.Inspection,
                Description = "Monthly inspection",
                Status = MaintenanceStatus.Pending
            };

            // Act
            _repository.Create(schedule);

            // Assert
            var result = _context.MaintenanceSchedules.Find(schedule.Id);
            Assert.IsNotNull(result);
            Assert.AreEqual(schedule.Description, result.Description);
            Assert.AreEqual(MaintenanceStatus.Pending, result.Status);
        }

        [TestMethod]
        public void GetById_ExistingSchedule_ReturnsSchedule()
        {
            // Arrange
            var attraction = CreateTestAttraction();
            _context.Attractions.Add(attraction);

            var schedule = CreateTestSchedule(attraction.Id);
            _context.MaintenanceSchedules.Add(schedule);
            _context.SaveChanges();

            // Act
            var result = _repository.GetById(schedule.Id);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(schedule.Id, result.Id);
            Assert.AreEqual(schedule.Description, result.Description);
        }

        [TestMethod]
        public void GetById_NonExistingSchedule_ReturnsNull()
        {
            // Act
            var result = _repository.GetById(Guid.NewGuid());

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetAll_MultipleSchedules_ReturnsAllSchedules()
        {
            // Arrange
            var attraction = CreateTestAttraction();
            _context.Attractions.Add(attraction);

            var schedule1 = CreateTestSchedule(attraction.Id);
            var schedule2 = CreateTestSchedule(attraction.Id);
            schedule2.Description = "Different description";

            _context.MaintenanceSchedules.AddRange(schedule1, schedule2);
            _context.SaveChanges();

            // Act
            var results = _repository.GetAll();

            // Assert
            Assert.AreEqual(2, results.Count);
        }

        [TestMethod]
        public void GetByAttractionId_ExistingSchedules_ReturnsSchedulesForAttraction()
        {
            // Arrange
            var attraction1 = CreateTestAttraction();
            var attraction2 = CreateTestAttraction();
            _context.Attractions.AddRange(attraction1, attraction2);

            var schedule1 = CreateTestSchedule(attraction1.Id);
            var schedule2 = CreateTestSchedule(attraction1.Id);
            var schedule3 = CreateTestSchedule(attraction2.Id);

            _context.MaintenanceSchedules.AddRange(schedule1, schedule2, schedule3);
            _context.SaveChanges();

            // Act
            var results = _repository.GetByAttractionId(attraction1.Id);

            // Assert
            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(results.All(s => s.AttractionId == attraction1.Id));
        }

        [TestMethod]
        public void GetByStatus_FiltersByStatus_ReturnsMatchingSchedules()
        {
            // Arrange
            var attraction = CreateTestAttraction();
            _context.Attractions.Add(attraction);

            var pendingSchedule = CreateTestSchedule(attraction.Id);
            pendingSchedule.Status = MaintenanceStatus.Pending;

            var completedSchedule = CreateTestSchedule(attraction.Id);
            completedSchedule.Status = MaintenanceStatus.Completed;

            _context.MaintenanceSchedules.AddRange(pendingSchedule, completedSchedule);
            _context.SaveChanges();

            // Act
            var results = _repository.GetByStatus(MaintenanceStatus.Pending);

            // Assert
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(MaintenanceStatus.Pending, results[0].Status);
        }

        [TestMethod]
        public void GetOverdueSchedules_ReturnsPendingPastDueSchedules()
        {
            // Arrange
            var attraction = CreateTestAttraction();
            _context.Attractions.Add(attraction);

            var overdueSchedule = CreateTestSchedule(attraction.Id);
            overdueSchedule.ScheduledDate = DateTime.Now.AddDays(-1);
            overdueSchedule.Status = MaintenanceStatus.Pending;

            var futureSchedule = CreateTestSchedule(attraction.Id);
            futureSchedule.ScheduledDate = DateTime.Now.AddDays(7);
            futureSchedule.Status = MaintenanceStatus.Pending;

            var completedOverdue = CreateTestSchedule(attraction.Id);
            completedOverdue.ScheduledDate = DateTime.Now.AddDays(-1);
            completedOverdue.Status = MaintenanceStatus.Completed;

            _context.MaintenanceSchedules.AddRange(overdueSchedule, futureSchedule, completedOverdue);
            _context.SaveChanges();

            // Act
            var results = _repository.GetOverdueSchedules();

            // Assert
            Assert.AreEqual(1, results.Count);
            Assert.IsTrue(results[0].ScheduledDate < DateTime.Now);
            Assert.AreEqual(MaintenanceStatus.Pending, results[0].Status);
        }

        [TestMethod]
        public void GetByDateRange_FiltersCorrectly_ReturnsSchedulesInRange()
        {
            // Arrange
            var attraction = CreateTestAttraction();
            _context.Attractions.Add(attraction);

            var dateFrom = DateTime.Now;
            var dateTo = DateTime.Now.AddDays(14);

            var scheduleInRange = CreateTestSchedule(attraction.Id);
            scheduleInRange.ScheduledDate = DateTime.Now.AddDays(7);

            var scheduleBeforeRange = CreateTestSchedule(attraction.Id);
            scheduleBeforeRange.ScheduledDate = DateTime.Now.AddDays(-1);

            var scheduleAfterRange = CreateTestSchedule(attraction.Id);
            scheduleAfterRange.ScheduledDate = DateTime.Now.AddDays(15);

            _context.MaintenanceSchedules.AddRange(scheduleInRange, scheduleBeforeRange, scheduleAfterRange);
            _context.SaveChanges();

            // Act
            var results = _repository.GetByDateRange(dateFrom, dateTo);

            // Assert
            Assert.AreEqual(1, results.Count);
            Assert.IsTrue(results[0].ScheduledDate >= dateFrom);
            Assert.IsTrue(results[0].ScheduledDate <= dateTo);
        }

        [TestMethod]
        public void Update_ExistingSchedule_UpdatesSuccessfully()
        {
            // Arrange
            var attraction = CreateTestAttraction();
            _context.Attractions.Add(attraction);

            var schedule = CreateTestSchedule(attraction.Id);
            _context.MaintenanceSchedules.Add(schedule);
            _context.SaveChanges();

            // Act
            schedule.Status = MaintenanceStatus.Completed;
            schedule.Description = "Updated description";
            _repository.Update(schedule);

            // Assert
            var result = _context.MaintenanceSchedules.Find(schedule.Id);
            Assert.IsNotNull(result);
            Assert.AreEqual(MaintenanceStatus.Completed, result.Status);
            Assert.AreEqual("Updated description", result.Description);
        }

        [TestMethod]
        public void Delete_ExistingSchedule_RemovesSchedule()
        {
            // Arrange
            var attraction = CreateTestAttraction();
            _context.Attractions.Add(attraction);

            var schedule = CreateTestSchedule(attraction.Id);
            _context.MaintenanceSchedules.Add(schedule);
            _context.SaveChanges();

            // Act
            _repository.Delete(schedule.Id);

            // Assert
            var result = _context.MaintenanceSchedules.Find(schedule.Id);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetUpcomingSchedules_ReturnsSchedulesWithinDays_Success()
        {
            // Arrange
            var attraction = CreateTestAttraction();
            _context.Attractions.Add(attraction);

            var schedule1 = CreateTestSchedule(attraction.Id);
            schedule1.ScheduledDate = DateTime.Now.AddDays(3);
            schedule1.Status = MaintenanceStatus.Pending;

            var schedule2 = CreateTestSchedule(attraction.Id);
            schedule2.ScheduledDate = DateTime.Now.AddDays(10);
            schedule2.Status = MaintenanceStatus.Pending;

            _context.MaintenanceSchedules.AddRange(schedule1, schedule2);
            _context.SaveChanges();

            // Act
            var results = _repository.GetUpcomingSchedules(7);

            // Assert
            Assert.AreEqual(1, results.Count);
            Assert.IsTrue((results[0].ScheduledDate - DateTime.Now).TotalDays <= 7);
        }

        [TestMethod]
        public void GetByAttractionIdAndDateRange_CombinesFilters_Success()
        {
            // Arrange
            var attraction1 = CreateTestAttraction();
            var attraction2 = CreateTestAttraction();
            _context.Attractions.AddRange(attraction1, attraction2);

            var dateFrom = DateTime.Now;
            var dateTo = DateTime.Now.AddDays(14);

            var targetSchedule = CreateTestSchedule(attraction1.Id);
            targetSchedule.ScheduledDate = DateTime.Now.AddDays(7);

            var wrongAttraction = CreateTestSchedule(attraction2.Id);
            wrongAttraction.ScheduledDate = DateTime.Now.AddDays(7);

            var wrongDate = CreateTestSchedule(attraction1.Id);
            wrongDate.ScheduledDate = DateTime.Now.AddDays(20);

            _context.MaintenanceSchedules.AddRange(targetSchedule, wrongAttraction, wrongDate);
            _context.SaveChanges();

            // Act
            var results = _repository.GetByAttractionIdAndDateRange(attraction1.Id, dateFrom, dateTo);

            // Assert
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
