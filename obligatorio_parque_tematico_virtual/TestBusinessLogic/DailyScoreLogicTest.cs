using Moq;
using Domain;
using IBusinessLogic;
using IBusinessLogic.Strategy;
using IDataAccess;
using BusinessLogic;

namespace TestBusinessLogic
{
    [TestClass]
    public class DailyScoreLogicTest
    {
        private DailyScoreLogic _dailyScoreLogic;
        private Mock<IUserRepository> _mockUserRepository;
        private Mock<IActiveStrategy> _mockActiveStrategy;
        private Mock<IScoreHistoryRepository> _mockScoreHistoryRepository;
        private Mock<IConcreteStrategy> _mockConcreteStrategy;

        [TestInitialize]
        public void Setup()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockActiveStrategy = new Mock<IActiveStrategy>();
            _mockScoreHistoryRepository = new Mock<IScoreHistoryRepository>();
            _mockConcreteStrategy = new Mock<IConcreteStrategy>();

            _mockConcreteStrategy.Setup(s => s.Name).Returns("TestStrategy");
            _mockActiveStrategy.Setup(s => s.GetStrategy()).ReturnsAsync(_mockConcreteStrategy.Object);

            _dailyScoreLogic = new DailyScoreLogic(_mockUserRepository.Object, _mockActiveStrategy.Object, _mockScoreHistoryRepository.Object);
        }

        [TestMethod]
        public async Task DateUpdated_ShouldResetScores_WhenDayChanged()
        {
            DateTime previousDate = new DateTime(2025, 9, 1, 23, 59, 0);
            DateTime currentDate = new DateTime(2025, 9, 2, 0, 1, 0);

            Mock<IDateSubject> mockSubject = new Mock<IDateSubject>();
            mockSubject.Setup(s => s.GetPreviousDateTime()).Returns(previousDate);
            mockSubject.Setup(s => s.GetCurrentDateTime()).ReturnsAsync(currentDate);

            await _dailyScoreLogic.DateUpdated(mockSubject.Object);

            _mockUserRepository.Verify(r => r.ResetScores(), Times.Once);
        }

        [TestMethod]
        public async Task DateUpdated_ShouldNotResetScores_WhenDayNotChanged()
        {
            DateTime previousDate = new DateTime(2025, 9, 1, 10, 0, 0);
            DateTime currentDate = new DateTime(2025, 9, 1, 14, 30, 0);

            Mock<IDateSubject> mockSubject = new Mock<IDateSubject>();
            mockSubject.Setup(s => s.GetPreviousDateTime()).Returns(previousDate);
            mockSubject.Setup(s => s.GetCurrentDateTime()).ReturnsAsync(currentDate);

            await _dailyScoreLogic.DateUpdated(mockSubject.Object);

            _mockUserRepository.Verify(r => r.ResetScores(), Times.Never);
        }

        [TestMethod]
        public async Task AddScoreToUser_ShouldCalculateAndAddScore()
        {
            Guid userId = Guid.NewGuid();
            Guid attractionId = Guid.NewGuid();
            int calculatedScore = 50;
            int initialScore = 100;
            int initialDailyScore = 80;

            User user = new User
            {
                Id = userId,
                Score = initialScore,
                DailyScore = initialDailyScore
            };

            Attraction attraction = new Attraction
            {
                Id = attractionId,
                Name = "Roller Coaster"
            };

            _mockActiveStrategy.Setup(s => s.CalculateScore(
                It.IsAny<User>(),
                It.IsAny<Attraction>(),
                It.IsAny<StrategyRequest>()
            )).Returns(calculatedScore);

            DateTime testDate = new DateTime(2025, 9, 1, 12, 0, 0);
            await _dailyScoreLogic.AddScoreToUser(user, attraction, testDate);

            Assert.AreEqual(initialScore + calculatedScore, user.Score, "Score debe incrementarse");
            Assert.AreEqual(initialDailyScore + calculatedScore, user.DailyScore, "DailyScore debe incrementarse");
            _mockUserRepository.Verify(r => r.Update(user), Times.Once);
        }

        [TestMethod]
        public async Task AddScoreToUser_ShouldPassCorrectStrategyRequest()
        {
            Guid userId = Guid.NewGuid();
            Guid attractionId = Guid.NewGuid();
            bool isSpecialEvent = true;

            User user = new User { Id = userId, Score = 0 };
            Attraction attraction = new Attraction { Id = attractionId };
            Event attractionEvent = new Event();

            StrategyRequest capturedRequest = null;

            _mockActiveStrategy.Setup(s => s.CalculateScore(
                It.IsAny<User>(),
                It.IsAny<Attraction>(),
                It.IsAny<StrategyRequest>()
            )).Callback<User, Attraction, StrategyRequest>((u, a, sr) => capturedRequest = sr)
              .Returns(10);

            DateTime testDate = new DateTime(2025, 9, 1, 12, 0, 0);
            await _dailyScoreLogic.AddScoreToUser(user, attraction, testDate, attractionEvent);

            Assert.AreEqual(userId, capturedRequest.UserId);
            Assert.AreEqual(attractionId, capturedRequest.AttractionId);
            Assert.AreEqual(isSpecialEvent, capturedRequest.IsSpecialEvent);
        }

        [TestMethod]
        public async Task AddScoreToUser_WithSpecialEvent_ShouldSetIsSpecialEventFlag()
        {
            User user = new User { Id = Guid.NewGuid(), Score = 0 };
            Attraction attraction = new Attraction { Id = Guid.NewGuid() };
            Event attractionEvent = new Event();

            StrategyRequest capturedRequest = null;

            _mockActiveStrategy.Setup(s => s.CalculateScore(
                It.IsAny<User>(),
                It.IsAny<Attraction>(),
                It.IsAny<StrategyRequest>()
            )).Callback<User, Attraction, StrategyRequest>((u, a, sr) => capturedRequest = sr)
              .Returns(20);

            DateTime testDate = new DateTime(2025, 9, 1, 12, 0, 0);
            await _dailyScoreLogic.AddScoreToUser(user, attraction, testDate, attractionEvent);

            Assert.IsTrue(capturedRequest.IsSpecialEvent);
        }

        [TestMethod]
        public async Task AddScoreToUser_WithoutSpecialEvent_ShouldNotSetIsSpecialEventFlag()
        {
            User user = new User { Id = Guid.NewGuid(), Score = 0 };
            Attraction attraction = new Attraction { Id = Guid.NewGuid() };

            StrategyRequest capturedRequest = null;

            _mockActiveStrategy.Setup(s => s.CalculateScore(
                It.IsAny<User>(),
                It.IsAny<Attraction>(),
                It.IsAny<StrategyRequest>()
            )).Callback<User, Attraction, StrategyRequest>((u, a, sr) => capturedRequest = sr)
              .Returns(15);

            DateTime testDate = new DateTime(2025, 9, 1, 12, 0, 0);
            await _dailyScoreLogic.AddScoreToUser(user, attraction, testDate);

            Assert.IsFalse(capturedRequest.IsSpecialEvent);
        }

        [TestMethod]
        public async Task AddScoreToUser_ShouldCallActiveStrategyCalculateScore()
        {
            User user = new User { Id = Guid.NewGuid(), Score = 0 };
            Attraction attraction = new Attraction { Id = Guid.NewGuid() };

            _mockActiveStrategy.Setup(s => s.CalculateScore(
                It.IsAny<User>(),
                It.IsAny<Attraction>(),
                It.IsAny<StrategyRequest>()
            )).Returns(25);

            DateTime testDate = new DateTime(2025, 9, 1, 12, 0, 0);
            await _dailyScoreLogic.AddScoreToUser(user, attraction, testDate);

            _mockActiveStrategy.Verify(s => s.CalculateScore(
                user,
                attraction,
                It.IsAny<StrategyRequest>()
            ), Times.Once);
        }
    }
}

