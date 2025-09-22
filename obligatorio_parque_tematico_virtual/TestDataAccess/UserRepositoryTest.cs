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
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new AppDbContext(options);
            _userRepository = new UserRepository(_context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Dispose();
        }

        [TestMethod]
        public void Create_ShouldAddUserToDatabase()
        {
            Administrator admin = new Administrator
            {
                Name = "Admin",
                LastName = "User",
                Email = "admin@test.com",
                Password = "hashedPassword"
            };

            User result = _userRepository.Create(admin);

            Assert.IsNotNull(result);
            Assert.AreEqual("admin@test.com", result.Email);
            Assert.AreEqual(1, _context.Users.Count());
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
                Email = "operator@test.com",
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
    }
}