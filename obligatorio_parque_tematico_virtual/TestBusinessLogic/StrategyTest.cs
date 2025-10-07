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
        public void ActiveStrategy_SetStrategy_ShouldSetStrategy()
        {
            var activeStrategy = new ActiveStrategy();
            var perAttractionStrategy = new PerAttraction();

            var currentDate = new DateTime(2024, 1, 15, 10, 0, 0);
            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "PerAttraction",
                CurrentDate = currentDate
            });

            IContreteStrategy result = activeStrategy.GetStrategy(currentDate);

            Assert.IsNotNull(result);
            Assert.AreEqual("PerAttraction", result.Name);
        }

        [TestMethod]
        public void ActiveStrategy_GetStrategy_ShouldThrowWhenNoStrategySet()
        {
            var activeStrategy = new ActiveStrategy();
            var currentDate = new DateTime(2024, 1, 15, 10, 0, 0);

            Assert.ThrowsException<InvalidOperationException>(() => activeStrategy.GetStrategy(currentDate));
        }

        [TestMethod]
        public void ActiveStrategy_SetStrategy_WithCombo_ShouldSetComboWithN()
        {
            var activeStrategy = new ActiveStrategy();
            var currentDate = new DateTime(2024, 1, 15, 10, 0, 0);

            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "Combo",
                N = 30,
                CurrentDate = currentDate
            });

            IContreteStrategy result = activeStrategy.GetStrategy(currentDate);

            Assert.IsNotNull(result);
            Assert.AreEqual("Combo", result.Name);
            Assert.IsInstanceOfType(result, typeof(Combo));
            Assert.AreEqual(30, ((Combo)result).N);
        }

        [TestMethod]
        public void ActiveStrategy_SetStrategy_WithCombo_ShouldThrowWhenNIsNull()
        {
            var activeStrategy = new ActiveStrategy();
            var currentDate = new DateTime(2024, 1, 15, 10, 0, 0);

            Assert.ThrowsException<ArgumentException>(() =>
                activeStrategy.SetStrategy(new SetStrategyRequest
                {
                    StrategyName = "Combo",
                    N = null,
                    CurrentDate = currentDate
                }));
        }

        [TestMethod]
        public void ActiveStrategy_SetStrategy_ShouldThrowForInvalidStrategyName()
        {
            var activeStrategy = new ActiveStrategy();
            var currentDate = new DateTime(2024, 1, 15, 10, 0, 0);

            Assert.ThrowsException<ArgumentException>(() =>
                activeStrategy.SetStrategy(new SetStrategyRequest
                {
                    StrategyName = "InvalidStrategy",
                    CurrentDate = currentDate
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
            var activeStrategy = new ActiveStrategy();
            var setDate = new DateTime(2024, 1, 15, 9, 0, 0);

            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "PerAttraction",
                CurrentDate = setDate
            });

            var queryDate = new DateTime(2024, 1, 15, 15, 0, 0);
            IContreteStrategy result = activeStrategy.GetStrategy(queryDate);

            Assert.IsNotNull(result);
            Assert.AreEqual("PerAttraction", result.Name);
        }
        
        [TestMethod]
        public void ActiveStrategy_GetStrategy_ShouldReturnSameStrategyRegardlessOfDate()
        {
            var activeStrategy = new ActiveStrategy();
            var setDate = new DateTime(2024, 1, 15, 9, 0, 0);

            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "PerAttraction",
                CurrentDate = setDate
            });

            var queryDate1 = new DateTime(2024, 1, 15, 10, 0, 0);
            var queryDate2 = new DateTime(2024, 1, 16, 10, 0, 0);
            var queryDate3 = new DateTime(2024, 2, 1, 10, 0, 0);

            IContreteStrategy result1 = activeStrategy.GetStrategy(queryDate1);
            IContreteStrategy result2 = activeStrategy.GetStrategy(queryDate2);
            IContreteStrategy result3 = activeStrategy.GetStrategy(queryDate3);

            Assert.AreEqual("PerAttraction", result1.Name);
            Assert.AreEqual("PerAttraction", result2.Name);
            Assert.AreEqual("PerAttraction", result3.Name);
        }

        [TestMethod]
        public void ActiveStrategy_GetStrategy_AfterMultipleSetsSameDay_ShouldReturnLatestStrategy()
        {
            var activeStrategy = new ActiveStrategy();
            var setDate = new DateTime(2024, 1, 15, 9, 0, 0);

            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "PerAttraction",
                CurrentDate = setDate
            });

            var setDate2 = new DateTime(2024, 1, 15, 12, 0, 0);
            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "PerEvent",
                CurrentDate = setDate2
            });

            var queryDate = new DateTime(2024, 1, 15, 15, 0, 0);
            IContreteStrategy result = activeStrategy.GetStrategy(queryDate);

            Assert.AreEqual("PerEvent", result.Name);
        }

        [TestMethod]
        public void ActiveStrategy_GetStrategy_AfterMultipleSetsDifferentDays_ShouldReturnLatestStrategy()
        {
            var activeStrategy = new ActiveStrategy();
            var setDate1 = new DateTime(2024, 1, 15, 9, 0, 0);

            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "PerAttraction",
                CurrentDate = setDate1
            });

            var setDate2 = new DateTime(2024, 1, 16, 9, 0, 0);
            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "PerEvent",
                CurrentDate = setDate2
            });

            var queryDate = new DateTime(2024, 1, 17, 10, 0, 0);
            IContreteStrategy result = activeStrategy.GetStrategy(queryDate);

            Assert.AreEqual("PerEvent", result.Name);
        }

        [TestMethod]
        public void ActiveStrategy_GetStrategy_AfterSettingCombo_ShouldReturnComboWithCorrectN()
        {
            var activeStrategy = new ActiveStrategy();
            var setDate = new DateTime(2024, 1, 15, 9, 0, 0);

            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "Combo",
                N = 45,
                CurrentDate = setDate
            });

            var queryDate = new DateTime(2024, 1, 15, 15, 0, 0);
            IContreteStrategy result = activeStrategy.GetStrategy(queryDate);

            Assert.AreEqual("Combo", result.Name);
            Assert.IsInstanceOfType(result, typeof(Combo));
            Assert.AreEqual(45, ((Combo)result).N);
        }

        [TestMethod]
        public void ActiveStrategy_GetStrategy_MultipleCallsSameDate_ShouldReturnSameStrategy()
        {
            var activeStrategy = new ActiveStrategy();
            var setDate = new DateTime(2024, 1, 15, 9, 0, 0);

            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "PerAttraction",
                CurrentDate = setDate
            });

            var queryDate = new DateTime(2024, 1, 15, 15, 0, 0);
            IContreteStrategy result1 = activeStrategy.GetStrategy(queryDate);
            IContreteStrategy result2 = activeStrategy.GetStrategy(queryDate);

            Assert.AreSame(result1, result2);
        }

        [TestMethod]
        public void ActiveStrategy_GetStrategy_AfterChangingFromComboToPerAttraction_ShouldReturnPerAttraction()
        {
            var activeStrategy = new ActiveStrategy();
            var setDate1 = new DateTime(2024, 1, 15, 9, 0, 0);

            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "Combo",
                N = 30,
                CurrentDate = setDate1
            });

            var setDate2 = new DateTime(2024, 1, 16, 9, 0, 0);
            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "PerAttraction",
                CurrentDate = setDate2
            });

            var queryDate = new DateTime(2024, 1, 16, 15, 0, 0);
            IContreteStrategy result = activeStrategy.GetStrategy(queryDate);

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