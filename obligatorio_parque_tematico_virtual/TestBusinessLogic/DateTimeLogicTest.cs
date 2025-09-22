using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using IBusinessLogic;
using BusinessLogic;

namespace TestBusinessLogic
{
    [TestClass]
    public class DateTimeLogicTest
    {
        private IDateTimeLogic _dateTimeLogic;

        [TestInitialize]
        public void Setup()
        {
            _dateTimeLogic = DateTimeLogic.Instance;
            DateTimeLogic.ResetInstance();
        }

        [TestMethod]
        public void GetCurrentDateTime_ShouldReturnSystemTime_WhenNotConfigured()
        {
            DateTime before = DateTime.Now;
            DateTime result = _dateTimeLogic.GetCurrentDateTime();
            DateTime after = DateTime.Now;

            Assert.IsTrue(result >= before && result <= after);
        }

        [TestMethod]
        public void SetDateTime_ShouldSetConfiguredTime()
        {
            DateTime configuredTime = new DateTime(2025, 9, 2, 14, 45, 0);

            _dateTimeLogic.SetDateTime(configuredTime);
            DateTime result = _dateTimeLogic.GetCurrentDateTime();

            Assert.AreEqual(configuredTime, result);
        }

        [TestMethod]
        public void GetCurrentDateTime_ShouldReturnConfiguredTime_AfterSetting()
        {
            DateTime configuredTime = new DateTime(2025, 10, 15, 10, 30, 0);

            _dateTimeLogic.SetDateTime(configuredTime);
            System.Threading.Thread.Sleep(100);
            DateTime result = _dateTimeLogic.GetCurrentDateTime();

            Assert.AreEqual(configuredTime, result);
        }

        [TestMethod]
        public void SetDateTime_WithStringFormat_ShouldParseCorrectly()
        {
            string dateTimeString = "2025-09-02T14:45";
            DateTime expectedTime = new DateTime(2025, 9, 2, 14, 45, 0);

            _dateTimeLogic.SetDateTime(dateTimeString);
            DateTime result = _dateTimeLogic.GetCurrentDateTime();

            Assert.AreEqual(expectedTime, result);
        }

        [TestMethod]
        public void DateTimeLogic_ShouldBeSingleton()
        {
            IDateTimeLogic instance1 = DateTimeLogic.Instance;
            IDateTimeLogic instance2 = DateTimeLogic.Instance;

            Assert.AreSame(instance1, instance2);
        }

        [TestMethod]
        public void SetDateTime_ShouldPersistAcrossInstances()
        {
            DateTime configuredTime = new DateTime(2025, 5, 5, 5, 5, 0);

            DateTimeLogic.Instance.SetDateTime(configuredTime);
            IDateTimeLogic newInstance = DateTimeLogic.Instance;

            Assert.AreEqual(configuredTime, newInstance.GetCurrentDateTime());
        }
    }
}