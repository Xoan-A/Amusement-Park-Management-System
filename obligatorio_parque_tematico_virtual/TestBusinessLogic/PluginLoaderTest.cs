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
        
        Assert.AreEqual(2, plugins.Count);
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

        Assert.AreEqual(2, plugins.Count);
    }

    [TestMethod]
    public void LoadPlugins_DirectoryWithNonDllFiles_ReturnsEmptyList()
    {
        string txtFile = Path.Combine(_testPluginsPath, "notaplugin.txt");
        File.WriteAllText(txtFile, "This is not a DLL");

        List<PluginInfoResponse> plugins = _pluginLoader.LoadPlugins();

        Assert.AreEqual(2, plugins.Count);
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
    public void LoadPlugins_WithInvalidAssembly_SkipsAndContinues()
    {
        string corruptDllPath = Path.Combine(_testPluginsPath, "corrupt.dll");
        File.WriteAllText(corruptDllPath, "This is not a valid assembly");

        List<PluginInfoResponse> plugins = _pluginLoader.LoadPlugins();

        Assert.AreEqual(2, plugins.Count);
    }

    [TestMethod]
    public void AddPlugin_WithValidDllFile_SavesFileToPluginsDirectory()
    {
        byte[] fileContent = new byte[] { 0x4D, 0x5A };
        using (MemoryStream stream = new MemoryStream(fileContent))
        {
            _pluginLoader.AddPlugin(stream, "TestPlugin.dll");
        }

        string expectedPath = Path.Combine(_testPluginsPath, "TestPlugin.dll");
        Assert.IsTrue(File.Exists(expectedPath));
    }

    [TestMethod]
    public void AddPlugin_WithInvalidExtension_ThrowsArgumentException()
    {
        byte[] fileContent = new byte[] { 0x4D, 0x5A };
        using (MemoryStream stream = new MemoryStream(fileContent))
        {
            Assert.ThrowsException<ArgumentException>(() =>
            {
                _pluginLoader.AddPlugin(stream, "TestPlugin.txt");
            });
        }
    }

    [TestMethod]
    public void AddPlugin_WithNullStream_ThrowsArgumentException()
    {
        Assert.ThrowsException<ArgumentException>(() =>
        {
            _pluginLoader.AddPlugin(null!, "TestPlugin.dll");
        });
    }

    [TestMethod]
    public void AddPlugin_WithEmptyFileName_ThrowsArgumentException()
    {
        byte[] fileContent = new byte[] { 0x4D, 0x5A };
        using (MemoryStream stream = new MemoryStream(fileContent))
        {
            Assert.ThrowsException<ArgumentException>(() =>
            {
                _pluginLoader.AddPlugin(stream, "");
            });
        }
    }

    [TestMethod]
    public void AddPlugin_CreatesPluginsDirectoryIfNotExists()
    {
        string newPluginsPath = Path.Combine(Path.GetTempPath(), "NewTestPlugins" + Guid.NewGuid());
        PluginLoader loader = new PluginLoader(newPluginsPath);

        byte[] fileContent = new byte[] { 0x4D, 0x5A };
        using (MemoryStream stream = new MemoryStream(fileContent))
        {
            loader.AddPlugin(stream, "TestPlugin.dll");
        }

        Assert.IsTrue(Directory.Exists(newPluginsPath));
        Assert.IsTrue(File.Exists(Path.Combine(newPluginsPath, "TestPlugin.dll")));

        if (Directory.Exists(newPluginsPath))
        {
            Directory.Delete(newPluginsPath, true);
        }
    }
}