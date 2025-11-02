using DataAccess.Context;
using DataAccess.Repositories;
using Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace TestDataAccess
{
    [TestClass]
    public class ScoreHistoryRepositoryTest
    {
        private AppDbContext _context = null!;
        private ScoreHistoryRepository _repository = null!;
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

            _repository = new ScoreHistoryRepository(_context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }

        [TestMethod]
        public void Create_ValidHistory_Success()
        {
            // Arrange
            var visitor = CreateTestVisitor();
            _context.Users.Add(visitor);
            _context.SaveChanges();

            var history = new ScoreHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = visitor.Id,
                Points = 100,
                Origin = ScoreOrigin.AttractionVisit,
                StrategyName = "PerAttraction",
                Description = "Visited Roller Coaster"
            };

            // Act
            _repository.Create(history);

            // Assert
            var result = _context.ScoreHistories.Find(history.Id);
            Assert.IsNotNull(result);
            Assert.AreEqual(history.Points, result.Points);
        }

        [TestMethod]
        public void GetByVisitor_ReturnsHistoryOrderedByDate()
        {
            // Arrange
            var visitor = CreateTestVisitor();
            _context.Users.Add(visitor);
            _context.SaveChanges();

            var history1 = CreateTestHistory(visitor.Id);
            var history2 = CreateTestHistory(visitor.Id);
            history2.CreatedAt = DateTime.UtcNow.AddHours(1);

            _context.ScoreHistories.AddRange(history1, history2);
            _context.SaveChanges();

            // Act
            var results = _repository.GetByVisitor(visitor.Id);

            // Assert
            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(results[0].CreatedAt >= results[1].CreatedAt);
        }

        [TestMethod]
        public void GetById_WithValidId_ReturnsScoreHistory()
        {
            // Arrange
            var visitor = CreateTestVisitor();
            _context.Users.Add(visitor);
            _context.SaveChanges();

            var history = CreateTestHistory(visitor.Id);
            _context.ScoreHistories.Add(history);
            _context.SaveChanges();

            // Act
            var result = _repository.GetById(history.Id);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(history.Id, result.Id);
            Assert.AreEqual(history.Points, result.Points);
            Assert.IsNotNull(result.Visitor);
            Assert.AreEqual(visitor.Name, result.Visitor.Name);
        }

        [TestMethod]
        public void GetById_WithInvalidId_ReturnsNull()
        {
            // Act
            var result = _repository.GetById(Guid.NewGuid());

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetByVisitorAndDateRange_FiltersCorrectly()
        {
            // Arrange
            var visitor = CreateTestVisitor();
            _context.Users.Add(visitor);
            _context.SaveChanges();

            var dateFrom = DateTime.UtcNow.AddDays(-7);
            var dateTo = DateTime.UtcNow;

            var history1 = CreateTestHistory(visitor.Id);
            history1.CreatedAt = DateTime.UtcNow.AddDays(-10); // Outside range

            var history2 = CreateTestHistory(visitor.Id);
            history2.CreatedAt = DateTime.UtcNow.AddDays(-3); // Inside range

            var history3 = CreateTestHistory(visitor.Id);
            history3.CreatedAt = DateTime.UtcNow.AddDays(-1); // Inside range

            var history4 = CreateTestHistory(visitor.Id);
            history4.CreatedAt = DateTime.UtcNow.AddDays(1); // Outside range (future)

            _context.ScoreHistories.AddRange(history1, history2, history3, history4);
            _context.SaveChanges();

            // Act
            var results = _repository.GetByVisitorAndDateRange(visitor.Id, dateFrom, dateTo);

            // Assert
            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(results.All(h => h.CreatedAt >= dateFrom && h.CreatedAt <= dateTo));
            Assert.IsTrue(results[0].CreatedAt >= results[1].CreatedAt); // Ordered desc
        }

        [TestMethod]
        public void GetByOrigin_WithAttractionVisit_ReturnsMatchingHistory()
        {
            // Arrange
            var visitor1 = CreateTestVisitor();
            var visitor2 = CreateTestVisitor();
            _context.Users.AddRange(visitor1, visitor2);
            _context.SaveChanges();

            var attractionHistory1 = CreateTestHistory(visitor1.Id);
            attractionHistory1.Origin = ScoreOrigin.AttractionVisit;

            var attractionHistory2 = CreateTestHistory(visitor2.Id);
            attractionHistory2.Origin = ScoreOrigin.AttractionVisit;

            var eventHistory = CreateTestHistory(visitor1.Id);
            eventHistory.Origin = ScoreOrigin.EventParticipation;

            _context.ScoreHistories.AddRange(attractionHistory1, attractionHistory2, eventHistory);
            _context.SaveChanges();

            // Act
            var results = _repository.GetByOrigin(ScoreOrigin.AttractionVisit);

            // Assert
            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(results.All(h => h.Origin == ScoreOrigin.AttractionVisit));
            Assert.IsNotNull(results[0].Visitor);
            Assert.IsTrue(results[0].CreatedAt >= results[1].CreatedAt); // Ordered desc
        }

        [TestMethod]
        public void GetAll_ReturnsAllHistoryOrderedByDate()
        {
            // Arrange
            var visitor1 = CreateTestVisitor();
            var visitor2 = CreateTestVisitor();
            _context.Users.AddRange(visitor1, visitor2);
            _context.SaveChanges();

            var history1 = CreateTestHistory(visitor1.Id);
            history1.CreatedAt = DateTime.UtcNow.AddDays(-2);

            var history2 = CreateTestHistory(visitor2.Id);
            history2.CreatedAt = DateTime.UtcNow.AddDays(-1);

            var history3 = CreateTestHistory(visitor1.Id);
            history3.CreatedAt = DateTime.UtcNow;

            _context.ScoreHistories.AddRange(history1, history2, history3);
            _context.SaveChanges();

            // Act
            var results = _repository.GetAll();

            // Assert
            Assert.AreEqual(3, results.Count);
            Assert.IsTrue(results[0].CreatedAt >= results[1].CreatedAt);
            Assert.IsTrue(results[1].CreatedAt >= results[2].CreatedAt);
            Assert.IsNotNull(results[0].Visitor);
            Assert.IsNotNull(results[1].Visitor);
            Assert.IsNotNull(results[2].Visitor);
        }

        private User CreateTestVisitor()
        {
            return new User
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                LastName = "Visitor",
                Email = $"visitor-{Guid.NewGuid()}@test.com",
                Password = "hashedpassword",
                BirthDate = new DateTime(1990, 1, 1)
            };
        }

        private ScoreHistory CreateTestHistory(Guid visitorId)
        {
            return new ScoreHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = visitorId,
                Points = 50,
                Origin = ScoreOrigin.AttractionVisit,
                StrategyName = "PerAttraction",
                Description = "Test score entry"
            };
        }
    }
}
