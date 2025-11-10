using IBusinessLogic;
using BusinessLogic;
using System.Security.Claims;

namespace TestBusinessLogic
{
    [TestClass]
    public class ClaimsLogicTest
    {
        private IClaimsLogic _claimsLogic;

        [TestInitialize]
        public void Setup()
        {
            _claimsLogic = new ClaimsLogic();
        }

        [TestMethod]
        public void GetCurrentUserId_ShouldReturnValidGuid_WhenUserHasNameIdentifierClaim()
        {
            Guid expectedUserId = Guid.NewGuid();
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, expectedUserId.ToString())
            };
            ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuthType");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

            Guid result = _claimsLogic.GetCurrentUserId(claimsPrincipal);

            Assert.AreEqual(expectedUserId, result);
        }

        [TestMethod]
        public void GetCurrentUserId_ShouldThrowUnauthorizedException_WhenNameIdentifierClaimIsNull()
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "TestUser")
            };
            ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuthType");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

            UnauthorizedAccessException exception = Assert.ThrowsException<UnauthorizedAccessException>(() =>
                _claimsLogic.GetCurrentUserId(claimsPrincipal)
            );

            Assert.AreEqual("User ID not found in token", exception.Message);
        }

        [TestMethod]
        public void GetCurrentUserId_ShouldThrowUnauthorizedException_WhenNameIdentifierClaimIsEmpty()
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, string.Empty)
            };
            ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuthType");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

            UnauthorizedAccessException exception = Assert.ThrowsException<UnauthorizedAccessException>(() =>
                _claimsLogic.GetCurrentUserId(claimsPrincipal)
            );

            Assert.AreEqual("User ID not found in token", exception.Message);
        }

        [TestMethod]
        public void GetCurrentUserId_ShouldThrowUnauthorizedException_WhenNoClaims()
        {
            ClaimsIdentity identity = new ClaimsIdentity();
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

            UnauthorizedAccessException exception = Assert.ThrowsException<UnauthorizedAccessException>(() =>
                _claimsLogic.GetCurrentUserId(claimsPrincipal)
            );

            Assert.AreEqual("User ID not found in token", exception.Message);
        }

        [TestMethod]
        public void GetCurrentUserId_ShouldThrowFormatException_WhenNameIdentifierIsNotValidGuid()
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "not-a-valid-guid")
            };
            ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuthType");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

            Assert.ThrowsException<FormatException>(() =>
                _claimsLogic.GetCurrentUserId(claimsPrincipal)
            );
        }

        [TestMethod]
        public void GetCurrentUserId_ShouldReturnCorrectGuid_WhenMultipleClaimsExist()
        {
            Guid expectedUserId = Guid.NewGuid();
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Email, "test@example.com"),
                new Claim(ClaimTypes.NameIdentifier, expectedUserId.ToString()),
                new Claim(ClaimTypes.Role, "Admin")
            };
            ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuthType");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

            Guid result = _claimsLogic.GetCurrentUserId(claimsPrincipal);

            Assert.AreEqual(expectedUserId, result);
        }
    }
}

