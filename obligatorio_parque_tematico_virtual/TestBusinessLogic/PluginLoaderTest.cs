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
        List<PluginInfoResponse> plugins = _pluginLoader.LoadPlugins();

        Assert.AreEqual(0, plugins.Count);
    }

    [TestMethod]
    public void GetPluginByName_NonExistentPlugin_ReturnsNull()
    {
        PluginInfoResponse? plugin = _pluginLoader.GetPluginByName("NonExistent");

        Assert.IsNull(plugin);
    }

    [TestMethod]
    public void CreateStrategyInstance_NonExistentPlugin_ThrowsException()
    {
        Assert.ThrowsException<KeyNotFoundException>(() => { _pluginLoader.CreateStrategyInstance("NonExistent"); });
    }

    [TestMethod]
    public void LoadPlugins_NonExistentDirectory_ReturnsEmptyList()
    {
        string nonExistentPath = Path.Combine(Path.GetTempPath(), "NonExistentPlugins" + Guid.NewGuid());
        PluginLoader loader = new PluginLoader(nonExistentPath);

        List<PluginInfoResponse> plugins = loader.LoadPlugins();

        Assert.AreEqual(0, plugins.Count);
    }

    [TestMethod]
    public void GetAvailablePluginNames_EmptyDirectory_ReturnsEmptyList()
    {
        List<string> pluginNames = _pluginLoader.GetAvailablePluginNames();

        Assert.AreEqual(0, pluginNames.Count);
    }

    [TestMethod]
    public void LoadPlugins_DirectoryWithNonDllFiles_ReturnsEmptyList()
    {
        string txtFile = Path.Combine(_testPluginsPath, "notaplugin.txt");
        File.WriteAllText(txtFile, "This is not a DLL");

        List<PluginInfoResponse> plugins = _pluginLoader.LoadPlugins();

        Assert.AreEqual(0, plugins.Count);
    }

    [TestMethod]
    public void LoadPlugins_CalledMultipleTimes_ClearsAndReloads()
    {
        List<PluginInfoResponse> firstLoad = _pluginLoader.LoadPlugins();
        List<PluginInfoResponse> secondLoad = _pluginLoader.LoadPlugins();

        Assert.AreEqual(firstLoad.Count, secondLoad.Count);
    }

    [TestMethod]
    public void CreateStrategyInstance_WithNonExistentPlugin_ThrowsKeyNotFoundException()
    {
        _pluginLoader.LoadPlugins();

        Assert.ThrowsException<KeyNotFoundException>(() =>
        {
            _pluginLoader.CreateStrategyInstance("NonExistentPlugin");
        });
    }

    [TestMethod]
    public void GetPluginByName_WithNonExistentPlugin_ReturnsNull()
    {
        _pluginLoader.LoadPlugins();

        PluginInfoResponse? result = _pluginLoader.GetPluginByName("NonExistentPlugin");

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetAvailablePluginNames_AfterLoadingPlugins_ReturnsPluginNames()
    {
        _pluginLoader.LoadPlugins();

        List<string> pluginNames = _pluginLoader.GetAvailablePluginNames();

        Assert.IsNotNull(pluginNames);
    }

    [TestMethod]
    public void LoadPlugins_WithInvalidAssembly_SkipsAndContinues()
    {
        string corruptDllPath = Path.Combine(_testPluginsPath, "corrupt.dll");
        File.WriteAllText(corruptDllPath, "This is not a valid assembly");

        List<PluginInfoResponse> plugins = _pluginLoader.LoadPlugins();

        Assert.AreEqual(0, plugins.Count);
    }
}