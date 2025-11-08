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
        public async Task Create_ValidRecord_Success()
        {
            Attraction attraction = CreateTestAttraction();
            User operatorUser = CreateTestOperator();
            _context.Attractions.Add(attraction);
            _context.Users.Add(operatorUser);
            await _context.SaveChangesAsync();

            MaintenanceRecord record = new MaintenanceRecord
            {
                Id = Guid.NewGuid(),
                AttractionId = attraction.Id,
                PerformedDate = DateTime.Now,
                PerformedBy = operatorUser.Id,
                Description = "Routine safety inspection completed",
                Duration = TimeSpan.FromHours(2),
                Notes = "All systems operational"
            };

            await _repository.CreateAsync(record);

            MaintenanceRecord? result = await _context.MaintenanceRecords.FindAsync(record.Id);
            Assert.IsNotNull(result);
            Assert.AreEqual(record.Description, result.Description);
            Assert.AreEqual(record.Duration, result.Duration);
        }

        [TestMethod]
        public async Task Create_RecordLinkedToSchedule_Success()
        {
            Attraction attraction = CreateTestAttraction();
            User operatorUser = CreateTestOperator();
            _context.Attractions.Add(attraction);
            _context.Users.Add(operatorUser);

            MaintenanceSchedule schedule = CreateTestSchedule(attraction.Id);
            _context.MaintenanceSchedules.Add(schedule);
            await _context.SaveChangesAsync();

            MaintenanceRecord record = new MaintenanceRecord
            {
                Id = Guid.NewGuid(),
                MaintenanceScheduleId = schedule.Id,
                AttractionId = attraction.Id,
                PerformedDate = DateTime.Now,
                PerformedBy = operatorUser.Id,
                Description = "Scheduled maintenance completed",
                Duration = TimeSpan.FromHours(1.5)
            };

            await _repository.CreateAsync(record);

            MaintenanceRecord? result = await _context.MaintenanceRecords.FindAsync(record.Id);
            Assert.IsNotNull(result);
            Assert.AreEqual(schedule.Id, result.MaintenanceScheduleId);
        }

        [TestMethod]
        public async Task GetById_ExistingRecord_ReturnsRecord()
        {
            Attraction attraction = CreateTestAttraction();
            User operatorUser = CreateTestOperator();
            _context.Attractions.Add(attraction);
            _context.Users.Add(operatorUser);

            MaintenanceRecord record = CreateTestRecord(attraction.Id, operatorUser.Id);
            _context.MaintenanceRecords.Add(record);
            await _context.SaveChangesAsync();

            MaintenanceRecord? result = await _repository.GetByIdAsync(record.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(record.Id, result.Id);
            Assert.AreEqual(record.Description, result.Description);
        }

        [TestMethod]
        public async Task GetById_NonExistingRecord_ReturnsNull()
        {
            MaintenanceRecord? result = await _repository.GetByIdAsync(Guid.NewGuid());

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetAll_MultipleRecords_ReturnsAllRecords()
        {
            Attraction attraction = CreateTestAttraction();
            User operatorUser = CreateTestOperator();
            _context.Attractions.Add(attraction);
            _context.Users.Add(operatorUser);

            MaintenanceRecord record1 = CreateTestRecord(attraction.Id, operatorUser.Id);
            MaintenanceRecord record2 = CreateTestRecord(attraction.Id, operatorUser.Id);
            record2.Description = "Different maintenance task";

            _context.MaintenanceRecords.AddRange(record1, record2);
            await _context.SaveChangesAsync();

            List<MaintenanceRecord> results = await _repository.GetAllAsync();

            Assert.AreEqual(2, results.Count);
        }

        [TestMethod]
        public async Task GetByAttractionId_ExistingRecords_ReturnsRecordsForAttraction()
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
            await _context.SaveChangesAsync();

            List<MaintenanceRecord> results = await _repository.GetByAttractionIdAsync(attraction1.Id);

            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(results.All(r => r.AttractionId == attraction1.Id));
        }

        [TestMethod]
        public async Task GetByScheduleId_ExistingRecords_ReturnsRecordsForSchedule()
        {
            Attraction attraction = CreateTestAttraction();
            User operatorUser = CreateTestOperator();
            _context.Attractions.Add(attraction);
            _context.Users.Add(operatorUser);

            MaintenanceSchedule schedule = CreateTestSchedule(attraction.Id);
            _context.MaintenanceSchedules.Add(schedule);
            await _context.SaveChangesAsync();

            MaintenanceRecord record1 = CreateTestRecord(attraction.Id, operatorUser.Id);
            record1.MaintenanceScheduleId = schedule.Id;

            MaintenanceRecord record2 = CreateTestRecord(attraction.Id, operatorUser.Id);
            record2.MaintenanceScheduleId = schedule.Id;

            MaintenanceRecord record3 = CreateTestRecord(attraction.Id, operatorUser.Id);

            _context.MaintenanceRecords.AddRange(record1, record2, record3);
            await _context.SaveChangesAsync();

            List<MaintenanceRecord> results = await _repository.GetByScheduleIdAsync(schedule.Id);

            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(results.All(r => r.MaintenanceScheduleId == schedule.Id));
        }

        [TestMethod]
        public async Task GetUnscheduledMaintenance_ReturnsRecordsWithNoSchedule()
        {
            Attraction attraction = CreateTestAttraction();
            User operatorUser = CreateTestOperator();
            _context.Attractions.Add(attraction);
            _context.Users.Add(operatorUser);

            MaintenanceSchedule schedule = CreateTestSchedule(attraction.Id);
            _context.MaintenanceSchedules.Add(schedule);
            await _context.SaveChangesAsync();

            MaintenanceRecord scheduledRecord = CreateTestRecord(attraction.Id, operatorUser.Id);
            scheduledRecord.MaintenanceScheduleId = schedule.Id;

            MaintenanceRecord unscheduledRecord1 = CreateTestRecord(attraction.Id, operatorUser.Id);
            MaintenanceRecord unscheduledRecord2 = CreateTestRecord(attraction.Id, operatorUser.Id);

            _context.MaintenanceRecords.AddRange(scheduledRecord, unscheduledRecord1, unscheduledRecord2);
            await _context.SaveChangesAsync();

            List<MaintenanceRecord> results = await _repository.GetUnscheduledMaintenanceAsync();

            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(results.All(r => r.MaintenanceScheduleId == null));
        }

        [TestMethod]
        public async Task GetByOperator_FiltersByOperator_ReturnsMatchingRecords()
        {
            Attraction attraction = CreateTestAttraction();
            User operator1 = CreateTestOperator();
            User operator2 = CreateTestOperator();
            operator2.Email = "operator2@test.com";

            _context.Attractions.Add(attraction);
            _context.Users.AddRange(operator1, operator2);
            await _context.SaveChangesAsync();

            MaintenanceRecord record1 = CreateTestRecord(attraction.Id, operator1.Id);
            MaintenanceRecord record2 = CreateTestRecord(attraction.Id, operator1.Id);
            MaintenanceRecord record3 = CreateTestRecord(attraction.Id, operator2.Id);

            _context.MaintenanceRecords.AddRange(record1, record2, record3);
            await _context.SaveChangesAsync();

            List<MaintenanceRecord> results = await _repository.GetByOperatorAsync(operator1.Id);

            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(results.All(r => r.PerformedBy == operator1.Id));
        }

        [TestMethod]
        public async Task GetByDateRange_FiltersCorrectly_ReturnsRecordsInRange()
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
            await _context.SaveChangesAsync();

            List<MaintenanceRecord> results = await _repository.GetByDateRangeAsync(dateFrom, dateTo);

            Assert.AreEqual(1, results.Count);
            Assert.IsTrue(results[0].PerformedDate >= dateFrom);
            Assert.IsTrue(results[0].PerformedDate <= dateTo);
        }

        [TestMethod]
        public async Task Update_ExistingRecord_UpdatesSuccessfully()
        {
            Attraction attraction = CreateTestAttraction();
            User operatorUser = CreateTestOperator();
            _context.Attractions.Add(attraction);
            _context.Users.Add(operatorUser);

            MaintenanceRecord record = CreateTestRecord(attraction.Id, operatorUser.Id);
            _context.MaintenanceRecords.Add(record);
            await _context.SaveChangesAsync();

            record.Notes = "Updated notes after review";
            record.Duration = TimeSpan.FromHours(3);
            await _repository.UpdateAsync(record);

            MaintenanceRecord? result = await _context.MaintenanceRecords.FindAsync(record.Id);
            Assert.IsNotNull(result);
            Assert.AreEqual("Updated notes after review", result.Notes);
            Assert.AreEqual(TimeSpan.FromHours(3), result.Duration);
        }

        [TestMethod]
        public async Task Delete_ExistingRecord_RemovesRecord()
        {
            Attraction attraction = CreateTestAttraction();
            User operatorUser = CreateTestOperator();
            _context.Attractions.Add(attraction);
            _context.Users.Add(operatorUser);

            MaintenanceRecord record = CreateTestRecord(attraction.Id, operatorUser.Id);
            _context.MaintenanceRecords.Add(record);
            await _context.SaveChangesAsync();

            await _repository.DeleteAsync(record.Id);

            MaintenanceRecord? result = await _context.MaintenanceRecords.FindAsync(record.Id);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetByAttractionIdAndDateRange_CombinesFilters_Success()
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
            await _context.SaveChangesAsync();

            List<MaintenanceRecord> results =
                await _repository.GetByAttractionIdAndDateRangeAsync(attraction1.Id, dateFrom, dateTo);

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
                Description = "Test maintenance record",
                Duration = TimeSpan.FromHours(2)
            };
        }
    }
}