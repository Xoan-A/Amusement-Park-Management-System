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
            User visitor = new User
            {
                Id = Guid.NewGuid(),
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "hashedpassword",
                Score = 1000
            };

            Reward reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Test Reward",
                Description = "Test description",
                PointsCost = 500,
                AvailableQuantity = 10
            };

            HasSufficientPointsSpecification specification = new HasSufficientPointsSpecification(reward.PointsCost);

            bool result = specification.IsSatisfiedBy(visitor);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void HasSufficientPointsSpecification_VisitorHasExactPoints_ReturnsTrue()
        {
            User visitor = new User
            {
                Id = Guid.NewGuid(),
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "hashedpassword",
                Score = 500
            };

            HasSufficientPointsSpecification specification = new HasSufficientPointsSpecification(500);

            bool result = specification.IsSatisfiedBy(visitor);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void HasSufficientPointsSpecification_VisitorHasInsufficientPoints_ReturnsFalse()
        {
            User visitor = new User
            {
                Id = Guid.NewGuid(),
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "hashedpassword",
                Score = 300
            };

            HasSufficientPointsSpecification specification = new HasSufficientPointsSpecification(500);

            bool result = specification.IsSatisfiedBy(visitor);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MeetsRequiredMembershipSpecification_NoMembershipRequired_ReturnsTrue()
        {
            User visitor = new User
            {
                Id = Guid.NewGuid(),
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "hashedpassword",
                MembershipLevel = MembershipLevel.Standard
            };

            MeetsRequiredMembershipSpecification specification = new MeetsRequiredMembershipSpecification(null);

            bool result = specification.IsSatisfiedBy(visitor);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void MeetsRequiredMembershipSpecification_VisitorMeetsRequirement_ReturnsTrue()
        {
            User visitor = new User
            {
                Id = Guid.NewGuid(),
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "hashedpassword",
                MembershipLevel = MembershipLevel.Premium
            };

            MeetsRequiredMembershipSpecification specification = new MeetsRequiredMembershipSpecification(MembershipLevel.Premium);

            bool result = specification.IsSatisfiedBy(visitor);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void MeetsRequiredMembershipSpecification_VisitorExceedsRequirement_ReturnsTrue()
        {
            User visitor = new User
            {
                Id = Guid.NewGuid(),
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "hashedpassword",
                MembershipLevel = MembershipLevel.VIP
            };

            MeetsRequiredMembershipSpecification specification = new MeetsRequiredMembershipSpecification(MembershipLevel.Premium);

            bool result = specification.IsSatisfiedBy(visitor);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void MeetsRequiredMembershipSpecification_VisitorDoesNotMeetRequirement_ReturnsFalse()
        {
            User visitor = new User
            {
                Id = Guid.NewGuid(),
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "hashedpassword",
                MembershipLevel = MembershipLevel.Standard
            };

            MeetsRequiredMembershipSpecification specification = new MeetsRequiredMembershipSpecification(MembershipLevel.Premium);

            bool result = specification.IsSatisfiedBy(visitor);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void RewardIsAvailableSpecification_RewardHasAvailableQuantity_ReturnsTrue()
        {
            Reward reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Test Reward",
                Description = "Test description",
                PointsCost = 500,
                AvailableQuantity = 5
            };

            RewardIsAvailableSpecification specification = new RewardIsAvailableSpecification();

            bool result = specification.IsSatisfiedBy(reward);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void RewardIsAvailableSpecification_RewardHasZeroQuantity_ReturnsFalse()
        {
            Reward reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Test Reward",
                Description = "Test description",
                PointsCost = 500,
                AvailableQuantity = 0
            };

            RewardIsAvailableSpecification specification = new RewardIsAvailableSpecification();

            bool result = specification.IsSatisfiedBy(reward);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void AndSpecification_BothSpecificationsSatisfied_ReturnsTrue()
        {
            User visitor = new User
            {
                Id = Guid.NewGuid(),
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "hashedpassword",
                Score = 1000,
                MembershipLevel = MembershipLevel.Premium
            };

            HasSufficientPointsSpecification pointsSpec = new HasSufficientPointsSpecification(500);
            MeetsRequiredMembershipSpecification membershipSpec = new MeetsRequiredMembershipSpecification(MembershipLevel.Premium);
            AndSpecification<User> andSpec = new AndSpecification<User>(pointsSpec, membershipSpec);

            bool result = andSpec.IsSatisfiedBy(visitor);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void AndSpecification_OneSpecificationNotSatisfied_ReturnsFalse()
        {
            User visitor = new User
            {
                Id = Guid.NewGuid(),
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "hashedpassword",
                Score = 300,
                MembershipLevel = MembershipLevel.Premium
            };

            HasSufficientPointsSpecification pointsSpec = new HasSufficientPointsSpecification(500);
            MeetsRequiredMembershipSpecification membershipSpec = new MeetsRequiredMembershipSpecification(MembershipLevel.Premium);
            AndSpecification<User> andSpec = new AndSpecification<User>(pointsSpec, membershipSpec);

            bool result = andSpec.IsSatisfiedBy(visitor);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void AndSpecification_BothSpecificationsNotSatisfied_ReturnsFalse()
        {
            User visitor = new User
            {
                Id = Guid.NewGuid(),
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "hashedpassword",
                Score = 300,
                MembershipLevel = MembershipLevel.Standard
            };

            HasSufficientPointsSpecification pointsSpec = new HasSufficientPointsSpecification(500);
            MeetsRequiredMembershipSpecification membershipSpec = new MeetsRequiredMembershipSpecification(MembershipLevel.Premium);
            AndSpecification<User> andSpec = new AndSpecification<User>(pointsSpec, membershipSpec);

            bool result = andSpec.IsSatisfiedBy(visitor);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void AndSpecification_ChainedSpecifications_ReturnsCorrectResult()
        {
            User visitor = new User
            {
                Id = Guid.NewGuid(),
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "hashedpassword",
                Score = 1000,
                MembershipLevel = MembershipLevel.VIP
            };

            HasSufficientPointsSpecification pointsSpec = new HasSufficientPointsSpecification(500);
            MeetsRequiredMembershipSpecification membershipSpec = new MeetsRequiredMembershipSpecification(MembershipLevel.Premium);
            ISpecification<User> chainedSpec = pointsSpec.And(membershipSpec);

            bool result = chainedSpec.IsSatisfiedBy(visitor);

            Assert.IsTrue(result);
        }
    }
}
