using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Domain;
using IBusinessLogic;
using BusinessLogic;

namespace TestBusinessLogic
{
    [TestClass]
    public class TokenLogicTest
    {
        private ITokenLogic _tokenLogic;

        [TestInitialize]
        public void Setup()
        {
            var jwtSettings = Microsoft.Extensions.Options.Options.Create(new Models.JwtSettings
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
            User admin = new User
            {
                Id = Guid.NewGuid(),
                Name = "Admin",
                LastName = "User",
                Email = "admin@test.com",
                Password = "hashedPassword"
            };
            admin.UserRoles = new System.Collections.Generic.List<UserRole>
            {
                new UserRole { Role = new Role { Name = Role.ADMINISTRATOR } }
            };

            string token = _tokenLogic.GenerateToken(admin);

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
            User op = new User
            {
                Id = Guid.NewGuid(),
                Name = "Operator",
                LastName = "User",
                Email = "operator@test.com",
                Password = "hashedPassword"
            };
            op.UserRoles = new System.Collections.Generic.List<UserRole>
            {
                new UserRole { Role = new Role { Name = Role.OPERATOR } }
            };

            string token = _tokenLogic.GenerateToken(op);

            Assert.IsNotNull(token);
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = handler.ReadJwtToken(token);

            Assert.AreEqual("operator@test.com", jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value);
            Assert.AreEqual("Operator", jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value);
        }

        [TestMethod]
        public void GenerateToken_ShouldReturnValidJwtToken_ForVisitor()
        {
            User visitor = new User
            {
                Id = Guid.NewGuid(),
                Name = "Visitor",
                LastName = "User",
                Email = "visitor@test.com",
                Password = "hashedPassword",
                BirthDate = new DateTime(1990, 1, 1),
                MembershipLevel = MembershipLevel.Premium
            };
            visitor.UserRoles = new System.Collections.Generic.List<UserRole>
            {
                new UserRole { Role = new Role { Name = Role.VISITOR } }
            };

            string token = _tokenLogic.GenerateToken(visitor);

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
            User admin = new User
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                LastName = "User",
                Email = "test@test.com",
                Password = "password"
            };
            admin.UserRoles = new System.Collections.Generic.List<UserRole>
            {
                new UserRole { Role = new Role { Name = Role.ADMINISTRATOR } }
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
            User admin = new User
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                LastName = "User",
                Email = "test@test.com",
                Password = "password"
            };
            admin.UserRoles = new System.Collections.Generic.List<UserRole>
            {
                new UserRole { Role = new Role { Name = Role.ADMINISTRATOR } }
            };

            string token1 = _tokenLogic.GenerateToken(admin);
            string token2 = _tokenLogic.GenerateToken(admin);

            Assert.AreNotEqual(token1, token2);
        }

        [TestMethod]
        public void GenerateToken_ShouldIncludeAllRoles_ForUserWithMultipleRoles()
        {
            User user = new User
            {
                Id = Guid.NewGuid(),
                Name = "Multi",
                LastName = "Role",
                Email = "multi@test.com",
                Password = "hashedPassword"
            };

            Role adminRole = new Role { Id = 1, Name = "Administrator" };
            Role operatorRole = new Role { Id = 2, Name = "Operator" };

            user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = adminRole.Id, Role = adminRole });
            user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = operatorRole.Id, Role = operatorRole });

            string token = _tokenLogic.GenerateToken(user);

            Assert.IsNotNull(token);
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = handler.ReadJwtToken(token);

            var roleClaims = jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
            Assert.AreEqual(2, roleClaims.Count);
            Assert.IsTrue(roleClaims.Any(c => c.Value == "Administrator"));
            Assert.IsTrue(roleClaims.Any(c => c.Value == "Operator"));
        }

        [TestMethod]
        public void GenerateToken_ShouldIncludeSingleRole_ForUserWithOneRole()
        {
            User user = new User
            {
                Id = Guid.NewGuid(),
                Name = "Single",
                LastName = "Role",
                Email = "single@test.com",
                Password = "hashedPassword"
            };

            Role visitorRole = new Role { Id = 3, Name = "Visitor" };
            user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = visitorRole.Id, Role = visitorRole });

            string token = _tokenLogic.GenerateToken(user);

            Assert.IsNotNull(token);
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = handler.ReadJwtToken(token);

            var roleClaims = jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
            Assert.AreEqual(1, roleClaims.Count);
            Assert.AreEqual("Visitor", roleClaims[0].Value);
        }

        [TestMethod]
        public void GenerateToken_ShouldNotIncludeRoleClaim_ForUserWithoutRoles()
        {
            User user = new User
            {
                Id = Guid.NewGuid(),
                Name = "No",
                LastName = "Roles",
                Email = "noroles@test.com",
                Password = "hashedPassword"
            };

            string token = _tokenLogic.GenerateToken(user);

            Assert.IsNotNull(token);
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = handler.ReadJwtToken(token);

            var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            Assert.IsNull(roleClaim);
        }
    }
}