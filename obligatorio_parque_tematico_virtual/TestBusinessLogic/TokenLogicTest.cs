using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Domain;
using IBusinessLogic;
using BusinessLogic;
using Models.Out;

namespace TestBusinessLogic
{
    [TestClass]
    public class TokenLogicTest
    {
        private ITokenLogic _tokenLogic;

        [TestInitialize]
        public void Setup()
        {
            Microsoft.Extensions.Options.IOptions<Models.JwtSettings> jwtSettings = Microsoft.Extensions.Options.Options.Create(new Models.JwtSettings
            {
                SecretKey = "MySecretKeyForJWTTokenGeneration1234567890",
                Issuer = "ParqueTematico",
                Audience = "ParqueTematico",
                ExpirationHours = 1
            });
            _tokenLogic = new TokenLogic(jwtSettings);
        }

        [TestMethod]
        public void GenerateToken_ShouldReturnValidJwtToken_ForAdministrator()
        {
            UserResponse admin = new UserResponse
            {
                Id = Guid.NewGuid(),
                Name = "Admin",
                LastName = "User",
                Email = "admin@test.com",
                UserRoles = new List<string> { Role.ADMINISTRATOR }
            };

            string token = _tokenLogic.GenerateToken(admin);

            Assert.IsTrue(token.Length > 50);

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = handler.ReadJwtToken(token);

            Assert.AreEqual("admin@test.com", jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value);
            Assert.AreEqual("Administrator", jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value);
            Assert.AreEqual("Admin User", jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value);
        }

        [TestMethod]
        public void GenerateToken_ShouldReturnValidJwtToken_ForOperator()
        {
            UserResponse op = new UserResponse
            {
                Id = Guid.NewGuid(),
                Name = "Operator",
                LastName = "User",
                Email = "operator@test.com",
                UserRoles = new List<string> { Role.OPERATOR }
            };

            string token = _tokenLogic.GenerateToken(op);

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = handler.ReadJwtToken(token);

            Assert.AreEqual("operator@test.com", jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value);
            Assert.AreEqual("Operator", jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value);
        }

        [TestMethod]
        public void GenerateToken_ShouldReturnValidJwtToken_ForVisitor()
        {
            UserResponse visitor = new UserResponse
            {
                Id = Guid.NewGuid(),
                Name = "Visitor",
                LastName = "User",
                Email = "visitor@test.com",
                BirthDate = new DateTime(1990, 1, 1),
                MembershipLevel = (int)Domain.MembershipLevel.Premium,
                UserRoles = new List<string> { Role.VISITOR }
            };

            string token = _tokenLogic.GenerateToken(visitor);

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = handler.ReadJwtToken(token);

            Assert.AreEqual("visitor@test.com", jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value);
            Assert.AreEqual("Visitor", jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value);
            Assert.AreEqual("Visitor User", jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value);
        }

        [TestMethod]
        public void GenerateToken_ShouldIncludeExpirationClaim()
        {
            UserResponse admin = new UserResponse
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                LastName = "User",
                Email = "test@test.com",
                UserRoles = new List<string> { Role.ADMINISTRATOR }
            };

            string token = _tokenLogic.GenerateToken(admin);

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = handler.ReadJwtToken(token);

            Assert.IsTrue(jwtToken.ValidTo > DateTime.UtcNow);
            Assert.IsTrue(jwtToken.ValidTo <= DateTime.UtcNow.AddHours(1));
        }

        [TestMethod]
        public void GenerateToken_ShouldGenerateDifferentTokens_ForSameUser()
        {
            UserResponse admin = new UserResponse
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                LastName = "User",
                Email = "test@test.com",
                UserRoles = new List<string> { Role.ADMINISTRATOR }
            };

            string token1 = _tokenLogic.GenerateToken(admin);
            string token2 = _tokenLogic.GenerateToken(admin);

            Assert.AreNotEqual(token1, token2);
        }

        [TestMethod]
        public void GenerateToken_ShouldIncludeAllRoles_ForUserWithMultipleRoles()
        {
            UserResponse user = new UserResponse
            {
                Id = Guid.NewGuid(),
                Name = "Multi",
                LastName = "Role",
                Email = "multi@test.com",
                UserRoles = new List<string> { Role.ADMINISTRATOR, Role.OPERATOR }
            };

            string token = _tokenLogic.GenerateToken(user);

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = handler.ReadJwtToken(token);

            List<Claim> roleClaims = jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
            Assert.AreEqual(2, roleClaims.Count);
            Assert.IsTrue(roleClaims.Any(c => c.Value == "Administrator"));
            Assert.IsTrue(roleClaims.Any(c => c.Value == "Operator"));
        }

        [TestMethod]
        public void GenerateToken_ShouldIncludeSingleRole_ForUserWithOneRole()
        {
            UserResponse user = new UserResponse
            {
                Id = Guid.NewGuid(),
                Name = "Single",
                LastName = "Role",
                Email = "single@test.com",
                UserRoles = new List<string> { Role.VISITOR }
            };

            string token = _tokenLogic.GenerateToken(user);

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = handler.ReadJwtToken(token);

            List<Claim> roleClaims = jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
            Assert.AreEqual(1, roleClaims.Count);
            Assert.AreEqual("Visitor", roleClaims[0].Value);
        }

        [TestMethod]
        public void GenerateToken_ShouldNotIncludeRoleClaim_ForUserWithoutRoles()
        {
            UserResponse user = new UserResponse
            {
                Id = Guid.NewGuid(),
                Name = "No",
                LastName = "Roles",
                Email = "noroles@test.com",
                UserRoles = new List<string>()
            };

            string token = _tokenLogic.GenerateToken(user);

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = handler.ReadJwtToken(token);

            Claim? roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            Assert.IsNull(roleClaim);
        }
    }
}
