using BusinessLogic;
using Domain;
using IBusinessLogic;
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
        private Mock<IDateTimeLogic> _mockDateTimeLogic;
        private Mock<IScoreHistoryRepository> _mockScoreHistoryRepository;
        private RedemptionLogic _redemptionLogic;

        [TestInitialize]
        public void Setup()
        {
            _mockRewardRepository = new Mock<IRewardRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockRedemptionHistoryRepository = new Mock<IRedemptionHistoryRepository>();
            _mockDateTimeLogic = new Mock<IDateTimeLogic>();
            _mockScoreHistoryRepository = new Mock<IScoreHistoryRepository>();
            _mockDateTimeLogic.Setup(x => x.GetCurrentDateTime()).ReturnsAsync(DateTime.Now);

            _redemptionLogic = new RedemptionLogic(
                _mockRewardRepository.Object,
                _mockUserRepository.Object,
                _mockRedemptionHistoryRepository.Object,
                _mockDateTimeLogic.Object,
                _mockScoreHistoryRepository.Object
            );
        }

        private void SetupSuccessfulRedemption(User visitor, Reward reward, DateTime? testDateTime = null)
        {
            _mockUserRepository.Setup(r => r.GetById(visitor.Id)).ReturnsAsync(visitor);
            _mockRewardRepository.Setup(r => r.GetByIdAsync(reward.Id)).ReturnsAsync(reward);
            
            if (testDateTime.HasValue)
            {
                _mockDateTimeLogic.Setup(x => x.GetCurrentDateTime()).ReturnsAsync(testDateTime.Value);
            }
            
            _mockRedemptionHistoryRepository.Setup(r => r.CreateAsync(It.IsAny<RedemptionHistory>()))
                .Returns(Task.CompletedTask);
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>())).Returns(Task.CompletedTask);
            _mockRewardRepository.Setup(r => r.UpdateAsync(It.IsAny<Reward>())).Returns(Task.CompletedTask);
        }        

        [TestMethod]
        public async Task RedeemReward_ValidRedemption_Success()
        {
            User visitor = new User
            {
                Id = Guid.NewGuid(),
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "hashedpassword",
                Score = 1000,
                DailyScore = 800,
                MembershipLevel = MembershipLevel.Premium
            };

            Reward reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "VIP Access",
                Description = "Get VIP access for a day",
                PointsCost = 500,
                AvailableQuantity = 10,
                RequiredMembershipLevel = MembershipLevel.Premium
            };

            SetupSuccessfulRedemption(visitor, reward);

            RedemptionHistoryModelOut redemption = await _redemptionLogic.RedeemReward(visitor.Id, reward.Id);

            Assert.AreEqual(visitor.Id, redemption.VisitorId);
            Assert.AreEqual(reward.Id, redemption.RewardId);
            Assert.AreEqual(500, redemption.PointsSpent);

            _mockRedemptionHistoryRepository.Verify(r => r.CreateAsync(It.IsAny<RedemptionHistory>()), Times.Once);
            _mockUserRepository.Verify(r => r.Update(It.Is<User>(u => u.Score == 500 && u.DailyScore == 800)), Times.Once);
            _mockRewardRepository.Verify(r => r.UpdateAsync(It.Is<Reward>(rw => rw.AvailableQuantity == 9)), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task RedeemReward_VisitorNotFound_ThrowsException()
        {
            Guid visitorId = Guid.NewGuid();
            Guid rewardId = Guid.NewGuid();

            _mockUserRepository.Setup(r => r.GetById(visitorId)).ReturnsAsync((User?)null);

            await _redemptionLogic.RedeemReward(visitorId, rewardId);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task RedeemReward_RewardNotFound_ThrowsException()
        {
            User visitor = new User
            {
                Id = Guid.NewGuid(),
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "hashedpassword",
                Score = 1000,
                DailyScore = 500
            };

            Guid rewardId = Guid.NewGuid();

            _mockUserRepository.Setup(r => r.GetById(visitor.Id)).ReturnsAsync(visitor);
            _mockRewardRepository.Setup(r => r.GetByIdAsync(rewardId)).ReturnsAsync((Reward?)null);

            await _redemptionLogic.RedeemReward(visitor.Id, rewardId);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task RedeemReward_InsufficientPoints_ThrowsException()
        {
            User visitor = new User
            {
                Id = Guid.NewGuid(),
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "hashedpassword",
                DailyScore = 200,
                Score = 300,
                MembershipLevel = MembershipLevel.Premium
            };

            Reward reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "VIP Access",
                Description = "Get VIP access for a day",
                PointsCost = 500,
                AvailableQuantity = 10,
                RequiredMembershipLevel = MembershipLevel.Premium
            };

            _mockUserRepository.Setup(r => r.GetById(visitor.Id)).ReturnsAsync(visitor);
            _mockRewardRepository.Setup(r => r.GetByIdAsync(reward.Id)).ReturnsAsync(reward);

            await _redemptionLogic.RedeemReward(visitor.Id, reward.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task RedeemReward_RewardNotAvailable_ThrowsException()
        {
            User visitor = new User
            {
                Id = Guid.NewGuid(),
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                DailyScore = 700,
                Password = "hashedpassword",
                Score = 1000,
                MembershipLevel = MembershipLevel.Premium
            };

            Reward reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "VIP Access",
                Description = "Get VIP access for a day",
                PointsCost = 500,
                AvailableQuantity = 0,
                RequiredMembershipLevel = MembershipLevel.Premium
            };

            _mockUserRepository.Setup(r => r.GetById(visitor.Id)).ReturnsAsync(visitor);
            _mockRewardRepository.Setup(r => r.GetByIdAsync(reward.Id)).ReturnsAsync(reward);

            await _redemptionLogic.RedeemReward(visitor.Id, reward.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task RedeemReward_InsufficientMembershipLevel_ThrowsException()
        {
            User visitor = new User
            {
                Id = Guid.NewGuid(),
                Name = "John",
                LastName = "Doe",
                DailyScore = 600,
                Email = "john@test.com",
                Password = "hashedpassword",
                Score = 1000,
                MembershipLevel = MembershipLevel.Standard
            };

            Reward reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "VIP Access",
                Description = "Get VIP access for a day",
                PointsCost = 500,
                AvailableQuantity = 10,
                RequiredMembershipLevel = MembershipLevel.Premium
            };

            _mockUserRepository.Setup(r => r.GetById(visitor.Id)).ReturnsAsync(visitor);
            _mockRewardRepository.Setup(r => r.GetByIdAsync(reward.Id)).ReturnsAsync(reward);

            RedemptionHistoryModelOut redemption = await _redemptionLogic.RedeemReward(visitor.Id, reward.Id);
        }

        [TestMethod]
        public async Task RedeemReward_NoMembershipRequired_Success()
        {
            User visitor = new User
            {
                Id = Guid.NewGuid(),
                Name = "John",
                LastName = "Doe",
                DailyScore = 150,
                Email = "john@test.com",
                Password = "hashedpassword",
                Score = 200,
                MembershipLevel = MembershipLevel.Standard
            };

            Reward reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Free Drink",
                Description = "Get a free drink",
                PointsCost = 100,
                AvailableQuantity = 50,
                RequiredMembershipLevel = null
            };

            SetupSuccessfulRedemption(visitor, reward);

            RedemptionHistoryModelOut redemption = await _redemptionLogic.RedeemReward(visitor.Id, reward.Id);

            Assert.IsNotNull(redemption);
        }

        [TestMethod]
        public async Task RedeemReward_ExactPoints_Success()
        {
            User visitor = new User
            {
                Id = Guid.NewGuid(),
                Name = "John",
                LastName = "Doe",
                DailyScore = 300,
                Email = "john@test.com",
                Password = "hashedpassword",
                Score = 500,
                MembershipLevel = MembershipLevel.Premium
            };

            Reward reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "Reward",
                Description = "Test",
                PointsCost = 500,
                AvailableQuantity = 1,
                RequiredMembershipLevel = MembershipLevel.Premium
            };

            SetupSuccessfulRedemption(visitor, reward);

            RedemptionHistoryModelOut redemption = await _redemptionLogic.RedeemReward(visitor.Id, reward.Id);

            Assert.IsNotNull(redemption);
        }

        [TestMethod]
        public async Task GetRedemptionHistory_ReturnsVisitorHistory()
        {
            Guid visitorId = Guid.NewGuid();
            List<RedemptionHistory> history = new List<RedemptionHistory>
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

            _mockRedemptionHistoryRepository.Setup(r => r.GetByVisitorIdAsync(visitorId)).ReturnsAsync(history);

            List<RedemptionHistoryModelOut> result = await _redemptionLogic.GetRedemptionHistory(visitorId);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(visitorId, result[0].VisitorId);
            _mockRedemptionHistoryRepository.Verify(r => r.GetByVisitorIdAsync(visitorId), Times.Once);
        }

        [TestMethod]
        public async Task GetRedemptionHistoryWithDateRange_FiltersCorrectly()
        {
            Guid visitorId = Guid.NewGuid();
            DateTime dateFrom = DateTime.Now.AddDays(-7);
            DateTime dateTo = DateTime.Now;
            List<RedemptionHistory> history = new List<RedemptionHistory>
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

            _mockRedemptionHistoryRepository.Setup(r => r.GetByVisitorIdWithDateRangeAsync(visitorId, dateFrom, dateTo))
            .ReturnsAsync(history);

            List<RedemptionHistoryModelOut> result =
            await _redemptionLogic.GetRedemptionHistoryWithDateRange(visitorId, dateFrom, dateTo);

            Assert.AreEqual(1, result.Count);
            _mockRedemptionHistoryRepository.Verify(
                r => r.GetByVisitorIdWithDateRangeAsync(visitorId, dateFrom, dateTo),
                Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task RedeemReward_WhenVisitorMembershipLevelIsNull_ThrowsInvalidOperationException()
        {
            Guid visitorId = Guid.NewGuid();
            Guid rewardId = Guid.NewGuid();

            User visitor = new User
            {
                Id = visitorId,
                Name = "John",
                LastName = "Doe",
                DailyScore = 400,
                Email = "john@test.com",
                Password = "pass",
                Score = 1000,
                MembershipLevel = null
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
            _mockRewardRepository.Setup(r => r.GetByIdAsync(rewardId)).ReturnsAsync(reward);

            await _redemptionLogic.RedeemReward(visitorId, rewardId);
        }

        [TestMethod]
        public async Task RedeemReward_CreatesScoreHistoryWithNegativePoints()
        {
            DateTime testDateTime = new DateTime(2025, 11, 7, 10, 0, 0);
            User visitor = new User
            {
                Id = Guid.NewGuid(),
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                DailyScore = 750,
                Password = "hashedpassword",
                Score = 1000,
                MembershipLevel = MembershipLevel.Premium
            };

            Reward reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = "VIP Access",
                Description = "Get VIP access for a day",
                PointsCost = 500,
                AvailableQuantity = 10,
                RequiredMembershipLevel = MembershipLevel.Premium
            };

            ScoreHistory? capturedScoreHistory = null;
            SetupSuccessfulRedemption(visitor, reward, testDateTime);
            _mockScoreHistoryRepository.Setup(r => r.CreateAsync(It.IsAny<ScoreHistory>()))
                .Callback<ScoreHistory>(sh => capturedScoreHistory = sh)
                .Returns(Task.CompletedTask);

            RedemptionHistoryModelOut result = await _redemptionLogic.RedeemReward(visitor.Id, reward.Id);

            _mockScoreHistoryRepository.Verify(r => r.CreateAsync(It.IsAny<ScoreHistory>()), Times.Once);
            Assert.AreEqual(visitor.Id, capturedScoreHistory.VisitorId);
            Assert.AreEqual(-500, capturedScoreHistory.Points,
                "Los puntos deben ser negativos al canjear una recompensa");
            Assert.AreEqual(ScoreOrigin.Redemption, capturedScoreHistory.Origin);
            Assert.AreEqual(reward.Id, capturedScoreHistory.RelatedEntityId);
        }
    }
}