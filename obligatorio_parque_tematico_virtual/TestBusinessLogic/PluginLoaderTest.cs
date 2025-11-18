using Microsoft.VisualStudio.TestTools.UnitTesting;
using BusinessLogic.Plugins;
using IBusinessLogic;
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

        Assert.AreEqual(3, plugins.Count);
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

        Assert.AreEqual(3, plugins.Count);
    }

    [TestMethod]
    public void LoadPlugins_DirectoryWithNonDllFiles_ReturnsEmptyList()
    {
        string txtFile = Path.Combine(_testPluginsPath, "notaplugin.txt");
        File.WriteAllText(txtFile, "This is not a DLL");

        List<PluginInfoResponse> plugins = _pluginLoader.LoadPlugins();

        Assert.AreEqual(3, plugins.Count);
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

        Assert.AreEqual(3, plugins.Count);
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
            Assert.ThrowsException<ArgumentException>(() => { _pluginLoader.AddPlugin(stream, "TestPlugin.txt"); });
        }
    }

    [TestMethod]
    public void AddPlugin_WithNullStream_ThrowsArgumentException()
    {
        Assert.ThrowsException<ArgumentException>(() => { _pluginLoader.AddPlugin(null!, "TestPlugin.dll"); });
    }

    [TestMethod]
    public void AddPlugin_WithEmptyFileName_ThrowsArgumentException()
    {
        byte[] fileContent = new byte[] { 0x4D, 0x5A };
        using (MemoryStream stream = new MemoryStream(fileContent))
        {
            Assert.ThrowsException<ArgumentException>(() => { _pluginLoader.AddPlugin(stream, ""); });
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

    [TestMethod]
    public void CreateStrategyInstance_WithBuiltInPluginPerEvent_ReturnsStrategyInstance()
    {
        IConcreteStrategy strategy = _pluginLoader.CreateStrategyInstance("PerEvent");

        Assert.IsNotNull(strategy);
        Assert.AreEqual("PerEvent", strategy.Name);
    }

    [TestMethod]
    public void CreateStrategyInstance_WithBuiltInPluginPerAttraction_ReturnsStrategyInstance()
    {
        IConcreteStrategy strategy = _pluginLoader.CreateStrategyInstance("PerAttraction");

        Assert.IsNotNull(strategy);
        Assert.AreEqual("PerAttraction", strategy.Name);
    }

    [TestMethod]
    public void CreateStrategyInstance_WithParametersCombo_ReturnsStrategyInstanceWithCorrectParameter()
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            { "n", 30 }
        };

        IConcreteStrategy strategy = _pluginLoader.CreateStrategyInstance("Combo", parameters);

        Assert.IsNotNull(strategy);
        Assert.AreEqual("Combo", strategy.Name);
    }

    [TestMethod]
    public void CreateStrategyInstance_WithMismatchedParametersCount_FallsBackToParameterlessConstructor()
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            { "x", 30 },
            { "y", 40 }
        };

        IConcreteStrategy strategy = _pluginLoader.CreateStrategyInstance("PerEvent", parameters);

        Assert.IsNotNull(strategy);
        Assert.AreEqual("PerEvent", strategy.Name);
    }

    [TestMethod]
    public void AddPlugin_WithNonReadableStream_ThrowsArgumentException()
    {
        MemoryStream stream = new MemoryStream(new byte[] { 0x4D, 0x5A });
        stream.Close();

        Assert.ThrowsException<ArgumentException>(() =>
        {
            _pluginLoader.AddPlugin(stream, "TestPlugin.dll");
        });

        stream.Dispose();
    }

    [TestMethod]
    public void AddPlugin_WithWhitespaceFileName_ThrowsArgumentException()
    {
        byte[] fileContent = new byte[] { 0x4D, 0x5A };
        MemoryStream stream = new MemoryStream(fileContent);

        Assert.ThrowsException<ArgumentException>(() =>
        {
            _pluginLoader.AddPlugin(stream, "   ");
        });

        stream.Dispose();
    }

    [TestMethod]
    public void AddPlugin_WithUppercaseDllExtension_SavesFileToPluginsDirectory()
    {
        byte[] fileContent = new byte[] { 0x4D, 0x5A };
        using (MemoryStream stream = new MemoryStream(fileContent))
        {
            _pluginLoader.AddPlugin(stream, "TestPlugin.DLL");
        }

        string expectedPath = Path.Combine(_testPluginsPath, "TestPlugin.DLL");
        Assert.IsTrue(File.Exists(expectedPath));
    }

    [TestMethod]
    public void AddPlugin_WithNullFileName_ThrowsArgumentException()
    {
        byte[] fileContent = new byte[] { 0x4D, 0x5A };
        MemoryStream stream = new MemoryStream(fileContent);

        Assert.ThrowsException<ArgumentException>(() =>
        {
            _pluginLoader.AddPlugin(stream, null!);
        });

        stream.Dispose();
    }
}