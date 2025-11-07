using Domain;

namespace TestDomain
{
    [TestClass]
    public class ScoreHistoryTest
    {
        [TestMethod]
        public void ScoreHistory_SetValidValues_Success()
        {
            ScoreHistory history = new ScoreHistory();
            Guid visitorId = Guid.NewGuid();
            int points = 100;
            ScoreOrigin origin = ScoreOrigin.AttractionVisit;
            string strategyName = "PerAttraction";
            string description = "Visited Roller Coaster";

            history.VisitorId = visitorId;
            history.Points = points;
            history.Origin = origin;
            history.StrategyName = strategyName;
            history.Description = description;

            Assert.AreEqual(visitorId, history.VisitorId);
            Assert.AreEqual(points, history.Points);
            Assert.AreEqual(origin, history.Origin);
            Assert.AreEqual(strategyName, history.StrategyName);
            Assert.AreEqual(description, history.Description);
        }

        [TestMethod]
        public void ScoreHistory_NegativePoints_AllowedForRedemptions()
        {
            ScoreHistory history = new ScoreHistory();

            history.Points = -50;
            history.Origin = ScoreOrigin.Redemption;

            Assert.AreEqual(-50, history.Points);
        }

        [TestMethod]
        public void ScoreHistory_SetDescription_TooLong_ThrowsException()
        {
            ScoreHistory history = new ScoreHistory();
            string longDescription = new string('a', 1001);

            Assert.ThrowsException<ArgumentException>(() =>
            {
                history.Description = longDescription;
            });
        }

        [TestMethod]
        public void ScoreHistory_SetDescription_EmptyOrNull_ThrowsException()
        {
            ScoreHistory history = new ScoreHistory();

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
            ScoreHistory history = new ScoreHistory();

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
            foreach (ScoreOrigin origin in Enum.GetValues(typeof(ScoreOrigin)))
            {
                ScoreHistory history = new ScoreHistory
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
            User visitor = new User
            {
                Id = Guid.NewGuid(),
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "password",
                BirthDate = new DateTime(1990, 1, 1)
            };

            ScoreHistory history = new ScoreHistory
            {
                VisitorId = visitor.Id,
                Visitor = visitor,
                Points = 100,
                Origin = ScoreOrigin.AttractionVisit,
                StrategyName = "PerAttraction",
                Description = "Visited attraction"
            };

            Assert.IsNotNull(history.Visitor);
            Assert.AreEqual(visitor.Id, history.Visitor.Id);
        }

        [TestMethod]
        public void ScoreHistory_SetRelatedEntityId_ValidGuid_Success()
        {
            ScoreHistory history = new ScoreHistory();
            Guid attractionId = Guid.NewGuid();

            history.RelatedEntityId = attractionId;

            Assert.AreEqual(attractionId, history.RelatedEntityId);
        }

        [TestMethod]
        public void ScoreHistory_SetRelatedEntityId_Null_Success()
        {
            ScoreHistory history = new ScoreHistory();

            history.RelatedEntityId = null;

            Assert.IsNull(history.RelatedEntityId);
        }
    }
}
