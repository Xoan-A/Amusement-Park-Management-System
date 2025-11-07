using BusinessLogic;
using Domain;
using IDataAccess;
using Models.In;
using Models.Out;
using Moq;

namespace TestBusinessLogic
{
    [TestClass]
    public class RewardLogicTest
    {
        private Mock<IRewardRepository> _mockRewardRepository;
        private RewardLogic _rewardLogic;

        [TestInitialize]
        public void Setup()
        {
            _mockRewardRepository = new Mock<IRewardRepository>();
            _rewardLogic = new RewardLogic(_mockRewardRepository.Object);
        }

        [TestMethod]
        public async Task CreateReward_ValidReward_Success()
        {
            RewardModelIn rewardModelIn = new RewardModelIn
            {
                Name = "VIP Access",
                Description = "Get VIP access for a day",
                PointsCost = 500,
                AvailableQuantity = 10,
                RequiredMembershipLevel = MembershipLevel.Premium
            };

            _mockRewardRepository.Setup(r => r.GetByNameAsync(rewardModelIn.Name)).ReturnsAsync((Reward?)null);
            _mockRewardRepository.Setup(r => r.CreateAsync(It.IsAny<Reward>())).Returns(Task.CompletedTask);

            RewardModelOut createdReward = await _rewardLogic.CreateReward(rewardModelIn);

            Assert.IsNotNull(createdReward);
            Assert.AreNotEqual(Guid.Empty, createdReward.Id);
            _mockRewardRepository.Verify(r => r.CreateAsync(It.IsAny<Reward>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreateReward_DuplicateName_ThrowsException()
        {
            Reward existingReward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Existing Reward",
                Description = "Already exists",
                PointsCost = 100,
                AvailableQuantity = 5
            };

            RewardModelIn newRewardModelIn = new RewardModelIn
            {
                Name = "Existing Reward",
                Description = "Duplicate name",
                PointsCost = 200,
                AvailableQuantity = 10
            };

            _mockRewardRepository.Setup(r => r.GetByNameAsync(newRewardModelIn.Name)).ReturnsAsync(existingReward);

            await _rewardLogic.CreateReward(newRewardModelIn);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateReward_NullReward_ThrowsException()
        {
            await _rewardLogic.CreateReward(null);
        }

        [TestMethod]
        public async Task GetAllRewards_ReturnsAllRewards()
        {
            List<Reward> rewards = new List<Reward>
            {
                new Reward
                {
                    Id = Guid.NewGuid(),
                    Name = "Reward 1",
                    Description = "Description 1",
                    PointsCost = 100,
                    AvailableQuantity = 10
                },
                new Reward
                {
                    Id = Guid.NewGuid(),
                    Name = "Reward 2",
                    Description = "Description 2",
                    PointsCost = 200,
                    AvailableQuantity = 5
                }
            };

            _mockRewardRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(rewards);

            List<RewardModelOut> result = await _rewardLogic.GetAllRewards();

            Assert.AreEqual(2, result.Count);
            _mockRewardRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [TestMethod]
        public async Task GetRewardById_ExistingReward_ReturnsReward()
        {
            Guid rewardId = Guid.NewGuid();
            Reward reward = new Reward
            {
                Id = rewardId,
                Name = "Test Reward",
                Description = "Test description",
                PointsCost = 300,
                AvailableQuantity = 7
            };

            _mockRewardRepository.Setup(r => r.GetByIdAsync(rewardId)).ReturnsAsync(reward);

            RewardModelOut result = await _rewardLogic.GetRewardById(rewardId);

            Assert.IsNotNull(result);
            Assert.AreEqual(rewardId, result.Id);
            Assert.AreEqual("Test Reward", result.Name);
            _mockRewardRepository.Verify(r => r.GetByIdAsync(rewardId), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GetRewardById_NonExistingReward_ThrowsException()
        {
            Guid rewardId = Guid.NewGuid();
            _mockRewardRepository.Setup(r => r.GetByIdAsync(rewardId)).ReturnsAsync((Reward?)null);

            await _rewardLogic.GetRewardById(rewardId);
        }

        [TestMethod]
        public async Task UpdateReward_ValidReward_Success()
        {
            Guid rewardId = Guid.NewGuid();
            Reward existingReward = new Reward
            {
                Id = rewardId,
                Name = "Original Name",
                Description = "Original description",
                PointsCost = 100,
                AvailableQuantity = 10
            };

            RewardModelIn updatedRewardModelIn = new RewardModelIn
            {
                Name = "Updated Name",
                Description = "Updated description",
                PointsCost = 150,
                AvailableQuantity = 8
            };

            _mockRewardRepository.Setup(r => r.GetByIdAsync(rewardId)).ReturnsAsync(existingReward);
            _mockRewardRepository.Setup(r => r.GetByNameAsync("Updated Name")).ReturnsAsync((Reward?)null);
            _mockRewardRepository.Setup(r => r.UpdateAsync(It.IsAny<Reward>())).Returns(Task.CompletedTask);

            RewardModelOut result = await _rewardLogic.UpdateReward(rewardId, updatedRewardModelIn);

            Assert.IsNotNull(result);
            Assert.AreEqual("Updated Name", result.Name);
            Assert.AreEqual(150, result.PointsCost);
            _mockRewardRepository.Verify(r => r.UpdateAsync(It.IsAny<Reward>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task UpdateReward_NonExistingReward_ThrowsException()
        {
            Guid rewardId = Guid.NewGuid();
            RewardModelIn updatedRewardModelIn = new RewardModelIn
            {
                Name = "Updated Name",
                Description = "Updated description",
                PointsCost = 150,
                AvailableQuantity = 8
            };

            _mockRewardRepository.Setup(r => r.GetByIdAsync(rewardId)).ReturnsAsync((Reward?)null);

            await _rewardLogic.UpdateReward(rewardId, updatedRewardModelIn);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task UpdateReward_DuplicateName_ThrowsException()
        {
            Guid rewardId = Guid.NewGuid();
            Reward existingReward = new Reward
            {
                Id = rewardId,
                Name = "Original Name",
                Description = "Original description",
                PointsCost = 100,
                AvailableQuantity = 10
            };

            Reward anotherReward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Another Reward",
                Description = "Another description",
                PointsCost = 200,
                AvailableQuantity = 5
            };

            RewardModelIn updatedRewardModelIn = new RewardModelIn
            {
                Name = "Another Reward",
                Description = "Updated description",
                PointsCost = 150,
                AvailableQuantity = 8
            };

            _mockRewardRepository.Setup(r => r.GetByIdAsync(rewardId)).ReturnsAsync(existingReward);
            _mockRewardRepository.Setup(r => r.GetByNameAsync("Another Reward")).ReturnsAsync(anotherReward);

            await _rewardLogic.UpdateReward(rewardId, updatedRewardModelIn);
        }

        [TestMethod]
        public async Task UpdateReward_SameName_Success()
        {
            Guid rewardId = Guid.NewGuid();
            Reward existingReward = new Reward
            {
                Id = rewardId,
                Name = "Same Name",
                Description = "Original description",
                PointsCost = 100,
                AvailableQuantity = 10
            };

            RewardModelIn updatedRewardModelIn = new RewardModelIn
            {
                Name = "Same Name",
                Description = "Updated description",
                PointsCost = 150,
                AvailableQuantity = 8
            };

            _mockRewardRepository.Setup(r => r.GetByIdAsync(rewardId)).ReturnsAsync(existingReward);
            _mockRewardRepository.Setup(r => r.GetByNameAsync("Same Name")).ReturnsAsync(existingReward);
            _mockRewardRepository.Setup(r => r.UpdateAsync(It.IsAny<Reward>())).Returns(Task.CompletedTask);

            RewardModelOut result = await _rewardLogic.UpdateReward(rewardId, updatedRewardModelIn);

            Assert.IsNotNull(result);
            Assert.AreEqual("Same Name", result.Name);
            _mockRewardRepository.Verify(r => r.UpdateAsync(It.IsAny<Reward>()), Times.Once);
        }

        [TestMethod]
        public async Task DeleteReward_ExistingReward_Success()
        {
            Guid rewardId = Guid.NewGuid();
            Reward reward = new Reward
            {
                Id = rewardId,
                Name = "To Delete",
                Description = "Will be deleted",
                PointsCost = 100,
                AvailableQuantity = 5
            };

            _mockRewardRepository.Setup(r => r.GetByIdAsync(rewardId)).ReturnsAsync(reward);
            _mockRewardRepository.Setup(r => r.DeleteAsync(rewardId)).Returns(Task.CompletedTask);

            await _rewardLogic.DeleteReward(rewardId);

            _mockRewardRepository.Verify(r => r.DeleteAsync(rewardId), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task DeleteReward_NonExistingReward_ThrowsException()
        {
            Guid rewardId = Guid.NewGuid();
            _mockRewardRepository.Setup(r => r.GetByIdAsync(rewardId)).ReturnsAsync((Reward?)null);

            await _rewardLogic.DeleteReward(rewardId);
        }

        [TestMethod]
        public async Task GetAvailableRewards_ReturnsOnlyAvailable()
        {
            List<Reward> availableRewards = new List<Reward>
            {
                new Reward
                {
                    Id = Guid.NewGuid(),
                    Name = "Available 1",
                    Description = "Has stock",
                    PointsCost = 100,
                    AvailableQuantity = 5
                },
                new Reward
                {
                    Id = Guid.NewGuid(),
                    Name = "Available 2",
                    Description = "Has stock",
                    PointsCost = 200,
                    AvailableQuantity = 3
                }
            };

            _mockRewardRepository.Setup(r => r.GetAvailableRewardsAsync()).ReturnsAsync(availableRewards);

            List<RewardModelOut> result = await _rewardLogic.GetAvailableRewards();

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(r => r.AvailableQuantity > 0));
            _mockRewardRepository.Verify(r => r.GetAvailableRewardsAsync(), Times.Once);
        }

        [TestMethod]
        public async Task GetRewardsByMembershipLevel_FiltersCorrectly()
        {
            List<Reward> premiumRewards = new List<Reward>
            {
                new Reward
                {
                    Id = Guid.NewGuid(),
                    Name = "Premium Reward",
                    Description = "Premium only",
                    PointsCost = 500,
                    AvailableQuantity = 5,
                    RequiredMembershipLevel = MembershipLevel.Premium
                }
            };

            _mockRewardRepository.Setup(r => r.GetRewardsByMembershipLevelAsync(MembershipLevel.Premium))
                .ReturnsAsync(premiumRewards);

            List<RewardModelOut> result = await _rewardLogic.GetRewardsByMembershipLevel(MembershipLevel.Premium);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(MembershipLevel.Premium, result[0].RequiredMembershipLevel);
            _mockRewardRepository.Verify(r => r.GetRewardsByMembershipLevelAsync(MembershipLevel.Premium), Times.Once);
        }
    }
}