using IBusinessLogic;
using BusinessLogic;
using Moq;
using IDataAccess;

namespace TestBusinessLogic
{
    [TestClass]
    public class DateTimeLogicTest
    {
        private DateTimeLogic _dateTimeLogic = null!;
        private Mock<IDateTimeRepository> _mockDateTimeRepository = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockDateTimeRepository = new Mock<IDateTimeRepository>(MockBehavior.Strict);
            _dateTimeLogic = new DateTimeLogic(_mockDateTimeRepository.Object, new List<IDateObserver>());
        }

        [TestMethod]
        public void GetCurrentDateTime_ShouldReturnSystemTime_WhenNotConfigured()
        {
            _mockDateTimeRepository.Setup(r => r.GetConfiguredDateTime()).Returns((DateTime?)null);

            DateTime before = DateTime.Now;
            DateTime result = _dateTimeLogic.GetCurrentDateTime();
            DateTime after = DateTime.Now;

            Assert.IsTrue(result >= before && result <= after);
            _mockDateTimeRepository.Verify(r => r.GetConfiguredDateTime(), Times.Once);
        }

        [TestMethod]
        public void GetCurrentDateTime_ShouldReturnConfiguredTime_WhenConfigured()
        {
            DateTime configuredTime = new DateTime(2025, 9, 2, 14, 45, 0);
            _mockDateTimeRepository.Setup(r => r.GetConfiguredDateTime()).Returns(configuredTime);

            DateTime result = _dateTimeLogic.GetCurrentDateTime();

            Assert.AreEqual(configuredTime, result);
            _mockDateTimeRepository.Verify(r => r.GetConfiguredDateTime(), Times.Once);
        }

        [TestMethod]
        public void SetDateTime_ShouldCallRepository()
        {
            DateTime configuredTime = new DateTime(2025, 9, 2, 14, 45, 0);
            _mockDateTimeRepository.Setup(r => r.GetConfiguredDateTime()).Returns(new DateTime(2025, 9, 1));
            _mockDateTimeRepository.Setup(r => r.SetConfiguredDateTime(configuredTime));

            _dateTimeLogic.SetDateTime(configuredTime);

            _mockDateTimeRepository.Verify(r => r.SetConfiguredDateTime(configuredTime), Times.Once);
            _mockDateTimeRepository.Verify(r => r.GetConfiguredDateTime(), Times.Once);
        }

        [TestMethod]
        public void SetDateTime_ShouldNotifyObservers()
        {
            DateTime configuredTime = new DateTime(2025, 9, 2, 14, 45, 0);
            _mockDateTimeRepository.Setup(r => r.GetConfiguredDateTime()).Returns(new DateTime(2025, 9, 1));
            _mockDateTimeRepository.Setup(r => r.SetConfiguredDateTime(configuredTime));

            Mock<IDateObserver> mockObserver = new Mock<IDateObserver>(MockBehavior.Strict);
            mockObserver.Setup(o => o.DateUpdated(_dateTimeLogic));
            _dateTimeLogic.Attach(mockObserver.Object);

            _dateTimeLogic.SetDateTime(configuredTime);

            mockObserver.Verify(o => o.DateUpdated(_dateTimeLogic), Times.Once);
            _mockDateTimeRepository.Verify(r => r.SetConfiguredDateTime(configuredTime), Times.Once);
            _mockDateTimeRepository.Verify(r => r.GetConfiguredDateTime(), Times.Once);
        }

        [TestMethod]
        public void Attach_ShouldAddObserver()
        {
            Mock<IDateObserver> mockObserver = new Mock<IDateObserver>(MockBehavior.Strict);
            mockObserver.Setup(o => o.DateUpdated(_dateTimeLogic));

            _dateTimeLogic.Attach(mockObserver.Object);
            _dateTimeLogic.Attach(mockObserver.Object);

            _mockDateTimeRepository.Setup(r => r.GetConfiguredDateTime()).Returns(new DateTime(2025, 9, 1));
            _mockDateTimeRepository.Setup(r => r.SetConfiguredDateTime(new DateTime(2025, 9, 2)));
            _dateTimeLogic.SetDateTime(new DateTime(2025, 9, 2));

            mockObserver.Verify(o => o.DateUpdated(_dateTimeLogic), Times.Once);
            _mockDateTimeRepository.Verify(r => r.GetConfiguredDateTime(), Times.Once);
            _mockDateTimeRepository.Verify(r => r.SetConfiguredDateTime(new DateTime(2025, 9, 2)), Times.Once);
        }

        [TestMethod]
        public void Detach_ShouldRemoveObserver()
        {
            Mock<IDateObserver> mockObserver = new Mock<IDateObserver>(MockBehavior.Strict);
            _dateTimeLogic.Attach(mockObserver.Object);
            _dateTimeLogic.Detach(mockObserver.Object);

            _mockDateTimeRepository.Setup(r => r.GetConfiguredDateTime()).Returns(new DateTime(2025, 9, 1));
            _mockDateTimeRepository.Setup(r => r.SetConfiguredDateTime(new DateTime(2025, 9, 2)));
            _dateTimeLogic.SetDateTime(new DateTime(2025, 9, 2));

            mockObserver.Verify(o => o.DateUpdated(_dateTimeLogic), Times.Never);
            _mockDateTimeRepository.Verify(r => r.GetConfiguredDateTime(), Times.Once);
            _mockDateTimeRepository.Verify(r => r.SetConfiguredDateTime(new DateTime(2025, 9, 2)), Times.Once);
        }

        [TestMethod]
        public void GetPreviousDateTime_ShouldReturnPreviousDateTime()
        {
            DateTime previousTime = new DateTime(2025, 9, 1, 10, 0, 0);
            DateTime newTime = new DateTime(2025, 9, 2, 14, 45, 0);

            _mockDateTimeRepository.Setup(r => r.GetConfiguredDateTime()).Returns(previousTime);
            _mockDateTimeRepository.Setup(r => r.SetConfiguredDateTime(newTime));
            _dateTimeLogic.SetDateTime(newTime);

            DateTime result = _dateTimeLogic.GetPreviousDateTime();

            Assert.AreEqual(previousTime, result);
            _mockDateTimeRepository.Verify(r => r.GetConfiguredDateTime(), Times.Once);
            _mockDateTimeRepository.Verify(r => r.SetConfiguredDateTime(newTime), Times.Once);
        }

        [TestMethod]
        public void NotifyDateChange_ShouldCallDateUpdatedOnAllObservers()
        {
            Mock<IDateObserver> mockObserver1 = new Mock<IDateObserver>(MockBehavior.Strict);
            mockObserver1.Setup(o => o.DateUpdated(_dateTimeLogic));
            Mock<IDateObserver> mockObserver2 = new Mock<IDateObserver>(MockBehavior.Strict);
            mockObserver2.Setup(o => o.DateUpdated(_dateTimeLogic));

            _dateTimeLogic.Attach(mockObserver1.Object);
            _dateTimeLogic.Attach(mockObserver2.Object);

            _mockDateTimeRepository.Setup(r => r.GetConfiguredDateTime()).Returns(new DateTime(2025, 9, 1));
            _mockDateTimeRepository.Setup(r => r.SetConfiguredDateTime(new DateTime(2025, 9, 2)));
            _dateTimeLogic.SetDateTime(new DateTime(2025, 9, 2));

            mockObserver1.Verify(o => o.DateUpdated(_dateTimeLogic), Times.Once);
            mockObserver2.Verify(o => o.DateUpdated(_dateTimeLogic), Times.Once);
            _mockDateTimeRepository.Verify(r => r.GetConfiguredDateTime(), Times.Once);
            _mockDateTimeRepository.Verify(r => r.SetConfiguredDateTime(new DateTime(2025, 9, 2)), Times.Once);
        }
    }
}