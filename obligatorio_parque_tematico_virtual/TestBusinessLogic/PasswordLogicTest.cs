using IBusinessLogic;
using BusinessLogic;

namespace TestBusinessLogic
{
    [TestClass]
    public class PasswordLogicTest
    {
        private IPasswordLogic _passwordLogic = null!;

        [TestInitialize]
        public void Setup()
        {
            _passwordLogic = new PasswordLogic();
        }

        [TestMethod]
        public void HashPassword_ShouldReturnDifferentValueThanPlainText()
        {
            string plainPassword = "myPassword123";

            string hashedPassword = _passwordLogic.HashPassword(plainPassword);

            Assert.AreNotEqual(plainPassword, hashedPassword);
        }

        [TestMethod]
        public void HashPassword_ShouldReturnLongHash()
        {
            string plainPassword = "myPassword123";

            string hashedPassword = _passwordLogic.HashPassword(plainPassword);

            Assert.IsTrue(hashedPassword.Length > 50);
        }

        [TestMethod]
        public void HashPassword_ShouldGenerateDifferentHashesForSamePassword()
        {
            string plainPassword = "samePassword";

            string hash1 = _passwordLogic.HashPassword(plainPassword);
            string hash2 = _passwordLogic.HashPassword(plainPassword);

            Assert.AreNotEqual(hash1, hash2);
        }

        [TestMethod]
        public void VerifyPassword_ShouldReturnTrue_WhenPasswordMatches()
        {
            string plainPassword = "correctPassword";
            string hashedPassword = _passwordLogic.HashPassword(plainPassword);

            bool result = _passwordLogic.VerifyPassword(plainPassword, hashedPassword);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void VerifyPassword_ShouldReturnFalse_WhenPasswordDoesNotMatch()
        {
            string plainPassword = "correctPassword";
            string wrongPassword = "wrongPassword";
            string hashedPassword = _passwordLogic.HashPassword(plainPassword);

            bool result = _passwordLogic.VerifyPassword(wrongPassword, hashedPassword);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void VerifyPassword_ShouldReturnFalse_WhenHashIsEmpty()
        {
            string plainPassword = "password";
            string emptyHash = "";

            bool result = _passwordLogic.VerifyPassword(plainPassword, emptyHash);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void HashPassword_ShouldHandleSpecialCharacters()
        {
            string complexPassword = "P@ssw0rd!#$%&*()";

            string hashedPassword = _passwordLogic.HashPassword(complexPassword);

            Assert.IsNotNull(hashedPassword);
        }

        [TestMethod]
        public void VerifyPassword_ShouldVerifyHashWithSpecialCharacters()
        {
            string complexPassword = "P@ssw0rd!#$%&*()";

            string hashedPassword = _passwordLogic.HashPassword(complexPassword);
            bool isValid = _passwordLogic.VerifyPassword(complexPassword, hashedPassword);

            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void VerifyPassword_ShouldReturnFalse_WhenHashIsMalformed()
        {
            string plainPassword = "password";
            string malformedHash = "this_is_not_a_valid_bcrypt_hash";

            bool result = _passwordLogic.VerifyPassword(plainPassword, malformedHash);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ValidatePassword_ShouldReturnTrue_WhenPasswordMeetsAllRequirements()
        {
            string validPassword = "Password123";

            bool result = _passwordLogic.ValidatePassword(validPassword);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ValidatePassword_ShouldReturnFalse_WhenPasswordIsTooShort()
        {
            string shortPassword = "Pass1";

            bool result = _passwordLogic.ValidatePassword(shortPassword);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ValidatePassword_ShouldReturnFalse_WhenPasswordHasNoUppercase()
        {
            string noUppercasePassword = "password123";

            bool result = _passwordLogic.ValidatePassword(noUppercasePassword);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ValidatePassword_ShouldReturnFalse_WhenPasswordHasNoLowercase()
        {
            string noLowercasePassword = "PASSWORD123";

            bool result = _passwordLogic.ValidatePassword(noLowercasePassword);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ValidatePassword_ShouldReturnFalse_WhenPasswordHasNoNumber()
        {
            string noNumberPassword = "PasswordABC";

            bool result = _passwordLogic.ValidatePassword(noNumberPassword);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ValidatePassword_ShouldReturnTrue_WhenPasswordIsExactlyEightCharacters()
        {
            string eightCharPassword = "Pass1234";

            bool result = _passwordLogic.ValidatePassword(eightCharPassword);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ValidatePassword_ShouldReturnTrue_WhenPasswordHasSpecialCharacters()
        {
            string specialCharPassword = "Pass@123";

            bool result = _passwordLogic.ValidatePassword(specialCharPassword);

            Assert.IsTrue(result);
        }
    }
}