using Microsoft.EntityFrameworkCore;
using Domain;
using DataAccess.Context;
using DataAccess.Repositories;
using IDataAccess;

namespace TestDataAccess;

[TestClass]
public class AttractionRepositoryTest
{
    private AppDbContext _context;
    private IAttractionRepository _attractionRepository;
    private Attraction attraction;
    
    [TestInitialize]
    public void Setup()
    {
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _attractionRepository = new AttractionRepository(_context);
        
        attraction = new Attraction();
        
        attraction.Name = "Race simulator";
        attraction.Description = "average race simulator";
        attraction.Type = AttractionType.Simulator;
        attraction.MinAge = 18;
        attraction.MaxCapacity = 10;
        attraction.CurrentCapacity = 0;
        attraction.IsActive = true;
    }
    
    [TestCleanup]
    public void Cleanup()
    {
        _context.Dispose();
    }
    
    [TestMethod]
        public void Create_ShouldAddAttractionToDatabase()
        {
            Attraction result = _attractionRepository.Create(attraction);

            Assert.IsNotNull(result);
            Assert.AreEqual("Race simulator", result.Name);
            Assert.AreEqual(1, _context.Attractions.Count());
        }

        [TestMethod]
        public void GetByName_ShouldReturnAttraction_WhenAttractionExists()
        {
            _context.Attractions.Add(attraction);
            _context.SaveChanges();

            Attraction result = _attractionRepository.GetByName("Race simulator");

            Assert.IsNotNull(result);
            Assert.AreEqual("Race simulator", result.Name);
        }

        [TestMethod]
        public void GetByName_ShouldReturnNull_WhenAttractionDoesNotExist()
        {
            Attraction result = _attractionRepository.GetByName("nonexistent");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetById_ShouldReturnAttraction_WhenAttractionExists()
        {
            _context.Attractions.Add(attraction);
            _context.SaveChanges();

            Attraction result = _attractionRepository.GetById(attraction.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(attraction.Id, result.Id);
        }

        [TestMethod]
        public void GetById_ShouldReturnNull_WhenAttractionDoesNotExist()
        {
            Guid nonExistentId = Guid.NewGuid();

            Attraction result = _attractionRepository.GetById(nonExistentId);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void IsNameUnique_ShouldReturnFalse_WhenNameExists()
        {
            _context.Attractions.Add(attraction);
            _context.SaveChanges();

            bool result = _attractionRepository.IsNameUnique("Race simulator");

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsNameUnique_ShouldReturnTrue_WhenNameDoesNotExist()
        {
            bool result = _attractionRepository.IsNameUnique("new name");

            Assert.IsTrue(result);
        }
        
        [TestMethod]
        public void GetAll_ShouldReturnAllAttractions()
        {
            Attraction attraction2 = new Attraction
            {
                Name = "Haunted House",
                Description = "A spooky experience",
                Type = AttractionType.Simulator,
                MinAge = 8,
                MaxCapacity = 15,
                CurrentCapacity = 3,
                IsActive = false
            };
            
            _context.Attractions.Add(attraction);
            _context.Attractions.Add(attraction2);
            _context.SaveChanges();
            List<Attraction> result = _attractionRepository.GetAll();

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
        }
        
        [TestMethod]
        public void Update_ShouldUpdateAttractionInDatabase()
        {
            _context.Attractions.Add(attraction);
            _context.SaveChanges();
            
            attraction.Name = "Updated Name";
            _attractionRepository.Update(attraction);

            Attraction result = _attractionRepository.GetById(attraction.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual("Updated Name", result.Name);
        }
}