using BusinessLogic;
using BusinessLogic.Specifications;
using Domain;
using IDataAccess;
using Models.Out;
using Moq;

namespace TestBusinessLogic
{
    [TestClass]
    public class RedemptionLogicTest
    {
        private Mock<IRewardRepository> _mockRewardRepository;
        private Mock<IUserRepository> _mockUserRepository;
        private Mock<IRedemptionHistoryRepository> _mockRedemptionHistoryRepository;
        private RedemptionLogic _redemptionLogic;

        [TestInitialize]
        public void Setup()
        {
            _mockRewardRepository = new Mock<IRewardRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockRedemptionHistoryRepository = new Mock<IRedemptionHistoryRepository>();

            _redemptionLogic = new RedemptionLogic(
                _mockRewardRepository.Object,
                _mockUserRepository.Object,
                _mockRedemptionHistoryRepository.Object
            );
        }

        [TestMethod]
        public void RedeemReward_ValidRedemption_Success()
        {
            // Arrange
            var visitor = new User
            {
                Id = Guid.NewGuid(),
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "hashedpassword",
                Score = 1000,
                MembershipLevel = MembershipLevel.Premium
            };

            var reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "VIP Access",
                Description = "Get VIP access for a day",
                PointsCost = 500,
                AvailableQuantity = 10,
                RequiredMembershipLevel = MembershipLevel.Premium
            };

            _mockUserRepository.Setup(r => r.GetById(visitor.Id)).ReturnsAsync(visitor);
            _mockRewardRepository.Setup(r => r.GetById(reward.Id)).Returns(reward);
            _mockRedemptionHistoryRepository.Setup(r => r.Create(It.IsAny<RedemptionHistory>()));
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>())).Returns(Task.CompletedTask);
            _mockRewardRepository.Setup(r => r.Update(It.IsAny<Reward>()));

            // Act
            var redemption = _redemptionLogic.RedeemReward(visitor.Id, reward.Id);

            // Assert
            Assert.IsNotNull(redemption);
            Assert.AreEqual(visitor.Id, redemption.VisitorId);
            Assert.AreEqual(reward.Id, redemption.RewardId);
            Assert.AreEqual(500, redemption.PointsSpent);

            _mockRedemptionHistoryRepository.Verify(r => r.Create(It.IsAny<RedemptionHistory>()), Times.Once);
            _mockUserRepository.Verify(r => r.Update(It.Is<User>(u => u.Score == 500)), Times.Once);
            _mockRewardRepository.Verify(r => r.Update(It.Is<Reward>(rw => rw.AvailableQuantity == 9)), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void RedeemReward_VisitorNotFound_ThrowsException()
        {
            // Arrange
            var visitorId = Guid.NewGuid();
            var rewardId = Guid.NewGuid();

            _mockUserRepository.Setup(r => r.GetById(visitorId)).ReturnsAsync((User?)null);

            // Act & Assert
            _redemptionLogic.RedeemReward(visitorId, rewardId);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void RedeemReward_RewardNotFound_ThrowsException()
        {
            // Arrange
            var visitor = new User
            {
                Id = Guid.NewGuid(),
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "hashedpassword",
                Score = 1000
            };

            var rewardId = Guid.NewGuid();

            _mockUserRepository.Setup(r => r.GetById(visitor.Id)).ReturnsAsync(visitor);
            _mockRewardRepository.Setup(r => r.GetById(rewardId)).Returns((Reward?)null);

            // Act & Assert
            _redemptionLogic.RedeemReward(visitor.Id, rewardId);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RedeemReward_InsufficientPoints_ThrowsException()
        {
            // Arrange
            var visitor = new User
            {
                Id = Guid.NewGuid(),
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "hashedpassword",
                Score = 300, // Insufficient points
                MembershipLevel = MembershipLevel.Premium
            };

            var reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "VIP Access",
                Description = "Get VIP access for a day",
                PointsCost = 500,
                AvailableQuantity = 10,
                RequiredMembershipLevel = MembershipLevel.Premium
            };

            _mockUserRepository.Setup(r => r.GetById(visitor.Id)).ReturnsAsync(visitor);
            _mockRewardRepository.Setup(r => r.GetById(reward.Id)).Returns(reward);

            // Act & Assert
            _redemptionLogic.RedeemReward(visitor.Id, reward.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RedeemReward_RewardNotAvailable_ThrowsException()
        {
            // Arrange
            var visitor = new User
            {
                Id = Guid.NewGuid(),
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "hashedpassword",
                Score = 1000,
                MembershipLevel = MembershipLevel.Premium
            };

            var reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "VIP Access",
                Description = "Get VIP access for a day",
                PointsCost = 500,
                AvailableQuantity = 0, // Out of stock
                RequiredMembershipLevel = MembershipLevel.Premium
            };

            _mockUserRepository.Setup(r => r.GetById(visitor.Id)).ReturnsAsync(visitor);
            _mockRewardRepository.Setup(r => r.GetById(reward.Id)).Returns(reward);

            // Act & Assert
            _redemptionLogic.RedeemReward(visitor.Id, reward.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RedeemReward_MembershipRequirementNotMet_ThrowsException()
        {
            // Arrange
            var visitor = new User
            {
                Id = Guid.NewGuid(),
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "hashedpassword",
                Score = 1000,
                MembershipLevel = MembershipLevel.Standard // Insufficient membership
            };

            var reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "VIP Access",
                Description = "Get VIP access for a day",
                PointsCost = 500,
                AvailableQuantity = 10,
                RequiredMembershipLevel = MembershipLevel.Premium // Requires Premium
            };

            _mockUserRepository.Setup(r => r.GetById(visitor.Id)).ReturnsAsync(visitor);
            _mockRewardRepository.Setup(r => r.GetById(reward.Id)).Returns(reward);

            // Act & Assert
            _redemptionLogic.RedeemReward(visitor.Id, reward.Id);
        }

        [TestMethod]
        public void RedeemReward_NoMembershipRequired_Success()
        {
            // Arrange
            var visitor = new User
            {
                Id = Guid.NewGuid(),
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "hashedpassword",
                Score = 200,
                MembershipLevel = MembershipLevel.Standard
            };

            var reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Free Drink",
                Description = "Get a free drink",
                PointsCost = 100,
                AvailableQuantity = 50,
                RequiredMembershipLevel = null // No membership required
            };

            _mockUserRepository.Setup(r => r.GetById(visitor.Id)).ReturnsAsync(visitor);
            _mockRewardRepository.Setup(r => r.GetById(reward.Id)).Returns(reward);
            _mockRedemptionHistoryRepository.Setup(r => r.Create(It.IsAny<RedemptionHistory>()));
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>())).Returns(Task.CompletedTask);
            _mockRewardRepository.Setup(r => r.Update(It.IsAny<Reward>()));

            // Act
            var redemption = _redemptionLogic.RedeemReward(visitor.Id, reward.Id);

            // Assert
            Assert.IsNotNull(redemption);
        }

        [TestMethod]
        public void RedeemReward_ExactPoints_Success()
        {
            // Arrange
            var visitor = new User
            {
                Id = Guid.NewGuid(),
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "hashedpassword",
                Score = 500, // Exact points
                MembershipLevel = MembershipLevel.Premium
            };

            var reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Reward",
                Description = "Test",
                PointsCost = 500,
                AvailableQuantity = 1,
                RequiredMembershipLevel = MembershipLevel.Premium
            };

            _mockUserRepository.Setup(r => r.GetById(visitor.Id)).ReturnsAsync(visitor);
            _mockRewardRepository.Setup(r => r.GetById(reward.Id)).Returns(reward);
            _mockRedemptionHistoryRepository.Setup(r => r.Create(It.IsAny<RedemptionHistory>()));
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>())).Returns(Task.CompletedTask);
            _mockRewardRepository.Setup(r => r.Update(It.IsAny<Reward>()));

            // Act
            var redemption = _redemptionLogic.RedeemReward(visitor.Id, reward.Id);

            // Assert
            Assert.IsNotNull(redemption);
        }

        [TestMethod]
        public void GetRedemptionHistory_ByVisitor_ReturnsHistory()
        {
            // Arrange
            var visitorId = Guid.NewGuid();
            var history = new List<RedemptionHistory>
            {
                new RedemptionHistory
                {
                    Id = Guid.NewGuid(),
                    VisitorId = visitorId,
                    RewardId = Guid.NewGuid(),
                    RedeemedAt = DateTime.Now,
                    PointsSpent = 500
                }
            };

            _mockRedemptionHistoryRepository.Setup(r => r.GetByVisitorId(visitorId)).Returns(history);

            // Act
            var result = _redemptionLogic.GetRedemptionHistory(visitorId);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(visitorId, result[0].VisitorId);
            _mockRedemptionHistoryRepository.Verify(r => r.GetByVisitorId(visitorId), Times.Once);
        }

        [TestMethod]
        public void GetRedemptionHistoryWithDateRange_FiltersCorrectly()
        {
            // Arrange
            var visitorId = Guid.NewGuid();
            var dateFrom = DateTime.Now.AddDays(-7);
            var dateTo = DateTime.Now;
            var history = new List<RedemptionHistory>
            {
                new RedemptionHistory
                {
                    Id = Guid.NewGuid(),
                    VisitorId = visitorId,
                    RewardId = Guid.NewGuid(),
                    RedeemedAt = DateTime.Now.AddDays(-3),
                    PointsSpent = 500
                }
            };

            _mockRedemptionHistoryRepository.Setup(r => r.GetByVisitorIdWithDateRange(visitorId, dateFrom, dateTo))
                .Returns(history);

            // Act
            var result = _redemptionLogic.GetRedemptionHistoryWithDateRange(visitorId, dateFrom, dateTo);

            // Assert
            Assert.AreEqual(1, result.Count);
            _mockRedemptionHistoryRepository.Verify(r => r.GetByVisitorIdWithDateRange(visitorId, dateFrom, dateTo), Times.Once);
        }

        [TestMethod]
        public void RedeemReward_WhenVisitorMembershipLevelIsNull_ThrowsInvalidOperationException()
        {
            // Arrange
            Guid visitorId = Guid.NewGuid();
            Guid rewardId = Guid.NewGuid();

            User visitor = new User
            {
                Id = visitorId,
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "pass",
                Score = 1000,
                MembershipLevel = null  // Null to test the specification branch
            };

            Reward reward = new Reward
            {
                Id = rewardId,
                Name = "Premium Reward",
                Description = "Exclusive item",
                PointsCost = 500,
                AvailableQuantity = 10,
                RequiredMembershipLevel = MembershipLevel.Premium
            };

            _mockUserRepository.Setup(r => r.GetById(visitorId)).ReturnsAsync(visitor);
            _mockRewardRepository.Setup(r => r.GetById(rewardId)).Returns(reward);

            // Act & Assert
            Assert.ThrowsException<InvalidOperationException>(
                () => _redemptionLogic.RedeemReward(visitorId, rewardId),
                "Should throw InvalidOperationException when visitor MembershipLevel is null and reward requires membership");
        }
    }
}
