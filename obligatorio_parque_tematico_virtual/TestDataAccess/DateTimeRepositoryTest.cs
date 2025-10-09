using Microsoft.EntityFrameworkCore;
using Domain;
using DataAccess.Context;
using DataAccess.Repositories;
using IDataAccess;

namespace TestDataAccess
{
    [TestClass]
    public class DateTimeRepositoryTest
    {
        private AppDbContext _context;
        private IDateTimeRepository _dateTimeRepository;

        [TestInitialize]
        public void Setup()
        {
            DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite("DataSource=:memory:")
                .Options;
            _context = new AppDbContext(options);
            _context.Database.OpenConnection();
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            _dateTimeRepository = new DateTimeRepository(_context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Database.CloseConnection();
            _context.Dispose();
        }

        [TestMethod]
        public async Task GetConfiguredDateTime_ShouldReturnNull_WhenNoConfigurationExists()
        {
            DateTime? result = await _dateTimeRepository.GetConfiguredDateTime();

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetConfiguredDateTime_ShouldReturnConfiguredDateTime_WhenConfigurationExists()
        {
            DateTime configuredTime = new DateTime(2025, 9, 2, 14, 45, 0);
            _context.DateTimeConfigurations.Add(new DateTimeConfiguration(configuredTime));
            _context.SaveChanges();

            DateTime? result = await _dateTimeRepository.GetConfiguredDateTime();

            Assert.IsNotNull(result);
            Assert.AreEqual(configuredTime, result.Value);
        }

        [TestMethod]
        public void SetConfiguredDateTime_ShouldCreateNewConfiguration_WhenNoneExists()
        {
            DateTime configuredTime = new DateTime(2025, 10, 15, 10, 30, 0);

            _dateTimeRepository.SetConfiguredDateTime(configuredTime);

            var configuration = _context.DateTimeConfigurations.FirstOrDefault();
            Assert.IsNotNull(configuration);
            Assert.AreEqual(configuredTime, configuration.CurrentDateTime);
            Assert.AreEqual(1, _context.DateTimeConfigurations.Count());
        }

        [TestMethod]
        public void SetConfiguredDateTime_ShouldUpdateExistingConfiguration_WhenOneExists()
        {
            DateTime initialTime = new DateTime(2025, 9, 2, 14, 45, 0);
            _context.DateTimeConfigurations.Add(new DateTimeConfiguration(initialTime));
            _context.SaveChanges();

            DateTime updatedTime = new DateTime(2025, 10, 15, 10, 30, 0);
            _dateTimeRepository.SetConfiguredDateTime(updatedTime);

            var configuration = _context.DateTimeConfigurations.FirstOrDefault();
            Assert.IsNotNull(configuration);
            Assert.AreEqual(updatedTime, configuration.CurrentDateTime);
            Assert.AreEqual(1, _context.DateTimeConfigurations.Count());
        }

        [TestMethod]
        public void SetConfiguredDateTime_ShouldNotCreateDuplicates_WhenCalledMultipleTimes()
        {
            DateTime time1 = new DateTime(2025, 1, 1, 10, 0, 0);
            DateTime time2 = new DateTime(2025, 2, 1, 10, 0, 0);
            DateTime time3 = new DateTime(2025, 3, 1, 10, 0, 0);

            _dateTimeRepository.SetConfiguredDateTime(time1);
            _dateTimeRepository.SetConfiguredDateTime(time2);
            _dateTimeRepository.SetConfiguredDateTime(time3);

            Assert.AreEqual(1, _context.DateTimeConfigurations.Count());
            var configuration = _context.DateTimeConfigurations.FirstOrDefault();
            Assert.IsNotNull(configuration);
            Assert.AreEqual(time3, configuration.CurrentDateTime);
        }

        [TestMethod]
        public void SetConfiguredDateTime_ShouldPersistChangesToDatabase()
        {
            DateTime configuredTime = new DateTime(2025, 5, 5, 5, 5, 0);

            _dateTimeRepository.SetConfiguredDateTime(configuredTime);

            var retrievedConfiguration = _context.DateTimeConfigurations.FirstOrDefault();
            Assert.IsNotNull(retrievedConfiguration);
            Assert.AreEqual(configuredTime, retrievedConfiguration.CurrentDateTime);
        }
    }
}
