using AutoMapper;
using Moq;
using Domain;
using IBusinessLogic;
using IDataAccess;
using BusinessLogic;
using Models.Out;
using Domain.Exceptions;
using Models.In;
using Models.Mapping;

namespace TestBusinessLogic
{
    [TestClass]
    public class UserLogicTest
    {
        private Mock<IUserRepository> _mockUserRepository = null!;
        private Mock<IPasswordLogic> _mockPasswordService = null!;
        private Mock<IRoleRepository> _mockRoleRepository = null!;
        private Mock<IUserValidationService> _mockValidationService = null!;
        private Mock<IParkEntryLogic> _mockParkEntryLogic = null!;
        private IMapper _mapper = null!;
        private IUserLogic _userLogic = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockUserRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            _mockPasswordService = new Mock<IPasswordLogic>(MockBehavior.Strict);
            _mockRoleRepository = new Mock<IRoleRepository>(MockBehavior.Strict);
            _mockValidationService = new Mock<IUserValidationService>(MockBehavior.Strict);
            _mockParkEntryLogic = new Mock<IParkEntryLogic>(MockBehavior.Strict);

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });
            _mapper = configuration.CreateMapper();

            _userLogic = new UserLogic(_mockUserRepository.Object, _mockPasswordService.Object,
                _mockRoleRepository.Object, _mockValidationService.Object, _mockParkEntryLogic.Object, _mapper);
        }

        [TestMethod]
        public void RegisterVisitor_ShouldCreateVisitor_WithStandardMembership()
        {
            string name = "John";
            string lastName = "Doe";
            string email = "john.doe@test.com";
            string password = "password123";
            string hashedPassword = "hashedPassword123";
            DateTime birthDate = new DateTime(1990, 5, 15);

            _mockValidationService.Setup(v => v.ValidateRequiredFields(name, lastName, email, password));
            _mockValidationService.Setup(v => v.ValidateEmail(email)).Returns(true);
            _mockValidationService.Setup(v => v.ValidateBirthDate(birthDate));
            _mockValidationService.Setup(v => v.ValidateEmailUniqueness(email));
            _mockUserRepository.Setup(r => r.IsEmailUnique(email)).Returns(true);
            _mockPasswordService.Setup(p => p.HashPassword(password)).Returns(hashedPassword);
            _mockRoleRepository.Setup(r => r.GetByName(Role.VISITOR)).Returns(new Role { Name = Role.VISITOR });

            User expectedUser = new User
            {
                Name = name,
                LastName = lastName,
                Email = email,
                Password = hashedPassword,
                BirthDate = birthDate,
                MembershipLevel = MembershipLevel.Standard
            };

            _mockUserRepository.Setup(r => r.Create(It.IsAny<User>())).Returns(expectedUser);

            RegisterVisitorRequest request = new RegisterVisitorRequest
            {
                Name = name,
                LastName = lastName,
                Email = email,
                Password = password,
                BirthDate = birthDate
            };

            UserResponse result = _userLogic.RegisterVisitor(request);

            Assert.AreEqual(name, result.Name);
            Assert.AreEqual(lastName, result.LastName);
            Assert.AreEqual(email, result.Email);
            Assert.AreEqual(birthDate, result.BirthDate);

            _mockValidationService.Verify(v => v.ValidateEmailUniqueness(email), Times.Once);
            _mockPasswordService.Verify(p => p.HashPassword(password), Times.Once);
            _mockUserRepository.Verify(r => r.Create(It.IsAny<User>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterVisitor_ShouldThrowException_WhenEmailIsNotUnique()
        {
            string name = "John";
            string lastName = "Doe";
            string email = "existing@test.com";
            string password = "password123";
            DateTime birthDate = new DateTime(1990, 5, 15);

            _mockValidationService.Setup(v => v.ValidateRequiredFields(name, lastName, email, password));
            _mockValidationService.Setup(v => v.ValidateEmail(email)).Returns(true);
            _mockValidationService.Setup(v => v.ValidateBirthDate(birthDate));
            _mockValidationService.Setup(v => v.ValidateEmailUniqueness(email))
                .Throws(new ArgumentException("Email must be unique"));
            _mockUserRepository.Setup(r => r.IsEmailUnique(email)).Returns(false);

            RegisterVisitorRequest request = new RegisterVisitorRequest
            {
                Name = name,
                LastName = lastName,
                Email = email,
                Password = password,
                BirthDate = birthDate
            };

            _userLogic.RegisterVisitor(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterVisitor_ShouldThrowException_WhenEmailIsEmpty()
        {
            string name = "John";
            string lastName = "Doe";
            string email = "";
            string password = "password123";
            DateTime birthDate = new DateTime(1990, 5, 15);

            _mockValidationService.Setup(v => v.ValidateRequiredFields(name, lastName, email, password))
                .Throws(new ArgumentException("Email is required"));

            RegisterVisitorRequest request = new RegisterVisitorRequest
            {
                Name = name,
                LastName = lastName,
                Email = email,
                Password = password,
                BirthDate = birthDate
            };

            _userLogic.RegisterVisitor(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterVisitor_ShouldThrowException_WhenPasswordIsEmpty()
        {
            string name = "John";
            string lastName = "Doe";
            string email = "john@test.com";
            string password = "";
            DateTime birthDate = new DateTime(1990, 5, 15);

            _mockValidationService.Setup(v => v.ValidateRequiredFields(name, lastName, email, password))
                .Throws(new ArgumentException("Password is required"));

            RegisterVisitorRequest request = new RegisterVisitorRequest
            {
                Name = name,
                LastName = lastName,
                Email = email,
                Password = password,
                BirthDate = birthDate
            };

            _userLogic.RegisterVisitor(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterVisitor_ShouldThrowException_WhenNameIsEmpty()
        {
            string name = "";
            string lastName = "Doe";
            string email = "john@test.com";
            string password = "password123";
            DateTime birthDate = new DateTime(1990, 5, 15);

            _mockValidationService.Setup(v => v.ValidateRequiredFields(name, lastName, email, password))
                .Throws(new ArgumentException("Name is required"));

            RegisterVisitorRequest request = new RegisterVisitorRequest
            {
                Name = name,
                LastName = lastName,
                Email = email,
                Password = password,
                BirthDate = birthDate
            };

            _userLogic.RegisterVisitor(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterVisitor_ShouldThrowException_WhenLastNameIsEmpty()
        {
            string name = "John";
            string lastName = "";
            string email = "john@test.com";
            string password = "password123";
            DateTime birthDate = new DateTime(1990, 5, 15);

            _mockValidationService.Setup(v => v.ValidateRequiredFields(name, lastName, email, password))
                .Throws(new ArgumentException("Last name is required"));

            RegisterVisitorRequest request = new RegisterVisitorRequest
            {
                Name = name,
                LastName = lastName,
                Email = email,
                Password = password,
                BirthDate = birthDate
            };

            _userLogic.RegisterVisitor(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterVisitor_ShouldThrowException_WhenBirthDateIsInFuture()
        {
            string name = "John";
            string lastName = "Doe";
            string email = "john@test.com";
            string password = "password123";
            DateTime futureBirthDate = DateTime.Now.AddDays(1);

            _mockValidationService.Setup(v => v.ValidateRequiredFields(name, lastName, email, password));
            _mockValidationService.Setup(v => v.ValidateEmail(email)).Returns(true);
            _mockValidationService.Setup(v => v.ValidateBirthDate(futureBirthDate))
                .Throws(new ArgumentException("Birth date cannot be in the future"));

            RegisterVisitorRequest request = new RegisterVisitorRequest
            {
                Name = name,
                LastName = lastName,
                Email = email,
                Password = password,
                BirthDate = futureBirthDate
            };

            _userLogic.RegisterVisitor(request);
        }

        [TestMethod]
        public void RegisterVisitor_ShouldHashPassword_BeforeCreating()
        {
            string name = "John";
            string lastName = "Doe";
            string email = "john@test.com";
            string plainPassword = "plainPassword";
            string hashedPassword = "hashedPassword123";
            DateTime birthDate = new DateTime(1990, 5, 15);

            _mockValidationService.Setup(v => v.ValidateRequiredFields(name, lastName, email, plainPassword));
            _mockValidationService.Setup(v => v.ValidateEmail(email)).Returns(true);
            _mockValidationService.Setup(v => v.ValidateBirthDate(birthDate));
            _mockValidationService.Setup(v => v.ValidateEmailUniqueness(email));
            _mockUserRepository.Setup(r => r.IsEmailUnique(email)).Returns(true);
            _mockPasswordService.Setup(p => p.HashPassword(plainPassword)).Returns(hashedPassword);
            _mockRoleRepository.Setup(r => r.GetByName(Role.VISITOR)).Returns(new Role { Name = Role.VISITOR });

            User createdUser = new User
            {
                Name = name,
                LastName = lastName,
                Email = email,
                Password = hashedPassword,
                BirthDate = birthDate,
                MembershipLevel = MembershipLevel.Standard
            };

            _mockUserRepository.Setup(r => r.Create(It.Is<User>(v => v.Password == hashedPassword)))
            .Returns(createdUser);

            RegisterVisitorRequest request = new RegisterVisitorRequest
            {
                Name = name,
                LastName = lastName,
                Email = email,
                Password = plainPassword,
                BirthDate = birthDate
            };

            UserResponse result = _userLogic.RegisterVisitor(request);

            Assert.IsNotNull(result);
            _mockPasswordService.Verify(p => p.HashPassword(plainPassword), Times.Once);
        }

        [TestMethod]
        public void RegisterEntry_ShouldDelegateToParkEntryLogic()
        {
            Guid attractionId = Guid.NewGuid();
            RegisterEntryRequest request = new RegisterEntryRequest { UserId = Guid.NewGuid() };

            _mockParkEntryLogic.Setup(p => p.RegisterEntry(attractionId, request));

            _userLogic.RegisterEntry(attractionId, request);

            _mockParkEntryLogic.Verify(p => p.RegisterEntry(attractionId, request), Times.Once);
        }

        [TestMethod]
        public void GetTopTenUsers_ShouldReturnTopTenUsersOrderedByScore()
        {
            List<User> expectedUsers = new List<User>
            {
                new User { Id = Guid.NewGuid(), Name = "User1", Score = 100 },
                new User { Id = Guid.NewGuid(), Name = "User2", Score = 90 },
                new User { Id = Guid.NewGuid(), Name = "User3", Score = 80 },
                new User { Id = Guid.NewGuid(), Name = "User4", Score = 70 },
                new User { Id = Guid.NewGuid(), Name = "User5", Score = 60 },
                new User { Id = Guid.NewGuid(), Name = "User6", Score = 50 },
                new User { Id = Guid.NewGuid(), Name = "User7", Score = 40 },
                new User { Id = Guid.NewGuid(), Name = "User8", Score = 30 },
                new User { Id = Guid.NewGuid(), Name = "User9", Score = 20 },
                new User { Id = Guid.NewGuid(), Name = "User10", Score = 10 }
            };

            _mockUserRepository.Setup(r => r.GetTopTen()).Returns(expectedUsers);

            TopTenResponse result = _userLogic.GetTopTenUsers();

            Assert.AreEqual(10, result.TopTenUsers.Count);
            Assert.AreEqual(100, result.TopTenUsers[0].Score);
            Assert.AreEqual(10, result.TopTenUsers[9].Score);
            _mockUserRepository.Verify(r => r.GetTopTen(), Times.Once);
        }

        [TestMethod]
        public void GetTopTenUsers_ShouldReturnEmptyList_WhenNoUsersExist()
        {
            List<User> emptyList = new List<User>();

            _mockUserRepository.Setup(r => r.GetTopTen()).Returns(emptyList);

            TopTenResponse result = _userLogic.GetTopTenUsers();

            Assert.AreEqual(0, result.TopTenUsers.Count);
            _mockUserRepository.Verify(r => r.GetTopTen(), Times.Once);
        }

        [TestMethod]
        public void GetTopTenUsers_ShouldReturnFewerThanTenUsers_WhenLessThanTenExist()
        {
            List<User> expectedUsers = new List<User>
            {
                new User { Id = Guid.NewGuid(), Name = "User1", Score = 50 },
                new User { Id = Guid.NewGuid(), Name = "User2", Score = 40 },
                new User { Id = Guid.NewGuid(), Name = "User3", Score = 30 }
            };

            _mockUserRepository.Setup(r => r.GetTopTen()).Returns(expectedUsers);

            TopTenResponse result = _userLogic.GetTopTenUsers();

            Assert.AreEqual(3, result.TopTenUsers.Count);
            Assert.AreEqual(50, result.TopTenUsers[0].Score);
            Assert.AreEqual(30, result.TopTenUsers[2].Score);
            _mockUserRepository.Verify(r => r.GetTopTen(), Times.Once);
        }

        [TestMethod]
        public void GetTopTenUsers_ShouldCallRepositoryGetTopTenOnce()
        {
            List<User> expectedUsers = new List<User>
            {
                new User { Id = Guid.NewGuid(), Name = "User1", Score = 100 }
            };

            _mockUserRepository.Setup(r => r.GetTopTen()).Returns(expectedUsers);

            _userLogic.GetTopTenUsers();

            _mockUserRepository.Verify(r => r.GetTopTen(), Times.Once);
        }

        [TestMethod]
        public void GetTopTenUsers_ShouldReturnOnlyTenUsers_WhenMoreThanTenExist()
        {
            List<User> expectedUsers = new List<User>
            {
                new User { Id = Guid.NewGuid(), Name = "User1", Score = 110 },
                new User { Id = Guid.NewGuid(), Name = "User2", Score = 100 },
                new User { Id = Guid.NewGuid(), Name = "User3", Score = 90 },
                new User { Id = Guid.NewGuid(), Name = "User4", Score = 80 },
                new User { Id = Guid.NewGuid(), Name = "User5", Score = 70 },
                new User { Id = Guid.NewGuid(), Name = "User6", Score = 60 },
                new User { Id = Guid.NewGuid(), Name = "User7", Score = 50 },
                new User { Id = Guid.NewGuid(), Name = "User8", Score = 40 },
                new User { Id = Guid.NewGuid(), Name = "User9", Score = 30 },
                new User { Id = Guid.NewGuid(), Name = "User10", Score = 20 }
            };

            _mockUserRepository.Setup(r => r.GetTopTen()).Returns(expectedUsers);

            TopTenResponse result = _userLogic.GetTopTenUsers();

            Assert.AreEqual(10, result.TopTenUsers.Count);
            Assert.AreEqual(110, result.TopTenUsers[0].Score);
            Assert.AreEqual(20, result.TopTenUsers[9].Score);
            _mockUserRepository.Verify(r => r.GetTopTen(), Times.Once);
        }

        [TestMethod]
        public void ModifyUser_ShouldUpdateAndReturnResponse_WhenDataIsValid()
        {
            Guid userId = Guid.NewGuid();
            Guid actorSub = userId;
            User originalUser = new User
            {
                Id = userId,
                Name = "Old",
                LastName = "Name",
                Email = "old@example.com",
                Password = "oldpass",
                BirthDate = new DateTime(1990, 1, 1),
                MembershipLevel = MembershipLevel.Standard,
                UserRoles = new System.Collections.Generic.List<UserRole>
                {
                    new UserRole { Role = new Role { Name = Role.VISITOR } }
                },
                Score = 10
            };

            ModifyUserRequest request = new ModifyUserRequest
            {
                Name = "New",
                LastName = "Surname",
                Email = "new@example.com",
                Password = "New#Pass1",
                BirthDate = new DateTime(1992, 2, 2)
            };

            _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns(originalUser);
            _mockValidationService.Setup(v => v.ValidateEmail("new@example.com")).Returns(true);
            _mockValidationService.Setup(v => v.ValidateBirthDate(new DateTime(1992, 2, 2)));
            _mockUserRepository.Setup(r => r.IsEmailUnique("new@example.com")).Returns(true);
            _mockPasswordService.Setup(p => p.HashPassword(request.Password)).Returns("hashed");
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));

            UserResponse response = _userLogic.ModifyUser(userId, actorSub, request);

            Assert.AreEqual(userId, response.Id);
            Assert.AreEqual(request.Email, response.Email);

            _mockUserRepository.Verify(r => r.IsEmailUnique("new@example.com"), Times.Once);
            _mockPasswordService.Verify(p => p.HashPassword("New#Pass1"), Times.Once);
            _mockUserRepository.Verify(r => r.Update(It.Is<User>(u =>
            u.Name == request.Name &&
            u.LastName == request.LastName &&
            u.Email == request.Email &&
            u.Password == "hashed" &&
            u.BirthDate == request.BirthDate
            )), Times.Once);
        }

        [TestMethod]
        public void ModifyUser_WhenEmailNotChanged_DoesNotCheckUniqueness()
        {
            Guid userId = Guid.NewGuid();
            Guid actorSub = userId;
            User originalUser = new User
            {
                Id = userId,
                Name = "Old",
                LastName = "Name",
                Email = "same@example.com",
                Password = "oldpass",
                BirthDate = new DateTime(1990, 1, 1)
            };

            ModifyUserRequest request = new ModifyUserRequest
            {
                Name = "New",
                LastName = "Surname",
                Email = "same@example.com",
                Password = "New#Pass1"
            };

            _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns(originalUser);
            _mockValidationService.Setup(v => v.ValidateEmail("same@example.com")).Returns(true);
            _mockPasswordService.Setup(p => p.HashPassword(request.Password)).Returns("hashed");
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));

            _userLogic.ModifyUser(userId, actorSub, request);

            _mockUserRepository.Verify(r => r.IsEmailUnique(It.IsAny<string>()), Times.Never);
            _mockUserRepository.Verify(
                r => r.Update(It.Is<User>(u => u.Email == "same@example.com" && u.Password == "hashed")), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ForbiddenException))]
        public void ModifyUser_WhenActorSubIsGuidEmpty_ThrowsForbidden()
        {
            Guid userId = Guid.NewGuid();
            ModifyUserRequest request = new ModifyUserRequest
            {
                Name = "A",
                LastName = "B",
                Email = "a@b.com",
                Password = "p"
            };

            _userLogic.ModifyUser(userId, Guid.Empty, request);
        }

        [TestMethod]
        [ExpectedException(typeof(ForbiddenException))]
        public void ModifyUser_WhenActorIsDifferentUser_ThrowsForbidden()
        {
            Guid userId = Guid.NewGuid();
            Guid actorSub = Guid.NewGuid();
            ModifyUserRequest request = new ModifyUserRequest
            {
                Name = "A",
                LastName = "B",
                Email = "a@b.com",
                Password = "p"
            };

            _userLogic.ModifyUser(userId, actorSub, request);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void ModifyUser_WhenUserNotFound_ThrowsNotFound()
        {
            Guid userId = Guid.NewGuid();
            Guid actorSub = userId;
            ModifyUserRequest request = new ModifyUserRequest
            {
                Name = "A",
                LastName = "B",
                Email = "a@b.com",
                Password = "p"
            };

            _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns((User)null);

            _userLogic.ModifyUser(userId, actorSub, request);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ModifyUser_WhenEmailNotUnique_ThrowsArgument()
        {
            Guid userId = Guid.NewGuid();
            Guid actorSub = userId;
            User originalUser = new User { Id = userId, Name = "Old", LastName = "Name", Email = "old@example.com" };
            ModifyUserRequest request = new ModifyUserRequest
            {
                Name = "A",
                LastName = "B",
                Email = "new@example.com",
                Password = "p"
            };

            _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns(originalUser);
            _mockValidationService.Setup(v => v.ValidateEmail("new@example.com")).Returns(true);
            _mockUserRepository.Setup(r => r.IsEmailUnique("new@example.com")).Returns(false);

            _userLogic.ModifyUser(userId, actorSub, request);
        }

        [TestMethod]
        public void ModifyUser_OnlyUpdatesName_WhenOnlyNameProvided()
        {
            Guid userId = Guid.NewGuid();
            Guid actorSub = userId;
            User originalUser = new User
            {
                Id = userId,
                Name = "OldName",
                LastName = "OldLastName",
                Email = "old@example.com",
                Password = "oldHashedPassword",
                UserRoles = new List<UserRole>()
            };

            ModifyUserRequest request = new ModifyUserRequest
            {
                Name = "NewName"
            };

            _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns(originalUser);
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));

            UserResponse response = _userLogic.ModifyUser(userId, actorSub, request);

            Assert.AreEqual("NewName", response.Name);
            Assert.AreEqual("OldLastName", response.LastName);
            Assert.AreEqual("old@example.com", response.Email);
            _mockUserRepository.Verify(r => r.Update(It.Is<User>(u =>
            u.Name == "NewName" &&
            u.LastName == "OldLastName" &&
            u.Email == "old@example.com" &&
            u.Password == "oldHashedPassword"
            )), Times.Once);
        }

        [TestMethod]
        public void ModifyUser_OnlyUpdatesEmail_WhenOnlyEmailProvided()
        {
            Guid userId = Guid.NewGuid();
            Guid actorSub = userId;
            User originalUser = new User
            {
                Id = userId,
                Name = "OldName",
                LastName = "OldLastName",
                Email = "old@example.com",
                Password = "oldHashedPassword",
                UserRoles = new List<UserRole>()
            };

            ModifyUserRequest request = new ModifyUserRequest
            {
                Email = "new@example.com"
            };

            _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns(originalUser);
            _mockValidationService.Setup(v => v.ValidateEmail("new@example.com")).Returns(true);
            _mockUserRepository.Setup(r => r.IsEmailUnique("new@example.com")).Returns(true);
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));

            UserResponse response = _userLogic.ModifyUser(userId, actorSub, request);

            Assert.AreEqual("OldName", response.Name);
            Assert.AreEqual("OldLastName", response.LastName);
            Assert.AreEqual("new@example.com", response.Email);
            _mockUserRepository.Verify(r => r.Update(It.Is<User>(u =>
            u.Name == "OldName" &&
            u.LastName == "OldLastName" &&
            u.Email == "new@example.com" &&
            u.Password == "oldHashedPassword"
            )), Times.Once);
        }

        [TestMethod]
        public void ModifyUser_OnlyUpdatesPassword_WhenOnlyPasswordProvided()
        {
            Guid userId = Guid.NewGuid();
            Guid actorSub = userId;
            User originalUser = new User
            {
                Id = userId,
                Name = "OldName",
                LastName = "OldLastName",
                Email = "old@example.com",
                Password = "oldHashedPassword",
                UserRoles = new List<UserRole>()
            };

            ModifyUserRequest request = new ModifyUserRequest
            {
                Password = "newPassword123"
            };

            _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns(originalUser);
            _mockPasswordService.Setup(p => p.HashPassword("newPassword123")).Returns("newHashedPassword");
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));

            UserResponse response = _userLogic.ModifyUser(userId, actorSub, request);

            Assert.AreEqual("OldName", response.Name);
            Assert.AreEqual("OldLastName", response.LastName);
            Assert.AreEqual("old@example.com", response.Email);
            _mockUserRepository.Verify(r => r.Update(It.Is<User>(u =>
            u.Name == "OldName" &&
            u.LastName == "OldLastName" &&
            u.Email == "old@example.com" &&
            u.Password == "newHashedPassword"
            )), Times.Once);
        }

        [TestMethod]
        public void ModifyUser_DoesNotUpdateAnything_WhenAllFieldsAreNull()
        {
            Guid userId = Guid.NewGuid();
            Guid actorSub = userId;
            User originalUser = new User
            {
                Id = userId,
                Name = "OldName",
                LastName = "OldLastName",
                Email = "old@example.com",
                Password = "oldHashedPassword",
                UserRoles = new List<UserRole>()
            };

            ModifyUserRequest request = new ModifyUserRequest();

            _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns(originalUser);
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));

            UserResponse response = _userLogic.ModifyUser(userId, actorSub, request);

            Assert.AreEqual("OldName", response.Name);
            Assert.AreEqual("OldLastName", response.LastName);
            Assert.AreEqual("old@example.com", response.Email);
            _mockUserRepository.Verify(r => r.Update(It.Is<User>(u =>
            u.Name == "OldName" &&
            u.LastName == "OldLastName" &&
            u.Email == "old@example.com" &&
            u.Password == "oldHashedPassword"
            )), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ModifyUser_WhenBirthDateInFuture_ThrowsArgument()
        {
            Guid userId = Guid.NewGuid();
            Guid actorSub = userId;
            DateTime currentDate = DateTime.Now;
            User originalUser = new User
            {
                Id = userId,
                Name = "Old",
                LastName = "Name",
                Email = "old@example.com",
                UserRoles = new List<UserRole>()
            };

            ModifyUserRequest request = new ModifyUserRequest
            {
                BirthDate = currentDate.AddDays(1)
            };

            _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns(originalUser);
            _mockValidationService.Setup(v => v.ValidateBirthDate(It.IsAny<DateTime>()))
                .Throws(new ArgumentException("Birth date cannot be after today."));

            _userLogic.ModifyUser(userId, actorSub, request);
        }

        [TestMethod]
        public void GetUserResponseById_ShouldReturnUserResponse_WhenUserExists()
        {
            Guid userId = Guid.NewGuid();
            User expectedUser = new User
            {
                Id = userId,
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                BirthDate = new DateTime(1990, 5, 15),
                MembershipLevel = MembershipLevel.Premium,
                Score = 100
            };

            _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns(expectedUser);

            UserResponse result = _userLogic.GetUserResponseById(userId);

            Assert.AreEqual(userId, result.Id);
            Assert.AreEqual("John", result.Name);
            Assert.AreEqual("john@test.com", result.Email);
            Assert.AreEqual(100, result.Score);

            _mockUserRepository.Verify(r => r.GetByIdWithRoles(userId), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void GetUserResponseById_ShouldThrowKeyNotFoundException_WhenUserNotFound()
        {
            Guid userId = Guid.NewGuid();

            _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns((User)null);

            _userLogic.GetUserResponseById(userId);
        }

        [TestMethod]
        public void RegisterVisitor_WhenVisitorRoleNotFound_CreatesVisitorWithoutRole()
        {
            string name = "John";
            string lastName = "Doe";
            string email = "john.doe@test.com";
            string password = "password123";
            string hashedPassword = "hashedPassword123";
            DateTime birthDate = new DateTime(1990, 5, 15);

            _mockValidationService.Setup(v => v.ValidateRequiredFields(name, lastName, email, password));
            _mockValidationService.Setup(v => v.ValidateEmail(email)).Returns(true);
            _mockValidationService.Setup(v => v.ValidateBirthDate(birthDate));
            _mockValidationService.Setup(v => v.ValidateEmailUniqueness(email));
            _mockUserRepository.Setup(r => r.IsEmailUnique(email)).Returns(true);
            _mockPasswordService.Setup(p => p.HashPassword(password)).Returns(hashedPassword);
            _mockRoleRepository.Setup(r => r.GetByName(Role.VISITOR)).Returns((Role)null);

            User createdUser = null;
            _mockUserRepository.Setup(r => r.Create(It.IsAny<User>()))
            .Callback<User>(u => createdUser = u)
            .Returns((User u) => u);

            RegisterVisitorRequest request = new RegisterVisitorRequest
            {
                Name = name,
                LastName = lastName,
                Email = email,
                Password = password,
                BirthDate = birthDate
            };

            UserResponse result = _userLogic.RegisterVisitor(request);

            Assert.AreEqual(0, createdUser.UserRoles.Count, "User should have no roles when visitor role is not found");

            _mockRoleRepository.Verify(r => r.GetByName(Role.VISITOR), Times.Once);
            _mockUserRepository.Verify(r => r.Create(It.IsAny<User>()), Times.Once);
        }

        [TestMethod]
        public void CreateUser_WhenRolesIsNull_CreatesUserWithoutRoles()
        {
            CreateUserRequest request = new CreateUserRequest
            {
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "password123",
                Roles = null
            };

            _mockValidationService.Setup(v => v.ValidateRequiredFields(request.Name, request.LastName, request.Email, request.Password));
            _mockValidationService.Setup(v => v.ValidateEmail(request.Email)).Returns(true);
            _mockValidationService.Setup(v => v.ValidateEmailUniqueness(request.Email));
            _mockUserRepository.Setup(r => r.IsEmailUnique(request.Email)).Returns(true);
            _mockPasswordService.Setup(p => p.HashPassword(request.Password)).Returns("hashed");

            User createdUser = null;
            _mockUserRepository.Setup(r => r.Create(It.IsAny<User>()))
            .Callback<User>(u => createdUser = u)
            .Returns((User u) => u);

            _userLogic.CreateUser(request);

            Assert.AreEqual(0, createdUser.UserRoles.Count);
        }

        [TestMethod]
        public void CreateUser_WhenRoleNotFoundInDatabase_SkipsNonexistentRole()
        {
            CreateUserRequest request = new CreateUserRequest
            {
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "password123",
                Roles = new List<string> { "Admin", "NonExistent" }
            };

            _mockValidationService.Setup(v => v.ValidateRequiredFields(request.Name, request.LastName, request.Email, request.Password));
            _mockValidationService.Setup(v => v.ValidateEmail(request.Email)).Returns(true);
            _mockValidationService.Setup(v => v.ValidateEmailUniqueness(request.Email));
            _mockUserRepository.Setup(r => r.IsEmailUnique(request.Email)).Returns(true);
            _mockPasswordService.Setup(p => p.HashPassword(request.Password)).Returns("hashed");

            Role adminRole = new Role { Id = 1, Name = "Admin" };
            _mockRoleRepository.Setup(r => r.GetByName("Admin")).Returns(adminRole);
            _mockRoleRepository.Setup(r => r.GetByName("NonExistent")).Returns((Role)null);

            User createdUser = null;
            _mockUserRepository.Setup(r => r.Create(It.IsAny<User>()))
            .Callback<User>(u => createdUser = u)
            .Returns((User u) => u);

            _userLogic.CreateUser(request);

            Assert.AreEqual(1, createdUser.UserRoles.Count);
            Assert.AreEqual("Admin", createdUser.UserRoles.First().Role.Name);
        }

        [TestMethod]
        public void RegisterExit_ShouldDelegateToParkEntryLogic()
        {
            Guid attractionId = Guid.NewGuid();
            RegisterExitRequest request = new RegisterExitRequest { userId = Guid.NewGuid() };

            _mockParkEntryLogic.Setup(p => p.RegisterExit(attractionId, request));

            _userLogic.RegisterExit(attractionId, request);

            _mockParkEntryLogic.Verify(p => p.RegisterExit(attractionId, request), Times.Once);
        }

        [TestMethod]
        public void ModifyUser_WhenBirthDateNotProvided_DoesNotUpdateBirthDate()
        {
            Guid userId = Guid.NewGuid();
            Guid actorSubClaim = userId;
            DateTime originalBirthDate = new DateTime(1990, 5, 15);

            User originalUser = new User
            {
                Id = userId,
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "hashed",
                BirthDate = originalBirthDate
            };

            ModifyUserRequest request = new ModifyUserRequest
            {
                Name = "Jane",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "newPassword123",
                BirthDate = null
            };

            _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns(originalUser);
            _mockValidationService.Setup(v => v.ValidateEmail(request.Email)).Returns(true);
            _mockUserRepository.Setup(r => r.IsEmailUnique(request.Email)).Returns(true);
            _mockPasswordService.Setup(p => p.HashPassword(request.Password)).Returns("newHashedPassword");
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));

            _userLogic.ModifyUser(userId, actorSubClaim, request);

            Assert.AreEqual(originalBirthDate, originalUser.BirthDate,
                "BirthDate should not change when null is provided");
            Assert.AreEqual("Jane", originalUser.Name);
        }


        [TestMethod]
        public void ChangeMembershipLevel_ValidLevel_UpdatesUserMembership()
        {
            Guid userId = Guid.NewGuid();
            int newMembershipLevel = 1;

            User user = new User
            {
                Id = userId,
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "hashed",
                MembershipLevel = MembershipLevel.Standard,
                Score = 100
            };

            _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns(user);
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));

            UserResponse result = _userLogic.ChangeMembershipLevel(userId, newMembershipLevel);

            Assert.AreEqual(userId, result.Id);
            Assert.AreEqual(1, result.MembershipLevel);
            Assert.AreEqual(MembershipLevel.Premium, user.MembershipLevel);
            _mockUserRepository.Verify(r => r.GetByIdWithRoles(userId), Times.Once);
            _mockUserRepository.Verify(r => r.Update(user), Times.Once);
        }

        [TestMethod]
        public void ChangeMembershipLevel_ToVIP_UpdatesCorrectly()
        {
            Guid userId = Guid.NewGuid();
            int newMembershipLevel = 2;

            User user = new User
            {
                Id = userId,
                Name = "Jane",
                LastName = "Smith",
                Email = "jane@test.com",
                Password = "hashed",
                MembershipLevel = MembershipLevel.Premium,
                Score = 500
            };

            _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns(user);
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));

            UserResponse result = _userLogic.ChangeMembershipLevel(userId, newMembershipLevel);

            Assert.AreEqual(2, result.MembershipLevel);
            Assert.AreEqual(MembershipLevel.VIP, user.MembershipLevel);
            _mockUserRepository.Verify(r => r.Update(user), Times.Once);
        }

        [TestMethod]
        public void ChangeMembershipLevel_ToStandard_UpdatesCorrectly()
        {
            Guid userId = Guid.NewGuid();
            int newMembershipLevel = 0;

            User user = new User
            {
                Id = userId,
                Name = "Bob",
                LastName = "Johnson",
                Email = "bob@test.com",
                Password = "hashed",
                MembershipLevel = MembershipLevel.VIP,
                Score = 300
            };

            _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns(user);
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));

            UserResponse result = _userLogic.ChangeMembershipLevel(userId, newMembershipLevel);

            Assert.AreEqual(0, result.MembershipLevel);
            Assert.AreEqual(MembershipLevel.Standard, user.MembershipLevel);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ChangeMembershipLevel_InvalidLevel_ThrowsArgumentException()
        {
            Guid userId = Guid.NewGuid();
            int invalidMembershipLevel = 999;

            _userLogic.ChangeMembershipLevel(userId, invalidMembershipLevel);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ChangeMembershipLevel_NegativeLevel_ThrowsArgumentException()
        {
            Guid userId = Guid.NewGuid();
            int negativeMembershipLevel = -1;

            _userLogic.ChangeMembershipLevel(userId, negativeMembershipLevel);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void ChangeMembershipLevel_UserNotFound_ThrowsKeyNotFoundException()
        {
            Guid userId = Guid.NewGuid();
            int newMembershipLevel = 1;

            _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns((User)null);

            _userLogic.ChangeMembershipLevel(userId, newMembershipLevel);
        }

        [TestMethod]
        public void ChangeMembershipLevel_CallsRepositoryMethods_InCorrectOrder()
        {
            Guid userId = Guid.NewGuid();
            int newMembershipLevel = 1;

            User user = new User
            {
                Id = userId,
                Name = "Test",
                LastName = "User",
                Email = "test@test.com",
                Password = "hashed",
                MembershipLevel = MembershipLevel.Standard
            };

            _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns(user);
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));

            _userLogic.ChangeMembershipLevel(userId, newMembershipLevel);

            _mockUserRepository.Verify(r => r.GetByIdWithRoles(userId), Times.Once);
            _mockUserRepository.Verify(r => r.Update(user), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterVisitor_ShouldThrowException_WhenEmailHasNoAtSymbol()
        {
            RegisterVisitorRequest request = new RegisterVisitorRequest
            {
                Name = "John",
                LastName = "Doe",
                Email = "invalidemail.com",
                Password = "password123",
                BirthDate = new DateTime(1990, 1, 1)
            };

            _mockValidationService.Setup(v => v.ValidateRequiredFields(request.Name, request.LastName, request.Email, request.Password));
            _mockValidationService.Setup(v => v.ValidateEmail(request.Email)).Returns(false);

            _userLogic.RegisterVisitor(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterVisitor_ShouldThrowException_WhenEmailStartsWithAt()
        {
            RegisterVisitorRequest request = new RegisterVisitorRequest
            {
                Name = "John",
                LastName = "Doe",
                Email = "@test.com",
                Password = "password123",
                BirthDate = new DateTime(1990, 1, 1)
            };

            _mockValidationService.Setup(v => v.ValidateRequiredFields(request.Name, request.LastName, request.Email, request.Password));
            _mockValidationService.Setup(v => v.ValidateEmail(request.Email)).Returns(false);

            _userLogic.RegisterVisitor(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterVisitor_ShouldThrowException_WhenEmailEndsWithAt()
        {
            RegisterVisitorRequest request = new RegisterVisitorRequest
            {
                Name = "John",
                LastName = "Doe",
                Email = "test@",
                Password = "password123",
                BirthDate = new DateTime(1990, 1, 1)
            };

            _mockValidationService.Setup(v => v.ValidateRequiredFields(request.Name, request.LastName, request.Email, request.Password));
            _mockValidationService.Setup(v => v.ValidateEmail(request.Email)).Returns(false);

            _userLogic.RegisterVisitor(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterVisitor_ShouldThrowException_WhenEmailHasMultipleAtSymbols()
        {
            RegisterVisitorRequest request = new RegisterVisitorRequest
            {
                Name = "John",
                LastName = "Doe",
                Email = "test@@test.com",
                Password = "password123",
                BirthDate = new DateTime(1990, 1, 1)
            };

            _mockValidationService.Setup(v => v.ValidateRequiredFields(request.Name, request.LastName, request.Email, request.Password));
            _mockValidationService.Setup(v => v.ValidateEmail(request.Email)).Returns(false);

            _userLogic.RegisterVisitor(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterVisitor_ShouldThrowException_WhenEmailDomainHasNoDot()
        {
            RegisterVisitorRequest request = new RegisterVisitorRequest
            {
                Name = "John",
                LastName = "Doe",
                Email = "test@testcom",
                Password = "password123",
                BirthDate = new DateTime(1990, 1, 1)
            };

            _mockValidationService.Setup(v => v.ValidateRequiredFields(request.Name, request.LastName, request.Email, request.Password));
            _mockValidationService.Setup(v => v.ValidateEmail(request.Email)).Returns(false);

            _userLogic.RegisterVisitor(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterVisitor_ShouldThrowException_WhenEmailDomainStartsWithDot()
        {
            RegisterVisitorRequest request = new RegisterVisitorRequest
            {
                Name = "John",
                LastName = "Doe",
                Email = "test@.test.com",
                Password = "password123",
                BirthDate = new DateTime(1990, 1, 1)
            };

            _mockValidationService.Setup(v => v.ValidateRequiredFields(request.Name, request.LastName, request.Email, request.Password));
            _mockValidationService.Setup(v => v.ValidateEmail(request.Email)).Returns(false);

            _userLogic.RegisterVisitor(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterVisitor_ShouldThrowException_WhenEmailDomainEndsWithDot()
        {
            RegisterVisitorRequest request = new RegisterVisitorRequest
            {
                Name = "John",
                LastName = "Doe",
                Email = "test@test.com.",
                Password = "password123",
                BirthDate = new DateTime(1990, 1, 1)
            };

            _mockValidationService.Setup(v => v.ValidateRequiredFields(request.Name, request.LastName, request.Email, request.Password));
            _mockValidationService.Setup(v => v.ValidateEmail(request.Email)).Returns(false);

            _userLogic.RegisterVisitor(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateUser_ShouldThrowException_WhenEmailIsInvalid()
        {
            _mockUserRepository.Setup(r => r.IsEmailUnique(It.IsAny<string>())).Returns(true);

            CreateUserRequest request = new CreateUserRequest
            {
                Name = "John",
                LastName = "Doe",
                Email = "invalidemail",
                Password = "password123",
                Roles = new List<string>()
            };

            _mockValidationService.Setup(v => v.ValidateRequiredFields(request.Name, request.LastName, request.Email, request.Password));
            _mockValidationService.Setup(v => v.ValidateEmail(request.Email)).Returns(false);

            _userLogic.CreateUser(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ModifyUser_ShouldThrowException_WhenEmailIsInvalid()
        {
            Guid userId = Guid.NewGuid();
            User user = new User
            {
                Id = userId,
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "hashed",
                BirthDate = new DateTime(1990, 1, 1)
            };

            _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns(user);
            _mockValidationService.Setup(v => v.ValidateEmail("invalidemail")).Returns(false);

            ModifyUserRequest request = new ModifyUserRequest
            {
                Email = "invalidemail"
            };

            _userLogic.ModifyUser(userId, userId, request);
        }
    }
}