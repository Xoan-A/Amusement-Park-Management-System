using Domain;

namespace TestDomain
{
    [TestClass]
    public class ScoreHistoryTest
    {
        [TestMethod]
        public void ScoreHistory_DefaultConstructor_SetsCreatedAt()
        {
            // Act
            var history = new ScoreHistory();

            // Assert
            Assert.IsTrue(history.CreatedAt > DateTime.MinValue);
            Assert.IsTrue(history.CreatedAt <= DateTime.UtcNow);
        }

        [TestMethod]
        public void ScoreHistory_SetValidValues_Success()
        {
            // Arrange
            var history = new ScoreHistory();
            var visitorId = Guid.NewGuid();
            var points = 100;
            var origin = ScoreOrigin.AttractionVisit;
            var strategyName = "PerAttraction";
            var description = "Visited Roller Coaster";

            // Act
            history.VisitorId = visitorId;
            history.Points = points;
            history.Origin = origin;
            history.StrategyName = strategyName;
            history.Description = description;

            // Assert
            Assert.AreEqual(visitorId, history.VisitorId);
            Assert.AreEqual(points, history.Points);
            Assert.AreEqual(origin, history.Origin);
            Assert.AreEqual(strategyName, history.StrategyName);
            Assert.AreEqual(description, history.Description);
        }

        [TestMethod]
        public void ScoreHistory_NegativePoints_AllowedForRedemptions()
        {
            // Arrange
            var history = new ScoreHistory();

            // Act
            history.Points = -50;
            history.Origin = ScoreOrigin.Redemption;

            // Assert
            Assert.AreEqual(-50, history.Points);
        }

        [TestMethod]
        public void ScoreHistory_SetDescription_TooLong_ThrowsException()
        {
            // Arrange
            var history = new ScoreHistory();
            var longDescription = new string('a', 1001);

            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() =>
            {
                history.Description = longDescription;
            });
        }

        [TestMethod]
        public void ScoreHistory_SetDescription_EmptyOrNull_ThrowsException()
        {
            // Arrange
            var history = new ScoreHistory();

            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() =>
            {
                history.Description = "";
            });

            Assert.ThrowsException<ArgumentException>(() =>
            {
                history.Description = null;
            });
        }

        [TestMethod]
        public void ScoreHistory_SetStrategyName_EmptyOrNull_ThrowsException()
        {
            // Arrange
            var history = new ScoreHistory();

            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() =>
            {
                history.StrategyName = "";
            });

            Assert.ThrowsException<ArgumentException>(() =>
            {
                history.StrategyName = null;
            });
        }

        [TestMethod]
        public void ScoreHistory_AllOriginTypes_CanBeSet()
        {
            // Arrange & Act & Assert
            foreach (ScoreOrigin origin in Enum.GetValues(typeof(ScoreOrigin)))
            {
                var history = new ScoreHistory
                {
                    VisitorId = Guid.NewGuid(),
                    Points = 10,
                    Origin = origin,
                    StrategyName = "Test",
                    Description = "Test description"
                };

                Assert.AreEqual(origin, history.Origin);
            }
        }

        [TestMethod]
        public void ScoreHistory_WithRelatedEntity_References_Success()
        {
            // Arrange
            var visitor = new User
            {
                Id = Guid.NewGuid(),
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "password",
                BirthDate = new DateTime(1990, 1, 1)
            };

            var history = new ScoreHistory
            {
                VisitorId = visitor.Id,
                Visitor = visitor,
                Points = 100,
                Origin = ScoreOrigin.AttractionVisit,
                StrategyName = "PerAttraction",
                Description = "Visited attraction"
            };

            // Assert
            Assert.IsNotNull(history.Visitor);
            Assert.AreEqual(visitor.Id, history.Visitor.Id);
        }

        [TestMethod]
        public void ScoreHistory_SetRelatedEntityId_ValidGuid_Success()
        {
            // Arrange
            var history = new ScoreHistory();
            var attractionId = Guid.NewGuid();

            // Act
            history.RelatedEntityId = attractionId;

            // Assert
            Assert.AreEqual(attractionId, history.RelatedEntityId);
        }

        [TestMethod]
        public void ScoreHistory_SetRelatedEntityId_Null_Success()
        {
            // Arrange
            var history = new ScoreHistory();

            // Act
            history.RelatedEntityId = null;

            // Assert
            Assert.IsNull(history.RelatedEntityId);
        }
    }
}
