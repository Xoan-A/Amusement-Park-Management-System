using Domain;

namespace TestDomain
{
    [TestClass]
    public class DateTimeConfigurationTest
    {
        [TestMethod]
        public void DateTimeConfiguration_ShouldCreateInstance_WithConstructor()
        {
            DateTime testDate = new DateTime(2025, 10, 7, 14, 30, 0);

            DateTimeConfiguration config = new DateTimeConfiguration(testDate);

            Assert.AreEqual(testDate, config.CurrentDateTime);
        }

        [TestMethod]
        public void DateTimeConfiguration_ShouldAllowSettingId()
        {
            DateTime testDate = new DateTime(2025, 10, 7, 14, 30, 0);
            DateTimeConfiguration config = new DateTimeConfiguration(testDate);

            config.Id = 5;

            Assert.AreEqual(5, config.Id);
        }

        [TestMethod]
        public void DateTimeConfiguration_ShouldAllowModifyingCurrentDateTime()
        {
            DateTime initialDate = new DateTime(2025, 10, 7, 14, 30, 0);
            DateTime newDate = new DateTime(2025, 12, 25, 10, 0, 0);
            DateTimeConfiguration config = new DateTimeConfiguration(initialDate);

            config.CurrentDateTime = newDate;

            Assert.AreEqual(newDate, config.CurrentDateTime);
        }

        [TestMethod]
        public void DateTimeConfiguration_ShouldHaveDefaultIdValue()
        {
            DateTime testDate = new DateTime(2025, 10, 7, 14, 30, 0);
            DateTimeConfiguration config = new DateTimeConfiguration(testDate);

            Assert.AreEqual(0, config.Id);
        }

        [TestMethod]
        public void DateTimeConfiguration_ShouldSupportDifferentDateTimeValues()
        {
            DateTime pastDate = new DateTime(2020, 1, 1, 0, 0, 0);
            DateTime futureDate = new DateTime(2030, 12, 31, 23, 59, 59);

            DateTimeConfiguration config1 = new DateTimeConfiguration(pastDate);
            DateTimeConfiguration config2 = new DateTimeConfiguration(futureDate);

            Assert.AreEqual(pastDate, config1.CurrentDateTime);
            Assert.AreEqual(futureDate, config2.CurrentDateTime);
        }

        [TestMethod]
        public void DateTimeConfiguration_ShouldPreserveDateTime_WithMilliseconds()
        {
            DateTime preciseDate = new DateTime(2025, 10, 7, 14, 30, 45, 123);

            DateTimeConfiguration config = new DateTimeConfiguration(preciseDate);

            Assert.AreEqual(preciseDate, config.CurrentDateTime);
            Assert.AreEqual(123, config.CurrentDateTime.Millisecond);
        }
    }
}