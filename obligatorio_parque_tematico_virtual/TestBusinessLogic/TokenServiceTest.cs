using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Domain;
using IBusinessLogic;
using BusinessLogic;

namespace TestBusinessLogic
{
    [TestClass]
    public class TokenServiceTest
    {
        private ITokenService _tokenService;

        [TestInitialize]
        public void Setup()
        {
            _tokenService = new TokenService();
        }

        [TestMethod]
        public void GenerateToken_ShouldReturnValidJwtToken_ForAdministrator()
        {
            Administrator admin = new Administrator
            {
                Id = Guid.NewGuid(),
                Name = "Admin",
                LastName = "User",
                Email = "admin@test.com",
                Password = "hashedPassword"
            };

            string token = _tokenService.GenerateToken(admin);

            Assert.IsNotNull(token);
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
            Operator op = new Operator
            {
                Id = Guid.NewGuid(),
                Name = "Operator",
                LastName = "User",
                Email = "operator@test.com",
                Password = "hashedPassword"
            };

            string token = _tokenService.GenerateToken(op);

            Assert.IsNotNull(token);
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = handler.ReadJwtToken(token);

            Assert.AreEqual("operator@test.com", jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value);
            Assert.AreEqual("Operator", jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value);
        }

        [TestMethod]
        public void GenerateToken_ShouldReturnValidJwtToken_ForVisitor()
        {
            Visitor visitor = new Visitor
            {
                Id = Guid.NewGuid(),
                Name = "Visitor",
                LastName = "User",
                Email = "visitor@test.com",
                Password = "hashedPassword",
                BirthDate = new DateTime(1990, 1, 1),
                MembershipLevel = MembershipLevel.Premium
            };

            string token = _tokenService.GenerateToken(visitor);

            Assert.IsNotNull(token);
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = handler.ReadJwtToken(token);

            Assert.AreEqual("visitor@test.com", jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value);
            Assert.AreEqual("Visitor", jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value);
            Assert.AreEqual("Visitor User", jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value);
        }

        [TestMethod]
        public void GenerateToken_ShouldIncludeExpirationClaim()
        {
            Administrator admin = new Administrator
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                LastName = "User",
                Email = "test@test.com",
                Password = "password"
            };

            string token = _tokenService.GenerateToken(admin);

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = handler.ReadJwtToken(token);

            Assert.IsTrue(jwtToken.ValidTo > DateTime.UtcNow);
            Assert.IsTrue(jwtToken.ValidTo <= DateTime.UtcNow.AddHours(1));
        }

        [TestMethod]
        public void GenerateToken_ShouldGenerateDifferentTokens_ForSameUser()
        {
            Administrator admin = new Administrator
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                LastName = "User",
                Email = "test@test.com",
                Password = "password"
            };

            string token1 = _tokenService.GenerateToken(admin);
            string token2 = _tokenService.GenerateToken(admin);

            Assert.AreNotEqual(token1, token2);
        }
    }
}