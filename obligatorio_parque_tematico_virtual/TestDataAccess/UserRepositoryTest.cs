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
        public void Create_ShouldAddUserToDatabase()
        {
            Administrator admin = new Administrator
            {
                Name = "Admin",
                LastName = "User",
                Email = "newadmin@test.com",
                Password = "hashedPassword"
            };

            User result = _userRepository.Create(admin);

            Assert.IsNotNull(result);
            Assert.AreEqual("newadmin@test.com", result.Email);
            Assert.AreEqual(3, _context.Users.Count()); // 2 from seed + 1 new
        }

        [TestMethod]
        public void GetByEmail_ShouldReturnUser_WhenUserExists()
        {
            Visitor visitor = new Visitor
            {
                Name = "Test",
                LastName = "User",
                Email = "test@test.com",
                Password = "password",
                BirthDate = new DateTime(1990, 1, 1)
            };
            _context.Users.Add(visitor);
            _context.SaveChanges();

            User result = _userRepository.GetByEmail("test@test.com");

            Assert.IsNotNull(result);
            Assert.AreEqual("test@test.com", result.Email);
            Assert.IsInstanceOfType(result, typeof(Visitor));
        }

        [TestMethod]
        public void GetByEmail_ShouldReturnNull_WhenUserDoesNotExist()
        {
            User result = _userRepository.GetByEmail("nonexistent@test.com");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetById_ShouldReturnUser_WhenUserExists()
        {
            Operator op = new Operator
            {
                Name = "Operator",
                LastName = "User",
                Email = "newoperator@test.com",
                Password = "password"
            };
            _context.Users.Add(op);
            _context.SaveChanges();

            User result = _userRepository.GetById(op.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(op.Id, result.Id);
            Assert.IsInstanceOfType(result, typeof(Operator));
        }

        [TestMethod]
        public void GetById_ShouldReturnNull_WhenUserDoesNotExist()
        {
            Guid nonExistentId = Guid.NewGuid();

            User result = _userRepository.GetById(nonExistentId);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void IsEmailUnique_ShouldReturnFalse_WhenEmailExists()
        {
            Administrator admin = new Administrator
            {
                Name = "Admin",
                LastName = "User",
                Email = "existing@test.com",
                Password = "password"
            };
            _context.Users.Add(admin);
            _context.SaveChanges();

            bool result = _userRepository.IsEmailUnique("existing@test.com");

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsEmailUnique_ShouldReturnTrue_WhenEmailDoesNotExist()
        {
            bool result = _userRepository.IsEmailUnique("new@test.com");

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void GetByIdWithRoles_ShouldReturnUser_WithRoles()
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

            User result = _userRepository.GetByIdWithRoles(user.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(user.Id, result.Id);
            Assert.AreEqual(1, result.UserRoles.Count);
            Assert.AreEqual(Role.ADMINISTRATOR, result.UserRoles.First().Role.Name);
        }

        [TestMethod]
        public void GetByEmailWithRoles_ShouldReturnUser_WithRoles()
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

            User result = _userRepository.GetByEmailWithRoles("test@test.com");

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

    }
}