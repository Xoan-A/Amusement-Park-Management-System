using AutoMapper;
using Domain;
using Models.Mapping;
using Models.Out;

namespace TestBusinessLogic
{
    [TestClass]
    public class MappingProfileTest
    {
        private IMapper _mapper = null!;

        [TestInitialize]
        public void Setup()
        {
            MapperConfiguration configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });
            _mapper = configuration.CreateMapper();
        }

        [TestMethod]
        public void MappingProfile_ConfigurationIsValid()
        {
            MapperConfiguration configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });

            configuration.AssertConfigurationIsValid();
        }

        [TestMethod]
        public void UserToUserResponse_ShouldMapCorrectly()
        {
            Role role = new Role { Id = 1, Name = "Administrator" };
            UserRole userRole = new UserRole { Role = role };
            User user = new User
            {
                Id = Guid.NewGuid(),
                Name = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Password = "hashedPassword",
                BirthDate = new DateTime(1990, 1, 1),
                MembershipLevel = MembershipLevel.Premium,
                Score = 100,
                UserRoles = new List<UserRole> { userRole }
            };

            UserResponse result = _mapper.Map<UserResponse>(user);

            Assert.AreEqual(user.Id, result.Id);
            Assert.AreEqual(user.Name, result.Name);
            Assert.AreEqual(user.LastName, result.LastName);
            Assert.AreEqual(user.Email, result.Email);
            Assert.AreEqual(user.BirthDate, result.BirthDate);
            Assert.AreEqual((int)MembershipLevel.Premium, result.MembershipLevel);
            Assert.AreEqual(user.Score, result.Score);
            Assert.AreEqual(1, result.UserRoles.Count);
            Assert.AreEqual("Administrator", result.UserRoles.First());
        }

        [TestMethod]
        public void AttractionToAttractionResponse_ShouldMapCorrectly()
        {
            Attraction attraction = new Attraction
            {
                Id = Guid.NewGuid(),
                Name = "Roller Coaster",
                Description = "A thrilling ride",
                Type = AttractionType.RollerCoaster,
                MinAge = 12,
                MaxCapacity = 100,
                CurrentCapacity = 50
            };

            AttractionResponse result = _mapper.Map<AttractionResponse>(attraction);

            Assert.AreEqual(attraction.Id, result.Id);
            Assert.AreEqual(attraction.Name, result.Name);
            Assert.AreEqual(attraction.Description, result.Description);
            Assert.AreEqual("RollerCoaster", result.Type);
            Assert.AreEqual(attraction.MinAge, result.MinAge);
            Assert.AreEqual(attraction.MaxCapacity, result.MaxCapacity);
            Assert.AreEqual(attraction.CurrentCapacity, result.CurrentCapacity);
            Assert.IsTrue(result.IsActive);
        }

        [TestMethod]
        public void EventToEventResponse_ShouldMapCorrectly()
        {
            Attraction attraction = new Attraction
            {
                Id = Guid.NewGuid(),
                Name = "Roller Coaster",
                Description = "A thrilling ride",
                Type = AttractionType.RollerCoaster,
                MinAge = 12,
                MaxCapacity = 100,
                CurrentCapacity = 50
            };

            Event eventEntity = new Event
            {
                Id = Guid.NewGuid(),
                Name = "Summer Festival",
                Date = new DateTime(2025, 7, 1),
                Hour = 14,
                MaxCapacity = 500,
                CurrentCapacity = 200,
                Cost = 50.00m
            };

            eventEntity.AddAttraction(attraction);

            EventResponse result = _mapper.Map<EventResponse>(eventEntity);

            Assert.AreEqual(eventEntity.Id, result.Id);
            Assert.AreEqual(eventEntity.Name, result.Name);
            Assert.AreEqual(eventEntity.Date, result.Date);
            Assert.AreEqual(eventEntity.Hour, result.Hour);
            Assert.AreEqual(eventEntity.MaxCapacity, result.MaxCapacity);
            Assert.AreEqual(eventEntity.CurrentCapacity, result.CurrentCapacity);
            Assert.AreEqual(eventEntity.Cost, result.Cost);
            Assert.AreEqual(1, result.Attractions.Count);
            Assert.AreEqual(attraction.Name, result.Attractions.First().Name);
        }
    }
}
