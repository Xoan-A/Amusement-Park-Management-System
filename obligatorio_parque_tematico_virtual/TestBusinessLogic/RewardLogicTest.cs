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
            RewardModelIn rewardModelIn = new RewardModelIn
            {
                Name = "VIP Access",
                Description = "Get VIP access for a day",
                PointsCost = 500,
                AvailableQuantity = 10,
                RequiredMembershipLevel = MembershipLevel.Premium
            };

            _mockRewardRepository.Setup(r => r.GetByName(rewardModelIn.Name)).Returns((Reward?)null);
            _mockRewardRepository.Setup(r => r.Create(It.IsAny<Reward>()));

            RewardModelOut createdReward = _rewardLogic.CreateReward(rewardModelIn);

            Assert.AreNotEqual(Guid.Empty, createdReward.Id);
            _mockRewardRepository.Verify(r => r.Create(It.IsAny<Reward>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateReward_DuplicateName_ThrowsException()
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

            _mockRewardRepository.Setup(r => r.GetByName(newRewardModelIn.Name)).Returns(existingReward);

            _rewardLogic.CreateReward(newRewardModelIn);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateReward_NullReward_ThrowsException()
        {
            _rewardLogic.CreateReward(null);
        }

        [TestMethod]
        public void GetAllRewards_ReturnsAllRewards()
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

            _mockRewardRepository.Setup(r => r.GetAll()).Returns(rewards);

            List<RewardModelOut> result = _rewardLogic.GetAllRewards();

            Assert.AreEqual(2, result.Count);
            _mockRewardRepository.Verify(r => r.GetAll(), Times.Once);
        }

        [TestMethod]
        public void GetRewardById_ExistingReward_ReturnsReward()
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

            _mockRewardRepository.Setup(r => r.GetById(rewardId)).Returns(reward);

            RewardModelOut result = _rewardLogic.GetRewardById(rewardId);

            Assert.AreEqual(rewardId, result.Id);
            Assert.AreEqual("Test Reward", result.Name);
            _mockRewardRepository.Verify(r => r.GetById(rewardId), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void GetRewardById_NonExistingReward_ThrowsException()
        {
            Guid rewardId = Guid.NewGuid();
            _mockRewardRepository.Setup(r => r.GetById(rewardId)).Returns((Reward?)null);

            _rewardLogic.GetRewardById(rewardId);
        }

        [TestMethod]
        public void UpdateReward_ValidReward_Success()
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

            _mockRewardRepository.Setup(r => r.GetById(rewardId)).Returns(existingReward);
            _mockRewardRepository.Setup(r => r.GetByName("Updated Name")).Returns((Reward?)null);
            _mockRewardRepository.Setup(r => r.Update(It.IsAny<Reward>()));

            RewardModelOut result = _rewardLogic.UpdateReward(rewardId, updatedRewardModelIn);

            Assert.AreEqual("Updated Name", result.Name);
            Assert.AreEqual(150, result.PointsCost);
            _mockRewardRepository.Verify(r => r.Update(It.IsAny<Reward>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void UpdateReward_NonExistingReward_ThrowsException()
        {
            Guid rewardId = Guid.NewGuid();
            RewardModelIn updatedRewardModelIn = new RewardModelIn
            {
                Name = "Updated Name",
                Description = "Updated description",
                PointsCost = 150,
                AvailableQuantity = 8
            };

            _mockRewardRepository.Setup(r => r.GetById(rewardId)).Returns((Reward?)null);

            _rewardLogic.UpdateReward(rewardId, updatedRewardModelIn);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void UpdateReward_DuplicateName_ThrowsException()
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

            _mockRewardRepository.Setup(r => r.GetById(rewardId)).Returns(existingReward);
            _mockRewardRepository.Setup(r => r.GetByName("Another Reward")).Returns(anotherReward);

            _rewardLogic.UpdateReward(rewardId, updatedRewardModelIn);
        }

        [TestMethod]
        public void UpdateReward_SameName_Success()
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

            _mockRewardRepository.Setup(r => r.GetById(rewardId)).Returns(existingReward);
            _mockRewardRepository.Setup(r => r.GetByName("Same Name")).Returns(existingReward);
            _mockRewardRepository.Setup(r => r.Update(It.IsAny<Reward>()));

            RewardModelOut result = _rewardLogic.UpdateReward(rewardId, updatedRewardModelIn);

            Assert.AreEqual("Same Name", result.Name);
            _mockRewardRepository.Verify(r => r.Update(It.IsAny<Reward>()), Times.Once);
        }

        [TestMethod]
        public void DeleteReward_ExistingReward_Success()
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

            _mockRewardRepository.Setup(r => r.GetById(rewardId)).Returns(reward);
            _mockRewardRepository.Setup(r => r.Delete(rewardId));

            _rewardLogic.DeleteReward(rewardId);

            _mockRewardRepository.Verify(r => r.Delete(rewardId), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void DeleteReward_NonExistingReward_ThrowsException()
        {
            Guid rewardId = Guid.NewGuid();
            _mockRewardRepository.Setup(r => r.GetById(rewardId)).Returns((Reward?)null);

            _rewardLogic.DeleteReward(rewardId);
        }

        [TestMethod]
        public void GetAvailableRewards_ReturnsOnlyAvailable()
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

            _mockRewardRepository.Setup(r => r.GetAvailableRewards()).Returns(availableRewards);

            List<RewardModelOut> result = _rewardLogic.GetAvailableRewards();

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(r => r.AvailableQuantity > 0));
            _mockRewardRepository.Verify(r => r.GetAvailableRewards(), Times.Once);
        }
    }
}