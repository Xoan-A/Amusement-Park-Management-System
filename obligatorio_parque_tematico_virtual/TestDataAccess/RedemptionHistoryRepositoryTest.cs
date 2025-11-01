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

            var options = new DbContextOptionsBuilder<AppDbContext>()
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
        public void Create_ValidRedemptionHistory_Success()
        {
            // Arrange
            var visitor = CreateTestVisitor();
            var reward = CreateTestReward();
            _context.Users.Add(visitor);
            _context.Rewards.Add(reward);
            _context.SaveChanges();

            var redemption = new RedemptionHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = visitor.Id,
                RewardId = reward.Id,
                RedeemedAt = DateTime.Now,
                PointsSpent = 500
            };

            // Act
            _repository.Create(redemption);
            _context.SaveChanges();

            // Assert
            var retrieved = _context.RedemptionHistories.Find(redemption.Id);
            Assert.IsNotNull(retrieved);
            Assert.AreEqual(visitor.Id, retrieved.VisitorId);
            Assert.AreEqual(reward.Id, retrieved.RewardId);
            Assert.AreEqual(500, retrieved.PointsSpent);
        }

        [TestMethod]
        public void GetByVisitorId_ExistingRedemptions_ReturnsAll()
        {
            // Arrange
            var visitor1 = CreateTestVisitor();
            var visitor2 = CreateTestVisitor();
            var reward1 = CreateTestReward("Reward 1");
            var reward2 = CreateTestReward("Reward 2");

            _context.Users.AddRange(visitor1, visitor2);
            _context.Rewards.AddRange(reward1, reward2);
            _context.SaveChanges();

            var redemption1 = new RedemptionHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = visitor1.Id,
                RewardId = reward1.Id,
                RedeemedAt = DateTime.Now.AddDays(-5),
                PointsSpent = 100
            };

            var redemption2 = new RedemptionHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = visitor1.Id,
                RewardId = reward2.Id,
                RedeemedAt = DateTime.Now.AddDays(-2),
                PointsSpent = 200
            };

            var redemption3 = new RedemptionHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = visitor2.Id,
                RewardId = reward1.Id,
                RedeemedAt = DateTime.Now,
                PointsSpent = 150
            };

            _context.RedemptionHistories.AddRange(redemption1, redemption2, redemption3);
            _context.SaveChanges();

            // Act
            var visitor1Redemptions = _repository.GetByVisitorId(visitor1.Id);

            // Assert
            Assert.AreEqual(2, visitor1Redemptions.Count);
            Assert.IsTrue(visitor1Redemptions.All(r => r.VisitorId == visitor1.Id));
        }

        [TestMethod]
        public void GetByVisitorId_NoRedemptions_ReturnsEmptyList()
        {
            // Arrange
            var visitor = CreateTestVisitor();
            _context.Users.Add(visitor);
            _context.SaveChanges();

            // Act
            var redemptions = _repository.GetByVisitorId(visitor.Id);

            // Assert
            Assert.AreEqual(0, redemptions.Count);
        }

        [TestMethod]
        public void GetByVisitorId_IncludesNavigationProperties_Success()
        {
            // Arrange
            var visitor = CreateTestVisitor();
            var reward = CreateTestReward();

            _context.Users.Add(visitor);
            _context.Rewards.Add(reward);
            _context.SaveChanges();

            var redemption = new RedemptionHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = visitor.Id,
                RewardId = reward.Id,
                RedeemedAt = DateTime.Now,
                PointsSpent = 500
            };

            _context.RedemptionHistories.Add(redemption);
            _context.SaveChanges();

            // Act
            var redemptions = _repository.GetByVisitorId(visitor.Id);

            // Assert
            Assert.AreEqual(1, redemptions.Count);
            Assert.IsNotNull(redemptions[0].Visitor);
            Assert.IsNotNull(redemptions[0].Reward);
            Assert.AreEqual(visitor.Name, redemptions[0].Visitor.Name);
            Assert.AreEqual(reward.Name, redemptions[0].Reward.Name);
        }

        [TestMethod]
        public void GetByVisitorIdWithDateRange_FiltersCorrectly()
        {
            // Arrange
            var visitor = CreateTestVisitor();
            var reward = CreateTestReward();

            _context.Users.Add(visitor);
            _context.Rewards.Add(reward);
            _context.SaveChanges();

            var oldRedemption = new RedemptionHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = visitor.Id,
                RewardId = reward.Id,
                RedeemedAt = DateTime.Now.AddDays(-10),
                PointsSpent = 100
            };

            var recentRedemption = new RedemptionHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = visitor.Id,
                RewardId = reward.Id,
                RedeemedAt = DateTime.Now.AddDays(-2),
                PointsSpent = 200
            };

            var veryRecentRedemption = new RedemptionHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = visitor.Id,
                RewardId = reward.Id,
                RedeemedAt = DateTime.Now,
                PointsSpent = 300
            };

            _context.RedemptionHistories.AddRange(oldRedemption, recentRedemption, veryRecentRedemption);
            _context.SaveChanges();

            // Act
            var dateFrom = DateTime.Now.AddDays(-5);
            var dateTo = DateTime.Now.AddDays(1);
            var filteredRedemptions = _repository.GetByVisitorIdWithDateRange(visitor.Id, dateFrom, dateTo);

            // Assert
            Assert.AreEqual(2, filteredRedemptions.Count);
            Assert.IsFalse(filteredRedemptions.Any(r => r.RedeemedAt < dateFrom));
            Assert.IsFalse(filteredRedemptions.Any(r => r.RedeemedAt > dateTo));
        }

        [TestMethod]
        public void GetByVisitorIdWithDateRange_OrderedByDateDescending()
        {
            // Arrange
            var visitor = CreateTestVisitor();
            var reward = CreateTestReward();

            _context.Users.Add(visitor);
            _context.Rewards.Add(reward);
            _context.SaveChanges();

            var redemption1 = new RedemptionHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = visitor.Id,
                RewardId = reward.Id,
                RedeemedAt = DateTime.Now.AddDays(-3),
                PointsSpent = 100
            };

            var redemption2 = new RedemptionHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = visitor.Id,
                RewardId = reward.Id,
                RedeemedAt = DateTime.Now.AddDays(-1),
                PointsSpent = 200
            };

            var redemption3 = new RedemptionHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = visitor.Id,
                RewardId = reward.Id,
                RedeemedAt = DateTime.Now.AddDays(-5),
                PointsSpent = 300
            };

            _context.RedemptionHistories.AddRange(redemption1, redemption2, redemption3);
            _context.SaveChanges();

            // Act
            var redemptions = _repository.GetByVisitorIdWithDateRange(
                visitor.Id,
                DateTime.Now.AddDays(-10),
                DateTime.Now
            );

            // Assert
            Assert.AreEqual(3, redemptions.Count);
            Assert.IsTrue(redemptions[0].RedeemedAt > redemptions[1].RedeemedAt);
            Assert.IsTrue(redemptions[1].RedeemedAt > redemptions[2].RedeemedAt);
        }

        [TestMethod]
        public void GetAll_ReturnsAllRedemptions()
        {
            // Arrange
            var visitor1 = CreateTestVisitor();
            var visitor2 = CreateTestVisitor();
            var reward = CreateTestReward();

            _context.Users.AddRange(visitor1, visitor2);
            _context.Rewards.Add(reward);
            _context.SaveChanges();

            var redemption1 = new RedemptionHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = visitor1.Id,
                RewardId = reward.Id,
                RedeemedAt = DateTime.Now,
                PointsSpent = 100
            };

            var redemption2 = new RedemptionHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = visitor2.Id,
                RewardId = reward.Id,
                RedeemedAt = DateTime.Now,
                PointsSpent = 200
            };

            _context.RedemptionHistories.AddRange(redemption1, redemption2);
            _context.SaveChanges();

            // Act
            var allRedemptions = _repository.GetAll();

            // Assert
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
