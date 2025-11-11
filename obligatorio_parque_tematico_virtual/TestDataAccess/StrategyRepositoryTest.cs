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

        StrategyConfiguration? seededConfig = _context.StrategyConfigurations.FirstOrDefault(s => s.Id == 1);
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
    public async Task Get_ShouldReturnConfiguration_WhenConfigurationExists()
    {
        strategyConfiguration.Id = 1;
        _context.StrategyConfigurations.Add(strategyConfiguration);
        await _context.SaveChangesAsync();

        StrategyConfiguration? result = await _strategyRepository.Get();

        Assert.AreEqual("Combo", result.StrategyName);
    }

    [TestMethod]
    public async Task Get_ShouldReturnNull_WhenConfigurationDoesNotExist()
    {
        StrategyConfiguration? result = await _strategyRepository.Get();

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task Update_ShouldCreateConfiguration_WhenConfigurationDoesNotExist()
    {
        await _strategyRepository.Update(strategyConfiguration);

        StrategyConfiguration? result = await _strategyRepository.Get();

        Assert.AreEqual(1, result.Id);
        Assert.AreEqual("Combo", result.StrategyName);
        Assert.AreEqual(5, result.N);
        Assert.AreEqual(1, _context.StrategyConfigurations.Count());
    }

    [TestMethod]
    public async Task Update_ShouldUpdateConfiguration_WhenConfigurationExists()
    {
        strategyConfiguration.Id = 1;
        _context.StrategyConfigurations.Add(strategyConfiguration);
        await _context.SaveChangesAsync();

        StrategyConfiguration updatedConfiguration = new StrategyConfiguration
        {
            StrategyName = "PerEvent",
            N = null,
        };

        await _strategyRepository.Update(updatedConfiguration);

        StrategyConfiguration? result = await _strategyRepository.Get();

        Assert.AreEqual(1, result.Id);
        Assert.AreEqual("PerEvent", result.StrategyName);
        Assert.IsNull(result.N);
        Assert.AreEqual(1, _context.StrategyConfigurations.Count());
    }

    [TestMethod]
    public async Task Update_ShouldAlwaysSetIdToOne_WhenCreatingNewConfiguration()
    {
        StrategyConfiguration configWithDifferentId = new StrategyConfiguration
        {
            Id = 999,
            StrategyName = "PerAttraction",
            N = null,
        };

        await _strategyRepository.Update(configWithDifferentId);

        StrategyConfiguration? result = await _strategyRepository.Get();

        Assert.AreEqual(1, result.Id);
        Assert.AreEqual("PerAttraction", result.StrategyName);
    }
}
