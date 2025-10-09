using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Domain;
using DataAccess.Context;
using DataAccess.Repositories;
using IDataAccess;

namespace TestDataAccess
{
    [TestClass]
    public class UserRepositoryTest
    {
        private AppDbContext _context;
        private IUserRepository _userRepository;

        [TestInitialize]
        public void Setup()
        {
            DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite("DataSource=:memory:")
                .Options;
            _context = new AppDbContext(options);
            _context.Database.OpenConnection();
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            _userRepository = new UserRepository(_context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Database.CloseConnection();
            _context.Dispose();
        }

        [TestMethod]
        public async Task Create_ShouldAddUserToDatabase()
        {
            User admin = new User
            {
                Name = "Admin",
                LastName = "User",
                Email = "newadmin@test.com",
                Password = "hashedPassword"
            };
            Role adminRole = _context.Roles.First(r => r.Name == Role.ADMINISTRATOR);
            admin.UserRoles = new System.Collections.Generic.List<UserRole>
            {
                new UserRole { RoleId = adminRole.Id }
            };

            User result = await _userRepository.Create(admin);

            Assert.IsNotNull(result);
            Assert.AreEqual("newadmin@test.com", result.Email);
            Assert.AreEqual(3, _context.Users.Count()); // 2 from seed + 1 new
        }

        [TestMethod]
        public async Task GetByEmail_ShouldReturnUser_WhenUserExists()
        {
            User visitor = new User
            {
                Name = "Test",
                LastName = "User",
                Email = "test@test.com",
                Password = "password",
                BirthDate = new DateTime(1990, 1, 1),
                MembershipLevel = MembershipLevel.Standard
            };
            Role visitorRole = _context.Roles.First(r => r.Name == Role.VISITOR);
            visitor.UserRoles = new System.Collections.Generic.List<UserRole>
            {
                new UserRole { RoleId = visitorRole.Id }
            };
            _context.Users.Add(visitor);
            _context.SaveChanges();

            User result = await _userRepository.GetByEmail("test@test.com");

            Assert.IsNotNull(result);
            Assert.AreEqual("test@test.com", result.Email);
            Assert.IsInstanceOfType(result, typeof(User));
        }

        [TestMethod]
        public async Task GetByEmail_ShouldReturnNull_WhenUserDoesNotExist()
        {
            User result = await _userRepository.GetByEmail("nonexistent@test.com");

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetById_ShouldReturnUser_WhenUserExists()
        {
            User op = new User
            {
                Name = "Operator",
                LastName = "User",
                Email = "newoperator@test.com",
                Password = "password"
            };
            Role operatorRole = _context.Roles.First(r => r.Name == Role.OPERATOR);
            op.UserRoles = new System.Collections.Generic.List<UserRole>
            {
                new UserRole { RoleId = operatorRole.Id }
            };
            _context.Users.Add(op);
            _context.SaveChanges();

            User result = await _userRepository.GetById(op.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(op.Id, result.Id);
            Assert.IsInstanceOfType(result, typeof(User));
        }

        [TestMethod]
        public async Task GetById_ShouldReturnNull_WhenUserDoesNotExist()
        {
            Guid nonExistentId = Guid.NewGuid();

            User result = await _userRepository.GetById(nonExistentId);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task IsEmailUnique_ShouldReturnFalse_WhenEmailExists()
        {
            User admin = new User
            {
                Name = "Admin",
                LastName = "User",
                Email = "existing@test.com",
                Password = "password"
            };
            Role adminRole = _context.Roles.First(r => r.Name == Role.ADMINISTRATOR);
            admin.UserRoles = new System.Collections.Generic.List<UserRole>
            {
                new UserRole { RoleId = adminRole.Id }
            };
            _context.Users.Add(admin);
            _context.SaveChanges();

            bool result = await _userRepository.IsEmailUnique("existing@test.com");

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task IsEmailUnique_ShouldReturnTrue_WhenEmailDoesNotExist()
        {
            bool result = await _userRepository.IsEmailUnique("new@test.com");

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task GetByIdWithRoles_ShouldReturnUser_WithRoles()
        {
            _context.Database.EnsureCreated();

            User user = new User
            {
                Name = "MultiRole",
                LastName = "User",
                Email = "multi@test.com",
                Password = "password"
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            Role adminRole = _context.Roles.First(r => r.Name == Role.ADMINISTRATOR);
            UserRole userRole = new UserRole { UserId = user.Id, RoleId = adminRole.Id };
            _context.UserRoles.Add(userRole);
            _context.SaveChanges();

            User result = await _userRepository.GetByIdWithRoles(user.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(user.Id, result.Id);
            Assert.AreEqual(1, result.UserRoles.Count);
            Assert.AreEqual(Role.ADMINISTRATOR, result.UserRoles.First().Role.Name);
        }

        [TestMethod]
        public async Task GetByEmailWithRoles_ShouldReturnUser_WithRoles()
        {
            _context.Database.EnsureCreated();

            User user = new User
            {
                Name = "Test",
                LastName = "User",
                Email = "test@test.com",
                Password = "password"
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            Role visitorRole = _context.Roles.First(r => r.Name == Role.VISITOR);
            UserRole userRole = new UserRole { UserId = user.Id, RoleId = visitorRole.Id };
            _context.UserRoles.Add(userRole);
            _context.SaveChanges();

            User result = await _userRepository.GetByEmailWithRoles("test@test.com");

            Assert.IsNotNull(result);
            Assert.AreEqual("test@test.com", result.Email);
            Assert.AreEqual(1, result.UserRoles.Count);
            Assert.AreEqual(Role.VISITOR, result.UserRoles.First().Role.Name);
        }

        [TestMethod]
        public async Task GetTopTen_ShouldReturnTopTenUsersOrderedByScore()
        {
            _context.Users.RemoveRange(_context.Users);
            _context.SaveChanges();

            for (int i = 1; i <= 15; i++)
            {
                User visitor = new User
                {
                    Name = $"User{i}",
                    LastName = "Test",
                    Email = $"user{i}@test.com",
                    Password = "password",
                    BirthDate = new DateTime(1990, 1, 1),
                    Score = i * 10
                };
                _context.Users.Add(visitor);
            }

            _context.SaveChanges();

            var result = await _userRepository.GetTopTen();

            Assert.IsNotNull(result);
            Assert.AreEqual(10, result.Count);
            Assert.AreEqual(150, result[0].Score);
            Assert.AreEqual(60, result[9].Score);
            for (int i = 0; i < result.Count - 1; i++)
            {
                Assert.IsTrue(result[i].Score >= result[i + 1].Score);
            }
        }

        [TestMethod]
        public async Task GetTopTen_ShouldReturnEmptyList_WhenNoUsersExist()
        {
            _context.Users.RemoveRange(_context.Users);
            _context.SaveChanges();

            var result = await _userRepository.GetTopTen();

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetTopTen_ShouldReturnFewerThanTenUsers_WhenLessThanTenExist()
        {
            _context.Users.RemoveRange(_context.Users);
            _context.SaveChanges();

            for (int i = 1; i <= 5; i++)
            {
                User visitor = new User
                {
                    Name = $"User{i}",
                    LastName = "Test",
                    Email = $"user{i}@test.com",
                    Password = "password",
                    BirthDate = new DateTime(1990, 1, 1),
                    Score = i * 20
                };
                _context.Users.Add(visitor);
            }

            _context.SaveChanges();

            var result = await _userRepository.GetTopTen();

            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.Count);
            Assert.AreEqual(100, result[0].Score);
            Assert.AreEqual(20, result[4].Score);
        }

        [TestMethod]
        public async Task GetTopTen_ShouldReturnExactlyTenUsers_WhenExactlyTenExist()
        {
            _context.Users.RemoveRange(_context.Users);
            _context.SaveChanges();

            for (int i = 1; i <= 10; i++)
            {
                User visitor = new User
                {
                    Name = $"User{i}",
                    LastName = "Test",
                    Email = $"user{i}@test.com",
                    Password = "password",
                    BirthDate = new DateTime(1990, 1, 1),
                    Score = i * 5
                };
                _context.Users.Add(visitor);
            }

            _context.SaveChanges();

            var result = await _userRepository.GetTopTen();

            Assert.IsNotNull(result);
            Assert.AreEqual(10, result.Count);
            Assert.AreEqual(50, result[0].Score);
            Assert.AreEqual(5, result[9].Score);
        }

        [TestMethod]
        public async Task GetTopTen_ShouldReturnUsersWithSameScore_InCorrectOrder()
        {
            _context.Users.RemoveRange(_context.Users);
            _context.SaveChanges();

            for (int i = 1; i <= 12; i++)
            {
                User visitor = new User
                {
                    Name = $"User{i}",
                    LastName = "Test",
                    Email = $"user{i}@test.com",
                    Password = "password",
                    BirthDate = new DateTime(1990, 1, 1),
                    Score = i <= 6 ? 100 : 50
                };
                _context.Users.Add(visitor);
            }

            _context.SaveChanges();

            var result = await _userRepository.GetTopTen();

            Assert.IsNotNull(result);
            Assert.AreEqual(10, result.Count);
            Assert.AreEqual(100, result[0].Score);
            Assert.AreEqual(100, result[5].Score);
            Assert.AreEqual(50, result[6].Score);
        }

        [TestMethod]
        public async Task ResetScores_ShouldSetAllUserScoresToZero()
        {
            _context.Users.RemoveRange(_context.Users);
            _context.SaveChanges();

            for (int i = 1; i <= 5; i++)
            {
                User visitor = new User
                {
                    Name = $"User{i}",
                    LastName = "Test",
                    Email = $"user{i}@test.com",
                    Password = "password",
                    BirthDate = new DateTime(1990, 1, 1),
                    Score = i * 20
                };
                _context.Users.Add(visitor);
            }

            _context.SaveChanges();

            await _userRepository.ResetScores();

            var users = _context.Users.ToList();
            Assert.AreEqual(5, users.Count);
            foreach (var user in users)
            {
                Assert.AreEqual(0, user.Score);
            }
        }

        [TestMethod]
        public async Task ResetScores_ShouldWorkWhenNoUsersExist()
        {
            _context.Users.RemoveRange(_context.Users);
            _context.SaveChanges();

            await _userRepository.ResetScores();

            var users = _context.Users.ToList();
            Assert.AreEqual(0, users.Count);
        }

        [TestMethod]
        public async Task ResetScores_ShouldPersistChangesToDatabase()
        {
            _context.Users.RemoveRange(_context.Users);
            _context.SaveChanges();

            var user1 = new User
            {
                Name = "User1",
                LastName = "Test",
                Email = "user1@test.com",
                Password = "password",
                BirthDate = new DateTime(1990, 1, 1),
                Score = 100
            };
            var user2 = new User
            {
                Name = "User2",
                LastName = "Test",
                Email = "user2@test.com",
                Password = "password",
                BirthDate = new DateTime(1990, 1, 1),
                Score = 200
            };
            _context.Users.Add(user1);
            _context.Users.Add(user2);
            _context.SaveChanges();

            await _userRepository.ResetScores();

            var retrievedUser1 = _context.Users.FirstOrDefault(u => u.Email == "user1@test.com");
            var retrievedUser2 = _context.Users.FirstOrDefault(u => u.Email == "user2@test.com");

            Assert.IsNotNull(retrievedUser1);
            Assert.IsNotNull(retrievedUser2);
            Assert.AreEqual(0, retrievedUser1.Score);
            Assert.AreEqual(0, retrievedUser2.Score);
        }

        [TestMethod]
        public async Task ResetScores_ShouldNotAffectOtherUserProperties()
        {
            _context.Users.RemoveRange(_context.Users);
            _context.SaveChanges();

            var user = new User
            {
                Name = "TestUser",
                LastName = "LastName",
                Email = "test@test.com",
                Password = "password",
                BirthDate = new DateTime(1990, 5, 15),
                Score = 500,
                MembershipLevel = MembershipLevel.Premium
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            await _userRepository.ResetScores();

            var retrievedUser = _context.Users.FirstOrDefault(u => u.Email == "test@test.com");

            Assert.IsNotNull(retrievedUser);
            Assert.AreEqual(0, retrievedUser.Score);
            Assert.AreEqual("TestUser", retrievedUser.Name);
            Assert.AreEqual("LastName", retrievedUser.LastName);
            Assert.AreEqual("test@test.com", retrievedUser.Email);
            Assert.AreEqual("password", retrievedUser.Password);
            Assert.AreEqual(new DateTime(1990, 5, 15), retrievedUser.BirthDate);
            Assert.AreEqual(MembershipLevel.Premium, retrievedUser.MembershipLevel);
        }

        [TestMethod]
        public async Task ResetScores_ShouldResetMultipleUsersWithDifferentScores()
        {
            _context.Users.RemoveRange(_context.Users);
            _context.SaveChanges();

            var scores = new[] { 10, 0, 500, 1000, 75 };
            for (int i = 0; i < scores.Length; i++)
            {
                User visitor = new User
                {
                    Name = $"User{i}",
                    LastName = "Test",
                    Email = $"user{i}@test.com",
                    Password = "password",
                    BirthDate = new DateTime(1990, 1, 1),
                    Score = scores[i]
                };
                _context.Users.Add(visitor);
            }

            _context.SaveChanges();

            await _userRepository.ResetScores();

            var users = _context.Users.ToList();
            Assert.AreEqual(scores.Length, users.Count);
            Assert.IsTrue(users.All(u => u.Score == 0));
        }

        [TestMethod]
        public async Task Update_ShouldUpdateUserInDatabase()
        {
            User user = new User
            {
                Name = "Original",
                LastName = "Name",
                Email = "original@test.com",
                Password = "password",
                BirthDate = new DateTime(1990, 1, 1),
                Score = 50
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            user.Name = "Updated";
            user.LastName = "NewName";
            user.Score = 100;

            await _userRepository.Update(user);

            var updatedUser = _context.Users.FirstOrDefault(u => u.Email == "original@test.com");
            Assert.AreEqual("Updated", updatedUser.Name);
            Assert.AreEqual("NewName", updatedUser.LastName);
            Assert.AreEqual(100, updatedUser.Score);
        }

        [TestMethod]
        public async Task Update_ShouldUpdateUserRoles()
        {
            User user = new User
            {
                Name = "TestUser",
                LastName = "Test",
                Email = "testuser@test.com",
                Password = "password"
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            Role visitorRole = _context.Roles.First(r => r.Name == Role.VISITOR);
            UserRole userRole = new UserRole { UserId = user.Id, RoleId = visitorRole.Id };
            user.UserRoles.Add(userRole);

            await _userRepository.Update(user);

            var updatedUser = await _userRepository.GetByIdWithRoles(user.Id);
            Assert.AreEqual(1, updatedUser.UserRoles.Count);
            Assert.AreEqual(Role.VISITOR, updatedUser.UserRoles.First().Role.Name);
        }
    }
}