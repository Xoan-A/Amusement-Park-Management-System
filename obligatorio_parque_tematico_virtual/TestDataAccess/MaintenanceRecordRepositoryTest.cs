using DataAccess.Context;
using DataAccess.Repositories;
using Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace TestDataAccess
{
    [TestClass]
    public class MaintenanceRecordRepositoryTest
    {
        private AppDbContext _context = null!;
        private MaintenanceRecordRepository _repository = null!;
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

            _repository = new MaintenanceRecordRepository(_context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }

        [TestMethod]
        public void Create_ValidRecord_Success()
        {
            // Arrange
            var attraction = CreateTestAttraction();
            var operatorUser = CreateTestOperator();
            _context.Attractions.Add(attraction);
            _context.Users.Add(operatorUser);
            _context.SaveChanges();

            var record = new MaintenanceRecord
            {
                Id = Guid.NewGuid(),
                AttractionId = attraction.Id,
                PerformedDate = DateTime.Now,
                PerformedBy = operatorUser.Id,
                MaintenanceType = MaintenanceType.Inspection,
                Description = "Routine safety inspection completed",
                Duration = TimeSpan.FromHours(2),
                Notes = "All systems operational"
            };

            // Act
            _repository.Create(record);

            // Assert
            var result = _context.MaintenanceRecords.Find(record.Id);
            Assert.IsNotNull(result);
            Assert.AreEqual(record.Description, result.Description);
            Assert.AreEqual(record.Duration, result.Duration);
        }

        [TestMethod]
        public void Create_RecordLinkedToSchedule_Success()
        {
            // Arrange
            var attraction = CreateTestAttraction();
            var operatorUser = CreateTestOperator();
            _context.Attractions.Add(attraction);
            _context.Users.Add(operatorUser);

            var schedule = CreateTestSchedule(attraction.Id);
            _context.MaintenanceSchedules.Add(schedule);
            _context.SaveChanges();

            var record = new MaintenanceRecord
            {
                Id = Guid.NewGuid(),
                MaintenanceScheduleId = schedule.Id,
                AttractionId = attraction.Id,
                PerformedDate = DateTime.Now,
                PerformedBy = operatorUser.Id,
                MaintenanceType = MaintenanceType.Inspection,
                Description = "Scheduled maintenance completed",
                Duration = TimeSpan.FromHours(1.5)
            };

            // Act
            _repository.Create(record);

            // Assert
            var result = _context.MaintenanceRecords.Find(record.Id);
            Assert.IsNotNull(result);
            Assert.AreEqual(schedule.Id, result.MaintenanceScheduleId);
        }

        [TestMethod]
        public void GetById_ExistingRecord_ReturnsRecord()
        {
            // Arrange
            var attraction = CreateTestAttraction();
            var operatorUser = CreateTestOperator();
            _context.Attractions.Add(attraction);
            _context.Users.Add(operatorUser);

            var record = CreateTestRecord(attraction.Id, operatorUser.Id);
            _context.MaintenanceRecords.Add(record);
            _context.SaveChanges();

            // Act
            var result = _repository.GetById(record.Id);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(record.Id, result.Id);
            Assert.AreEqual(record.Description, result.Description);
        }

        [TestMethod]
        public void GetById_NonExistingRecord_ReturnsNull()
        {
            // Act
            var result = _repository.GetById(Guid.NewGuid());

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetAll_MultipleRecords_ReturnsAllRecords()
        {
            // Arrange
            var attraction = CreateTestAttraction();
            var operatorUser = CreateTestOperator();
            _context.Attractions.Add(attraction);
            _context.Users.Add(operatorUser);

            var record1 = CreateTestRecord(attraction.Id, operatorUser.Id);
            var record2 = CreateTestRecord(attraction.Id, operatorUser.Id);
            record2.Description = "Different maintenance task";

            _context.MaintenanceRecords.AddRange(record1, record2);
            _context.SaveChanges();

            // Act
            var results = _repository.GetAll();

            // Assert
            Assert.AreEqual(2, results.Count);
        }

        [TestMethod]
        public void GetByAttractionId_ExistingRecords_ReturnsRecordsForAttraction()
        {
            // Arrange
            var attraction1 = CreateTestAttraction();
            var attraction2 = CreateTestAttraction();
            var operatorUser = CreateTestOperator();
            _context.Attractions.AddRange(attraction1, attraction2);
            _context.Users.Add(operatorUser);

            var record1 = CreateTestRecord(attraction1.Id, operatorUser.Id);
            var record2 = CreateTestRecord(attraction1.Id, operatorUser.Id);
            var record3 = CreateTestRecord(attraction2.Id, operatorUser.Id);

            _context.MaintenanceRecords.AddRange(record1, record2, record3);
            _context.SaveChanges();

            // Act
            var results = _repository.GetByAttractionId(attraction1.Id);

            // Assert
            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(results.All(r => r.AttractionId == attraction1.Id));
        }

        [TestMethod]
        public void GetByScheduleId_ExistingRecords_ReturnsRecordsForSchedule()
        {
            // Arrange
            var attraction = CreateTestAttraction();
            var operatorUser = CreateTestOperator();
            _context.Attractions.Add(attraction);
            _context.Users.Add(operatorUser);

            var schedule = CreateTestSchedule(attraction.Id);
            _context.MaintenanceSchedules.Add(schedule);
            _context.SaveChanges();

            var record1 = CreateTestRecord(attraction.Id, operatorUser.Id);
            record1.MaintenanceScheduleId = schedule.Id;

            var record2 = CreateTestRecord(attraction.Id, operatorUser.Id);
            record2.MaintenanceScheduleId = schedule.Id;

            var record3 = CreateTestRecord(attraction.Id, operatorUser.Id);
            // record3 has no schedule link

            _context.MaintenanceRecords.AddRange(record1, record2, record3);
            _context.SaveChanges();

            // Act
            var results = _repository.GetByScheduleId(schedule.Id);

            // Assert
            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(results.All(r => r.MaintenanceScheduleId == schedule.Id));
        }

        [TestMethod]
        public void GetUnscheduledMaintenance_ReturnsRecordsWithNoSchedule()
        {
            // Arrange
            var attraction = CreateTestAttraction();
            var operatorUser = CreateTestOperator();
            _context.Attractions.Add(attraction);
            _context.Users.Add(operatorUser);

            var schedule = CreateTestSchedule(attraction.Id);
            _context.MaintenanceSchedules.Add(schedule);
            _context.SaveChanges();

            var scheduledRecord = CreateTestRecord(attraction.Id, operatorUser.Id);
            scheduledRecord.MaintenanceScheduleId = schedule.Id;

            var unscheduledRecord1 = CreateTestRecord(attraction.Id, operatorUser.Id);
            var unscheduledRecord2 = CreateTestRecord(attraction.Id, operatorUser.Id);

            _context.MaintenanceRecords.AddRange(scheduledRecord, unscheduledRecord1, unscheduledRecord2);
            _context.SaveChanges();

            // Act
            var results = _repository.GetUnscheduledMaintenance();

            // Assert
            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(results.All(r => r.MaintenanceScheduleId == null));
        }

        [TestMethod]
        public void GetByOperator_FiltersByOperator_ReturnsMatchingRecords()
        {
            // Arrange
            var attraction = CreateTestAttraction();
            var operator1 = CreateTestOperator();
            var operator2 = CreateTestOperator();
            operator2.Email = "operator2@test.com";

            _context.Attractions.Add(attraction);
            _context.Users.AddRange(operator1, operator2);
            _context.SaveChanges();

            var record1 = CreateTestRecord(attraction.Id, operator1.Id);
            var record2 = CreateTestRecord(attraction.Id, operator1.Id);
            var record3 = CreateTestRecord(attraction.Id, operator2.Id);

            _context.MaintenanceRecords.AddRange(record1, record2, record3);
            _context.SaveChanges();

            // Act
            var results = _repository.GetByOperator(operator1.Id);

            // Assert
            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(results.All(r => r.PerformedBy == operator1.Id));
        }

        [TestMethod]
        public void GetByDateRange_FiltersCorrectly_ReturnsRecordsInRange()
        {
            // Arrange
            var attraction = CreateTestAttraction();
            var operatorUser = CreateTestOperator();
            _context.Attractions.Add(attraction);
            _context.Users.Add(operatorUser);

            var dateFrom = DateTime.Now.AddDays(-7);
            var dateTo = DateTime.Now;

            var recordInRange = CreateTestRecord(attraction.Id, operatorUser.Id);
            recordInRange.PerformedDate = DateTime.Now.AddDays(-3);

            var recordBeforeRange = CreateTestRecord(attraction.Id, operatorUser.Id);
            recordBeforeRange.PerformedDate = DateTime.Now.AddDays(-10);

            var recordAfterRange = CreateTestRecord(attraction.Id, operatorUser.Id);
            recordAfterRange.PerformedDate = DateTime.Now.AddDays(1);

            _context.MaintenanceRecords.AddRange(recordInRange, recordBeforeRange, recordAfterRange);
            _context.SaveChanges();

            // Act
            var results = _repository.GetByDateRange(dateFrom, dateTo);

            // Assert
            Assert.AreEqual(1, results.Count);
            Assert.IsTrue(results[0].PerformedDate >= dateFrom);
            Assert.IsTrue(results[0].PerformedDate <= dateTo);
        }

        [TestMethod]
        public void GetByMaintenanceType_FiltersByType_ReturnsMatchingRecords()
        {
            // Arrange
            var attraction = CreateTestAttraction();
            var operatorUser = CreateTestOperator();
            _context.Attractions.Add(attraction);
            _context.Users.Add(operatorUser);

            var inspectionRecord = CreateTestRecord(attraction.Id, operatorUser.Id);
            inspectionRecord.MaintenanceType = MaintenanceType.Inspection;

            var repairRecord = CreateTestRecord(attraction.Id, operatorUser.Id);
            repairRecord.MaintenanceType = MaintenanceType.Repair;

            _context.MaintenanceRecords.AddRange(inspectionRecord, repairRecord);
            _context.SaveChanges();

            // Act
            var results = _repository.GetByMaintenanceType(MaintenanceType.Inspection);

            // Assert
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(MaintenanceType.Inspection, results[0].MaintenanceType);
        }

        [TestMethod]
        public void Update_ExistingRecord_UpdatesSuccessfully()
        {
            // Arrange
            var attraction = CreateTestAttraction();
            var operatorUser = CreateTestOperator();
            _context.Attractions.Add(attraction);
            _context.Users.Add(operatorUser);

            var record = CreateTestRecord(attraction.Id, operatorUser.Id);
            _context.MaintenanceRecords.Add(record);
            _context.SaveChanges();

            // Act
            record.Notes = "Updated notes after review";
            record.Duration = TimeSpan.FromHours(3);
            _repository.Update(record);

            // Assert
            var result = _context.MaintenanceRecords.Find(record.Id);
            Assert.IsNotNull(result);
            Assert.AreEqual("Updated notes after review", result.Notes);
            Assert.AreEqual(TimeSpan.FromHours(3), result.Duration);
        }

        [TestMethod]
        public void Delete_ExistingRecord_RemovesRecord()
        {
            // Arrange
            var attraction = CreateTestAttraction();
            var operatorUser = CreateTestOperator();
            _context.Attractions.Add(attraction);
            _context.Users.Add(operatorUser);

            var record = CreateTestRecord(attraction.Id, operatorUser.Id);
            _context.MaintenanceRecords.Add(record);
            _context.SaveChanges();

            // Act
            _repository.Delete(record.Id);

            // Assert
            var result = _context.MaintenanceRecords.Find(record.Id);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetByAttractionIdAndDateRange_CombinesFilters_Success()
        {
            // Arrange
            var attraction1 = CreateTestAttraction();
            var attraction2 = CreateTestAttraction();
            var operatorUser = CreateTestOperator();
            _context.Attractions.AddRange(attraction1, attraction2);
            _context.Users.Add(operatorUser);

            var dateFrom = DateTime.Now.AddDays(-7);
            var dateTo = DateTime.Now;

            var targetRecord = CreateTestRecord(attraction1.Id, operatorUser.Id);
            targetRecord.PerformedDate = DateTime.Now.AddDays(-3);

            var wrongAttraction = CreateTestRecord(attraction2.Id, operatorUser.Id);
            wrongAttraction.PerformedDate = DateTime.Now.AddDays(-3);

            var wrongDate = CreateTestRecord(attraction1.Id, operatorUser.Id);
            wrongDate.PerformedDate = DateTime.Now.AddDays(-20);

            _context.MaintenanceRecords.AddRange(targetRecord, wrongAttraction, wrongDate);
            _context.SaveChanges();

            // Act
            var results = _repository.GetByAttractionIdAndDateRange(attraction1.Id, dateFrom, dateTo);

            // Assert
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(targetRecord.Id, results[0].Id);
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

        private User CreateTestOperator()
        {
            return new User
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                LastName = "Operator",
                Email = $"operator-{Guid.NewGuid()}@test.com",
                Password = "hashedpassword",
                BirthDate = new DateTime(1990, 1, 1)
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

        private MaintenanceRecord CreateTestRecord(Guid attractionId, Guid operatorId)
        {
            return new MaintenanceRecord
            {
                Id = Guid.NewGuid(),
                AttractionId = attractionId,
                PerformedDate = DateTime.Now,
                PerformedBy = operatorId,
                MaintenanceType = MaintenanceType.Inspection,
                Description = "Test maintenance record",
                Duration = TimeSpan.FromHours(2)
            };
        }
    }
}
