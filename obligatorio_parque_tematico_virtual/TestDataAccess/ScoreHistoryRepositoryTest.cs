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

            DbContextOptionsBuilder<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection);

            _context = new AppDbContext(options.Options);
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
        public async Task Create_ValidHistory_Success()
        {
            User visitor = CreateTestVisitor();
            _context.Users.Add(visitor);
            await _context.SaveChangesAsync();

            ScoreHistory history = new ScoreHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = visitor.Id,
                Points = 100,
                Origin = ScoreOrigin.AttractionVisit,
                StrategyName = "PerAttraction",
            };

            await _repository.CreateAsync(history);

            ScoreHistory result = await _context.ScoreHistories.FindAsync(history.Id);

            Assert.AreEqual(history.Points, result.Points);
        }

        [TestMethod]
        public async Task GetByVisitor_ReturnsHistoryOrderedByDate()
        {
            User visitor = CreateTestVisitor();
            _context.Users.Add(visitor);
            await _context.SaveChangesAsync();

            ScoreHistory history1 = CreateTestHistory(visitor.Id);
            ScoreHistory history2 = CreateTestHistory(visitor.Id);
            history2.CreatedAt = DateTime.UtcNow.AddHours(1);

            _context.ScoreHistories.AddRange(history1, history2);
            await _context.SaveChangesAsync();

            List<ScoreHistory> results = await _repository.GetByVisitorAsync(visitor.Id);

            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(results[0].CreatedAt >= results[1].CreatedAt);
        }

        [TestMethod]
        public async Task GetByVisitorAndDateRange_FiltersCorrectly()
        {
            User visitor = CreateTestVisitor();
            _context.Users.Add(visitor);
            await _context.SaveChangesAsync();

            DateTime dateFrom = DateTime.UtcNow.AddDays(-7);
            DateTime dateTo = DateTime.UtcNow;

            ScoreHistory history1 = CreateTestHistory(visitor.Id);
            history1.CreatedAt = DateTime.UtcNow.AddDays(-10);

            ScoreHistory history2 = CreateTestHistory(visitor.Id);
            history2.CreatedAt = DateTime.UtcNow.AddDays(-3);

            ScoreHistory history3 = CreateTestHistory(visitor.Id);
            history3.CreatedAt = DateTime.UtcNow.AddDays(-1);

            ScoreHistory history4 = CreateTestHistory(visitor.Id);
            history4.CreatedAt = DateTime.UtcNow.AddDays(1);

            _context.ScoreHistories.AddRange(history1, history2, history3, history4);
            await _context.SaveChangesAsync();

            List<ScoreHistory> results = await _repository.GetByVisitorAndDateRangeAsync(visitor.Id, dateFrom, dateTo);

            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(results.All(h => h.CreatedAt >= dateFrom && h.CreatedAt <= dateTo));
            Assert.IsTrue(results[0].CreatedAt >= results[1].CreatedAt);
        }

        [TestMethod]
        public async Task GetAll_ReturnsAllHistoryOrderedByDate()
        {
            User visitor1 = CreateTestVisitor();
            User visitor2 = CreateTestVisitor();
            _context.Users.AddRange(visitor1, visitor2);
            await _context.SaveChangesAsync();

            ScoreHistory history1 = CreateTestHistory(visitor1.Id);
            history1.CreatedAt = DateTime.UtcNow.AddDays(-2);

            ScoreHistory history2 = CreateTestHistory(visitor2.Id);
            history2.CreatedAt = DateTime.UtcNow.AddDays(-1);

            ScoreHistory history3 = CreateTestHistory(visitor1.Id);
            history3.CreatedAt = DateTime.UtcNow;

            _context.ScoreHistories.AddRange(history1, history2, history3);
            await _context.SaveChangesAsync();

            List<ScoreHistory> results = await _repository.GetAllAsync();

            Assert.AreEqual(3, results.Count);
            Assert.IsTrue(results[0].CreatedAt >= results[1].CreatedAt);
            Assert.IsTrue(results[1].CreatedAt >= results[2].CreatedAt);
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
            };
        }
    }
}