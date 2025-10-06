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
        public void ActiveStrategy_GetStrategy_OnSameDayAsUpdate_ShouldReturnPreviousStrategy()
        {
            var activeStrategy = new ActiveStrategy();
            var updateDate = new DateTime(2024, 1, 15, 9, 0, 0);

            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "PerAttraction",
                CurrentDate = new DateTime(2024, 1, 14, 10, 0, 0)
            });

            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "PerEvent",
                CurrentDate = updateDate
            });

            var queryDate = new DateTime(2024, 1, 15, 15, 0, 0);
            IContreteStrategy result = activeStrategy.GetStrategy(queryDate);

            Assert.IsNotNull(result);
            Assert.AreEqual("PerAttraction", result.Name);
        }

        [TestMethod]
        public void ActiveStrategy_GetStrategy_OnDayAfterUpdate_ShouldReturnNewStrategy()
        {
            var activeStrategy = new ActiveStrategy();

            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "PerAttraction",
                CurrentDate = new DateTime(2024, 1, 14, 10, 0, 0)
            });

            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "PerEvent",
                CurrentDate = new DateTime(2024, 1, 15, 9, 0, 0)
            });

            var queryDate = new DateTime(2024, 1, 16, 10, 0, 0);
            IContreteStrategy result = activeStrategy.GetStrategy(queryDate);

            Assert.IsNotNull(result);
            Assert.AreEqual("PerEvent", result.Name);
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
        public void ActiveStrategy_GetStrategy_UpdatedAtMidnight_ShouldUsePreviousStrategySameDay()
        {
            var activeStrategy = new ActiveStrategy();

            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "PerAttraction",
                CurrentDate = new DateTime(2024, 1, 14, 10, 0, 0)
            });

            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "Combo",
                N = 30,
                CurrentDate = new DateTime(2024, 1, 15, 0, 0, 0)
            });

            var queryDate = new DateTime(2024, 1, 15, 23, 59, 59);
            IContreteStrategy result = activeStrategy.GetStrategy(queryDate);

            Assert.IsNotNull(result);
            Assert.AreEqual("PerAttraction", result.Name);
        }

        [TestMethod]
        public void ActiveStrategy_GetStrategy_SettingMultipleDifferentStrategies_ShouldReturnCorrectStrategy()
        {
            var activeStrategy = new ActiveStrategy();

            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "PerAttraction",
                CurrentDate = new DateTime(2024, 1, 14, 9, 0, 0)
            });

            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "PerEvent",
                CurrentDate = new DateTime(2024, 1, 14, 10, 0, 0)
            });

            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "Combo",
                N = 30,
                CurrentDate = new DateTime(2024, 1, 14, 15, 0, 0)
            });

            var queryDay1 = new DateTime(2024, 1, 14, 18, 0, 0);
            IContreteStrategy resultDay1 = activeStrategy.GetStrategy(queryDay1);

            Assert.IsNotNull(resultDay1);
            Assert.AreEqual("PerAttraction", resultDay1.Name);

            activeStrategy.SetStrategy(new SetStrategyRequest
            {
                StrategyName = "PerEvent",
                CurrentDate = new DateTime(2024, 1, 15, 9, 0, 0)
            });

            var queryDay2 = new DateTime(2024, 1, 15, 14, 0, 0);
            IContreteStrategy resultDay2 = activeStrategy.GetStrategy(queryDay2);

            Assert.IsNotNull(resultDay2);
            Assert.AreEqual("Combo", resultDay2.Name);

            var queryDay3 = new DateTime(2024, 1, 16, 10, 0, 0);
            IContreteStrategy resultDay3 = activeStrategy.GetStrategy(queryDay3);

            Assert.IsNotNull(resultDay3);
            Assert.AreEqual("PerEvent", resultDay3.Name);
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