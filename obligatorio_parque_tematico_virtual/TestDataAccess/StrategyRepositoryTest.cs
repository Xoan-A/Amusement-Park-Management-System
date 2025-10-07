using Microsoft.EntityFrameworkCore;
using Domain;
using DataAccess.Context;
using DataAccess.Repositories;
using IDataAccess;

namespace TestDataAccess;

[TestClass]
public class StrategyRepositoryTest
{
    private AppDbContext _context;
    private IStrategyRepository _strategyRepository;
    private StrategyConfiguration strategyConfiguration;

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
        _strategyRepository = new StrategyRepository(_context);

        var seededConfig = _context.StrategyConfigurations.FirstOrDefault(s => s.Id == 1);
        if (seededConfig != null)
        {
            _context.StrategyConfigurations.Remove(seededConfig);
            _context.SaveChanges();
        }

        strategyConfiguration = new StrategyConfiguration
        {
            StrategyName = "Combo",
            N = 5,
        };
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }

    [TestMethod]
    public void Get_ShouldReturnConfiguration_WhenConfigurationExists()
    {
        strategyConfiguration.Id = 1;
        _context.StrategyConfigurations.Add(strategyConfiguration);
        _context.SaveChanges();

        StrategyConfiguration? result = _strategyRepository.Get();

        Assert.IsNotNull(result);
        Assert.AreEqual("Combo", result.StrategyName);
    }

    [TestMethod]
    public void Get_ShouldReturnNull_WhenConfigurationDoesNotExist()
    {
        StrategyConfiguration? result = _strategyRepository.Get();

        Assert.IsNull(result);
    }

    [TestMethod]
    public void Update_ShouldCreateConfiguration_WhenConfigurationDoesNotExist()
    {
        _strategyRepository.Update(strategyConfiguration);

        StrategyConfiguration? result = _strategyRepository.Get();

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Id);
        Assert.AreEqual("Combo", result.StrategyName);
        Assert.AreEqual(5, result.N);
        Assert.AreEqual(1, _context.StrategyConfigurations.Count());
    }

    [TestMethod]
    public void Update_ShouldUpdateConfiguration_WhenConfigurationExists()
    {
        strategyConfiguration.Id = 1;
        _context.StrategyConfigurations.Add(strategyConfiguration);
        _context.SaveChanges();

        StrategyConfiguration updatedConfiguration = new StrategyConfiguration
        {
            StrategyName = "PerEvent",
            N = null,
        };

        _strategyRepository.Update(updatedConfiguration);

        StrategyConfiguration? result = _strategyRepository.Get();

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Id);
        Assert.AreEqual("PerEvent", result.StrategyName);
        Assert.IsNull(result.N);
        Assert.AreEqual(1, _context.StrategyConfigurations.Count());
    }

    [TestMethod]
    public void Update_ShouldAlwaysSetIdToOne_WhenCreatingNewConfiguration()
    {
        StrategyConfiguration configWithDifferentId = new StrategyConfiguration
        {
            Id = 999,
            StrategyName = "PerAttraction",
            N = null,
        };

        _strategyRepository.Update(configWithDifferentId);

        StrategyConfiguration? result = _strategyRepository.Get();

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Id);
        Assert.AreEqual("PerAttraction", result.StrategyName);
    }
}
