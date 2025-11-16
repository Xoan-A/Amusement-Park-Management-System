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
            List<ScoreHistory> histories = new List<ScoreHistory>
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
                    StrategyName = "PerAttraction",
                    CreatedAt = DateTime.UtcNow
                }
            };

            _mockRepository.Setup(r => r.GetByVisitor(_visitorId)).Returns(histories);

            List<ScoreHistoryModelOut> result = _scoreHistoryLogic.GetMyScoreHistory(_visitorId);

            Assert.AreEqual(_visitorId, result[0].VisitorId);
            Assert.AreEqual(100, result[0].Points);
            Assert.AreEqual("AttractionVisit", result[0].Origin);
            Assert.AreEqual("PerAttraction", result[0].StrategyName);
            _mockRepository.Verify(r => r.GetByVisitor(_visitorId), Times.Once);
        }

        [TestMethod]
        public void GetVisitorScoreHistory_WithDateRange_FiltersCorrectly()
        {
            DateTime dateFrom = DateTime.UtcNow.AddDays(-7);
            DateTime dateTo = DateTime.UtcNow;
            List<ScoreHistory> histories = new List<ScoreHistory>
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
                    StrategyName = "PerEvent",
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                }
            };

            _mockRepository.Setup(r => r.GetByVisitorAndDateRange(_visitorId, dateFrom, dateTo))
            .Returns(histories);

            List<ScoreHistoryModelOut> result = _scoreHistoryLogic.GetVisitorScoreHistory(_visitorId, dateFrom, dateTo);

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
            List<ScoreHistory> histories = new List<ScoreHistory>
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
                    StrategyName = "PerEvent",
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                }
            };

            _mockRepository.Setup(r => r.GetByVisitor(_visitorId)).Returns(histories);

            List<ScoreHistoryModelOut> result = _scoreHistoryLogic.GetVisitorScoreHistory(_visitorId, null, null);

            Assert.AreEqual(2, result.Count);
            _mockRepository.Verify(r => r.GetByVisitor(_visitorId), Times.Once);
            _mockRepository.Verify(
                r => r.GetByVisitorAndDateRange(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()),
                Times.Never);
        }

        [TestMethod]
        public void GetAllScoreHistory_ReturnsAllHistoryWithVisitorNames()
        {
            Guid visitor1Id = Guid.NewGuid();
            Guid visitor2Id = Guid.NewGuid();
            List<ScoreHistory> histories = new List<ScoreHistory>
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
                    StrategyName = "PerEvent",
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                }
            };

            _mockRepository.Setup(r => r.GetAll()).Returns(histories);

            List<ScoreHistoryModelOut> result = _scoreHistoryLogic.GetAllScoreHistory();

            Assert.AreEqual(2, result.Count);

            ScoreHistoryModelOut aliceHistory = result.FirstOrDefault(h => h.VisitorName == "Alice");
            Assert.AreEqual(150, aliceHistory.Points);
            Assert.AreEqual("AttractionVisit", aliceHistory.Origin);

            ScoreHistoryModelOut charlieHistory = result.FirstOrDefault(h => h.VisitorName == "Charlie");
            Assert.AreEqual(200, charlieHistory.Points);
            Assert.AreEqual("EventParticipation", charlieHistory.Origin);

            _mockRepository.Verify(r => r.GetAll(), Times.Once);
        }

        [TestMethod]
        public void GetVisitorScoreHistory_WithOnlyDateFrom_ReturnsAll()
        {
            DateTime dateFrom = DateTime.UtcNow.AddDays(-7);
            List<ScoreHistory> histories = new List<ScoreHistory>
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
                    StrategyName = "PerAttraction",
                    CreatedAt = DateTime.UtcNow
                }
            };

            _mockRepository.Setup(r => r.GetByVisitor(_visitorId)).Returns(histories);

            List<ScoreHistoryModelOut> result = _scoreHistoryLogic.GetVisitorScoreHistory(_visitorId, dateFrom, null);

            Assert.AreEqual(1, result.Count);
            _mockRepository.Verify(r => r.GetByVisitor(_visitorId), Times.Once);
            _mockRepository.Verify(
                r => r.GetByVisitorAndDateRange(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()),
                Times.Never);
        }

        [TestMethod]
        public void GetVisitorScoreHistory_WithOnlyDateTo_ReturnsAll()
        {
            DateTime dateTo = DateTime.UtcNow;
            List<ScoreHistory> histories = new List<ScoreHistory>
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
                    StrategyName = "PerEvent",
                    CreatedAt = DateTime.UtcNow
                }
            };

            _mockRepository.Setup(r => r.GetByVisitor(_visitorId)).Returns(histories);

            List<ScoreHistoryModelOut> result = _scoreHistoryLogic.GetVisitorScoreHistory(_visitorId, null, dateTo);

            Assert.AreEqual(1, result.Count);
            _mockRepository.Verify(r => r.GetByVisitor(_visitorId), Times.Once);
            _mockRepository.Verify(
                r => r.GetByVisitorAndDateRange(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()),
                Times.Never);
        }

        [TestMethod]
        public void GetMyScoreHistory_WithRelatedEntityName_MapsCorrectly()
        {
            List<ScoreHistory> histories = new List<ScoreHistory>
            {
                new ScoreHistory
                {
                    Id = Guid.NewGuid(),
                    VisitorId = _visitorId,
                    Visitor = new User
                    {
                        Id = _visitorId,
                        Name = "Test User",
                        LastName = "Name",
                        Email = "test@test.com",
                        Password = "pass",
                        BirthDate = DateTime.Now.AddYears(-25)
                    },
                    Points = 100,
                    Origin = ScoreOrigin.AttractionVisit,
                    RelatedEntityId = Guid.NewGuid(),
                    RelatedEntityName = "Roller Coaster",
                    StrategyName = "PerAttraction",
                    CreatedAt = DateTime.UtcNow
                }
            };

            _mockRepository.Setup(r => r.GetByVisitor(_visitorId)).Returns(histories);

            List<ScoreHistoryModelOut> result = _scoreHistoryLogic.GetMyScoreHistory(_visitorId);

            Assert.AreEqual("Roller Coaster", result[0].RelatedEntityName);
            Assert.AreEqual(ScoreOrigin.AttractionVisit.ToString(), result[0].Origin);
        }

        [TestMethod]
        public void GetMyScoreHistory_WithNullRelatedEntityName_MapsNull()
        {
            List<ScoreHistory> histories = new List<ScoreHistory>
            {
                new ScoreHistory
                {
                    Id = Guid.NewGuid(),
                    VisitorId = _visitorId,
                    Visitor = new User
                    {
                        Id = _visitorId,
                        Name = "Test User",
                        LastName = "Name",
                        Email = "test@test.com",
                        Password = "pass",
                        BirthDate = DateTime.Now.AddYears(-25)
                    },
                    Points = 50,
                    Origin = ScoreOrigin.SpecialMission,
                    RelatedEntityId = null,
                    RelatedEntityName = null,
                    StrategyName = "SpecialMissionStrategy",
                    CreatedAt = DateTime.UtcNow
                }
            };

            _mockRepository.Setup(r => r.GetByVisitor(_visitorId)).Returns(histories);

            List<ScoreHistoryModelOut> result = _scoreHistoryLogic.GetMyScoreHistory(_visitorId);

            Assert.AreEqual(1, result.Count);
            Assert.IsNull(result[0].RelatedEntityName);
            Assert.IsNull(result[0].RelatedEntityId);
        }
    }
}