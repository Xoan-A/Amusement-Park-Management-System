using Domain;
using BusinessLogic;
using IBusinessLogic.Strategy;
using Models.In;
using Moq;
using IDataAccess;

namespace TestBusinessLogic
{
    [TestClass]
    public class StrategyTest
    {
        private Mock<IStrategyRepository> CreateMockRepository()
        {
            var mock = new Mock<IStrategyRepository>();
            StrategyConfiguration? storedConfig = null;

            mock.Setup(x => x.Get()).Returns(() => storedConfig);
            mock.Setup(x => x.Update(It.IsAny<StrategyConfiguration>()))
                .Callback<StrategyConfiguration>(config => storedConfig = config);

            return mock;
        }
        [TestMethod]
        public void ActiveStrategy_SetStrategy_ShouldSetStrategy()
        {
            var mockRepo = CreateMockRepository();
            var activeStrategy = new ActiveStrategy(mockRepo.Object);

            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "PerAttraction",
            });

            IContreteStrategy result = activeStrategy.GetStrategy();

            Assert.IsNotNull(result);
            Assert.AreEqual("PerAttraction", result.Name);
        }

        [TestMethod]
        public void ActiveStrategy_GetStrategy_ShouldReturnDefaultWhenNoStrategySet()
        {
            var mockRepo = CreateMockRepository();
            var activeStrategy = new ActiveStrategy(mockRepo.Object);

            IContreteStrategy result = activeStrategy.GetStrategy();

            Assert.IsNotNull(result);
            Assert.AreEqual("PerAttraction", result.Name);
        }

        [TestMethod]
        public void ActiveStrategy_SetStrategy_WithCombo_ShouldSetComboWithN()
        {
            var mockRepo = CreateMockRepository();
            var activeStrategy = new ActiveStrategy(mockRepo.Object);

            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "Combo",
                N = 30,
            });

            IContreteStrategy result = activeStrategy.GetStrategy();

            Assert.IsNotNull(result);
            Assert.AreEqual("Combo", result.Name);
            Assert.IsInstanceOfType(result, typeof(Combo));
            Assert.AreEqual(30, ((Combo)result).N);
        }

        [TestMethod]
        public void ActiveStrategy_SetStrategy_WithCombo_ShouldThrowWhenNIsNull()
        {
            var mockRepo = CreateMockRepository();
            var activeStrategy = new ActiveStrategy(mockRepo.Object);

            Assert.ThrowsException<ArgumentException>(() =>
                activeStrategy.SetStrategy(new SetStrategyRequest
                {
                    StrategyName = "Combo",
                    N = null,
                }));
        }

        [TestMethod]
        public void ActiveStrategy_SetStrategy_ShouldThrowForInvalidStrategyName()
        {
            var mockRepo = CreateMockRepository();
            var activeStrategy = new ActiveStrategy(mockRepo.Object);

            Assert.ThrowsException<ArgumentException>(() =>
                activeStrategy.SetStrategy(new SetStrategyRequest
                {
                    StrategyName = "InvalidStrategy",
                }));
        }

        [TestMethod]
        public void ActiveStrategy_BasicCalculation_RollerCoaster_ShouldReturn2()
        {
            var user = new User { Name = "Test" };
            var attraction = new Attraction { Type = AttractionType.RollerCoaster };

            var request = new StrategyRequest
            {
                User = user,
                Attraction = attraction
            };

            int score = ActiveStrategy.BasicCalculation(request);

            Assert.AreEqual(2, score);
        }

        [TestMethod]
        public void ActiveStrategy_BasicCalculation_Simulator_ShouldReturn2()
        {
            var user = new User { Name = "Test" };
            var attraction = new Attraction { Type = AttractionType.Simulator };

            var request = new StrategyRequest
            {
                User = user,
                Attraction = attraction
            };

            int score = ActiveStrategy.BasicCalculation(request);

            Assert.AreEqual(2, score);
        }

        [TestMethod]
        public void ActiveStrategy_BasicCalculation_Performance_ShouldReturn3()
        {
            var user = new User { Name = "Test" };
            var attraction = new Attraction { Type = AttractionType.Performance };

            var request = new StrategyRequest
            {
                User = user,
                Attraction = attraction
            };

            int score = ActiveStrategy.BasicCalculation(request);

            Assert.AreEqual(3, score);
        }

        [TestMethod]
        public void ActiveStrategy_BasicCalculation_InteractiveZone_ShouldReturn4()
        {
            var user = new User { Name = "Test" };
            var attraction = new Attraction { Type = AttractionType.InteractiveZone };

            var request = new StrategyRequest
            {
                User = user,
                Attraction = attraction
            };

            int score = ActiveStrategy.BasicCalculation(request);

            Assert.AreEqual(4, score);
        }

        [TestMethod]
        public void PerAttraction_CalculateScore_ShouldReturnBasicCalculation()
        {
            var strategy = new PerAttraction();
            var user = new User { Name = "Test" };
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
            var user = new User { Name = "Test" };
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
            var user = new User { Name = "Test" };
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

        [TestMethod]
        public void Combo_CalculateScore_FirstVisit_ShouldReturnBaseScore()
        {
            var strategy = new Combo(30);
            var user = new User { Name = "Test" };
            var attraction = new Attraction { Type = AttractionType.Performance };

            DateTime firstVisit = new DateTime(2025, 10, 5, 10, 0, 0);
            user.RegisterEntry(attraction, firstVisit);

            var request = new StrategyRequest
            {
                User = user,
                Attraction = attraction,
                EnterDate = firstVisit
            };

            int score = strategy.CalculateScore(request);

            Assert.AreEqual(3, score);
        }

        [TestMethod]
        public void Combo_CalculateScore_SameAttraction_ShouldReturnBaseScore()
        {
            var strategy = new Combo(30);
            var user = new User { Name = "Test" };
            var attraction1 = new Attraction { Id = Guid.NewGuid(), Type = AttractionType.Performance };

            DateTime firstVisit = new DateTime(2025, 10, 5, 10, 0, 0);
            user.RegisterEntry(attraction1, firstVisit);

            DateTime secondVisit = new DateTime(2025, 10, 5, 10, 20, 0);
            user.RegisterEntry(attraction1, secondVisit);

            var request = new StrategyRequest
            {
                User = user,
                Attraction = attraction1,
                EnterDate = secondVisit
            };

            int score = strategy.CalculateScore(request);

            Assert.AreEqual(3, score);
        }

        [TestMethod]
        public void Combo_CalculateScore_DifferentAttractionWithinTime_ShouldReturnDoubleScore()
        {
            var strategy = new Combo(30);
            var user = new User { Name = "Test" };
            var attraction1 = new Attraction { Id = Guid.NewGuid(), Type = AttractionType.RollerCoaster };
            var attraction2 = new Attraction { Id = Guid.NewGuid(), Type = AttractionType.Performance };

            DateTime firstVisit = new DateTime(2025, 10, 5, 10, 0, 0);
            user.RegisterEntry(attraction1, firstVisit);

            DateTime secondVisit = new DateTime(2025, 10, 5, 10, 20, 0);
            user.RegisterEntry(attraction2, secondVisit);

            var request = new StrategyRequest
            {
                User = user,
                Attraction = attraction2,
                EnterDate = secondVisit
            };

            int score = strategy.CalculateScore(request);

            Assert.AreEqual(6, score);
        }

        [TestMethod]
        public void Combo_CalculateScore_DifferentAttractionOutsideTime_ShouldReturnBaseScore()
        {
            var strategy = new Combo(30);
            var user = new User { Name = "Test" };
            var attraction1 = new Attraction { Id = Guid.NewGuid(), Type = AttractionType.RollerCoaster };
            var attraction2 = new Attraction { Id = Guid.NewGuid(), Type = AttractionType.Performance };

            DateTime firstVisit = new DateTime(2025, 10, 5, 10, 0, 0);
            user.RegisterEntry(attraction1, firstVisit);

            DateTime secondVisit = new DateTime(2025, 10, 5, 10, 40, 0);
            user.RegisterEntry(attraction2, secondVisit);

            var request = new StrategyRequest
            {
                User = user,
                Attraction = attraction2,
                EnterDate = secondVisit
            };

            int score = strategy.CalculateScore(request);

            Assert.AreEqual(3, score);
        }

        [TestMethod]
        public void Combo_CalculateScore_MultipleVisits_ShouldCheckMostRecent()
        {
            var strategy = new Combo(30);
            var user = new User { Name = "Test" };
            var attraction1 = new Attraction { Id = Guid.NewGuid(), Type = AttractionType.RollerCoaster };
            var attraction2 = new Attraction { Id = Guid.NewGuid(), Type = AttractionType.Simulator };
            var attraction3 = new Attraction { Id = Guid.NewGuid(), Type = AttractionType.Performance };

            DateTime firstVisit = new DateTime(2025, 10, 5, 10, 0, 0);
            user.RegisterEntry(attraction1, firstVisit);

            DateTime secondVisit = new DateTime(2025, 10, 5, 10, 25, 0);
            user.RegisterEntry(attraction2, secondVisit);

            DateTime thirdVisit = new DateTime(2025, 10, 5, 10, 40, 0);
            user.RegisterEntry(attraction3, thirdVisit);

            var request = new StrategyRequest
            {
                User = user,
                Attraction = attraction3,
                EnterDate = thirdVisit
            };

            int score = strategy.CalculateScore(request);

            Assert.AreEqual(6, score);
        }

        [TestMethod]
        public void Combo_Name_ShouldBeCombo()
        {
            var strategy = new Combo(30);

            Assert.AreEqual("Combo", strategy.Name);
        }

        [TestMethod]
        public void Combo_Constructor_ShouldSetN()
        {
            var strategy = new Combo(45);

            Assert.AreEqual(45, strategy.N);
        }

        [TestMethod]
        public void ActiveStrategy_GetStrategy_FirstStrategySet_ShouldReturnStrategyImmediately()
        {
            var mockRepo = CreateMockRepository();
            var activeStrategy = new ActiveStrategy(mockRepo.Object);

            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "PerAttraction",
            });

            IContreteStrategy result = activeStrategy.GetStrategy();

            Assert.IsNotNull(result);
            Assert.AreEqual("PerAttraction", result.Name);
        }

        [TestMethod]
        public void ActiveStrategy_GetStrategy_ShouldReturnSameStrategyRegardlessOfDate()
        {
            var mockRepo = CreateMockRepository();
            var activeStrategy = new ActiveStrategy(mockRepo.Object);

            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "PerAttraction",
            });

            IContreteStrategy result1 = activeStrategy.GetStrategy();
            IContreteStrategy result2 = activeStrategy.GetStrategy();
            IContreteStrategy result3 = activeStrategy.GetStrategy();

            Assert.AreEqual("PerAttraction", result1.Name);
            Assert.AreEqual("PerAttraction", result2.Name);
            Assert.AreEqual("PerAttraction", result3.Name);
        }

        [TestMethod]
        public void ActiveStrategy_GetStrategy_AfterMultipleSets_ShouldReturnLatestStrategy()
        {
            var mockRepo = CreateMockRepository();
            var activeStrategy = new ActiveStrategy(mockRepo.Object);

            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "PerAttraction",
            });

            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "PerEvent",
            });

            IContreteStrategy result = activeStrategy.GetStrategy();

            Assert.AreEqual("PerEvent", result.Name);
        }

        [TestMethod]
        public void ActiveStrategy_GetStrategy_AfterSettingCombo_ShouldReturnComboWithCorrectN()
        {
            var mockRepo = CreateMockRepository();
            var activeStrategy = new ActiveStrategy(mockRepo.Object);

            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "Combo",
                N = 45,
            });

            IContreteStrategy result = activeStrategy.GetStrategy();

            Assert.AreEqual("Combo", result.Name);
            Assert.IsInstanceOfType(result, typeof(Combo));
            Assert.AreEqual(45, ((Combo)result).N);
        }

        [TestMethod]
        public void ActiveStrategy_GetStrategy_AfterChangingFromComboToPerAttraction_ShouldReturnPerAttraction()
        {
            var mockRepo = CreateMockRepository();
            var activeStrategy = new ActiveStrategy(mockRepo.Object);

            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "Combo",
                N = 30,
            });

            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "PerAttraction",
            });

            IContreteStrategy result = activeStrategy.GetStrategy();

            Assert.AreEqual("PerAttraction", result.Name);
            Assert.IsNotInstanceOfType(result, typeof(Combo));
        }

        [TestMethod]
        public void Combo_CalculateScore_ShouldThrowWhenUserIsNull()
        {
            var strategy = new Combo(30);
            var attraction = new Attraction { Type = AttractionType.Performance };

            var request = new StrategyRequest
            {
                User = null!,
                Attraction = attraction,
                EnterDate = new DateTime(2025, 10, 5, 10, 0, 0)
            };

            Assert.ThrowsException<ArgumentException>(() => strategy.CalculateScore(request));
        }

        [TestMethod]
        public void Combo_CalculateScore_ShouldThrowWhenAttractionIsNull()
        {
            var strategy = new Combo(30);
            var user = new User { Name = "Test" };

            var request = new StrategyRequest
            {
                User = user,
                Attraction = null!,
                EnterDate = new DateTime(2025, 10, 5, 10, 0, 0)
            };

            Assert.ThrowsException<ArgumentException>(() => strategy.CalculateScore(request));
        }

        [TestMethod]
        public void Combo_CalculateScore_ShouldThrowWhenEnterDateIsNull()
        {
            var strategy = new Combo(30);
            var user = new User { Name = "Test" };
            var attraction = new Attraction { Type = AttractionType.Performance };

            var request = new StrategyRequest
            {
                User = user,
                Attraction = attraction,
                EnterDate = null
            };

            Assert.ThrowsException<ArgumentException>(() => strategy.CalculateScore(request));
        }
    }
}