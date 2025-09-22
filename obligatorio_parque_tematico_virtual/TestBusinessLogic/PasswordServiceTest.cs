using Microsoft.VisualStudio.TestTools.UnitTesting;
using IBusinessLogic;
using BusinessLogic;

namespace TestBusinessLogic
{
    [TestClass]
    public class PasswordServiceTest
    {
        private IPasswordService _passwordService;

        [TestInitialize]
        public void Setup()
        {
            _passwordService = new PasswordService();
        }

        [TestMethod]
        public void HashPassword_ShouldReturnHashedPassword()
        {
            string plainPassword = "myPassword123";

            string hashedPassword = _passwordService.HashPassword(plainPassword);

            Assert.IsNotNull(hashedPassword);
            Assert.AreNotEqual(plainPassword, hashedPassword);
            Assert.IsTrue(hashedPassword.Length > 50);
        }

        [TestMethod]
        public void HashPassword_ShouldGenerateDifferentHashesForSamePassword()
        {
            string plainPassword = "samePassword";

            string hash1 = _passwordService.HashPassword(plainPassword);
            string hash2 = _passwordService.HashPassword(plainPassword);

            Assert.AreNotEqual(hash1, hash2);
        }

        [TestMethod]
        public void VerifyPassword_ShouldReturnTrue_WhenPasswordMatches()
        {
            string plainPassword = "correctPassword";
            string hashedPassword = _passwordService.HashPassword(plainPassword);

            bool result = _passwordService.VerifyPassword(plainPassword, hashedPassword);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void VerifyPassword_ShouldReturnFalse_WhenPasswordDoesNotMatch()
        {
            string plainPassword = "correctPassword";
            string wrongPassword = "wrongPassword";
            string hashedPassword = _passwordService.HashPassword(plainPassword);

            bool result = _passwordService.VerifyPassword(wrongPassword, hashedPassword);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void VerifyPassword_ShouldReturnFalse_WhenHashIsEmpty()
        {
            string plainPassword = "password";
            string emptyHash = "";

            bool result = _passwordService.VerifyPassword(plainPassword, emptyHash);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void HashPassword_ShouldHandleSpecialCharacters()
        {
            string complexPassword = "P@ssw0rd!#$%&*()";

            string hashedPassword = _passwordService.HashPassword(complexPassword);
            bool isValid = _passwordService.VerifyPassword(complexPassword, hashedPassword);

            Assert.IsNotNull(hashedPassword);
            Assert.IsTrue(isValid);
        }
    }
}