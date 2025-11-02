using Microsoft.VisualStudio.TestTools.UnitTesting;
using BusinessLogic.Plugins;
using IBusinessLogic.Strategy;
using Models.Out;
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

    [TestMethod]
    public void LoadPlugins_NonExistentDirectory_ReturnsEmptyList()
    {
        // Arrange
        string nonExistentPath = Path.Combine(Path.GetTempPath(), "NonExistentPlugins" + Guid.NewGuid());
        var loader = new PluginLoader(nonExistentPath);

        // Act
        var plugins = loader.LoadPlugins();

        // Assert
        Assert.IsNotNull(plugins);
        Assert.AreEqual(0, plugins.Count);
    }

    [TestMethod]
    public void GetAvailablePluginNames_EmptyDirectory_ReturnsEmptyList()
    {
        // Act
        var pluginNames = _pluginLoader.GetAvailablePluginNames();

        // Assert
        Assert.IsNotNull(pluginNames);
        Assert.AreEqual(0, pluginNames.Count);
    }

    [TestMethod]
    public void LoadPlugins_DirectoryWithNonDllFiles_ReturnsEmptyList()
    {
        // Arrange
        string txtFile = Path.Combine(_testPluginsPath, "notaplugin.txt");
        File.WriteAllText(txtFile, "This is not a DLL");

        // Act
        var plugins = _pluginLoader.LoadPlugins();

        // Assert
        Assert.IsNotNull(plugins);
        Assert.AreEqual(0, plugins.Count);
    }

    [TestMethod]
    public void LoadPlugins_CalledMultipleTimes_ClearsAndReloads()
    {
        // Act
        var firstLoad = _pluginLoader.LoadPlugins();
        var secondLoad = _pluginLoader.LoadPlugins();

        // Assert
        Assert.AreEqual(firstLoad.Count, secondLoad.Count);
    }

    [TestMethod]
    public void CreateStrategyInstance_WithNonExistentPlugin_ThrowsKeyNotFoundException()
    {
        // Arrange
        _pluginLoader.LoadPlugins(); // Empty plugins

        // Act & Assert
        Assert.ThrowsException<KeyNotFoundException>(() =>
        {
            _pluginLoader.CreateStrategyInstance("NonExistentPlugin");
        });
    }

    [TestMethod]
    public void GetPluginByName_WithNonExistentPlugin_ReturnsNull()
    {
        // Arrange
        _pluginLoader.LoadPlugins(); // Empty plugins

        // Act
        var result = _pluginLoader.GetPluginByName("NonExistentPlugin");

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetAvailablePluginNames_AfterLoadingPlugins_ReturnsPluginNames()
    {
        // Arrange
        _pluginLoader.LoadPlugins();

        // Act
        var pluginNames = _pluginLoader.GetAvailablePluginNames();

        // Assert
        Assert.IsNotNull(pluginNames);
        Assert.AreEqual(0, pluginNames.Count); // Empty directory
    }

    [TestMethod]
    public void LoadPlugins_WithCorruptDll_SkipsInvalidAssembly()
    {
        // Arrange - Create a corrupt DLL file (text file with .dll extension)
        string corruptDllPath = Path.Combine(_testPluginsPath, "corrupt.dll");
        File.WriteAllText(corruptDllPath, "This is not a valid assembly");

        // Act
        var plugins = _pluginLoader.LoadPlugins();

        // Assert - Should skip the corrupt file and return empty list
        Assert.IsNotNull(plugins);
        Assert.AreEqual(0, plugins.Count);
    }
}
