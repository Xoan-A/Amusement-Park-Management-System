using Domain;
using BusinessLogic;
using IBusinessLogic;
using Models.In;
using Moq;
using IDataAccess;

namespace TestBusinessLogic
{
    [TestClass]
    public class StrategyTest
    {
        private Mock<IStrategyRepository> _mockRepo;
        private Mock<IPluginLoader> _mockPluginLoader;
        private StrategyConfiguration? _storedConfig;

        [TestInitialize]
        public void SetupMocks()
        {
            _storedConfig = null;
            _mockRepo = new Mock<IStrategyRepository>();
            _mockPluginLoader = new Mock<IPluginLoader>();

            _mockRepo.Setup(x => x.Get()).Returns(() => _storedConfig);
            _mockRepo.Setup(x => x.Update(It.IsAny<StrategyConfiguration>()))
            .Callback<StrategyConfiguration>(config => _storedConfig = config)
            ;

            _mockPluginLoader.Setup(x => x.CreateStrategyInstance("PerAttraction", null))
            .Returns(new PerAttraction());
            _mockPluginLoader.Setup(x => x.CreateStrategyInstance("PerEvent", null))
            .Returns(new PerEvent());
            _mockPluginLoader.Setup(x => x.CreateStrategyInstance("Combo", It.IsAny<Dictionary<string, object>>()))
            .Returns((string name, Dictionary<string, object> p) => new Combo((int)p["n"]));
            _mockPluginLoader.Setup(x => x.CreateStrategyInstance(It.IsNotIn("PerAttraction", "PerEvent", "Combo"),
                It.IsAny<Dictionary<string, object>>()))
            .Throws<KeyNotFoundException>();
        }

        [TestMethod]
        public void ActiveStrategy_SetStrategy_ShouldSetStrategy()
        {
            ActiveStrategy activeStrategy = new ActiveStrategy(_mockRepo.Object, _mockPluginLoader.Object);

            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "PerAttraction",
            });

            IConcreteStrategy result = activeStrategy.GetStrategy();

            Assert.AreEqual("PerAttraction", result.Name);
        }

        [TestMethod]
        public void ActiveStrategy_GetStrategy_ShouldReturnDefaultWhenNoStrategySet()
        {
            SetupMocks();
            ActiveStrategy activeStrategy = new ActiveStrategy(_mockRepo.Object, _mockPluginLoader.Object);

            IConcreteStrategy result = activeStrategy.GetStrategy();

            Assert.AreEqual("PerAttraction", result.Name);
        }

        [TestMethod]
        public void ActiveStrategy_SetStrategy_WithCombo_ShouldSetComboWithN()
        {
            SetupMocks();
            ActiveStrategy activeStrategy = new ActiveStrategy(_mockRepo.Object, _mockPluginLoader.Object);

            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "Combo",
                N = 30,
            });

            IConcreteStrategy result = activeStrategy.GetStrategy();

            Assert.AreEqual("Combo", result.Name);
            Assert.IsInstanceOfType(result, typeof(Combo));
            Assert.AreEqual(30, ((Combo)result).N);
        }

        [TestMethod]
        public void ActiveStrategy_SetStrategy_WithCombo_ShouldThrowWhenNIsNull()
        {
            SetupMocks();
            ActiveStrategy activeStrategy = new ActiveStrategy(_mockRepo.Object, _mockPluginLoader.Object);

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
            ActiveStrategy activeStrategy = new ActiveStrategy(_mockRepo.Object, _mockPluginLoader.Object);

            Assert.ThrowsException<ArgumentException>(() =>
            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "InvalidStrategy",
            }));
        }

        [TestMethod]
        public void ActiveStrategy_BasicCalculation_RollerCoaster_ShouldReturn2()
        {
            User user = new User { Name = "Test" };
            Attraction attraction = new Attraction { Type = AttractionType.RollerCoaster };

            int score = ActiveStrategy.BasicCalculation(user, attraction);

            Assert.AreEqual(2, score);
        }

        [TestMethod]
        public void ActiveStrategy_BasicCalculation_Simulator_ShouldReturn2()
        {
            User user = new User { Name = "Test" };
            Attraction attraction = new Attraction { Type = AttractionType.Simulator };

            int score = ActiveStrategy.BasicCalculation(user, attraction);

            Assert.AreEqual(2, score);
        }

        [TestMethod]
        public void ActiveStrategy_BasicCalculation_Performance_ShouldReturn3()
        {
            User user = new User { Name = "Test" };
            Attraction attraction = new Attraction { Type = AttractionType.Performance };

            int score = ActiveStrategy.BasicCalculation(user, attraction);

            Assert.AreEqual(3, score);
        }

        [TestMethod]
        public void ActiveStrategy_BasicCalculation_InteractiveZone_ShouldReturn4()
        {
            User user = new User { Name = "Test" };
            Attraction attraction = new Attraction { Type = AttractionType.InteractiveZone };

            int score = ActiveStrategy.BasicCalculation(user, attraction);

            Assert.AreEqual(4, score);
        }

        [TestMethod]
        public void PerAttraction_CalculateScore_ShouldReturnBasicCalculation()
        {
            PerAttraction strategy = new PerAttraction();
            User user = new User { Name = "Test" };
            Attraction attraction = new Attraction { Type = AttractionType.Performance };

            StrategyRequest request = new StrategyRequest();

            int score = strategy.CalculateScore(user, attraction, request);

            Assert.AreEqual(3, score);
        }

        [TestMethod]
        public void PerAttraction_Name_ShouldBePerAttraction()
        {
            PerAttraction strategy = new PerAttraction();

            Assert.AreEqual("PerAttraction", strategy.Name);
        }

        [TestMethod]
        public void PerEvent_CalculateScore_NotSpecialEvent_ShouldReturnBaseScore()
        {
            PerEvent strategy = new PerEvent();
            User user = new User { Name = "Test" };
            Attraction attraction = new Attraction { Type = AttractionType.Performance };

            StrategyRequest request = new StrategyRequest
            {
                IsSpecialEvent = false
            };

            int score = strategy.CalculateScore(user, attraction, request);

            Assert.AreEqual(3, score);
        }

        [TestMethod]
        public void PerEvent_CalculateScore_SpecialEvent_ShouldReturnDoubleScore()
        {
            PerEvent strategy = new PerEvent();
            User user = new User { Name = "Test" };
            Attraction attraction = new Attraction { Type = AttractionType.Performance };

            StrategyRequest request = new StrategyRequest
            {
                IsSpecialEvent = true
            };

            int score = strategy.CalculateScore(user, attraction, request);

            Assert.AreEqual(6, score);
        }

        [TestMethod]
        public void PerEvent_Name_ShouldBePerEvent()
        {
            PerEvent strategy = new PerEvent();

            Assert.AreEqual("PerEvent", strategy.Name);
        }

        [TestMethod]
        public void Combo_CalculateScore_FirstVisit_ShouldReturnBaseScore()
        {
            SetupMocks();
            Combo strategy = new Combo(30);
            User user = new User { Name = "Test" };
            Attraction attraction = new Attraction { Type = AttractionType.Performance };

            DateTime firstVisit = new DateTime(2025, 10, 5, 10, 0, 0);
            user.RegisterEntry(attraction, firstVisit);

            StrategyRequest request = new StrategyRequest
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
            Combo strategy = new Combo(30);
            User user = new User { Name = "Test" };
            Attraction attraction1 = new Attraction { Id = Guid.NewGuid(), Type = AttractionType.Performance };

            DateTime firstVisit = new DateTime(2025, 10, 5, 10, 0, 0);
            user.RegisterEntry(attraction1, firstVisit);

            DateTime secondVisit = new DateTime(2025, 10, 5, 10, 20, 0);
            user.RegisterEntry(attraction1, secondVisit);

            StrategyRequest request = new StrategyRequest
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
            Combo strategy = new Combo(30);
            User user = new User { Name = "Test" };
            Attraction attraction1 = new Attraction { Id = Guid.NewGuid(), Type = AttractionType.RollerCoaster };
            Attraction attraction2 = new Attraction { Id = Guid.NewGuid(), Type = AttractionType.Performance };

            DateTime firstVisit = new DateTime(2025, 10, 5, 10, 0, 0);
            user.RegisterEntry(attraction1, firstVisit);

            DateTime secondVisit = new DateTime(2025, 10, 5, 10, 20, 0);
            user.RegisterEntry(attraction2, secondVisit);

            StrategyRequest request = new StrategyRequest
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
            Combo strategy = new Combo(30);
            User user = new User { Name = "Test" };
            Attraction attraction1 = new Attraction { Id = Guid.NewGuid(), Type = AttractionType.RollerCoaster };
            Attraction attraction2 = new Attraction { Id = Guid.NewGuid(), Type = AttractionType.Performance };

            DateTime firstVisit = new DateTime(2025, 10, 5, 10, 0, 0);
            user.RegisterEntry(attraction1, firstVisit);

            DateTime secondVisit = new DateTime(2025, 10, 5, 10, 40, 0);
            user.RegisterEntry(attraction2, secondVisit);

            StrategyRequest request = new StrategyRequest
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
            Combo strategy = new Combo(30);
            User user = new User { Name = "Test" };
            Attraction attraction1 = new Attraction { Id = Guid.NewGuid(), Type = AttractionType.RollerCoaster };
            Attraction attraction2 = new Attraction { Id = Guid.NewGuid(), Type = AttractionType.Simulator };
            Attraction attraction3 = new Attraction { Id = Guid.NewGuid(), Type = AttractionType.Performance };

            DateTime firstVisit = new DateTime(2025, 10, 5, 10, 0, 0);
            user.RegisterEntry(attraction1, firstVisit);

            DateTime secondVisit = new DateTime(2025, 10, 5, 10, 25, 0);
            user.RegisterEntry(attraction2, secondVisit);

            DateTime thirdVisit = new DateTime(2025, 10, 5, 10, 40, 0);
            user.RegisterEntry(attraction3, thirdVisit);

            StrategyRequest request = new StrategyRequest
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
            Combo strategy = new Combo(30);

            Assert.AreEqual("Combo", strategy.Name);
        }

        [TestMethod]
        public void Combo_Constructor_ShouldSetN()
        {
            SetupMocks();
            Combo strategy = new Combo(45);

            Assert.AreEqual(45, strategy.N);
        }

        [TestMethod]
        public void ActiveStrategy_GetStrategy_FirstStrategySet_ShouldReturnStrategyImmediately()
        {
            SetupMocks();
            ActiveStrategy activeStrategy = new ActiveStrategy(_mockRepo.Object, _mockPluginLoader.Object);

            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "PerAttraction",
            });

            IConcreteStrategy result = activeStrategy.GetStrategy();

            Assert.AreEqual("PerAttraction", result.Name);
        }

        [TestMethod]
        public void ActiveStrategy_GetStrategy_ShouldReturnSameStrategyRegardlessOfDate()
        {
            SetupMocks();
            ActiveStrategy activeStrategy = new ActiveStrategy(_mockRepo.Object, _mockPluginLoader.Object);

            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "PerAttraction",
            });

            IConcreteStrategy result1 = activeStrategy.GetStrategy();
            IConcreteStrategy result2 = activeStrategy.GetStrategy();
            IConcreteStrategy result3 = activeStrategy.GetStrategy();

            Assert.AreEqual("PerAttraction", result1.Name);
            Assert.AreEqual("PerAttraction", result2.Name);
            Assert.AreEqual("PerAttraction", result3.Name);
        }

        [TestMethod]
        public void ActiveStrategy_GetStrategy_AfterMultipleSets_ShouldReturnLatestStrategy()
        {
            SetupMocks();
            ActiveStrategy activeStrategy = new ActiveStrategy(_mockRepo.Object, _mockPluginLoader.Object);

            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "PerAttraction",
            });

            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "PerEvent",
            });

            IConcreteStrategy result = activeStrategy.GetStrategy();

            Assert.AreEqual("PerEvent", result.Name);
        }

        [TestMethod]
        public void ActiveStrategy_GetStrategy_AfterSettingCombo_ShouldReturnComboWithCorrectN()
        {
            SetupMocks();
            ActiveStrategy activeStrategy = new ActiveStrategy(_mockRepo.Object, _mockPluginLoader.Object);

            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "Combo",
                N = 45,
            });

            IConcreteStrategy result = activeStrategy.GetStrategy();

            Assert.AreEqual("Combo", result.Name);
            Assert.IsInstanceOfType(result, typeof(Combo));
            Assert.AreEqual(45, ((Combo)result).N);
        }

        [TestMethod]
        public void ActiveStrategy_GetStrategy_AfterChangingFromComboToPerAttraction_ShouldReturnPerAttraction()
        {
            ActiveStrategy activeStrategy = new ActiveStrategy(_mockRepo.Object, _mockPluginLoader.Object);

            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "Combo",
                N = 30,
            });

            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "PerAttraction",
            });

            IConcreteStrategy result = activeStrategy.GetStrategy();

            Assert.AreEqual("PerAttraction", result.Name);
            Assert.IsNotInstanceOfType(result, typeof(Combo));
        }

        [TestMethod]
        public void Combo_CalculateScore_ShouldThrowWhenUserIsNull()
        {
            Combo strategy = new Combo(30);
            Attraction attraction = new Attraction { Type = AttractionType.Performance };

            StrategyRequest request = new StrategyRequest
            {
                EnterDate = new DateTime(2025, 10, 5, 10, 0, 0)
            };

            Assert.ThrowsException<ArgumentException>(() => strategy.CalculateScore(null!, attraction, request));
        }

        [TestMethod]
        public void Combo_CalculateScore_ShouldThrowWhenAttractionIsNull()
        {
            SetupMocks();
            Combo strategy = new Combo(30);
            User user = new User { Name = "Test" };

            StrategyRequest request = new StrategyRequest
            {
                EnterDate = new DateTime(2025, 10, 5, 10, 0, 0)
            };

            Assert.ThrowsException<ArgumentException>(() => strategy.CalculateScore(user, null!, request));
        }

        [TestMethod]
        public void Combo_CalculateScore_ShouldThrowWhenEnterDateIsNull()
        {
            SetupMocks();
            Combo strategy = new Combo(30);
            User user = new User { Name = "Test" };
            Attraction attraction = new Attraction { Type = AttractionType.Performance };

            StrategyRequest request = new StrategyRequest
            {
                EnterDate = null
            };

            Assert.ThrowsException<ArgumentException>(() => strategy.CalculateScore(user, attraction, request));
        }

        [TestMethod]
        public void ActiveStrategy_SetStrategy_WithPerEvent_ShouldPersistToDatabase()
        {
            SetupMocks();
            ActiveStrategy activeStrategy = new ActiveStrategy(_mockRepo.Object, _mockPluginLoader.Object);

            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "PerEvent",
            });

            Assert.AreEqual("PerEvent", _storedConfig.StrategyName);
            _mockRepo.Verify(x => x.Update(It.IsAny<StrategyConfiguration>()), Times.Once);
        }

        [TestMethod]
        public void ActiveStrategy_LoadStrategyFromDatabase_ShouldLoadPerEvent()
        {
            _storedConfig = new StrategyConfiguration
            {
                Id = 1,
                StrategyName = "PerEvent",
                N = null
            };
            ActiveStrategy activeStrategy = new ActiveStrategy(_mockRepo.Object, _mockPluginLoader.Object);

            IConcreteStrategy result = activeStrategy.GetStrategy();

            Assert.AreEqual("PerEvent", result.Name);
        }

        [TestMethod]
        public void ActiveStrategy_LoadStrategyFromDatabase_ShouldLoadComboWithN()
        {
            _storedConfig = new StrategyConfiguration
            {
                Id = 1,
                StrategyName = "Combo",
                N = 50
            };
            ActiveStrategy activeStrategy = new ActiveStrategy(_mockRepo.Object, _mockPluginLoader.Object);

            IConcreteStrategy result = activeStrategy.GetStrategy();

            Assert.AreEqual("Combo", result.Name);
            Assert.IsInstanceOfType(result, typeof(Combo));
            Assert.AreEqual(50, ((Combo)result).N);
        }

        [TestMethod]
        public void ActiveStrategy_LoadStrategyFromDatabase_ShouldDefaultToPerAttraction_WhenInvalidStrategy()
        {
            _storedConfig = new StrategyConfiguration
            {
                Id = 1,
                StrategyName = "InvalidStrategyName",
                N = null
            };
            ActiveStrategy activeStrategy = new ActiveStrategy(_mockRepo.Object, _mockPluginLoader.Object);

            IConcreteStrategy result = activeStrategy.GetStrategy();

            Assert.AreEqual("PerAttraction", result.Name);
        }

        [TestMethod]
        public void ActiveStrategy_LoadStrategyFromDatabase_ShouldThrow_WhenComboNIsNull()
        {
            _storedConfig = new StrategyConfiguration
            {
                Id = 1,
                StrategyName = "Combo",
                N = null
            };

            Assert.ThrowsException<ArgumentException>(() =>
            new ActiveStrategy(_mockRepo.Object, _mockPluginLoader.Object));
        }

        [TestMethod]
        public void CalculateScore_WithRealActiveStrategy_DelegatesToUnderlyingStrategy()
        {
            SetupMocks();
            ActiveStrategy activeStrategy = new ActiveStrategy(_mockRepo.Object, _mockPluginLoader.Object);

            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "PerAttraction"
            });

            User user = new User { Name = "Test" };
            Attraction attraction = new Attraction { Type = AttractionType.RollerCoaster };
            StrategyRequest request = new StrategyRequest();

            int score = activeStrategy.CalculateScore(user, attraction, request);

            Assert.AreEqual(2, score);
        }
    }
}