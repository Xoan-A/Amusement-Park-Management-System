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

            DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();

            _repository = new MaintenanceRecordRepository(_context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Dispose();
            _connection.Close();
            _connection.Dispose();
        }

        [TestMethod]
        public void Create_ValidRecord_Success()
        {
            Attraction attraction = CreateTestAttraction();
            User operatorUser = CreateTestOperator();
            _context.Attractions.Add(attraction);
            _context.Users.Add(operatorUser);
            _context.SaveChanges();

            MaintenanceRecord record = new MaintenanceRecord
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

            _repository.Create(record);

            MaintenanceRecord? result = _context.MaintenanceRecords.Find(record.Id);
            Assert.IsNotNull(result);
            Assert.AreEqual(record.Description, result.Description);
            Assert.AreEqual(record.Duration, result.Duration);
        }

        [TestMethod]
        public void Create_RecordLinkedToSchedule_Success()
        {
            Attraction attraction = CreateTestAttraction();
            User operatorUser = CreateTestOperator();
            _context.Attractions.Add(attraction);
            _context.Users.Add(operatorUser);

            MaintenanceSchedule schedule = CreateTestSchedule(attraction.Id);
            _context.MaintenanceSchedules.Add(schedule);
            _context.SaveChanges();

            MaintenanceRecord record = new MaintenanceRecord
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

            _repository.Create(record);

            MaintenanceRecord? result = _context.MaintenanceRecords.Find(record.Id);
            Assert.IsNotNull(result);
            Assert.AreEqual(schedule.Id, result.MaintenanceScheduleId);
        }

        [TestMethod]
        public void GetById_ExistingRecord_ReturnsRecord()
        {
            Attraction attraction = CreateTestAttraction();
            User operatorUser = CreateTestOperator();
            _context.Attractions.Add(attraction);
            _context.Users.Add(operatorUser);

            MaintenanceRecord record = CreateTestRecord(attraction.Id, operatorUser.Id);
            _context.MaintenanceRecords.Add(record);
            _context.SaveChanges();

            MaintenanceRecord? result = _repository.GetById(record.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(record.Id, result.Id);
            Assert.AreEqual(record.Description, result.Description);
        }

        [TestMethod]
        public void GetById_NonExistingRecord_ReturnsNull()
        {
            MaintenanceRecord? result = _repository.GetById(Guid.NewGuid());

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetAll_MultipleRecords_ReturnsAllRecords()
        {
            Attraction attraction = CreateTestAttraction();
            User operatorUser = CreateTestOperator();
            _context.Attractions.Add(attraction);
            _context.Users.Add(operatorUser);

            MaintenanceRecord record1 = CreateTestRecord(attraction.Id, operatorUser.Id);
            MaintenanceRecord record2 = CreateTestRecord(attraction.Id, operatorUser.Id);
            record2.Description = "Different maintenance task";

            _context.MaintenanceRecords.AddRange(record1, record2);
            _context.SaveChanges();

            List<MaintenanceRecord> results = _repository.GetAll();

            Assert.AreEqual(2, results.Count);
        }

        [TestMethod]
        public void GetByAttractionId_ExistingRecords_ReturnsRecordsForAttraction()
        {
            Attraction attraction1 = CreateTestAttraction();
            Attraction attraction2 = CreateTestAttraction();
            User operatorUser = CreateTestOperator();
            _context.Attractions.AddRange(attraction1, attraction2);
            _context.Users.Add(operatorUser);

            MaintenanceRecord record1 = CreateTestRecord(attraction1.Id, operatorUser.Id);
            MaintenanceRecord record2 = CreateTestRecord(attraction1.Id, operatorUser.Id);
            MaintenanceRecord record3 = CreateTestRecord(attraction2.Id, operatorUser.Id);

            _context.MaintenanceRecords.AddRange(record1, record2, record3);
            _context.SaveChanges();

            List<MaintenanceRecord> results = _repository.GetByAttractionId(attraction1.Id);

            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(results.All(r => r.AttractionId == attraction1.Id));
        }

        [TestMethod]
        public void GetByScheduleId_ExistingRecords_ReturnsRecordsForSchedule()
        {
            Attraction attraction = CreateTestAttraction();
            User operatorUser = CreateTestOperator();
            _context.Attractions.Add(attraction);
            _context.Users.Add(operatorUser);

            MaintenanceSchedule schedule = CreateTestSchedule(attraction.Id);
            _context.MaintenanceSchedules.Add(schedule);
            _context.SaveChanges();

            MaintenanceRecord record1 = CreateTestRecord(attraction.Id, operatorUser.Id);
            record1.MaintenanceScheduleId = schedule.Id;

            MaintenanceRecord record2 = CreateTestRecord(attraction.Id, operatorUser.Id);
            record2.MaintenanceScheduleId = schedule.Id;

            MaintenanceRecord record3 = CreateTestRecord(attraction.Id, operatorUser.Id);

            _context.MaintenanceRecords.AddRange(record1, record2, record3);
            _context.SaveChanges();

            List<MaintenanceRecord> results = _repository.GetByScheduleId(schedule.Id);

            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(results.All(r => r.MaintenanceScheduleId == schedule.Id));
        }

        [TestMethod]
        public void GetUnscheduledMaintenance_ReturnsRecordsWithNoSchedule()
        {
            Attraction attraction = CreateTestAttraction();
            User operatorUser = CreateTestOperator();
            _context.Attractions.Add(attraction);
            _context.Users.Add(operatorUser);

            MaintenanceSchedule schedule = CreateTestSchedule(attraction.Id);
            _context.MaintenanceSchedules.Add(schedule);
            _context.SaveChanges();

            MaintenanceRecord scheduledRecord = CreateTestRecord(attraction.Id, operatorUser.Id);
            scheduledRecord.MaintenanceScheduleId = schedule.Id;

            MaintenanceRecord unscheduledRecord1 = CreateTestRecord(attraction.Id, operatorUser.Id);
            MaintenanceRecord unscheduledRecord2 = CreateTestRecord(attraction.Id, operatorUser.Id);

            _context.MaintenanceRecords.AddRange(scheduledRecord, unscheduledRecord1, unscheduledRecord2);
            _context.SaveChanges();

            List<MaintenanceRecord> results = _repository.GetUnscheduledMaintenance();

            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(results.All(r => r.MaintenanceScheduleId == null));
        }

        [TestMethod]
        public void GetByOperator_FiltersByOperator_ReturnsMatchingRecords()
        {
            Attraction attraction = CreateTestAttraction();
            User operator1 = CreateTestOperator();
            User operator2 = CreateTestOperator();
            operator2.Email = "operator2@test.com";

            _context.Attractions.Add(attraction);
            _context.Users.AddRange(operator1, operator2);
            _context.SaveChanges();

            MaintenanceRecord record1 = CreateTestRecord(attraction.Id, operator1.Id);
            MaintenanceRecord record2 = CreateTestRecord(attraction.Id, operator1.Id);
            MaintenanceRecord record3 = CreateTestRecord(attraction.Id, operator2.Id);

            _context.MaintenanceRecords.AddRange(record1, record2, record3);
            _context.SaveChanges();

            List<MaintenanceRecord> results = _repository.GetByOperator(operator1.Id);

            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(results.All(r => r.PerformedBy == operator1.Id));
        }

        [TestMethod]
        public void GetByDateRange_FiltersCorrectly_ReturnsRecordsInRange()
        {
            Attraction attraction = CreateTestAttraction();
            User operatorUser = CreateTestOperator();
            _context.Attractions.Add(attraction);
            _context.Users.Add(operatorUser);

            DateTime dateFrom = DateTime.Now.AddDays(-7);
            DateTime dateTo = DateTime.Now;

            MaintenanceRecord recordInRange = CreateTestRecord(attraction.Id, operatorUser.Id);
            recordInRange.PerformedDate = DateTime.Now.AddDays(-3);

            MaintenanceRecord recordBeforeRange = CreateTestRecord(attraction.Id, operatorUser.Id);
            recordBeforeRange.PerformedDate = DateTime.Now.AddDays(-10);

            MaintenanceRecord recordAfterRange = CreateTestRecord(attraction.Id, operatorUser.Id);
            recordAfterRange.PerformedDate = DateTime.Now.AddDays(1);

            _context.MaintenanceRecords.AddRange(recordInRange, recordBeforeRange, recordAfterRange);
            _context.SaveChanges();

            List<MaintenanceRecord> results = _repository.GetByDateRange(dateFrom, dateTo);

            Assert.AreEqual(1, results.Count);
            Assert.IsTrue(results[0].PerformedDate >= dateFrom);
            Assert.IsTrue(results[0].PerformedDate <= dateTo);
        }

        [TestMethod]
        public void GetByMaintenanceType_FiltersByType_ReturnsMatchingRecords()
        {
            Attraction attraction = CreateTestAttraction();
            User operatorUser = CreateTestOperator();
            _context.Attractions.Add(attraction);
            _context.Users.Add(operatorUser);

            MaintenanceRecord inspectionRecord = CreateTestRecord(attraction.Id, operatorUser.Id);
            inspectionRecord.MaintenanceType = MaintenanceType.Inspection;

            MaintenanceRecord repairRecord = CreateTestRecord(attraction.Id, operatorUser.Id);
            repairRecord.MaintenanceType = MaintenanceType.Repair;

            _context.MaintenanceRecords.AddRange(inspectionRecord, repairRecord);
            _context.SaveChanges();

            List<MaintenanceRecord> results = _repository.GetByMaintenanceType(MaintenanceType.Inspection);

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(MaintenanceType.Inspection, results[0].MaintenanceType);
        }

        [TestMethod]
        public void Update_ExistingRecord_UpdatesSuccessfully()
        {
            Attraction attraction = CreateTestAttraction();
            User operatorUser = CreateTestOperator();
            _context.Attractions.Add(attraction);
            _context.Users.Add(operatorUser);

            MaintenanceRecord record = CreateTestRecord(attraction.Id, operatorUser.Id);
            _context.MaintenanceRecords.Add(record);
            _context.SaveChanges();

            record.Notes = "Updated notes after review";
            record.Duration = TimeSpan.FromHours(3);
            _repository.Update(record);

            MaintenanceRecord? result = _context.MaintenanceRecords.Find(record.Id);
            Assert.IsNotNull(result);
            Assert.AreEqual("Updated notes after review", result.Notes);
            Assert.AreEqual(TimeSpan.FromHours(3), result.Duration);
        }

        [TestMethod]
        public void Delete_ExistingRecord_RemovesRecord()
        {
            Attraction attraction = CreateTestAttraction();
            User operatorUser = CreateTestOperator();
            _context.Attractions.Add(attraction);
            _context.Users.Add(operatorUser);

            MaintenanceRecord record = CreateTestRecord(attraction.Id, operatorUser.Id);
            _context.MaintenanceRecords.Add(record);
            _context.SaveChanges();

            _repository.Delete(record.Id);

            MaintenanceRecord? result = _context.MaintenanceRecords.Find(record.Id);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetByAttractionIdAndDateRange_CombinesFilters_Success()
        {
            Attraction attraction1 = CreateTestAttraction();
            Attraction attraction2 = CreateTestAttraction();
            User operatorUser = CreateTestOperator();
            _context.Attractions.AddRange(attraction1, attraction2);
            _context.Users.Add(operatorUser);

            DateTime dateFrom = DateTime.Now.AddDays(-7);
            DateTime dateTo = DateTime.Now;

            MaintenanceRecord targetRecord = CreateTestRecord(attraction1.Id, operatorUser.Id);
            targetRecord.PerformedDate = DateTime.Now.AddDays(-3);

            MaintenanceRecord wrongAttraction = CreateTestRecord(attraction2.Id, operatorUser.Id);
            wrongAttraction.PerformedDate = DateTime.Now.AddDays(-3);

            MaintenanceRecord wrongDate = CreateTestRecord(attraction1.Id, operatorUser.Id);
            wrongDate.PerformedDate = DateTime.Now.AddDays(-20);

            _context.MaintenanceRecords.AddRange(targetRecord, wrongAttraction, wrongDate);
            _context.SaveChanges();

            List<MaintenanceRecord> results =
                _repository.GetByAttractionIdAndDateRange(attraction1.Id, dateFrom, dateTo);

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