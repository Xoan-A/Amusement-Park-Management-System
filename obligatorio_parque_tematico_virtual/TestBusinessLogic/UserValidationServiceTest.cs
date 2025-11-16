using Moq;
using Domain;
using IBusinessLogic;
using IDataAccess;
using BusinessLogic;

namespace TestBusinessLogic
{
    [TestClass]
    public class UserValidationServiceTest
    {
        private Mock<IUserRepository> _mockUserRepository = null!;
        private Mock<IDateTimeLogic> _mockDateTimeLogic = null!;
        private IUserValidationService _validationService = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockDateTimeLogic = new Mock<IDateTimeLogic>();
            _mockDateTimeLogic.Setup(x => x.GetCurrentDateTime()).Returns(new DateTime(2025, 1, 15));

            _validationService = new UserValidationService(_mockUserRepository.Object, _mockDateTimeLogic.Object);
        }

        [TestMethod]
        public void ValidateEmail_ShouldReturnTrue_WhenEmailIsValid()
        {
            string validEmail = "user@example.com";

            bool result = _validationService.ValidateEmail(validEmail);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ValidateEmail_ShouldReturnFalse_WhenEmailIsEmpty()
        {
            string emptyEmail = "";

            bool result = _validationService.ValidateEmail(emptyEmail);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ValidateEmail_ShouldReturnFalse_WhenEmailHasNoAtSymbol()
        {
            string invalidEmail = "userexample.com";

            bool result = _validationService.ValidateEmail(invalidEmail);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ValidateEmail_ShouldReturnFalse_WhenEmailStartsWithAt()
        {
            string invalidEmail = "@example.com";

            bool result = _validationService.ValidateEmail(invalidEmail);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ValidateEmail_ShouldReturnFalse_WhenEmailEndsWithAt()
        {
            string invalidEmail = "user@";

            bool result = _validationService.ValidateEmail(invalidEmail);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ValidateEmail_ShouldReturnFalse_WhenEmailHasMultipleAtSymbols()
        {
            string invalidEmail = "user@@example.com";

            bool result = _validationService.ValidateEmail(invalidEmail);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ValidateEmail_ShouldReturnFalse_WhenDomainHasNoDot()
        {
            string invalidEmail = "user@examplecom";

            bool result = _validationService.ValidateEmail(invalidEmail);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ValidateBirthDate_ShouldNotThrow_WhenBirthDateIsInThePast()
        {
            DateTime validBirthDate = new DateTime(2000, 1, 1);

            _validationService.ValidateBirthDate(validBirthDate);
        }

        [TestMethod]
        public void ValidateBirthDate_ShouldThrowArgumentException_WhenBirthDateIsToday()
        {
            DateTime today = new DateTime(2025, 1, 15);

            ArgumentException exception = Assert.ThrowsException<ArgumentException>(
                () => _validationService.ValidateBirthDate(today)
            );

            Assert.AreEqual("Birth date cannot be after today.", exception.Message);
        }

        [TestMethod]
        public void ValidateBirthDate_ShouldThrowArgumentException_WhenBirthDateIsInTheFuture()
        {
            DateTime futureBirthDate = new DateTime(2026, 1, 1);

            ArgumentException exception = Assert.ThrowsException<ArgumentException>(
                () => _validationService.ValidateBirthDate(futureBirthDate)
            );

            Assert.AreEqual("Birth date cannot be after today.", exception.Message);
        }

        [TestMethod]
        public void ValidateRequiredFields_ShouldNotThrow_WhenAllFieldsAreProvided()
        {
            string name = "John";
            string lastName = "Doe";
            string email = "john@example.com";
            string password = "password123";

            _validationService.ValidateRequiredFields(name, lastName, email, password);
        }

        [TestMethod]
        public void ValidateRequiredFields_ShouldThrowArgumentException_WhenNameIsEmpty()
        {
            ArgumentException exception = Assert.ThrowsException<ArgumentException>(
                () => _validationService.ValidateRequiredFields("", "Doe", "john@example.com", "password123")
            );

            Assert.AreEqual("Name, last name, email, and password must be provided.", exception.Message);
        }

        [TestMethod]
        public void ValidateRequiredFields_ShouldThrowArgumentException_WhenLastNameIsEmpty()
        {
            ArgumentException exception = Assert.ThrowsException<ArgumentException>(
                () => _validationService.ValidateRequiredFields("John", "", "john@example.com", "password123")
            );

            Assert.AreEqual("Name, last name, email, and password must be provided.", exception.Message);
        }

        [TestMethod]
        public void ValidateRequiredFields_ShouldThrowArgumentException_WhenEmailIsEmpty()
        {
            ArgumentException exception = Assert.ThrowsException<ArgumentException>(
                () => _validationService.ValidateRequiredFields("John", "Doe", "", "password123")
            );

            Assert.AreEqual("Name, last name, email, and password must be provided.", exception.Message);
        }

        [TestMethod]
        public void ValidateRequiredFields_ShouldThrowArgumentException_WhenPasswordIsEmpty()
        {
            ArgumentException exception = Assert.ThrowsException<ArgumentException>(
                () => _validationService.ValidateRequiredFields("John", "Doe", "john@example.com", "")
            );

            Assert.AreEqual("Name, last name, email, and password must be provided.", exception.Message);
        }

        [TestMethod]
        public void ValidateEmailUniqueness_ShouldNotThrow_WhenEmailIsUnique()
        {
            string uniqueEmail = "unique@example.com";
            _mockUserRepository.Setup(r => r.IsEmailUnique(uniqueEmail)).Returns(true);

            _validationService.ValidateEmailUniqueness(uniqueEmail);

            _mockUserRepository.Verify(r => r.IsEmailUnique(uniqueEmail), Times.Once);
        }

        [TestMethod]
        public void ValidateEmailUniqueness_ShouldThrowArgumentException_WhenEmailIsNotUnique()
        {
            string existingEmail = "existing@example.com";
            _mockUserRepository.Setup(r => r.IsEmailUnique(existingEmail)).Returns(false);

            ArgumentException exception = Assert.ThrowsException<ArgumentException>(
                () => _validationService.ValidateEmailUniqueness(existingEmail)
            );

            Assert.AreEqual("Email is already in use.", exception.Message);
            _mockUserRepository.Verify(r => r.IsEmailUnique(existingEmail), Times.Once);
        }

        [TestMethod]
        public void ValidateMembershipLevel_ShouldNotThrow_WhenMembershipLevelIsValid()
        {
            string validLevel = "Premium";

            _validationService.ValidateMembershipLevel(validLevel);
        }

        [TestMethod]
        public void ValidateMembershipLevel_ShouldThrowArgumentException_WhenMembershipLevelIsInvalid()
        {
            string invalidLevel = "InvalidLevel";

            ArgumentException exception = Assert.ThrowsException<ArgumentException>(
                () => _validationService.ValidateMembershipLevel(invalidLevel)
            );

            Assert.AreEqual("Invalid membership level.", exception.Message);
        }

        [TestMethod]
        public void ValidateMembershipLevel_ShouldNotThrow_WhenMembershipLevelIsStandard()
        {
            string standardLevel = "Standard";

            _validationService.ValidateMembershipLevel(standardLevel);
        }

        [TestMethod]
        public void ValidateMembershipLevel_ShouldNotThrow_WhenMembershipLevelIsVIP()
        {
            string vipLevel = "VIP";

            _validationService.ValidateMembershipLevel(vipLevel);
        }
    }
}
