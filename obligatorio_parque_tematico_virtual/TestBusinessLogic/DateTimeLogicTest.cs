using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using IBusinessLogic;
using BusinessLogic;
using Moq;
using IDataAccess;

namespace TestBusinessLogic
{
    [TestClass]
    public class DateTimeLogicTest
    {
        private IDateTimeLogic _dateTimeLogic;
        private Mock<IDateTimeRepository> _mockDateTimeRepository;

        [TestInitialize]
        public void Setup()
        {
            _mockDateTimeRepository = new Mock<IDateTimeRepository>();
            _dateTimeLogic = new DateTimeLogic(_mockDateTimeRepository.Object);
        }

        [TestMethod]
        public void GetCurrentDateTime_ShouldReturnSystemTime_WhenNotConfigured()
        {
            _mockDateTimeRepository.Setup(r => r.GetConfiguredDateTime()).Returns((DateTime?)null);

            DateTime before = DateTime.Now;
            DateTime result = _dateTimeLogic.GetCurrentDateTime();
            DateTime after = DateTime.Now;

            Assert.IsTrue(result >= before && result <= after);
        }

        [TestMethod]
        public void GetCurrentDateTime_ShouldReturnConfiguredTime_WhenConfigured()
        {
            DateTime configuredTime = new DateTime(2025, 9, 2, 14, 45, 0);
            _mockDateTimeRepository.Setup(r => r.GetConfiguredDateTime()).Returns(configuredTime);

            DateTime result = _dateTimeLogic.GetCurrentDateTime();

            Assert.AreEqual(configuredTime, result);
        }

        [TestMethod]
        public void SetDateTime_ShouldCallRepository()
        {
            DateTime configuredTime = new DateTime(2025, 9, 2, 14, 45, 0);

            _dateTimeLogic.SetDateTime(configuredTime);

            _mockDateTimeRepository.Verify(r => r.SetConfiguredDateTime(configuredTime), Times.Once);
        }

        [TestMethod]
        public void SetDateTime_WithStringFormat_ShouldParseAndCallRepository()
        {
            string dateTimeString = "2025-09-02T14:45";
            DateTime expectedTime = new DateTime(2025, 9, 2, 14, 45, 0);

            _dateTimeLogic.SetDateTime(dateTimeString);

            _mockDateTimeRepository.Verify(r => r.SetConfiguredDateTime(expectedTime), Times.Once);
        }
    }
}