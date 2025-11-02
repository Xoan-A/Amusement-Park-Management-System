using DataAccess.Context;
using DataAccess.Repositories;
using Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace TestDataAccess
{
    [TestClass]
    public class RewardRepositoryTest
    {
        private AppDbContext _context;
        private SqliteConnection _connection;
        private RewardRepository _repository;

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

            _repository = new RewardRepository(_context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Dispose();
            _connection.Close();
        }

        [TestMethod]
        public void Create_ValidReward_Success()
        {
            // Arrange
            var reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "VIP Access",
                Description = "Get VIP access for a day",
                PointsCost = 500,
                AvailableQuantity = 10,
                RequiredMembershipLevel = MembershipLevel.Premium
            };

            // Act
            _repository.Create(reward);
            _context.SaveChanges();

            // Assert
            var retrieved = _context.Rewards.Find(reward.Id);
            Assert.IsNotNull(retrieved);
            Assert.AreEqual("VIP Access", retrieved.Name);
            Assert.AreEqual(500, retrieved.PointsCost);
        }

        [TestMethod]
        public void GetAll_ReturnsAllRewards()
        {
            // Arrange
            var reward1 = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Reward 1",
                Description = "Description 1",
                PointsCost = 100,
                AvailableQuantity = 10
            };

            var reward2 = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Reward 2",
                Description = "Description 2",
                PointsCost = 200,
                AvailableQuantity = 5
            };

            _context.Rewards.Add(reward1);
            _context.Rewards.Add(reward2);
            _context.SaveChanges();

            // Act
            var rewards = _repository.GetAll();

            // Assert
            Assert.AreEqual(2, rewards.Count);
        }

        [TestMethod]
        public void GetById_ExistingReward_ReturnsReward()
        {
            // Arrange
            var reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Test Reward",
                Description = "Test description",
                PointsCost = 300,
                AvailableQuantity = 7
            };

            _context.Rewards.Add(reward);
            _context.SaveChanges();

            // Act
            var retrieved = _repository.GetById(reward.Id);

            // Assert
            Assert.IsNotNull(retrieved);
            Assert.AreEqual(reward.Id, retrieved.Id);
            Assert.AreEqual("Test Reward", retrieved.Name);
        }

        [TestMethod]
        public void GetById_NonExistingReward_ReturnsNull()
        {
            // Act
            var retrieved = _repository.GetById(Guid.NewGuid());

            // Assert
            Assert.IsNull(retrieved);
        }

        [TestMethod]
        public void Update_ExistingReward_Success()
        {
            // Arrange
            var reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Original Name",
                Description = "Original description",
                PointsCost = 100,
                AvailableQuantity = 10
            };

            _context.Rewards.Add(reward);
            _context.SaveChanges();

            // Act
            reward.Name = "Updated Name";
            reward.PointsCost = 150;
            _repository.Update(reward);
            _context.SaveChanges();

            // Assert
            var updated = _context.Rewards.Find(reward.Id);
            Assert.AreEqual("Updated Name", updated.Name);
            Assert.AreEqual(150, updated.PointsCost);
        }

        [TestMethod]
        public void Delete_ExistingReward_Success()
        {
            // Arrange
            var reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "To Delete",
                Description = "Will be deleted",
                PointsCost = 100,
                AvailableQuantity = 5
            };

            _context.Rewards.Add(reward);
            _context.SaveChanges();

            // Act
            _repository.Delete(reward.Id);
            _context.SaveChanges();

            // Assert
            var deleted = _context.Rewards.Find(reward.Id);
            Assert.IsNull(deleted);
        }

        [TestMethod]
        public void GetAvailableRewards_ReturnsOnlyRewardsWithQuantity()
        {
            // Arrange
            var availableReward1 = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Available 1",
                Description = "Has stock",
                PointsCost = 100,
                AvailableQuantity = 5
            };

            var availableReward2 = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Available 2",
                Description = "Has stock",
                PointsCost = 200,
                AvailableQuantity = 3
            };

            var unavailableReward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Unavailable",
                Description = "Out of stock",
                PointsCost = 150,
                AvailableQuantity = 0
            };

            _context.Rewards.AddRange(availableReward1, availableReward2, unavailableReward);
            _context.SaveChanges();

            // Act
            var availableRewards = _repository.GetAvailableRewards();

            // Assert
            Assert.AreEqual(2, availableRewards.Count);
            Assert.IsTrue(availableRewards.All(r => r.AvailableQuantity > 0));
        }

        [TestMethod]
        public void GetRewardsByMembershipLevel_NoRequirement_ReturnsAll()
        {
            // Arrange
            var reward1 = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "No Requirement",
                Description = "Anyone can get this",
                PointsCost = 100,
                AvailableQuantity = 10,
                RequiredMembershipLevel = null
            };

            var reward2 = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Premium Required",
                Description = "Premium only",
                PointsCost = 500,
                AvailableQuantity = 5,
                RequiredMembershipLevel = MembershipLevel.Premium
            };

            _context.Rewards.AddRange(reward1, reward2);
            _context.SaveChanges();

            // Act
            var rewards = _repository.GetRewardsByMembershipLevel(null);

            // Assert
            Assert.AreEqual(1, rewards.Count);
            Assert.AreEqual("No Requirement", rewards[0].Name);
        }

        [TestMethod]
        public void GetRewardsByMembershipLevel_SpecificLevel_ReturnsMatching()
        {
            // Arrange
            var standardReward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Standard",
                Description = "Standard level",
                PointsCost = 100,
                AvailableQuantity = 10,
                RequiredMembershipLevel = MembershipLevel.Standard
            };

            var premiumReward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Premium",
                Description = "Premium level",
                PointsCost = 500,
                AvailableQuantity = 5,
                RequiredMembershipLevel = MembershipLevel.Premium
            };

            var vipReward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "VIP",
                Description = "VIP level",
                PointsCost = 1000,
                AvailableQuantity = 2,
                RequiredMembershipLevel = MembershipLevel.VIP
            };

            _context.Rewards.AddRange(standardReward, premiumReward, vipReward);
            _context.SaveChanges();

            // Act
            var premiumRewards = _repository.GetRewardsByMembershipLevel(MembershipLevel.Premium);

            // Assert
            Assert.AreEqual(1, premiumRewards.Count);
            Assert.AreEqual("Premium", premiumRewards[0].Name);
        }

        [TestMethod]
        public void GetByName_ExistingName_ReturnsReward()
        {
            // Arrange
            var reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Unique Name",
                Description = "Test",
                PointsCost = 100,
                AvailableQuantity = 5
            };

            _context.Rewards.Add(reward);
            _context.SaveChanges();

            // Act
            var retrieved = _repository.GetByName("Unique Name");

            // Assert
            Assert.IsNotNull(retrieved);
            Assert.AreEqual("Unique Name", retrieved.Name);
        }

        [TestMethod]
        public void GetByName_NonExistingName_ReturnsNull()
        {
            // Act
            var retrieved = _repository.GetByName("Non Existing");

            // Assert
            Assert.IsNull(retrieved);
        }
    }
}
