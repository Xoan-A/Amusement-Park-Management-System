using Domain;

namespace TestDomain
{
    [TestClass]
    public class RedemptionHistoryTest
    {
        [TestMethod]
        public void RedemptionHistory_ValidCreation_Success()
        {
            // Arrange
            var visitorId = Guid.NewGuid();
            var rewardId = Guid.NewGuid();
            var redeemedAt = DateTime.Now;

            // Act
            var redemption = new RedemptionHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = visitorId,
                RewardId = rewardId,
                RedeemedAt = redeemedAt,
                PointsSpent = 500
            };

            // Assert
            Assert.AreEqual(visitorId, redemption.VisitorId);
            Assert.AreEqual(rewardId, redemption.RewardId);
            Assert.AreEqual(redeemedAt, redemption.RedeemedAt);
            Assert.AreEqual(500, redemption.PointsSpent);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RedemptionHistory_VisitorIdIsEmpty_ThrowsException()
        {
            // Arrange & Act & Assert
            var redemption = new RedemptionHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = Guid.Empty,
                RewardId = Guid.NewGuid(),
                RedeemedAt = DateTime.Now,
                PointsSpent = 100
            };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RedemptionHistory_RewardIdIsEmpty_ThrowsException()
        {
            // Arrange & Act & Assert
            var redemption = new RedemptionHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = Guid.NewGuid(),
                RewardId = Guid.Empty,
                RedeemedAt = DateTime.Now,
                PointsSpent = 100
            };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RedemptionHistory_PointsSpentIsZero_ThrowsException()
        {
            // Arrange & Act & Assert
            var redemption = new RedemptionHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = Guid.NewGuid(),
                RewardId = Guid.NewGuid(),
                RedeemedAt = DateTime.Now,
                PointsSpent = 0
            };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RedemptionHistory_PointsSpentIsNegative_ThrowsException()
        {
            // Arrange & Act & Assert
            var redemption = new RedemptionHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = Guid.NewGuid(),
                RewardId = Guid.NewGuid(),
                RedeemedAt = DateTime.Now,
                PointsSpent = -100
            };
        }

        [TestMethod]
        public void RedemptionHistory_DefaultRedeemedAt_UsesCurrentTime()
        {
            // Arrange
            var beforeCreation = DateTime.Now;

            // Act
            var redemption = new RedemptionHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = Guid.NewGuid(),
                RewardId = Guid.NewGuid(),
                PointsSpent = 100
            };
            var afterCreation = DateTime.Now;

            // Assert
            Assert.IsTrue(redemption.RedeemedAt >= beforeCreation && redemption.RedeemedAt <= afterCreation);
        }

        [TestMethod]
        public void RedemptionHistory_WithNavigationProperties_Success()
        {
            // Arrange
            var visitor = new User
            {
                Id = Guid.NewGuid(),
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "hashedpassword",
                BirthDate = new DateTime(1990, 1, 1),
                MembershipLevel = MembershipLevel.Premium
            };

            var reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "VIP Access",
                Description = "Get VIP access for a day",
                PointsCost = 500,
                AvailableQuantity = 10
            };

            // Act
            var redemption = new RedemptionHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = visitor.Id,
                RewardId = reward.Id,
                RedeemedAt = DateTime.Now,
                PointsSpent = 500,
                Visitor = visitor,
                Reward = reward
            };

            // Assert
            Assert.AreEqual(visitor.Id, redemption.Visitor.Id);
            Assert.AreEqual(reward.Id, redemption.Reward.Id);
            Assert.AreEqual("John", redemption.Visitor.Name);
            Assert.AreEqual("VIP Access", redemption.Reward.Name);
        }

        [TestMethod]
        public void RedemptionHistory_PointsSpentCanDifferFromCurrentRewardCost_Success()
        {
            // Arrange & Act
            // This tests the scenario where reward cost changes after redemption
            // but we preserve the historical points spent
            var redemption = new RedemptionHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = Guid.NewGuid(),
                RewardId = Guid.NewGuid(),
                RedeemedAt = DateTime.Now,
                PointsSpent = 300  // Historical cost
            };

            // Assert
            Assert.AreEqual(300, redemption.PointsSpent);
        }
    }
}
