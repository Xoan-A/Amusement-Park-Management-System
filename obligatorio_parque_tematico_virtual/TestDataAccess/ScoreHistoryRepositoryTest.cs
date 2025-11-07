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
                Description = "Visited Roller Coaster"
            };

            await _repository.CreateAsync(history);

            ScoreHistory result = await _context.ScoreHistories.FindAsync(history.Id);
            Assert.IsNotNull(result);
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
        public async Task GetById_WithValidId_ReturnsScoreHistory()
        {
            User visitor = CreateTestVisitor();
            _context.Users.Add(visitor);
            await _context.SaveChangesAsync();

            ScoreHistory history = CreateTestHistory(visitor.Id);
            _context.ScoreHistories.Add(history);
            await _context.SaveChangesAsync();

            ScoreHistory result = await _repository.GetByIdAsync(history.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(history.Id, result.Id);
            Assert.AreEqual(history.Points, result.Points);
            Assert.IsNotNull(result.Visitor);
            Assert.AreEqual(visitor.Name, result.Visitor.Name);
        }

        [TestMethod]
        public async Task GetById_WithInvalidId_ReturnsNull()
        {
            ScoreHistory result = await _repository.GetByIdAsync(Guid.NewGuid());

            Assert.IsNull(result);
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
        public async Task GetByOrigin_WithAttractionVisit_ReturnsMatchingHistory()
        {
            User visitor1 = CreateTestVisitor();
            User visitor2 = CreateTestVisitor();
            _context.Users.AddRange(visitor1, visitor2);
            await _context.SaveChangesAsync();

            ScoreHistory attractionHistory1 = CreateTestHistory(visitor1.Id);
            attractionHistory1.Origin = ScoreOrigin.AttractionVisit;

            ScoreHistory attractionHistory2 = CreateTestHistory(visitor2.Id);
            attractionHistory2.Origin = ScoreOrigin.AttractionVisit;

            ScoreHistory eventHistory = CreateTestHistory(visitor1.Id);
            eventHistory.Origin = ScoreOrigin.EventParticipation;

            _context.ScoreHistories.AddRange(attractionHistory1, attractionHistory2, eventHistory);
            await _context.SaveChangesAsync();

            List<ScoreHistory> results = await _repository.GetByOriginAsync(ScoreOrigin.AttractionVisit);

            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(results.All(h => h.Origin == ScoreOrigin.AttractionVisit));
            Assert.IsNotNull(results[0].Visitor);
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