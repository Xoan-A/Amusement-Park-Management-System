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
        private Mock<IStrategyRepository> _mockRepo;
        private StrategyConfiguration? _storedConfig;

        [TestInitialize]
        private void SetupMocks()
        {
            _storedConfig = null;
            _mockRepo = new Mock<IStrategyRepository>();

            _mockRepo.Setup(x => x.Get()).ReturnsAsync(() => _storedConfig);
            _mockRepo.Setup(x => x.Update(It.IsAny<StrategyConfiguration>()))
                .Callback<StrategyConfiguration>(config => _storedConfig = config)
                .Returns(Task.CompletedTask);
        }

        [TestMethod]
        public async Task ActiveStrategy_SetStrategy_ShouldSetStrategy()
        {
            var activeStrategy = new ActiveStrategy(_mockRepo.Object);

            await activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "PerAttraction",
            });

            IConcreteStrategy result = await activeStrategy.GetStrategy();

            Assert.IsNotNull(result);
            Assert.AreEqual("PerAttraction", result.Name);
        }

        [TestMethod]
        public async Task ActiveStrategy_GetStrategy_ShouldReturnDefaultWhenNoStrategySet()
        {
            SetupMocks();
            var activeStrategy = new ActiveStrategy(_mockRepo.Object);

            IConcreteStrategy result = await activeStrategy.GetStrategy();

            Assert.IsNotNull(result);
            Assert.AreEqual("PerAttraction", result.Name);
        }

        [TestMethod]
        public async Task ActiveStrategy_SetStrategy_WithCombo_ShouldSetComboWithN()
        {
            SetupMocks();
            var activeStrategy = new ActiveStrategy(_mockRepo.Object);

            await activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "Combo",
                N = 30,
            });

            IConcreteStrategy result = await activeStrategy.GetStrategy();

            Assert.IsNotNull(result);
            Assert.AreEqual("Combo", result.Name);
            Assert.IsInstanceOfType(result, typeof(Combo));
            Assert.AreEqual(30, ((Combo)result).N);
        }

        [TestMethod]
        public void ActiveStrategy_SetStrategy_WithCombo_ShouldThrowWhenNIsNull()
        {
            SetupMocks();
            var activeStrategy = new ActiveStrategy(_mockRepo.Object);

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
            SetupMocks();
            var activeStrategy = new ActiveStrategy(_mockRepo.Object);

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

            int score = ActiveStrategy.BasicCalculation(user, attraction);

            Assert.AreEqual(2, score);
        }

        [TestMethod]
        public void ActiveStrategy_BasicCalculation_Simulator_ShouldReturn2()
        {
            var user = new User { Name = "Test" };
            var attraction = new Attraction { Type = AttractionType.Simulator };

            int score = ActiveStrategy.BasicCalculation(user, attraction);

            Assert.AreEqual(2, score);
        }

        [TestMethod]
        public void ActiveStrategy_BasicCalculation_Performance_ShouldReturn3()
        {
            var user = new User { Name = "Test" };
            var attraction = new Attraction { Type = AttractionType.Performance };

            int score = ActiveStrategy.BasicCalculation(user, attraction);

            Assert.AreEqual(3, score);
        }

        [TestMethod]
        public void ActiveStrategy_BasicCalculation_InteractiveZone_ShouldReturn4()
        {
            var user = new User { Name = "Test" };
            var attraction = new Attraction { Type = AttractionType.InteractiveZone };

            int score = ActiveStrategy.BasicCalculation(user, attraction);

            Assert.AreEqual(4, score);
        }

        [TestMethod]
        public void PerAttraction_CalculateScore_ShouldReturnBasicCalculation()
        {
            var strategy = new PerAttraction();
            var user = new User { Name = "Test" };
            var attraction = new Attraction { Type = AttractionType.Performance };

            var request = new StrategyRequest();

            int score = strategy.CalculateScore(user, attraction, request);

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
                IsSepcialEvent = false
            };

            int score = strategy.CalculateScore(user, attraction, request);

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
                IsSepcialEvent = true
            };

            int score = strategy.CalculateScore(user, attraction, request);

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
            SetupMocks();
            var strategy = new Combo(30);
            var user = new User { Name = "Test" };
            var attraction = new Attraction { Type = AttractionType.Performance };

            DateTime firstVisit = new DateTime(2025, 10, 5, 10, 0, 0);
            user.RegisterEntry(attraction, firstVisit);

            var request = new StrategyRequest
            {
                EnterDate = firstVisit
            };

            int score = strategy.CalculateScore(user, attraction, request);

            Assert.AreEqual(3, score);
        }

        [TestMethod]
        public void Combo_CalculateScore_SameAttraction_ShouldReturnBaseScore()
        {
            SetupMocks();
            var strategy = new Combo(30);
            var user = new User { Name = "Test" };
            var attraction1 = new Attraction { Id = Guid.NewGuid(), Type = AttractionType.Performance };

            DateTime firstVisit = new DateTime(2025, 10, 5, 10, 0, 0);
            user.RegisterEntry(attraction1, firstVisit);

            DateTime secondVisit = new DateTime(2025, 10, 5, 10, 20, 0);
            user.RegisterEntry(attraction1, secondVisit);

            var request = new StrategyRequest
            {
                EnterDate = secondVisit
            };

            int score = strategy.CalculateScore(user, attraction1, request);

            Assert.AreEqual(3, score);
        }

        [TestMethod]
        public void Combo_CalculateScore_DifferentAttractionWithinTime_ShouldReturnDoubleScore()
        {
            SetupMocks();
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
                EnterDate = secondVisit
            };

            int score = strategy.CalculateScore(user, attraction2, request);

            Assert.AreEqual(6, score);
        }

        [TestMethod]
        public void Combo_CalculateScore_DifferentAttractionOutsideTime_ShouldReturnBaseScore()
        {
            SetupMocks();
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
                EnterDate = secondVisit
            };

            int score = strategy.CalculateScore(user, attraction2, request);

            Assert.AreEqual(3, score);
        }

        [TestMethod]
        public void Combo_CalculateScore_MultipleVisits_ShouldCheckMostRecent()
        {
            SetupMocks();
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
                EnterDate = thirdVisit
            };

            int score = strategy.CalculateScore(user, attraction3, request);

            Assert.AreEqual(6, score);
        }

        [TestMethod]
        public void Combo_Name_ShouldBeCombo()
        {
            SetupMocks();
            var strategy = new Combo(30);

            Assert.AreEqual("Combo", strategy.Name);
        }

        [TestMethod]
        public void Combo_Constructor_ShouldSetN()
        {
            SetupMocks();
            var strategy = new Combo(45);

            Assert.AreEqual(45, strategy.N);
        }

        [TestMethod]
        public async Task ActiveStrategy_GetStrategy_FirstStrategySet_ShouldReturnStrategyImmediately()
        {
            SetupMocks();
            var activeStrategy = new ActiveStrategy(_mockRepo.Object);

            await activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "PerAttraction",
            });

            IConcreteStrategy result = await activeStrategy.GetStrategy();

            Assert.IsNotNull(result);
            Assert.AreEqual("PerAttraction", result.Name);
        }

        [TestMethod]
        public async Task ActiveStrategy_GetStrategy_ShouldReturnSameStrategyRegardlessOfDate()
        {
            SetupMocks();
            var activeStrategy = new ActiveStrategy(_mockRepo.Object);

            await activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "PerAttraction",
            });

            IConcreteStrategy result1 = await activeStrategy.GetStrategy();
            IConcreteStrategy result2 = await activeStrategy.GetStrategy();
            IConcreteStrategy result3 = await activeStrategy.GetStrategy();

            Assert.AreEqual("PerAttraction", result1.Name);
            Assert.AreEqual("PerAttraction", result2.Name);
            Assert.AreEqual("PerAttraction", result3.Name);
        }

        [TestMethod]
        public async Task ActiveStrategy_GetStrategy_AfterMultipleSets_ShouldReturnLatestStrategy()
        {
            SetupMocks();
            var activeStrategy = new ActiveStrategy(_mockRepo.Object);

            await activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "PerAttraction",
            });

            await activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "PerEvent",
            });

            IConcreteStrategy result = await activeStrategy.GetStrategy();

            Assert.AreEqual("PerEvent", result.Name);
        }

        [TestMethod]
        public async Task ActiveStrategy_GetStrategy_AfterSettingCombo_ShouldReturnComboWithCorrectN()
        {
            SetupMocks();
            var activeStrategy = new ActiveStrategy(_mockRepo.Object);

            await activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "Combo",
                N = 45,
            });

            IConcreteStrategy result = await activeStrategy.GetStrategy();

            Assert.AreEqual("Combo", result.Name);
            Assert.IsInstanceOfType(result, typeof(Combo));
            Assert.AreEqual(45, ((Combo)result).N);
        }

        [TestMethod]
        public async Task ActiveStrategy_GetStrategy_AfterChangingFromComboToPerAttraction_ShouldReturnPerAttraction()
        {
            var activeStrategy = new ActiveStrategy(_mockRepo.Object);

            await activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "Combo",
                N = 30,
            });

            await activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "PerAttraction",
            });

            IConcreteStrategy result = await activeStrategy.GetStrategy();

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
                EnterDate = new DateTime(2025, 10, 5, 10, 0, 0)
            };

            Assert.ThrowsException<ArgumentException>(() => strategy.CalculateScore(null!, attraction, request));
        }

        [TestMethod]
        public void Combo_CalculateScore_ShouldThrowWhenAttractionIsNull()
        {
            SetupMocks();
            var strategy = new Combo(30);
            var user = new User { Name = "Test" };

            var request = new StrategyRequest
            {
                EnterDate = new DateTime(2025, 10, 5, 10, 0, 0)
            };

            Assert.ThrowsException<ArgumentException>(() => strategy.CalculateScore(user, null!, request));
        }

        [TestMethod]
        public void Combo_CalculateScore_ShouldThrowWhenEnterDateIsNull()
        {
            SetupMocks();
            var strategy = new Combo(30);
            var user = new User { Name = "Test" };
            var attraction = new Attraction { Type = AttractionType.Performance };

            var request = new StrategyRequest
            {
                EnterDate = null
            };

            Assert.ThrowsException<ArgumentException>(() => strategy.CalculateScore(user, attraction, request));
        }
    }
}