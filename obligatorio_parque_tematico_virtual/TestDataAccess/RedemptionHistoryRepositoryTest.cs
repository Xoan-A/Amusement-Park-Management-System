using DataAccess.Context;
using DataAccess.Repositories;
using Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace TestDataAccess
{
    [TestClass]
    public class RedemptionHistoryRepositoryTest
    {
        private AppDbContext _context;
        private SqliteConnection _connection;
        private RedemptionHistoryRepository _repository;

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

            _repository = new RedemptionHistoryRepository(_context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Dispose();
            _connection.Close();
        }

        [TestMethod]
        public async Task Create_ValidRedemptionHistory_Success()
        {
            User visitor = CreateTestVisitor();
            Reward reward = CreateTestReward();
            _context.Users.Add(visitor);
            _context.Rewards.Add(reward);
            await _context.SaveChangesAsync();

            RedemptionHistory redemption = new RedemptionHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = visitor.Id,
                RewardId = reward.Id,
                RedeemedAt = DateTime.Now,
                PointsSpent = 500
            };

            await _repository.CreateAsync(redemption);

            RedemptionHistory? retrieved = await _context.RedemptionHistories.FindAsync(redemption.Id);
            Assert.IsNotNull(retrieved);
            Assert.AreEqual(visitor.Id, retrieved.VisitorId);
            Assert.AreEqual(reward.Id, retrieved.RewardId);
            Assert.AreEqual(500, retrieved.PointsSpent);
        }

        [TestMethod]
        public async Task GetByVisitorId_ExistingRedemptions_ReturnsAll()
        {
            User visitor1 = CreateTestVisitor();
            User visitor2 = CreateTestVisitor();
            Reward reward1 = CreateTestReward("Reward 1");
            Reward reward2 = CreateTestReward("Reward 2");

            _context.Users.AddRange(visitor1, visitor2);
            _context.Rewards.AddRange(reward1, reward2);
            await _context.SaveChangesAsync();

            RedemptionHistory redemption1 = new RedemptionHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = visitor1.Id,
                RewardId = reward1.Id,
                RedeemedAt = DateTime.Now.AddDays(-5),
                PointsSpent = 100
            };

            RedemptionHistory redemption2 = new RedemptionHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = visitor1.Id,
                RewardId = reward2.Id,
                RedeemedAt = DateTime.Now.AddDays(-2),
                PointsSpent = 200
            };

            RedemptionHistory redemption3 = new RedemptionHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = visitor2.Id,
                RewardId = reward1.Id,
                RedeemedAt = DateTime.Now,
                PointsSpent = 150
            };

            _context.RedemptionHistories.AddRange(redemption1, redemption2, redemption3);
            await _context.SaveChangesAsync();

            List<RedemptionHistory> visitor1Redemptions = await _repository.GetByVisitorIdAsync(visitor1.Id);

            Assert.AreEqual(2, visitor1Redemptions.Count);
            Assert.IsTrue(visitor1Redemptions.All(r => r.VisitorId == visitor1.Id));
        }

        [TestMethod]
        public async Task GetByVisitorId_NoRedemptions_ReturnsEmptyList()
        {
            User visitor = CreateTestVisitor();
            _context.Users.Add(visitor);
            await _context.SaveChangesAsync();

            List<RedemptionHistory> redemptions = await _repository.GetByVisitorIdAsync(visitor.Id);

            Assert.AreEqual(0, redemptions.Count);
        }

        [TestMethod]
        public async Task GetByVisitorId_IncludesNavigationProperties_Success()
        {
            User visitor = CreateTestVisitor();
            Reward reward = CreateTestReward();

            _context.Users.Add(visitor);
            _context.Rewards.Add(reward);
            await _context.SaveChangesAsync();

            RedemptionHistory redemption = new RedemptionHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = visitor.Id,
                RewardId = reward.Id,
                RedeemedAt = DateTime.Now,
                PointsSpent = 500
            };

            _context.RedemptionHistories.Add(redemption);
            await _context.SaveChangesAsync();

            List<RedemptionHistory> redemptions = await _repository.GetByVisitorIdAsync(visitor.Id);

            Assert.AreEqual(1, redemptions.Count);
            Assert.IsNotNull(redemptions[0].Visitor);
            Assert.IsNotNull(redemptions[0].Reward);
            Assert.AreEqual(visitor.Name, redemptions[0].Visitor.Name);
            Assert.AreEqual(reward.Name, redemptions[0].Reward.Name);
        }

        [TestMethod]
        public async Task GetByVisitorIdWithDateRange_FiltersCorrectly()
        {
            User visitor = CreateTestVisitor();
            Reward reward = CreateTestReward();

            _context.Users.Add(visitor);
            _context.Rewards.Add(reward);
            await _context.SaveChangesAsync();

            RedemptionHistory oldRedemption = new RedemptionHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = visitor.Id,
                RewardId = reward.Id,
                RedeemedAt = DateTime.Now.AddDays(-10),
                PointsSpent = 100
            };

            RedemptionHistory recentRedemption = new RedemptionHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = visitor.Id,
                RewardId = reward.Id,
                RedeemedAt = DateTime.Now.AddDays(-2),
                PointsSpent = 200
            };

            RedemptionHistory veryRecentRedemption = new RedemptionHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = visitor.Id,
                RewardId = reward.Id,
                RedeemedAt = DateTime.Now,
                PointsSpent = 300
            };

            _context.RedemptionHistories.AddRange(oldRedemption, recentRedemption, veryRecentRedemption);
            await _context.SaveChangesAsync();

            DateTime dateFrom = DateTime.Now.AddDays(-5);
            DateTime dateTo = DateTime.Now.AddDays(1);
            List<RedemptionHistory> filteredRedemptions =
                await _repository.GetByVisitorIdWithDateRangeAsync(visitor.Id, dateFrom, dateTo);

            Assert.AreEqual(2, filteredRedemptions.Count);
            Assert.IsFalse(filteredRedemptions.Any(r => r.RedeemedAt < dateFrom));
            Assert.IsFalse(filteredRedemptions.Any(r => r.RedeemedAt > dateTo));
        }

        [TestMethod]
        public async Task GetByVisitorIdWithDateRange_OrderedByDateDescending()
        {
            User visitor = CreateTestVisitor();
            Reward reward = CreateTestReward();

            _context.Users.Add(visitor);
            _context.Rewards.Add(reward);
            await _context.SaveChangesAsync();

            RedemptionHistory redemption1 = new RedemptionHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = visitor.Id,
                RewardId = reward.Id,
                RedeemedAt = DateTime.Now.AddDays(-3),
                PointsSpent = 100
            };

            RedemptionHistory redemption2 = new RedemptionHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = visitor.Id,
                RewardId = reward.Id,
                RedeemedAt = DateTime.Now.AddDays(-1),
                PointsSpent = 200
            };

            RedemptionHistory redemption3 = new RedemptionHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = visitor.Id,
                RewardId = reward.Id,
                RedeemedAt = DateTime.Now.AddDays(-5),
                PointsSpent = 300
            };

            _context.RedemptionHistories.AddRange(redemption1, redemption2, redemption3);
            await _context.SaveChangesAsync();

            List<RedemptionHistory> redemptions = await _repository.GetByVisitorIdWithDateRangeAsync(
                visitor.Id,
                DateTime.Now.AddDays(-10),
                DateTime.Now
            );

            Assert.AreEqual(3, redemptions.Count);
            Assert.IsTrue(redemptions[0].RedeemedAt > redemptions[1].RedeemedAt);
            Assert.IsTrue(redemptions[1].RedeemedAt > redemptions[2].RedeemedAt);
        }

        [TestMethod]
        public async Task GetAll_ReturnsAllRedemptions()
        {
            User visitor1 = CreateTestVisitor();
            User visitor2 = CreateTestVisitor();
            Reward reward = CreateTestReward();

            _context.Users.AddRange(visitor1, visitor2);
            _context.Rewards.Add(reward);
            await _context.SaveChangesAsync();

            RedemptionHistory redemption1 = new RedemptionHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = visitor1.Id,
                RewardId = reward.Id,
                RedeemedAt = DateTime.Now,
                PointsSpent = 100
            };

            RedemptionHistory redemption2 = new RedemptionHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = visitor2.Id,
                RewardId = reward.Id,
                RedeemedAt = DateTime.Now,
                PointsSpent = 200
            };

            _context.RedemptionHistories.AddRange(redemption1, redemption2);
            await _context.SaveChangesAsync();

            List<RedemptionHistory> allRedemptions = await _repository.GetAllAsync();

            Assert.AreEqual(2, allRedemptions.Count);
        }

        private User CreateTestVisitor()
        {
            return new User
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                LastName = "Visitor",
                Email = $"visitor{Guid.NewGuid()}@test.com",
                Password = "hashedpassword",
                BirthDate = new DateTime(1990, 1, 1),
                MembershipLevel = MembershipLevel.Standard
            };
        }

        private Reward CreateTestReward(string name = "Test Reward")
        {
            return new Reward
            {
                Id = Guid.NewGuid(),
                Name = $"{name} {Guid.NewGuid()}",
                Description = "Test description",
                PointsCost = 500,
                AvailableQuantity = 10
            };
        }
    }
}