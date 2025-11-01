using BusinessLogic.Specifications;
using Domain;

namespace TestBusinessLogic.Specifications
{
    [TestClass]
    public class RedemptionSpecificationTest
    {
        [TestMethod]
        public void HasSufficientPointsSpecification_VisitorHasEnoughPoints_ReturnsTrue()
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

            var reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Test Reward",
                Description = "Test description",
                PointsCost = 500,
                AvailableQuantity = 10
            };

            var specification = new HasSufficientPointsSpecification(reward.PointsCost);

            // Act
            var result = specification.IsSatisfiedBy(visitor);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void HasSufficientPointsSpecification_VisitorHasExactPoints_ReturnsTrue()
        {
            // Arrange
            var visitor = new User
            {
                Id = Guid.NewGuid(),
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "hashedpassword",
                Score = 500
            };

            var specification = new HasSufficientPointsSpecification(500);

            // Act
            var result = specification.IsSatisfiedBy(visitor);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void HasSufficientPointsSpecification_VisitorHasInsufficientPoints_ReturnsFalse()
        {
            // Arrange
            var visitor = new User
            {
                Id = Guid.NewGuid(),
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "hashedpassword",
                Score = 300
            };

            var specification = new HasSufficientPointsSpecification(500);

            // Act
            var result = specification.IsSatisfiedBy(visitor);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MeetsRequiredMembershipSpecification_NoMembershipRequired_ReturnsTrue()
        {
            // Arrange
            var visitor = new User
            {
                Id = Guid.NewGuid(),
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "hashedpassword",
                MembershipLevel = MembershipLevel.Standard
            };

            var specification = new MeetsRequiredMembershipSpecification(null);

            // Act
            var result = specification.IsSatisfiedBy(visitor);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void MeetsRequiredMembershipSpecification_VisitorMeetsRequirement_ReturnsTrue()
        {
            // Arrange
            var visitor = new User
            {
                Id = Guid.NewGuid(),
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "hashedpassword",
                MembershipLevel = MembershipLevel.Premium
            };

            var specification = new MeetsRequiredMembershipSpecification(MembershipLevel.Premium);

            // Act
            var result = specification.IsSatisfiedBy(visitor);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void MeetsRequiredMembershipSpecification_VisitorExceedsRequirement_ReturnsTrue()
        {
            // Arrange
            var visitor = new User
            {
                Id = Guid.NewGuid(),
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "hashedpassword",
                MembershipLevel = MembershipLevel.VIP
            };

            var specification = new MeetsRequiredMembershipSpecification(MembershipLevel.Premium);

            // Act
            var result = specification.IsSatisfiedBy(visitor);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void MeetsRequiredMembershipSpecification_VisitorDoesNotMeetRequirement_ReturnsFalse()
        {
            // Arrange
            var visitor = new User
            {
                Id = Guid.NewGuid(),
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "hashedpassword",
                MembershipLevel = MembershipLevel.Standard
            };

            var specification = new MeetsRequiredMembershipSpecification(MembershipLevel.Premium);

            // Act
            var result = specification.IsSatisfiedBy(visitor);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void RewardIsAvailableSpecification_RewardHasAvailableQuantity_ReturnsTrue()
        {
            // Arrange
            var reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Test Reward",
                Description = "Test description",
                PointsCost = 500,
                AvailableQuantity = 5
            };

            var specification = new RewardIsAvailableSpecification();

            // Act
            var result = specification.IsSatisfiedBy(reward);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void RewardIsAvailableSpecification_RewardHasZeroQuantity_ReturnsFalse()
        {
            // Arrange
            var reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Test Reward",
                Description = "Test description",
                PointsCost = 500,
                AvailableQuantity = 0
            };

            var specification = new RewardIsAvailableSpecification();

            // Act
            var result = specification.IsSatisfiedBy(reward);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void AndSpecification_BothSpecificationsSatisfied_ReturnsTrue()
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

            var pointsSpec = new HasSufficientPointsSpecification(500);
            var membershipSpec = new MeetsRequiredMembershipSpecification(MembershipLevel.Premium);
            var andSpec = new AndSpecification<User>(pointsSpec, membershipSpec);

            // Act
            var result = andSpec.IsSatisfiedBy(visitor);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void AndSpecification_OneSpecificationNotSatisfied_ReturnsFalse()
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

            var pointsSpec = new HasSufficientPointsSpecification(500);
            var membershipSpec = new MeetsRequiredMembershipSpecification(MembershipLevel.Premium);
            var andSpec = new AndSpecification<User>(pointsSpec, membershipSpec);

            // Act
            var result = andSpec.IsSatisfiedBy(visitor);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void AndSpecification_BothSpecificationsNotSatisfied_ReturnsFalse()
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
                MembershipLevel = MembershipLevel.Standard // Insufficient membership
            };

            var pointsSpec = new HasSufficientPointsSpecification(500);
            var membershipSpec = new MeetsRequiredMembershipSpecification(MembershipLevel.Premium);
            var andSpec = new AndSpecification<User>(pointsSpec, membershipSpec);

            // Act
            var result = andSpec.IsSatisfiedBy(visitor);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void AndSpecification_ChainedSpecifications_ReturnsCorrectResult()
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
                MembershipLevel = MembershipLevel.VIP
            };

            var pointsSpec = new HasSufficientPointsSpecification(500);
            var membershipSpec = new MeetsRequiredMembershipSpecification(MembershipLevel.Premium);
            var chainedSpec = pointsSpec.And(membershipSpec);

            // Act
            var result = chainedSpec.IsSatisfiedBy(visitor);

            // Assert
            Assert.IsTrue(result);
        }
    }
}
