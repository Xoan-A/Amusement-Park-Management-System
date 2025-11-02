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
        public void CreateReward_ValidReward_Success()
        {
            // Arrange
            var rewardModelIn = new RewardModelIn
            {
                Name = "VIP Access",
                Description = "Get VIP access for a day",
                PointsCost = 500,
                AvailableQuantity = 10,
                RequiredMembershipLevel = MembershipLevel.Premium
            };

            _mockRewardRepository.Setup(r => r.GetByName(rewardModelIn.Name)).Returns((Reward?)null);
            _mockRewardRepository.Setup(r => r.Create(It.IsAny<Reward>()));

            // Act
            var createdReward = _rewardLogic.CreateReward(rewardModelIn);

            // Assert
            Assert.IsNotNull(createdReward);
            Assert.AreNotEqual(Guid.Empty, createdReward.Id);
            _mockRewardRepository.Verify(r => r.Create(It.IsAny<Reward>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateReward_DuplicateName_ThrowsException()
        {
            // Arrange
            var existingReward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Existing Reward",
                Description = "Already exists",
                PointsCost = 100,
                AvailableQuantity = 5
            };

            var newRewardModelIn = new RewardModelIn
            {
                Name = "Existing Reward",
                Description = "Duplicate name",
                PointsCost = 200,
                AvailableQuantity = 10
            };

            _mockRewardRepository.Setup(r => r.GetByName(newRewardModelIn.Name)).Returns(existingReward);

            // Act & Assert
            _rewardLogic.CreateReward(newRewardModelIn);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateReward_NullReward_ThrowsException()
        {
            // Act & Assert
            _rewardLogic.CreateReward(null);
        }

        [TestMethod]
        public void GetAllRewards_ReturnsAllRewards()
        {
            // Arrange
            var rewards = new List<Reward>
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

            _mockRewardRepository.Setup(r => r.GetAll()).Returns(rewards);

            // Act
            var result = _rewardLogic.GetAllRewards();

            // Assert
            Assert.AreEqual(2, result.Count);
            _mockRewardRepository.Verify(r => r.GetAll(), Times.Once);
        }

        [TestMethod]
        public void GetRewardById_ExistingReward_ReturnsReward()
        {
            // Arrange
            var rewardId = Guid.NewGuid();
            var reward = new Reward
            {
                Id = rewardId,
                Name = "Test Reward",
                Description = "Test description",
                PointsCost = 300,
                AvailableQuantity = 7
            };

            _mockRewardRepository.Setup(r => r.GetById(rewardId)).Returns(reward);

            // Act
            var result = _rewardLogic.GetRewardById(rewardId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(rewardId, result.Id);
            Assert.AreEqual("Test Reward", result.Name);
            _mockRewardRepository.Verify(r => r.GetById(rewardId), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void GetRewardById_NonExistingReward_ThrowsException()
        {
            // Arrange
            var rewardId = Guid.NewGuid();
            _mockRewardRepository.Setup(r => r.GetById(rewardId)).Returns((Reward?)null);

            // Act & Assert
            _rewardLogic.GetRewardById(rewardId);
        }

        [TestMethod]
        public void UpdateReward_ValidReward_Success()
        {
            // Arrange
            var rewardId = Guid.NewGuid();
            var existingReward = new Reward
            {
                Id = rewardId,
                Name = "Original Name",
                Description = "Original description",
                PointsCost = 100,
                AvailableQuantity = 10
            };

            var updatedRewardModelIn = new RewardModelIn
            {
                Name = "Updated Name",
                Description = "Updated description",
                PointsCost = 150,
                AvailableQuantity = 8
            };

            _mockRewardRepository.Setup(r => r.GetById(rewardId)).Returns(existingReward);
            _mockRewardRepository.Setup(r => r.GetByName("Updated Name")).Returns((Reward?)null);
            _mockRewardRepository.Setup(r => r.Update(It.IsAny<Reward>()));

            // Act
            var result = _rewardLogic.UpdateReward(rewardId, updatedRewardModelIn);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Updated Name", result.Name);
            Assert.AreEqual(150, result.PointsCost);
            _mockRewardRepository.Verify(r => r.Update(It.IsAny<Reward>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void UpdateReward_NonExistingReward_ThrowsException()
        {
            // Arrange
            var rewardId = Guid.NewGuid();
            var updatedRewardModelIn = new RewardModelIn
            {
                Name = "Updated Name",
                Description = "Updated description",
                PointsCost = 150,
                AvailableQuantity = 8
            };

            _mockRewardRepository.Setup(r => r.GetById(rewardId)).Returns((Reward?)null);

            // Act & Assert
            _rewardLogic.UpdateReward(rewardId, updatedRewardModelIn);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void UpdateReward_DuplicateName_ThrowsException()
        {
            // Arrange
            var rewardId = Guid.NewGuid();
            var existingReward = new Reward
            {
                Id = rewardId,
                Name = "Original Name",
                Description = "Original description",
                PointsCost = 100,
                AvailableQuantity = 10
            };

            var anotherReward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Another Reward",
                Description = "Another description",
                PointsCost = 200,
                AvailableQuantity = 5
            };

            var updatedRewardModelIn = new RewardModelIn
            {
                Name = "Another Reward", // Trying to use another reward's name
                Description = "Updated description",
                PointsCost = 150,
                AvailableQuantity = 8
            };

            _mockRewardRepository.Setup(r => r.GetById(rewardId)).Returns(existingReward);
            _mockRewardRepository.Setup(r => r.GetByName("Another Reward")).Returns(anotherReward);

            // Act & Assert
            _rewardLogic.UpdateReward(rewardId, updatedRewardModelIn);
        }

        [TestMethod]
        public void UpdateReward_SameName_Success()
        {
            // Arrange
            var rewardId = Guid.NewGuid();
            var existingReward = new Reward
            {
                Id = rewardId,
                Name = "Same Name",
                Description = "Original description",
                PointsCost = 100,
                AvailableQuantity = 10
            };

            var updatedRewardModelIn = new RewardModelIn
            {
                Name = "Same Name", // Keeping the same name
                Description = "Updated description",
                PointsCost = 150,
                AvailableQuantity = 8
            };

            _mockRewardRepository.Setup(r => r.GetById(rewardId)).Returns(existingReward);
            _mockRewardRepository.Setup(r => r.GetByName("Same Name")).Returns(existingReward);
            _mockRewardRepository.Setup(r => r.Update(It.IsAny<Reward>()));

            // Act
            var result = _rewardLogic.UpdateReward(rewardId, updatedRewardModelIn);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Same Name", result.Name);
            _mockRewardRepository.Verify(r => r.Update(It.IsAny<Reward>()), Times.Once);
        }

        [TestMethod]
        public void DeleteReward_ExistingReward_Success()
        {
            // Arrange
            var rewardId = Guid.NewGuid();
            var reward = new Reward
            {
                Id = rewardId,
                Name = "To Delete",
                Description = "Will be deleted",
                PointsCost = 100,
                AvailableQuantity = 5
            };

            _mockRewardRepository.Setup(r => r.GetById(rewardId)).Returns(reward);
            _mockRewardRepository.Setup(r => r.Delete(rewardId));

            // Act
            _rewardLogic.DeleteReward(rewardId);

            // Assert
            _mockRewardRepository.Verify(r => r.Delete(rewardId), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void DeleteReward_NonExistingReward_ThrowsException()
        {
            // Arrange
            var rewardId = Guid.NewGuid();
            _mockRewardRepository.Setup(r => r.GetById(rewardId)).Returns((Reward?)null);

            // Act & Assert
            _rewardLogic.DeleteReward(rewardId);
        }

        [TestMethod]
        public void GetAvailableRewards_ReturnsOnlyAvailable()
        {
            // Arrange
            var availableRewards = new List<Reward>
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

            _mockRewardRepository.Setup(r => r.GetAvailableRewards()).Returns(availableRewards);

            // Act
            var result = _rewardLogic.GetAvailableRewards();

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(r => r.AvailableQuantity > 0));
            _mockRewardRepository.Verify(r => r.GetAvailableRewards(), Times.Once);
        }

        [TestMethod]
        public void GetRewardsByMembershipLevel_FiltersCorrectly()
        {
            // Arrange
            var premiumRewards = new List<Reward>
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

            _mockRewardRepository.Setup(r => r.GetRewardsByMembershipLevel(MembershipLevel.Premium))
                .Returns(premiumRewards);

            // Act
            var result = _rewardLogic.GetRewardsByMembershipLevel(MembershipLevel.Premium);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(MembershipLevel.Premium, result[0].RequiredMembershipLevel);
            _mockRewardRepository.Verify(r => r.GetRewardsByMembershipLevel(MembershipLevel.Premium), Times.Once);
        }
    }
}
