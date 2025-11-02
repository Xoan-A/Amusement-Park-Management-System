using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using BusinessLogic;
using IDataAccess;
using Domain;
using Models.Out;

namespace TestBusinessLogic
{
    [TestClass]
    public class ScoreHistoryLogicTest
    {
        private Mock<IScoreHistoryRepository> _mockRepository = null!;
        private ScoreHistoryLogic _scoreHistoryLogic = null!;
        private Guid _visitorId;

        [TestInitialize]
        public void Setup()
        {
            _mockRepository = new Mock<IScoreHistoryRepository>();
            _scoreHistoryLogic = new ScoreHistoryLogic(_mockRepository.Object);
            _visitorId = Guid.NewGuid();
        }

        [TestMethod]
        public void GetMyScoreHistory_WithVisitorId_ReturnsScoreHistory()
        {
            // Arrange
            var histories = new List<ScoreHistory>
            {
                new ScoreHistory
                {
                    Id = Guid.NewGuid(),
                    VisitorId = _visitorId,
                    Visitor = new User
                    {
                        Id = _visitorId,
                        Name = "John",
                        LastName = "Doe",
                        Email = "john@test.com",
                        Password = "pass",
                        BirthDate = DateTime.Now.AddYears(-25)
                    },
                    Points = 100,
                    Origin = ScoreOrigin.AttractionVisit,
                    Description = "Visited Roller Coaster",
                    StrategyName = "PerAttraction",
                    CreatedAt = DateTime.UtcNow
                }
            };

            _mockRepository.Setup(r => r.GetByVisitor(_visitorId)).Returns(histories);

            // Act
            var result = _scoreHistoryLogic.GetMyScoreHistory(_visitorId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(_visitorId, result[0].VisitorId);
            Assert.AreEqual("John", result[0].VisitorName);
            Assert.AreEqual(100, result[0].Points);
            Assert.AreEqual("AttractionVisit", result[0].Origin);
            Assert.AreEqual("PerAttraction", result[0].StrategyName);
            _mockRepository.Verify(r => r.GetByVisitor(_visitorId), Times.Once);
        }

        [TestMethod]
        public void GetVisitorScoreHistory_WithDateRange_FiltersCorrectly()
        {
            // Arrange
            var dateFrom = DateTime.UtcNow.AddDays(-7);
            var dateTo = DateTime.UtcNow;
            var histories = new List<ScoreHistory>
            {
                new ScoreHistory
                {
                    Id = Guid.NewGuid(),
                    VisitorId = _visitorId,
                    Visitor = new User
                    {
                        Id = _visitorId,
                        Name = "Jane",
                        LastName = "Smith",
                        Email = "jane@test.com",
                        Password = "pass",
                        BirthDate = DateTime.Now.AddYears(-30)
                    },
                    Points = 50,
                    Origin = ScoreOrigin.EventParticipation,
                    Description = "Participated in event",
                    StrategyName = "PerEvent",
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                }
            };

            _mockRepository.Setup(r => r.GetByVisitorAndDateRange(_visitorId, dateFrom, dateTo))
                .Returns(histories);

            // Act
            var result = _scoreHistoryLogic.GetVisitorScoreHistory(_visitorId, dateFrom, dateTo);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(_visitorId, result[0].VisitorId);
            Assert.AreEqual("Jane", result[0].VisitorName);
            Assert.AreEqual(50, result[0].Points);
            Assert.AreEqual("EventParticipation", result[0].Origin);
            _mockRepository.Verify(r => r.GetByVisitorAndDateRange(_visitorId, dateFrom, dateTo), Times.Once);
            _mockRepository.Verify(r => r.GetByVisitor(It.IsAny<Guid>()), Times.Never);
        }

        [TestMethod]
        public void GetVisitorScoreHistory_WithoutDateRange_ReturnsAll()
        {
            // Arrange
            var histories = new List<ScoreHistory>
            {
                new ScoreHistory
                {
                    Id = Guid.NewGuid(),
                    VisitorId = _visitorId,
                    Visitor = new User
                    {
                        Id = _visitorId,
                        Name = "Bob",
                        LastName = "Johnson",
                        Email = "bob@test.com",
                        Password = "pass",
                        BirthDate = DateTime.Now.AddYears(-20)
                    },
                    Points = 75,
                    Origin = ScoreOrigin.AttractionVisit,
                    Description = "Visited Water Slide",
                    StrategyName = "PerAttraction",
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                },
                new ScoreHistory
                {
                    Id = Guid.NewGuid(),
                    VisitorId = _visitorId,
                    Visitor = new User
                    {
                        Id = _visitorId,
                        Name = "Bob",
                        LastName = "Johnson",
                        Email = "bob@test.com",
                        Password = "pass",
                        BirthDate = DateTime.Now.AddYears(-20)
                    },
                    Points = 25,
                    Origin = ScoreOrigin.EventParticipation,
                    Description = "Participated in contest",
                    StrategyName = "PerEvent",
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                }
            };

            _mockRepository.Setup(r => r.GetByVisitor(_visitorId)).Returns(histories);

            // Act
            var result = _scoreHistoryLogic.GetVisitorScoreHistory(_visitorId, null, null);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(_visitorId, result[0].VisitorId);
            Assert.AreEqual("Bob", result[0].VisitorName);
            _mockRepository.Verify(r => r.GetByVisitor(_visitorId), Times.Once);
            _mockRepository.Verify(r => r.GetByVisitorAndDateRange(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
        }

        [TestMethod]
        public void GetAllScoreHistory_ReturnsAllHistoryWithVisitorNames()
        {
            // Arrange
            var visitor1Id = Guid.NewGuid();
            var visitor2Id = Guid.NewGuid();
            var histories = new List<ScoreHistory>
            {
                new ScoreHistory
                {
                    Id = Guid.NewGuid(),
                    VisitorId = visitor1Id,
                    Visitor = new User
                    {
                        Id = visitor1Id,
                        Name = "Alice",
                        LastName = "Williams",
                        Email = "alice@test.com",
                        Password = "pass",
                        BirthDate = DateTime.Now.AddYears(-28)
                    },
                    Points = 150,
                    Origin = ScoreOrigin.AttractionVisit,
                    Description = "Visited Ferris Wheel",
                    StrategyName = "PerAttraction",
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new ScoreHistory
                {
                    Id = Guid.NewGuid(),
                    VisitorId = visitor2Id,
                    Visitor = new User
                    {
                        Id = visitor2Id,
                        Name = "Charlie",
                        LastName = "Brown",
                        Email = "charlie@test.com",
                        Password = "pass",
                        BirthDate = DateTime.Now.AddYears(-35)
                    },
                    Points = 200,
                    Origin = ScoreOrigin.EventParticipation,
                    Description = "Won grand prize",
                    StrategyName = "PerEvent",
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                }
            };

            _mockRepository.Setup(r => r.GetAll()).Returns(histories);

            // Act
            var result = _scoreHistoryLogic.GetAllScoreHistory();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);

            var aliceHistory = result.FirstOrDefault(h => h.VisitorName == "Alice");
            Assert.IsNotNull(aliceHistory);
            Assert.AreEqual(150, aliceHistory.Points);
            Assert.AreEqual("AttractionVisit", aliceHistory.Origin);

            var charlieHistory = result.FirstOrDefault(h => h.VisitorName == "Charlie");
            Assert.IsNotNull(charlieHistory);
            Assert.AreEqual(200, charlieHistory.Points);
            Assert.AreEqual("EventParticipation", charlieHistory.Origin);

            _mockRepository.Verify(r => r.GetAll(), Times.Once);
        }

        [TestMethod]
        public void GetVisitorScoreHistory_WithOnlyDateFrom_ReturnsAll()
        {
            // Arrange
            var dateFrom = DateTime.UtcNow.AddDays(-7);
            var histories = new List<ScoreHistory>
            {
                new ScoreHistory
                {
                    Id = Guid.NewGuid(),
                    VisitorId = _visitorId,
                    Visitor = new User
                    {
                        Id = _visitorId,
                        Name = "Test",
                        LastName = "User",
                        Email = "test@test.com",
                        Password = "pass",
                        BirthDate = DateTime.Now.AddYears(-25)
                    },
                    Points = 60,
                    Origin = ScoreOrigin.AttractionVisit,
                    Description = "Test entry",
                    StrategyName = "PerAttraction",
                    CreatedAt = DateTime.UtcNow
                }
            };

            _mockRepository.Setup(r => r.GetByVisitor(_visitorId)).Returns(histories);

            // Act
            var result = _scoreHistoryLogic.GetVisitorScoreHistory(_visitorId, dateFrom, null);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            _mockRepository.Verify(r => r.GetByVisitor(_visitorId), Times.Once);
            _mockRepository.Verify(r => r.GetByVisitorAndDateRange(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
        }

        [TestMethod]
        public void GetVisitorScoreHistory_WithOnlyDateTo_ReturnsAll()
        {
            // Arrange
            var dateTo = DateTime.UtcNow;
            var histories = new List<ScoreHistory>
            {
                new ScoreHistory
                {
                    Id = Guid.NewGuid(),
                    VisitorId = _visitorId,
                    Visitor = new User
                    {
                        Id = _visitorId,
                        Name = "Test",
                        LastName = "User",
                        Email = "test@test.com",
                        Password = "pass",
                        BirthDate = DateTime.Now.AddYears(-25)
                    },
                    Points = 45,
                    Origin = ScoreOrigin.EventParticipation,
                    Description = "Test entry",
                    StrategyName = "PerEvent",
                    CreatedAt = DateTime.UtcNow
                }
            };

            _mockRepository.Setup(r => r.GetByVisitor(_visitorId)).Returns(histories);

            // Act
            var result = _scoreHistoryLogic.GetVisitorScoreHistory(_visitorId, null, dateTo);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            _mockRepository.Verify(r => r.GetByVisitor(_visitorId), Times.Once);
            _mockRepository.Verify(r => r.GetByVisitorAndDateRange(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
        }
    }
}
