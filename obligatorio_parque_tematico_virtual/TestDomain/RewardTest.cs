using Domain;
using Domain.Enums;

namespace TestDomain
{
    [TestClass]
    public class RewardTest
    {
        [TestMethod]
        public void Reward_ValidCreation_Success()
        {
            // Arrange & Act
            var reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "VIP Park Access",
                Description = "Get exclusive access to VIP areas for one day",
                PointsCost = 500,
                AvailableQuantity = 10,
                RequiredMembershipLevel = MembershipLevel.Premium
            };

            // Assert
            Assert.AreEqual("VIP Park Access", reward.Name);
            Assert.AreEqual("Get exclusive access to VIP areas for one day", reward.Description);
            Assert.AreEqual(500, reward.PointsCost);
            Assert.AreEqual(10, reward.AvailableQuantity);
            Assert.AreEqual(MembershipLevel.Premium, reward.RequiredMembershipLevel);
        }

        [TestMethod]
        public void Reward_ValidCreationWithoutMembershipRequirement_Success()
        {
            // Arrange & Act
            var reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Free Drink Voucher",
                Description = "Redeem for a free drink at any park location",
                PointsCost = 50,
                AvailableQuantity = 100,
                RequiredMembershipLevel = null
            };

            // Assert
            Assert.AreEqual("Free Drink Voucher", reward.Name);
            Assert.IsNull(reward.RequiredMembershipLevel);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Reward_NameIsNull_ThrowsException()
        {
            // Arrange & Act & Assert
            var reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = null,
                Description = "Valid description",
                PointsCost = 100,
                AvailableQuantity = 10
            };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Reward_NameIsEmpty_ThrowsException()
        {
            // Arrange & Act & Assert
            var reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "",
                Description = "Valid description",
                PointsCost = 100,
                AvailableQuantity = 10
            };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Reward_NameIsWhitespace_ThrowsException()
        {
            // Arrange & Act & Assert
            var reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "   ",
                Description = "Valid description",
                PointsCost = 100,
                AvailableQuantity = 10
            };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Reward_DescriptionIsNull_ThrowsException()
        {
            // Arrange & Act & Assert
            var reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Valid Name",
                Description = null,
                PointsCost = 100,
                AvailableQuantity = 10
            };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Reward_DescriptionIsEmpty_ThrowsException()
        {
            // Arrange & Act & Assert
            var reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Valid Name",
                Description = "",
                PointsCost = 100,
                AvailableQuantity = 10
            };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Reward_PointsCostIsZero_ThrowsException()
        {
            // Arrange & Act & Assert
            var reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Valid Name",
                Description = "Valid description",
                PointsCost = 0,
                AvailableQuantity = 10
            };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Reward_PointsCostIsNegative_ThrowsException()
        {
            // Arrange & Act & Assert
            var reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Valid Name",
                Description = "Valid description",
                PointsCost = -100,
                AvailableQuantity = 10
            };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Reward_AvailableQuantityIsNegative_ThrowsException()
        {
            // Arrange & Act & Assert
            var reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Valid Name",
                Description = "Valid description",
                PointsCost = 100,
                AvailableQuantity = -1
            };
        }

        [TestMethod]
        public void Reward_AvailableQuantityIsZero_Success()
        {
            // Arrange & Act
            var reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Out of Stock Reward",
                Description = "Currently unavailable",
                PointsCost = 100,
                AvailableQuantity = 0
            };

            // Assert
            Assert.AreEqual(0, reward.AvailableQuantity);
        }

        [TestMethod]
        public void Reward_DecrementQuantity_Success()
        {
            // Arrange
            var reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Test Reward",
                Description = "Test description",
                PointsCost = 100,
                AvailableQuantity = 10
            };

            // Act
            reward.DecrementQuantity();

            // Assert
            Assert.AreEqual(9, reward.AvailableQuantity);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Reward_DecrementQuantityWhenZero_ThrowsException()
        {
            // Arrange
            var reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Test Reward",
                Description = "Test description",
                PointsCost = 100,
                AvailableQuantity = 0
            };

            // Act & Assert
            reward.DecrementQuantity();
        }

        [TestMethod]
        public void Reward_IsAvailable_WhenQuantityGreaterThanZero_ReturnsTrue()
        {
            // Arrange
            var reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Test Reward",
                Description = "Test description",
                PointsCost = 100,
                AvailableQuantity = 5
            };

            // Act
            var isAvailable = reward.IsAvailable();

            // Assert
            Assert.IsTrue(isAvailable);
        }

        [TestMethod]
        public void Reward_IsAvailable_WhenQuantityIsZero_ReturnsFalse()
        {
            // Arrange
            var reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Test Reward",
                Description = "Test description",
                PointsCost = 100,
                AvailableQuantity = 0
            };

            // Act
            var isAvailable = reward.IsAvailable();

            // Assert
            Assert.IsFalse(isAvailable);
        }

        [TestMethod]
        public void Reward_NameMaxLength_Success()
        {
            // Arrange
            var longName = new string('A', 100);

            // Act
            var reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = longName,
                Description = "Valid description",
                PointsCost = 100,
                AvailableQuantity = 10
            };

            // Assert
            Assert.AreEqual(longName, reward.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Reward_NameExceedsMaxLength_ThrowsException()
        {
            // Arrange & Act & Assert
            var tooLongName = new string('A', 101);
            var reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = tooLongName,
                Description = "Valid description",
                PointsCost = 100,
                AvailableQuantity = 10
            };
        }

        [TestMethod]
        public void Reward_DescriptionMaxLength_Success()
        {
            // Arrange
            var longDescription = new string('A', 500);

            // Act
            var reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Valid Name",
                Description = longDescription,
                PointsCost = 100,
                AvailableQuantity = 10
            };

            // Assert
            Assert.AreEqual(longDescription, reward.Description);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Reward_DescriptionExceedsMaxLength_ThrowsException()
        {
            // Arrange & Act & Assert
            var tooLongDescription = new string('A', 501);
            var reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Valid Name",
                Description = tooLongDescription,
                PointsCost = 100,
                AvailableQuantity = 10
            };
        }
    }
}
