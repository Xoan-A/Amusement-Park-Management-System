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
        public async Task Create_ValidReward_Success()
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

            await _repository.CreateAsync(reward);

            Reward? retrieved = await _context.Rewards.FindAsync(reward.Id);
            Assert.IsNotNull(retrieved);
            Assert.AreEqual("VIP Access", retrieved.Name);
            Assert.AreEqual(500, retrieved.PointsCost);
        }

        [TestMethod]
        public async Task GetAll_ReturnsAllRewards()
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
            await _context.SaveChangesAsync();

            List<Reward> rewards = await _repository.GetAllAsync();

            Assert.AreEqual(2, rewards.Count);
        }

        [TestMethod]
        public async Task GetById_ExistingReward_ReturnsReward()
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
            await _context.SaveChangesAsync();

            Reward? retrieved = await _repository.GetByIdAsync(reward.Id);

            Assert.IsNotNull(retrieved);
            Assert.AreEqual(reward.Id, retrieved.Id);
            Assert.AreEqual("Test Reward", retrieved.Name);
        }

        [TestMethod]
        public async Task GetById_NonExistingReward_ReturnsNull()
        {
            Reward? retrieved = await _repository.GetByIdAsync(Guid.NewGuid());

            Assert.IsNull(retrieved);
        }

        [TestMethod]
        public async Task Update_ExistingReward_Success()
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
            await _context.SaveChangesAsync();

            reward.Name = "Updated Name";
            reward.PointsCost = 150;
            await _repository.UpdateAsync(reward);

            Reward? updated = await _context.Rewards.FindAsync(reward.Id);
            Assert.AreEqual("Updated Name", updated.Name);
            Assert.AreEqual(150, updated.PointsCost);
        }

        [TestMethod]
        public async Task Delete_ExistingReward_Success()
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
            await _context.SaveChangesAsync();

            await _repository.DeleteAsync(reward.Id);

            Reward? deleted = await _context.Rewards.FindAsync(reward.Id);
            Assert.IsNull(deleted);
        }

        [TestMethod]
        public async Task GetAvailableRewards_ReturnsOnlyRewardsWithQuantity()
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
            await _context.SaveChangesAsync();

            List<Reward> availableRewards = await _repository.GetAvailableRewardsAsync();

            Assert.AreEqual(2, availableRewards.Count);
            Assert.IsTrue(availableRewards.All(r => r.AvailableQuantity > 0));
        }

        [TestMethod]
        public async Task GetRewardsByMembershipLevel_NoRequirement_ReturnsAll()
        {
            Reward reward1 = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "No Requirement",
                Description = "Anyone can get this",
                PointsCost = 100,
                AvailableQuantity = 10,
                RequiredMembershipLevel = null
            };

            Reward reward2 = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Premium Required",
                Description = "Premium only",
                PointsCost = 500,
                AvailableQuantity = 5,
                RequiredMembershipLevel = MembershipLevel.Premium
            };

            _context.Rewards.AddRange(reward1, reward2);
            await _context.SaveChangesAsync();

            List<Reward> rewards = await _repository.GetRewardsByMembershipLevelAsync(null);

            Assert.AreEqual(1, rewards.Count);
            Assert.AreEqual("No Requirement", rewards[0].Name);
        }

        [TestMethod]
        public async Task GetRewardsByMembershipLevel_SpecificLevel_ReturnsMatching()
        {
            Reward standardReward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Standard",
                Description = "Standard level",
                PointsCost = 100,
                AvailableQuantity = 10,
                RequiredMembershipLevel = MembershipLevel.Standard
            };

            Reward premiumReward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Premium",
                Description = "Premium level",
                PointsCost = 500,
                AvailableQuantity = 5,
                RequiredMembershipLevel = MembershipLevel.Premium
            };

            Reward vipReward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "VIP",
                Description = "VIP level",
                PointsCost = 1000,
                AvailableQuantity = 2,
                RequiredMembershipLevel = MembershipLevel.VIP
            };

            _context.Rewards.AddRange(standardReward, premiumReward, vipReward);
            await _context.SaveChangesAsync();

            List<Reward> premiumRewards = await _repository.GetRewardsByMembershipLevelAsync(MembershipLevel.Premium);

            Assert.AreEqual(1, premiumRewards.Count);
            Assert.AreEqual("Premium", premiumRewards[0].Name);
        }

        [TestMethod]
        public async Task GetByName_ExistingName_ReturnsReward()
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
            await _context.SaveChangesAsync();

            Reward? retrieved = await _repository.GetByNameAsync("Unique Name");

            Assert.IsNotNull(retrieved);
            Assert.AreEqual("Unique Name", retrieved.Name);
        }

        [TestMethod]
        public async Task GetByName_NonExistingName_ReturnsNull()
        {
            Reward? retrieved = await _repository.GetByNameAsync("Non Existing");

            Assert.IsNull(retrieved);
        }
    }
}