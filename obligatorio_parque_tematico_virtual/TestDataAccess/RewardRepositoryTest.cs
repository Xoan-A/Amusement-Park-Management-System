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

            DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
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
            Reward reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "VIP Access",
                Description = "Get VIP access for a day",
                PointsCost = 500,
                AvailableQuantity = 10,
                RequiredMembershipLevel = MembershipLevel.Premium
            };

            _repository.Create(reward);

            Reward? retrieved = _context.Rewards.Find(reward.Id);

            Assert.AreEqual("VIP Access", retrieved.Name);
            Assert.AreEqual(500, retrieved.PointsCost);
        }

        [TestMethod]
        public void GetAll_ReturnsAllRewards()
        {
            Reward reward1 = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Reward 1",
                Description = "Description 1",
                PointsCost = 100,
                AvailableQuantity = 10
            };

            Reward reward2 = new Reward
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

            List<Reward> rewards = _repository.GetAll();

            Assert.AreEqual(2, rewards.Count);
        }

        [TestMethod]
        public void GetById_ExistingReward_ReturnsReward()
        {
            Reward reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Test Reward",
                Description = "Test description",
                PointsCost = 300,
                AvailableQuantity = 7
            };

            _context.Rewards.Add(reward);
            _context.SaveChanges();

            Reward? retrieved = _repository.GetById(reward.Id);

            Assert.AreEqual(reward.Id, retrieved.Id);
            Assert.AreEqual("Test Reward", retrieved.Name);
        }

        [TestMethod]
        public void GetById_NonExistingReward_ReturnsNull()
        {
            Reward? retrieved = _repository.GetById(Guid.NewGuid());

            Assert.IsNull(retrieved);
        }

        [TestMethod]
        public void Update_ExistingReward_Success()
        {
            Reward reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Original Name",
                Description = "Original description",
                PointsCost = 100,
                AvailableQuantity = 10
            };

            _context.Rewards.Add(reward);
            _context.SaveChanges();

            reward.Name = "Updated Name";
            reward.PointsCost = 150;
            _repository.Update(reward);

            Reward? updated = _context.Rewards.Find(reward.Id);
            Assert.AreEqual("Updated Name", updated.Name);
            Assert.AreEqual(150, updated.PointsCost);
        }

        [TestMethod]
        public void Delete_ExistingReward_Success()
        {
            Reward reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "To Delete",
                Description = "Will be deleted",
                PointsCost = 100,
                AvailableQuantity = 5
            };

            _context.Rewards.Add(reward);
            _context.SaveChanges();

            _repository.Delete(reward.Id);

            Reward? deleted = _context.Rewards.Find(reward.Id);
            Assert.IsNull(deleted);
        }

        [TestMethod]
        public void GetAvailableRewards_ReturnsOnlyRewardsWithQuantity()
        {
            Reward availableReward1 = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Available 1",
                Description = "Has stock",
                PointsCost = 100,
                AvailableQuantity = 5
            };

            Reward availableReward2 = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Available 2",
                Description = "Has stock",
                PointsCost = 200,
                AvailableQuantity = 3
            };

            Reward unavailableReward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Unavailable",
                Description = "Out of stock",
                PointsCost = 150,
                AvailableQuantity = 0
            };

            _context.Rewards.AddRange(availableReward1, availableReward2, unavailableReward);
            _context.SaveChanges();

            List<Reward> availableRewards = _repository.GetAvailableRewards();

            Assert.AreEqual(2, availableRewards.Count);
            Assert.IsTrue(availableRewards.All(r => r.AvailableQuantity > 0));
        }

        [TestMethod]
        public void GetByName_ExistingName_ReturnsReward()
        {
            Reward reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Unique Name",
                Description = "Test",
                PointsCost = 100,
                AvailableQuantity = 5
            };

            _context.Rewards.Add(reward);
            _context.SaveChanges();

            Reward? retrieved = _repository.GetByName("Unique Name");

            Assert.AreEqual("Unique Name", retrieved.Name);
        }

        [TestMethod]
        public void GetByName_NonExistingName_ReturnsNull()
        {
            Reward? retrieved = _repository.GetByName("Non Existing");

            Assert.IsNull(retrieved);
        }
    }
}