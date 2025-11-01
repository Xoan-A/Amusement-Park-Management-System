using Microsoft.VisualStudio.TestTools.UnitTesting;
using BusinessLogic.Plugins;
using IBusinessLogic.Strategy;
using System.Reflection;

namespace BusinessLogic.Tests;

[TestClass]
public class PluginLoaderTest
{
    private PluginLoader _pluginLoader = null!;
    private string _testPluginsPath = null!;

    [TestInitialize]
    public void Setup()
    {
        _testPluginsPath = Path.Combine(Path.GetTempPath(), "TestPlugins");
        Directory.CreateDirectory(_testPluginsPath);
        _pluginLoader = new PluginLoader(_testPluginsPath);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_testPluginsPath))
        {
            Directory.Delete(_testPluginsPath, true);
        }
    }

    [TestMethod]
    public void LoadPlugins_EmptyDirectory_ReturnsEmptyList()
    {
        // Act
        var plugins = _pluginLoader.LoadPlugins();

        // Assert
        Assert.IsNotNull(plugins);
        Assert.AreEqual(0, plugins.Count);
    }

    [TestMethod]
    public void GetPluginByName_NonExistentPlugin_ReturnsNull()
    {
        // Act
        var plugin = _pluginLoader.GetPluginByName("NonExistent");

        // Assert
        Assert.IsNull(plugin);
    }

    [TestMethod]
    public void CreateStrategyInstance_NonExistentPlugin_ThrowsException()
    {
        // Act & Assert
        Assert.ThrowsException<KeyNotFoundException>(() =>
        {
            _pluginLoader.CreateStrategyInstance("NonExistent");
        });
    }
}
