using Microsoft.EntityFrameworkCore;
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
            User admin = new User
            {
                Name = "Admin",
                LastName = "User",
                Email = "newadmin@test.com",
                Password = "hashedPassword"
            };
            Role adminRole = _context.Roles.First(r => r.Name == Role.Administrator);
            admin.UserRoles = new System.Collections.Generic.List<UserRole>
            {
                new UserRole { RoleId = adminRole.Id }
            };

            User result = _userRepository.Create(admin);

            Assert.AreEqual("newadmin@test.com", result.Email);
            Assert.AreEqual(3, _context.Users.Count());
        }

        [TestMethod]
        public void GetById_ShouldReturnUser_WhenUserExists()
        {
            User op = new User
            {
                Name = "Operator",
                LastName = "User",
                Email = "newoperator@test.com",
                Password = "password"
            };
            Role operatorRole = _context.Roles.First(r => r.Name == Role.Operator);
            op.UserRoles = new System.Collections.Generic.List<UserRole>
            {
                new UserRole { RoleId = operatorRole.Id }
            };
            _context.Users.Add(op);
            _context.SaveChanges();

            User result = _userRepository.GetById(op.Id);

            Assert.AreEqual(op.Id, result.Id);
            Assert.IsInstanceOfType(result, typeof(User));
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
            User admin = new User
            {
                Name = "Admin",
                LastName = "User",
                Email = "existing@test.com",
                Password = "password"
            };
            Role adminRole = _context.Roles.First(r => r.Name == Role.Administrator);
            admin.UserRoles = new System.Collections.Generic.List<UserRole>
            {
                new UserRole { RoleId = adminRole.Id }
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

            Role adminRole = _context.Roles.First(r => r.Name == Role.Administrator);
            UserRole userRole = new UserRole { UserId = user.Id, RoleId = adminRole.Id };
            _context.UserRoles.Add(userRole);
            _context.SaveChanges();

            User result = _userRepository.GetByIdWithRoles(user.Id);

            Assert.AreEqual(user.Id, result.Id);
            Assert.AreEqual(1, result.UserRoles.Count);
            Assert.AreEqual(Role.Administrator, result.UserRoles.First().Role.Name);
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

            Role visitorRole = _context.Roles.First(r => r.Name == Role.Visitor);
            UserRole userRole = new UserRole { UserId = user.Id, RoleId = visitorRole.Id };
            _context.UserRoles.Add(userRole);
            _context.SaveChanges();

            User result = _userRepository.GetByEmailWithRoles("test@test.com");

            Assert.AreEqual("test@test.com", result.Email);
            Assert.AreEqual(1, result.UserRoles.Count);
            Assert.AreEqual(Role.Visitor, result.UserRoles.First().Role.Name);
        }

        [TestMethod]
        public void GetTopTen_ShouldReturnTopTenUsersOrderedByScore()
        {
            _context.Users.RemoveRange(_context.Users);
            _context.SaveChanges();

            Role visitorRole = _context.Roles.First(r => r.Name == Role.Visitor);

            for (int i = 1; i <= 15; i++)
            {
                User visitor = new User
                {
                    Name = $"User{i}",
                    LastName = "Test",
                    Email = $"user{i}@test.com",
                    Password = "password",
                    BirthDate = new DateTime(1990, 1, 1),
                    Score = i * 10,
                    DailyScore = i * 10,
                    UserRoles = new List<UserRole>
                    {
                        new UserRole { RoleId = visitorRole.Id }
                    }
                };
                _context.Users.Add(visitor);
            }

            _context.SaveChanges();

            List<User> result = _userRepository.GetTopTen();

            Assert.AreEqual(10, result.Count);
            Assert.AreEqual(150, result[0].DailyScore);
            Assert.AreEqual(60, result[9].DailyScore);
            for (int i = 0; i < result.Count - 1; i++)
            {
                Assert.IsTrue(result[i].DailyScore >= result[i + 1].DailyScore);
            }
        }

        [TestMethod]
        public void GetTopTen_ShouldReturnEmptyList_WhenNoUsersExist()
        {
            _context.Users.RemoveRange(_context.Users);
            _context.SaveChanges();

            List<User> result = _userRepository.GetTopTen();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetTopTen_ShouldReturnFewerThanTenUsers_WhenLessThanTenExist()
        {
            _context.Users.RemoveRange(_context.Users);
            _context.SaveChanges();

            Role visitorRole = _context.Roles.First(r => r.Name == Role.Visitor);

            for (int i = 1; i <= 5; i++)
            {
                User visitor = new User
                {
                    Name = $"User{i}",
                    LastName = "Test",
                    Email = $"user{i}@test.com",
                    Password = "password",
                    BirthDate = new DateTime(1990, 1, 1),
                    Score = i * 20,
                    DailyScore = i * 20,
                    UserRoles = new List<UserRole>
                    {
                        new UserRole { RoleId = visitorRole.Id }
                    }
                };
                _context.Users.Add(visitor);
            }

            _context.SaveChanges();

            List<User> result = _userRepository.GetTopTen();

            Assert.AreEqual(5, result.Count);
            Assert.AreEqual(100, result[0].DailyScore);
            Assert.AreEqual(20, result[4].DailyScore);
        }

        [TestMethod]
        public void GetTopTen_ShouldReturnExactlyTenUsers_WhenExactlyTenExist()
        {
            _context.Users.RemoveRange(_context.Users);
            _context.SaveChanges();

            Role visitorRole = _context.Roles.First(r => r.Name == Role.Visitor);

            for (int i = 1; i <= 10; i++)
            {
                User visitor = new User
                {
                    Name = $"User{i}",
                    LastName = "Test",
                    Email = $"user{i}@test.com",
                    Password = "password",
                    BirthDate = new DateTime(1990, 1, 1),
                    Score = i * 5,
                    DailyScore = i * 5,
                    UserRoles = new List<UserRole>
                    {
                        new UserRole { RoleId = visitorRole.Id }
                    }
                };
                _context.Users.Add(visitor);
            }

            _context.SaveChanges();

            List<User> result = _userRepository.GetTopTen();

            Assert.AreEqual(10, result.Count);
            Assert.AreEqual(50, result[0].DailyScore);
            Assert.AreEqual(5, result[9].DailyScore);
        }

        [TestMethod]
        public void GetTopTen_ShouldReturnUsersWithSameScore_InCorrectOrder()
        {
            _context.Users.RemoveRange(_context.Users);
            _context.SaveChanges();

            Role visitorRole = _context.Roles.First(r => r.Name == Role.Visitor);

            for (int i = 1; i <= 12; i++)
            {
                User visitor = new User
                {
                    Name = $"User{i}",
                    LastName = "Test",
                    Email = $"user{i}@test.com",
                    Password = "password",
                    BirthDate = new DateTime(1990, 1, 1),
                    Score = i <= 6 ? 100 : 50,
                    DailyScore = i <= 6 ? 100 : 50,
                    UserRoles = new List<UserRole>
                    {
                        new UserRole { RoleId = visitorRole.Id }
                    }
                };
                _context.Users.Add(visitor);
            }

            _context.SaveChanges();

            List<User> result = _userRepository.GetTopTen();

            Assert.AreEqual(10, result.Count);
            Assert.AreEqual(100, result[0].DailyScore);
            Assert.AreEqual(100, result[5].DailyScore);
            Assert.AreEqual(50, result[6].DailyScore);
        }

        [TestMethod]
        public void GetTopTen_ShouldOnlyReturnUsersWithVisitorRole()
        {
            Role visitorRole = _context.Roles.First(r => r.Name == Role.Visitor);
            Role adminRole = _context.Roles.First(r => r.Name == Role.Administrator);

            User visitorWithRole = new User
            {
                Name = "VisitorWithRole",
                LastName = "Test",
                Email = "withvisitor@test.com",
                Password = "password",
                BirthDate = new DateTime(1990, 1, 1),
                Score = 100,
                DailyScore = 100,
                UserRoles = new List<UserRole>
                {
                    new UserRole { RoleId = visitorRole.Id }
                }
            };
            _context.Users.Add(visitorWithRole);

            User userWithoutVisitorRole = new User
            {
                Name = "UserWithoutVisitor",
                LastName = "Test",
                Email = "withoutvisitor@test.com",
                Password = "password",
                BirthDate = new DateTime(1990, 1, 1),
                Score = 200,
                DailyScore = 200,
                UserRoles = new List<UserRole>
                {
                    new UserRole { RoleId = adminRole.Id }
                }
            };
            _context.Users.Add(userWithoutVisitorRole);

            _context.SaveChanges();

            List<User> result = _userRepository.GetTopTen();

            Assert.AreEqual(1, result.Count, "Solo debe devolver el usuario con rol Visitor");
            Assert.AreEqual("VisitorWithRole", result[0].Name);
            Assert.AreEqual(100, result[0].DailyScore);
        }

        [TestMethod]
        public void ResetScores_ShouldSetAllUserScoresToZero()
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
                    Score = i * 20,
                    DailyScore = i * 20
                };
                _context.Users.Add(visitor);
            }

            _context.SaveChanges();

            _userRepository.ResetScores();

            List<User> users = _context.Users.ToList();
            Assert.AreEqual(5, users.Count);
            Assert.IsTrue(users.All(u => u.DailyScore == 0));
            Assert.IsTrue(users.All(u => u.Score != 0));
        }

        [TestMethod]
        public void ResetScores_ShouldWorkWhenNoUsersExist()
        {
            _context.Users.RemoveRange(_context.Users);
            _context.SaveChanges();

            _userRepository.ResetScores();

            List<User> users = _context.Users.ToList();
            Assert.AreEqual(0, users.Count);
        }

        [TestMethod]
        public void ResetScores_ShouldPersistChangesToDatabase()
        {
            _context.Users.RemoveRange(_context.Users);
            _context.SaveChanges();

            User user1 = new User
            {
                Name = "User1",
                LastName = "Test",
                Email = "user1@test.com",
                Password = "password",
                BirthDate = new DateTime(1990, 1, 1),
                Score = 100,
                DailyScore = 100
            };
            User user2 = new User
            {
                Name = "User2",
                LastName = "Test",
                Email = "user2@test.com",
                Password = "password",
                BirthDate = new DateTime(1990, 1, 1),
                Score = 200,
                DailyScore = 200
            };
            _context.Users.Add(user1);
            _context.Users.Add(user2);
            _context.SaveChanges();

            _userRepository.ResetScores();

            User? retrievedUser1 = _context.Users.FirstOrDefault(u => u.Email == "user1@test.com");
            User? retrievedUser2 = _context.Users.FirstOrDefault(u => u.Email == "user2@test.com");

            Assert.AreEqual(0, retrievedUser1.DailyScore);
            Assert.AreEqual(0, retrievedUser2.DailyScore);
            Assert.AreEqual(100, retrievedUser1.Score);
        }

        [TestMethod]
        public void ResetScores_ShouldNotAffectOtherUserProperties()
        {
            _context.Users.RemoveRange(_context.Users);
            _context.SaveChanges();

            User user = new User
            {
                Name = "TestUser",
                LastName = "LastName",
                Email = "test@test.com",
                Password = "password",
                BirthDate = new DateTime(1990, 5, 15),
                Score = 500,
                DailyScore = 500,
                MembershipLevel = MembershipLevel.Premium
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            _userRepository.ResetScores();

            User? retrievedUser = _context.Users.FirstOrDefault(u => u.Email == "test@test.com");

            Assert.AreEqual(0, retrievedUser.DailyScore);
            Assert.AreEqual(500, retrievedUser.Score, "Score no debe cambiar");
            Assert.AreEqual("TestUser", retrievedUser.Name);
            Assert.AreEqual("password", retrievedUser.Password);
        }

        [TestMethod]
        public void ResetScores_ShouldResetMultipleUsersWithDifferentScores()
        {
            _context.Users.RemoveRange(_context.Users);
            _context.SaveChanges();

            int[] scores = new[] { 10, 0, 500, 1000, 75 };
            for (int i = 0; i < scores.Length; i++)
            {
                User visitor = new User
                {
                    Name = $"User{i}",
                    LastName = "Test",
                    Email = $"user{i}@test.com",
                    Password = "password",
                    BirthDate = new DateTime(1990, 1, 1),
                    Score = scores[i],
                    DailyScore = scores[i]
                };
                _context.Users.Add(visitor);
            }

            _context.SaveChanges();

            _userRepository.ResetScores();

            List<User> users = _context.Users.ToList();
            Assert.AreEqual(scores.Length, users.Count);
            Assert.IsTrue(users.All(u => u.DailyScore == 0), "Todos los DailyScore deben ser 0");
            Assert.IsFalse(users.All(u => u.Score == 0), "Los Score NO deben ser todos 0");
        }

        [TestMethod]
        public void Update_ShouldUpdateUserInDatabase()
        {
            User user = new User
            {
                Name = "Original",
                LastName = "Name",
                Email = "original@test.com",
                Password = "password",
                BirthDate = new DateTime(1990, 1, 1),
                Score = 50,
                DailyScore = 50
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            user.Name = "Updated";
            user.LastName = "NewName";
            user.Score = 100;
            user.DailyScore = 100;

            _userRepository.Update(user);

            User? updatedUser = _context.Users.FirstOrDefault(u => u.Email == "original@test.com");
            Assert.AreEqual("Updated", updatedUser.Name);
            Assert.AreEqual("NewName", updatedUser.LastName);
            Assert.AreEqual(100, updatedUser.Score);
            Assert.AreEqual(100, updatedUser.DailyScore);
        }

        [TestMethod]
        public void Update_ShouldUpdateUserRoles()
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

            Role visitorRole = _context.Roles.First(r => r.Name == Role.Visitor);
            UserRole userRole = new UserRole { UserId = user.Id, RoleId = visitorRole.Id };
            user.UserRoles.Add(userRole);

            _userRepository.Update(user);

            User? updatedUser = _userRepository.GetByIdWithRoles(user.Id);
            Assert.AreEqual(1, updatedUser.UserRoles.Count);
            Assert.AreEqual(Role.Visitor, updatedUser.UserRoles.First().Role.Name);
        }

        [TestMethod]
        public void Update_ShouldInsertNewReportsWhenUserHasVisitorReports()
        {
            User user = new User
            {
                Name = "Visitor",
                LastName = "Test",
                Email = "visitor@test.com",
                Password = "password",
                BirthDate = new DateTime(1990, 1, 1),
                Score = 0
            };
            _context.Users.Add(user);

            Attraction attraction = new Attraction
            {
                Name = "Test Attraction",
                Description = "Test",
                Type = AttractionType.RollerCoaster,
                MaxCapacity = 100,
                CurrentCapacity = 0,
                MinAge = 10
            };
            _context.Attractions.Add(attraction);
            _context.SaveChanges();

            User? trackedUser = _userRepository.GetById(user.Id);
            trackedUser.RegisterEntry(attraction, DateTime.Now);
            trackedUser.Score = 10;

            _userRepository.Update(trackedUser);

            List<Report> reports = _context.Reports.ToList();
            List<VisitorReport> visitorReports = _context.VisitorReports.ToList();

            Assert.AreEqual(1, reports.Count, "Report should be inserted");
            Assert.AreEqual(1, visitorReports.Count, "VisitorReport should be inserted");
            Assert.AreEqual(attraction.Id, reports[0].AttractionId);
            Assert.AreEqual(user.Id, visitorReports[0].VisitorId);
        }

        [TestMethod]
        public void Update_ShouldInsertMultipleVisitorReportsForDifferentDates()
        {
            User user = new User
            {
                Name = "Visitor",
                LastName = "Test",
                Email = "multiday@test.com",
                Password = "password",
                BirthDate = new DateTime(1990, 1, 1),
                Score = 0
            };
            _context.Users.Add(user);

            Attraction attraction = new Attraction
            {
                Name = "Multi Day Attraction",
                Description = "Test",
                Type = AttractionType.Performance,
                MaxCapacity = 50,
                CurrentCapacity = 0,
                MinAge = 5
            };
            _context.Attractions.Add(attraction);
            _context.SaveChanges();

            User? trackedUser = _userRepository.GetById(user.Id);
            DateTime date1 = new DateTime(2025, 10, 1, 10, 0, 0);
            DateTime date2 = new DateTime(2025, 10, 2, 14, 0, 0);

            trackedUser.RegisterEntry(attraction, date1);
            trackedUser.RegisterEntry(attraction, date2);
            trackedUser.Score = 20;

            _userRepository.Update(trackedUser);

            List<Report> reports = _context.Reports.ToList();
            List<VisitorReport> visitorReports = _context.VisitorReports.ToList();

            Assert.AreEqual(2, reports.Count, "Two Reports should be inserted");
            Assert.AreEqual(2, visitorReports.Count, "Two VisitorReports should be inserted");
            Assert.IsTrue(visitorReports.Any(vr => vr.Date.Date == date1.Date));
            Assert.IsTrue(visitorReports.Any(vr => vr.Date.Date == date2.Date));
        }

        [TestMethod]
        public void Update_WithDetachedEntity_AttachesAndSaves()
        {
            User user = new User
            {
                Id = Guid.NewGuid(),
                Name = "Detached",
                LastName = "User",
                Email = "detached@test.com",
                Password = "hashedpassword",
                BirthDate = new DateTime(1990, 1, 1),
                Score = 100
            };

            _userRepository.Create(user);
            _context.ChangeTracker.Clear();

            User detachedUser = new User
            {
                Id = user.Id,
                Name = "Detached",
                LastName = "User",
                Email = "detached@test.com",
                Password = "hashedpassword",
                BirthDate = new DateTime(1990, 1, 1),
                Score = 200
            };

            _userRepository.Update(detachedUser);

            User updatedUser = _userRepository.GetById(user.Id);
            Assert.AreEqual(200, updatedUser.Score);
        }

        [TestMethod]
        public void Update_ShouldUpdateDetachedUser()
        {
            User user = new User
            {
                Name = "Detached",
                LastName = "User",
                Email = "detached2@test.com",
                Password = "password",
                BirthDate = new DateTime(1995, 3, 10),
                Score = 25
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            Guid userId = user.Id;

            _context.Entry(user).State = EntityState.Detached;

            Assert.AreEqual(EntityState.Detached, _context.Entry(user).State);

            User detachedUser = new User
            {
                Id = userId,
                Name = "Updated Detached",
                LastName = "Updated User",
                Email = "detached2@test.com",
                Password = "newpassword",
                BirthDate = new DateTime(1995, 3, 10),
                Score = 100
            };

            Assert.AreEqual(EntityState.Detached, _context.Entry(detachedUser).State);

            _userRepository.Update(detachedUser);

            User? updatedUser = _context.Users.FirstOrDefault(u => u.Id == userId);
            Assert.AreEqual("Updated Detached", updatedUser.Name);
            Assert.AreEqual("newpassword", updatedUser.Password);
            Assert.AreEqual(100, updatedUser.Score);
        }

        [TestMethod]
        public void GetAllUsers_ShouldReturnAllUsersWithRoles()
        {
            List<User> result = _userRepository.GetAllUsers();

            Assert.IsTrue(result.Count >= 2);
            Assert.IsTrue(result.All(u => u.UserRoles != null));
            Assert.IsTrue(result.All(u => u.UserRoles.All(ur => ur.Role != null)));
        }

        [TestMethod]
        public void GetAllUsers_ShouldReturnEmptyList_WhenNoUsersExist()
        {
            _context.Users.RemoveRange(_context.Users);
            _context.SaveChanges();

            List<User> result = _userRepository.GetAllUsers();

            Assert.AreEqual(0, result.Count);
        }
    }
}