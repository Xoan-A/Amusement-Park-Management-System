using Domain;
using BusinessLogic;
using IBusinessLogic.Strategy;
using Models.In;

namespace TestBusinessLogic
{
    [TestClass]
    public class StrategyTest
    {
        [TestMethod]
        public void PerAttraction_CalculateScore_ShouldReturnBasicCalculation()
        {
            var strategy = new PerAttraction();
            var user = new Visitor { Name = "Test" };
            var attraction = new Attraction { Type = AttractionType.Performance };

            var request = new StrategyRequest
            {
                User = user,
                Attraction = attraction
            };

            int score = strategy.CalculateScore(request);

            Assert.AreEqual(3, score);
        }

        [TestMethod]
        public void PerAttraction_Name_ShouldBePerAttraction()
        {
            var strategy = new PerAttraction();

            Assert.AreEqual("PerAttraction", strategy.Name);
        }
        
        [TestMethod]
        public void PerEvent_CalculateScore_NotSpecialEvent_ShouldReturnBaseScore()
        {
            var strategy = new PerEvent();
            var user = new Visitor { Name = "Test" };
            var attraction = new Attraction { Type = AttractionType.Performance };

            var request = new StrategyRequest
            {
                User = user,
                Attraction = attraction,
                IsSepcialEvent = false
            };

            int score = strategy.CalculateScore(request);

            Assert.AreEqual(3, score);
        }

        [TestMethod]
        public void PerEvent_CalculateScore_SpecialEvent_ShouldReturnDoubleScore()
        {
            var strategy = new PerEvent();
            var user = new Visitor { Name = "Test" };
            var attraction = new Attraction { Type = AttractionType.Performance };

            var request = new StrategyRequest
            {
                User = user,
                Attraction = attraction,
                IsSepcialEvent = true
            };

            int score = strategy.CalculateScore(request);

            Assert.AreEqual(6, score);
        }

        [TestMethod]
        public void PerEvent_Name_ShouldBePerEvent()
        {
            var strategy = new PerEvent();

            Assert.AreEqual("PerEvent", strategy.Name);
        }
    }
}