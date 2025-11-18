using Moq;
using Domain;
using IBusinessLogic;
using IDataAccess;
using BusinessLogic;
using Models.In;

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
            _mockUserRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            _mockActiveStrategy = new Mock<IActiveStrategy>(MockBehavior.Strict);
            _mockScoreHistoryRepository = new Mock<IScoreHistoryRepository>(MockBehavior.Strict);
            _mockConcreteStrategy = new Mock<IConcreteStrategy>(MockBehavior.Strict);

            _mockConcreteStrategy.Setup(s => s.Name).Returns("TestStrategy");
            _mockActiveStrategy.Setup(s => s.GetStrategy()).Returns(_mockConcreteStrategy.Object);

            _dailyScoreLogic = new DailyScoreLogic(_mockUserRepository.Object, _mockActiveStrategy.Object,
                _mockScoreHistoryRepository.Object);
        }

        [TestMethod]
        public void DateUpdated_ShouldResetScores_WhenDayChanged()
        {
            DateTime previousDate = new DateTime(2025, 9, 1, 23, 59, 0);
            DateTime currentDate = new DateTime(2025, 9, 2, 0, 1, 0);

            Mock<IDateSubject> mockSubject = new Mock<IDateSubject>(MockBehavior.Strict);
            mockSubject.Setup(s => s.GetPreviousDateTime()).Returns(previousDate);
            mockSubject.Setup(s => s.GetCurrentDateTime()).Returns(currentDate);
            _mockUserRepository.Setup(r => r.ResetScores());

            _dailyScoreLogic.DateUpdated(mockSubject.Object);

            _mockUserRepository.Verify(r => r.ResetScores(), Times.Once);
            mockSubject.Verify(s => s.GetPreviousDateTime(), Times.Once);
            mockSubject.Verify(s => s.GetCurrentDateTime(), Times.Once);
        }

        [TestMethod]
        public void DateUpdated_ShouldNotResetScores_WhenDayNotChanged()
        {
            DateTime previousDate = new DateTime(2025, 9, 1, 10, 0, 0);
            DateTime currentDate = new DateTime(2025, 9, 1, 14, 30, 0);

            Mock<IDateSubject> mockSubject = new Mock<IDateSubject>(MockBehavior.Strict);
            mockSubject.Setup(s => s.GetPreviousDateTime()).Returns(previousDate);
            mockSubject.Setup(s => s.GetCurrentDateTime()).Returns(currentDate);

            _dailyScoreLogic.DateUpdated(mockSubject.Object);

            _mockUserRepository.Verify(r => r.ResetScores(), Times.Never);
            mockSubject.Verify(s => s.GetPreviousDateTime(), Times.Once);
            mockSubject.Verify(s => s.GetCurrentDateTime(), Times.Once);
        }

        [TestMethod]
        public void AddScoreToUser_ShouldIncrementScore()
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
            _mockUserRepository.Setup(r => r.Update(user));
            _mockScoreHistoryRepository.Setup(r => r.Create(It.IsAny<ScoreHistory>()));

            DateTime testDate = new DateTime(2025, 9, 1, 12, 0, 0);
            _dailyScoreLogic.AddScoreToUser(user, attraction, testDate);

            Assert.AreEqual(initialScore + calculatedScore, user.Score);
            _mockActiveStrategy.Verify(s => s.CalculateScore(
                It.IsAny<User>(),
                It.IsAny<Attraction>(),
                It.IsAny<StrategyRequest>()
            ), Times.Once);
            _mockUserRepository.Verify(r => r.Update(user), Times.Once);
            _mockScoreHistoryRepository.Verify(r => r.Create(It.IsAny<ScoreHistory>()), Times.Once);
        }

        [TestMethod]
        public void AddScoreToUser_ShouldIncrementDailyScore()
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
            _mockUserRepository.Setup(r => r.Update(user));
            _mockScoreHistoryRepository.Setup(r => r.Create(It.IsAny<ScoreHistory>()));

            DateTime testDate = new DateTime(2025, 9, 1, 12, 0, 0);
            _dailyScoreLogic.AddScoreToUser(user, attraction, testDate);

            Assert.AreEqual(initialDailyScore + calculatedScore, user.DailyScore);
            _mockActiveStrategy.Verify(s => s.CalculateScore(
                It.IsAny<User>(),
                It.IsAny<Attraction>(),
                It.IsAny<StrategyRequest>()
            ), Times.Once);
            _mockUserRepository.Verify(r => r.Update(user), Times.Once);
            _mockScoreHistoryRepository.Verify(r => r.Create(It.IsAny<ScoreHistory>()), Times.Once);
        }

        [TestMethod]
        public void AddScoreToUser_ShouldPassCorrectUserId()
        {
            Guid userId = Guid.NewGuid();
            Guid attractionId = Guid.NewGuid();

            User user = new User { Id = userId, Score = 0 };
            Attraction attraction = new Attraction { Id = attractionId };
            Event attractionEvent = new Event();

            StrategyRequest? capturedRequest = null;

            _mockActiveStrategy.Setup(s => s.CalculateScore(
                It.IsAny<User>(),
                It.IsAny<Attraction>(),
                It.IsAny<StrategyRequest>()
            )).Callback<User, Attraction, StrategyRequest>((u, a, sr) => capturedRequest = sr)
            .Returns(10);
            _mockUserRepository.Setup(r => r.Update(user));
            _mockScoreHistoryRepository.Setup(r => r.Create(It.IsAny<ScoreHistory>()));

            DateTime testDate = new DateTime(2025, 9, 1, 12, 0, 0);
            _dailyScoreLogic.AddScoreToUser(user, attraction, testDate, attractionEvent);

            Assert.AreEqual(userId, capturedRequest!.UserId);
            _mockActiveStrategy.Verify(s => s.CalculateScore(
                It.IsAny<User>(),
                It.IsAny<Attraction>(),
                It.IsAny<StrategyRequest>()
            ), Times.Once);
            _mockUserRepository.Verify(r => r.Update(user), Times.Once);
            _mockScoreHistoryRepository.Verify(r => r.Create(It.IsAny<ScoreHistory>()), Times.Once);
        }

        [TestMethod]
        public void AddScoreToUser_ShouldPassCorrectAttractionId()
        {
            Guid userId = Guid.NewGuid();
            Guid attractionId = Guid.NewGuid();

            User user = new User { Id = userId, Score = 0 };
            Attraction attraction = new Attraction { Id = attractionId };
            Event attractionEvent = new Event();

            StrategyRequest? capturedRequest = null;

            _mockActiveStrategy.Setup(s => s.CalculateScore(
                It.IsAny<User>(),
                It.IsAny<Attraction>(),
                It.IsAny<StrategyRequest>()
            )).Callback<User, Attraction, StrategyRequest>((u, a, sr) => capturedRequest = sr)
            .Returns(10);
            _mockUserRepository.Setup(r => r.Update(user));
            _mockScoreHistoryRepository.Setup(r => r.Create(It.IsAny<ScoreHistory>()));

            DateTime testDate = new DateTime(2025, 9, 1, 12, 0, 0);
            _dailyScoreLogic.AddScoreToUser(user, attraction, testDate, attractionEvent);

            Assert.AreEqual(attractionId, capturedRequest!.AttractionId);
            _mockActiveStrategy.Verify(s => s.CalculateScore(
                It.IsAny<User>(),
                It.IsAny<Attraction>(),
                It.IsAny<StrategyRequest>()
            ), Times.Once);
            _mockUserRepository.Verify(r => r.Update(user), Times.Once);
            _mockScoreHistoryRepository.Verify(r => r.Create(It.IsAny<ScoreHistory>()), Times.Once);
        }

        [TestMethod]
        public void AddScoreToUser_WithSpecialEvent_ShouldSetIsSpecialEventFlag()
        {
            User user = new User { Id = Guid.NewGuid(), Score = 0 };
            Attraction attraction = new Attraction { Id = Guid.NewGuid() };
            Event attractionEvent = new Event();

            StrategyRequest? capturedRequest = null;

            _mockActiveStrategy.Setup(s => s.CalculateScore(
                It.IsAny<User>(),
                It.IsAny<Attraction>(),
                It.IsAny<StrategyRequest>()
            )).Callback<User, Attraction, StrategyRequest>((u, a, sr) => capturedRequest = sr)
            .Returns(20);
            _mockUserRepository.Setup(r => r.Update(user));
            _mockScoreHistoryRepository.Setup(r => r.Create(It.IsAny<ScoreHistory>()));

            DateTime testDate = new DateTime(2025, 9, 1, 12, 0, 0);
            _dailyScoreLogic.AddScoreToUser(user, attraction, testDate, attractionEvent);

            Assert.IsTrue(capturedRequest!.IsSpecialEvent);
            _mockActiveStrategy.Verify(s => s.CalculateScore(
                It.IsAny<User>(),
                It.IsAny<Attraction>(),
                It.IsAny<StrategyRequest>()
            ), Times.Once);
            _mockUserRepository.Verify(r => r.Update(user), Times.Once);
            _mockScoreHistoryRepository.Verify(r => r.Create(It.IsAny<ScoreHistory>()), Times.Once);
        }

        [TestMethod]
        public void AddScoreToUser_WithoutSpecialEvent_ShouldNotSetIsSpecialEventFlag()
        {
            User user = new User { Id = Guid.NewGuid(), Score = 0 };
            Attraction attraction = new Attraction { Id = Guid.NewGuid() };

            StrategyRequest? capturedRequest = null;

            _mockActiveStrategy.Setup(s => s.CalculateScore(
                It.IsAny<User>(),
                It.IsAny<Attraction>(),
                It.IsAny<StrategyRequest>()
            )).Callback<User, Attraction, StrategyRequest>((u, a, sr) => capturedRequest = sr)
            .Returns(15);
            _mockUserRepository.Setup(r => r.Update(user));
            _mockScoreHistoryRepository.Setup(r => r.Create(It.IsAny<ScoreHistory>()));

            DateTime testDate = new DateTime(2025, 9, 1, 12, 0, 0);
            _dailyScoreLogic.AddScoreToUser(user, attraction, testDate);

            Assert.IsFalse(capturedRequest!.IsSpecialEvent);
            _mockActiveStrategy.Verify(s => s.CalculateScore(
                It.IsAny<User>(),
                It.IsAny<Attraction>(),
                It.IsAny<StrategyRequest>()
            ), Times.Once);
            _mockUserRepository.Verify(r => r.Update(user), Times.Once);
            _mockScoreHistoryRepository.Verify(r => r.Create(It.IsAny<ScoreHistory>()), Times.Once);
        }

        [TestMethod]
        public void AddScoreToUser_ShouldCallActiveStrategyCalculateScore()
        {
            User user = new User { Id = Guid.NewGuid(), Score = 0 };
            Attraction attraction = new Attraction { Id = Guid.NewGuid() };

            _mockActiveStrategy.Setup(s => s.CalculateScore(
                It.IsAny<User>(),
                It.IsAny<Attraction>(),
                It.IsAny<StrategyRequest>()
            )).Returns(25);
            _mockUserRepository.Setup(r => r.Update(user));
            _mockScoreHistoryRepository.Setup(r => r.Create(It.IsAny<ScoreHistory>()));

            DateTime testDate = new DateTime(2025, 9, 1, 12, 0, 0);
            _dailyScoreLogic.AddScoreToUser(user, attraction, testDate);

            _mockActiveStrategy.Verify(s => s.CalculateScore(
                user,
                attraction,
                It.IsAny<StrategyRequest>()
            ), Times.Once);
        }

        [TestMethod]
        public void AddScoreToUser_ShouldCreateScoreHistory_WithAttractionName()
        {
            Guid userId = Guid.NewGuid();
            Guid attractionId = Guid.NewGuid();
            int calculatedScore = 50;

            User user = new User
            {
                Id = userId,
                Score = 100,
                DailyScore = 80
            };

            Attraction attraction = new Attraction
            {
                Id = attractionId,
                Name = "Roller Coaster"
            };

            ScoreHistory capturedHistory = null;

            _mockActiveStrategy.Setup(s => s.CalculateScore(
                It.IsAny<User>(),
                It.IsAny<Attraction>(),
                It.IsAny<StrategyRequest>()
            )).Returns(calculatedScore);
            _mockUserRepository.Setup(r => r.Update(user));
            _mockScoreHistoryRepository.Setup(r => r.Create(It.IsAny<ScoreHistory>()))
                .Callback<ScoreHistory>(h => capturedHistory = h);

            DateTime testDate = new DateTime(2025, 9, 1, 12, 0, 0);
            _dailyScoreLogic.AddScoreToUser(user, attraction, testDate);

            Assert.AreEqual("Roller Coaster", capturedHistory.RelatedEntityName);
            Assert.AreEqual(ScoreOrigin.AttractionVisit, capturedHistory.Origin);
        }

        [TestMethod]
        public void AddScoreToUser_WithEvent_ShouldCreateScoreHistory_WithEventName()
        {
            Guid userId = Guid.NewGuid();
            Guid eventId = Guid.NewGuid();
            int calculatedScore = 100;

            User user = new User
            {
                Id = userId,
                Score = 100,
                DailyScore = 80
            };

            Attraction attraction = new Attraction
            {
                Id = Guid.NewGuid(),
                Name = "Concert"
            };

            Event attractionEvent = new Event
            {
                Id = eventId,
                Name = "Summer Festival 2025"
            };

            ScoreHistory capturedHistory = null;

            _mockActiveStrategy.Setup(s => s.CalculateScore(
                It.IsAny<User>(),
                It.IsAny<Attraction>(),
                It.IsAny<StrategyRequest>()
            )).Returns(calculatedScore);
            _mockUserRepository.Setup(r => r.Update(user));
            _mockScoreHistoryRepository.Setup(r => r.Create(It.IsAny<ScoreHistory>()))
                .Callback<ScoreHistory>(h => capturedHistory = h);

            DateTime testDate = new DateTime(2025, 9, 1, 12, 0, 0);
            _dailyScoreLogic.AddScoreToUser(user, attraction, testDate, attractionEvent);

            Assert.AreEqual("Summer Festival 2025", capturedHistory.RelatedEntityName);
            Assert.AreEqual(ScoreOrigin.EventParticipation, capturedHistory.Origin);
        }
    }
}