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
