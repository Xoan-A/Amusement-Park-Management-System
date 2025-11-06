using IBusinessLogic;
using BusinessLogic;
using Moq;
using IDataAccess;

namespace TestBusinessLogic
{
    [TestClass]
    public class DateTimeLogicTest
    {
        private DateTimeLogic _dateTimeLogic;
        private Mock<IDateTimeRepository> _mockDateTimeRepository;

        [TestInitialize]
        public void Setup()
        {
            _mockDateTimeRepository = new Mock<IDateTimeRepository>();
            _dateTimeLogic = new DateTimeLogic(_mockDateTimeRepository.Object);
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
            _mockDateTimeRepository.Setup(r => r.GetConfiguredDateTime()).ReturnsAsync(new DateTime(2025, 9, 1));

            await _dateTimeLogic.SetDateTime(configuredTime);

            _mockDateTimeRepository.Verify(r => r.SetConfiguredDateTime(configuredTime), Times.Once);
        }

        [TestMethod]
        public async Task SetDateTime_ShouldNotifyObservers()
        {
            DateTime configuredTime = new DateTime(2025, 9, 2, 14, 45, 0);
            _mockDateTimeRepository.Setup(r => r.GetConfiguredDateTime()).ReturnsAsync(new DateTime(2025, 9, 1));

            Mock<IDateObserver> mockObserver = new Mock<IDateObserver>();
            _dateTimeLogic.Attach(mockObserver.Object);

            await _dateTimeLogic.SetDateTime(configuredTime);

            mockObserver.Verify(o => o.DateUpdated(_dateTimeLogic), Times.Once);
        }

        [TestMethod]
        public async Task SetDateTime_WithStringFormat_ShouldParseAndCallRepository()
        {
            string dateTimeString = "2025-09-02T14:45";
            DateTime expectedTime = new DateTime(2025, 9, 2, 14, 45, 0);
            _mockDateTimeRepository.Setup(r => r.GetConfiguredDateTime()).ReturnsAsync(new DateTime(2025, 9, 1));

            await _dateTimeLogic.SetDateTime(dateTimeString);

            _mockDateTimeRepository.Verify(r => r.SetConfiguredDateTime(expectedTime), Times.Once);
        }

        [TestMethod]
        public async Task SetDateTime_WithStringFormat_ShouldNotifyObservers()
        {
            string dateTimeString = "2025-09-02T14:45";
            _mockDateTimeRepository.Setup(r => r.GetConfiguredDateTime()).ReturnsAsync(new DateTime(2025, 9, 1));

            Mock<IDateObserver> mockObserver = new Mock<IDateObserver>();
            _dateTimeLogic.Attach(mockObserver.Object);

            await _dateTimeLogic.SetDateTime(dateTimeString);

            mockObserver.Verify(o => o.DateUpdated(_dateTimeLogic), Times.Once);
        }

        [TestMethod]
        public void Attach_ShouldAddObserver()
        {
            Mock<IDateObserver> mockObserver = new Mock<IDateObserver>();

            _dateTimeLogic.Attach(mockObserver.Object);
            _dateTimeLogic.Attach(mockObserver.Object);

            _mockDateTimeRepository.Setup(r => r.GetConfiguredDateTime()).ReturnsAsync(new DateTime(2025, 9, 1));
            _dateTimeLogic.SetDateTime(new DateTime(2025, 9, 2)).Wait();

            mockObserver.Verify(o => o.DateUpdated(_dateTimeLogic), Times.Once);
        }

        [TestMethod]
        public void Detach_ShouldRemoveObserver()
        {
            Mock<IDateObserver> mockObserver = new Mock<IDateObserver>();
            _dateTimeLogic.Attach(mockObserver.Object);
            _dateTimeLogic.Detach(mockObserver.Object);

            _mockDateTimeRepository.Setup(r => r.GetConfiguredDateTime()).ReturnsAsync(new DateTime(2025, 9, 1));
            _dateTimeLogic.SetDateTime(new DateTime(2025, 9, 2)).Wait();

            mockObserver.Verify(o => o.DateUpdated(_dateTimeLogic), Times.Never);
        }

        [TestMethod]
        public void GetPreviousDateTime_ShouldReturnPreviousDateTime()
        {
            DateTime previousTime = new DateTime(2025, 9, 1, 10, 0, 0);
            DateTime newTime = new DateTime(2025, 9, 2, 14, 45, 0);
            
            _mockDateTimeRepository.Setup(r => r.GetConfiguredDateTime()).ReturnsAsync(previousTime);
            _dateTimeLogic.SetDateTime(newTime).Wait();

            DateTime result = _dateTimeLogic.GetPreviousDateTime();

            Assert.AreEqual(previousTime, result);
        }

        [TestMethod]
        public async Task NotifyDateChange_ShouldCallDateUpdatedOnAllObservers()
        {
            Mock<IDateObserver> mockObserver1 = new Mock<IDateObserver>();
            Mock<IDateObserver> mockObserver2 = new Mock<IDateObserver>();

            _dateTimeLogic.Attach(mockObserver1.Object);
            _dateTimeLogic.Attach(mockObserver2.Object);

            _mockDateTimeRepository.Setup(r => r.GetConfiguredDateTime()).ReturnsAsync(new DateTime(2025, 9, 1));
            await _dateTimeLogic.SetDateTime(new DateTime(2025, 9, 2));

            mockObserver1.Verify(o => o.DateUpdated(_dateTimeLogic), Times.Once);
            mockObserver2.Verify(o => o.DateUpdated(_dateTimeLogic), Times.Once);
        }
    }
}