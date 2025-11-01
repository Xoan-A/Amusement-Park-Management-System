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
        private Mock<IUserRepository> _mockUserRepository;

        [TestInitialize]
        public void Setup()
        {
            _mockDateTimeRepository = new Mock<IDateTimeRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _dateTimeLogic = new DateTimeLogic(_mockDateTimeRepository.Object, _mockUserRepository.Object);
        }

        [TestMethod]
        public async Task GetCurrentDateTime_ShouldReturnSystemTime_WhenNotConfigured()
        {
            _mockDateTimeRepository.Setup(r => r.GetConfiguredDateTime()).ReturnsAsync((DateTime?)null);

            DateTime before = DateTime.Now;
            DateTime result = await _dateTimeLogic.GetCurrentDateTime();
            DateTime after = DateTime.Now;

            Assert.IsTrue(result >= before && result <= after);
        }

        [TestMethod]
        public async Task GetCurrentDateTime_ShouldReturnConfiguredTime_WhenConfigured()
        {
            DateTime configuredTime = new DateTime(2025, 9, 2, 14, 45, 0);
            _mockDateTimeRepository.Setup(r => r.GetConfiguredDateTime()).ReturnsAsync(configuredTime);

            DateTime result = await _dateTimeLogic.GetCurrentDateTime();

            Assert.AreEqual(configuredTime, result);
        }

        [TestMethod]
        public async Task SetDateTime_ShouldCallRepository()
        {
            DateTime configuredTime = new DateTime(2025, 9, 2, 14, 45, 0);

            await _dateTimeLogic.SetDateTime(configuredTime);

            _mockDateTimeRepository.Verify(r => r.SetConfiguredDateTime(configuredTime), Times.Once);
        }

        [TestMethod]
        public async Task SetDateTime_ShouldResetUserScores()
        {
            DateTime configuredTime = new DateTime(2025, 9, 2, 14, 45, 0);

            await _dateTimeLogic.SetDateTime(configuredTime);

            _mockUserRepository.Verify(r => r.ResetScores(), Times.Once);
        }

        [TestMethod]
        public async Task SetDateTime_WithStringFormat_ShouldParseAndCallRepository()
        {
            string dateTimeString = "2025-09-02T14:45";
            DateTime expectedTime = new DateTime(2025, 9, 2, 14, 45, 0);

            await _dateTimeLogic.SetDateTime(dateTimeString);

            _mockDateTimeRepository.Verify(r => r.SetConfiguredDateTime(expectedTime), Times.Once);
        }

        [TestMethod]
        public async Task SetDateTime_WithStringFormat_ShouldResetUserScores()
        {
            string dateTimeString = "2025-09-02T14:45";

            await _dateTimeLogic.SetDateTime(dateTimeString);

            _mockUserRepository.Verify(r => r.ResetScores(), Times.Once);
        }
    }
}