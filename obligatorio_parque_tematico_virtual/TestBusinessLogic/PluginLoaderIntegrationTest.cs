using BusinessLogic.Plugins;
using IBusinessLogic.Strategy;
using Models.Out;

namespace TestBusinessLogic;

[TestClass]
public class PluginLoaderIntegrationTest
{
    private string? _tempPluginDirectory;

    [TestInitialize]
    public void Setup()
    {
        _tempPluginDirectory = CreateTempPluginDirectory();
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (_tempPluginDirectory != null && Directory.Exists(_tempPluginDirectory))
        {
            try
            {
                Directory.Delete(_tempPluginDirectory, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    [TestMethod]
    public void LoadPlugins_WithValidPluginDll_DiscoversPluginSuccessfully()
    {
        // Arrange
        CopyExamplePluginDll(_tempPluginDirectory!);
        var pluginLoader = new PluginLoader(_tempPluginDirectory!);

        // Act
        var plugins = pluginLoader.LoadPlugins();

        // Assert
        Assert.AreEqual(1, plugins.Count);
        Assert.AreEqual("PuntuacionPorHora", plugins[0].Name);
        Assert.IsNotNull(plugins[0].Description);
        Assert.IsTrue(plugins[0].Description.Contains("hour"));
        Assert.IsNotNull(plugins[0].Author);
        Assert.AreEqual("Theme Park Team", plugins[0].Author);
    }

    [TestMethod]
    public void GetPluginByName_WithLoadedPlugin_ReturnsPluginInfo()
    {
        // Arrange
        CopyExamplePluginDll(_tempPluginDirectory!);
        var pluginLoader = new PluginLoader(_tempPluginDirectory!);
        pluginLoader.LoadPlugins();

        // Act
        var plugin = pluginLoader.GetPluginByName("PuntuacionPorHora");

        // Assert
        Assert.IsNotNull(plugin);
        Assert.AreEqual("PuntuacionPorHora", plugin.Name);
        Assert.IsTrue(plugin.Description.Contains("hour"));
        Assert.AreEqual("Theme Park Team", plugin.Author);
    }

    [TestMethod]
    public void CreateStrategyInstance_WithLoadedPlugin_CreatesValidStrategy()
    {
        // Arrange
        CopyExamplePluginDll(_tempPluginDirectory!);
        var pluginLoader = new PluginLoader(_tempPluginDirectory!);
        pluginLoader.LoadPlugins();

        // Act
        var strategy = pluginLoader.CreateStrategyInstance("PuntuacionPorHora");

        // Assert
        Assert.IsNotNull(strategy);
        Assert.IsInstanceOfType(strategy, typeof(IConcreteStrategy));
        Assert.AreEqual("PuntuacionPorHora", strategy.Name);
    }

    [TestMethod]
    public void DiscoverPlugins_ReadsPluginDescriptionAttribute()
    {
        // Arrange
        CopyExamplePluginDll(_tempPluginDirectory!);
        var pluginLoader = new PluginLoader(_tempPluginDirectory!);

        // Act
        var plugins = pluginLoader.LoadPlugins();

        // Assert - Tests PluginDescriptionAttribute coverage
        Assert.IsTrue(plugins.Any());
        var plugin = plugins.First();
        Assert.IsNotNull(plugin.Description);
        Assert.IsTrue(plugin.Description.Length > 0);
        Assert.IsTrue(plugin.Description.Contains("hour") || plugin.Description.Contains("peak"));
    }

    [TestMethod]
    public void DiscoverPlugins_ReadsPluginAuthorAttribute()
    {
        // Arrange
        CopyExamplePluginDll(_tempPluginDirectory!);
        var pluginLoader = new PluginLoader(_tempPluginDirectory!);

        // Act
        var plugins = pluginLoader.LoadPlugins();

        // Assert - Tests PluginAuthorAttribute coverage
        Assert.IsTrue(plugins.Any());
        var plugin = plugins.First();
        Assert.IsNotNull(plugin.Author);
        Assert.AreEqual("Theme Park Team", plugin.Author);
    }

    [TestMethod]
    public void MapToResponse_ConvertsPluginInfoToResponse()
    {
        // Arrange
        CopyExamplePluginDll(_tempPluginDirectory!);
        var pluginLoader = new PluginLoader(_tempPluginDirectory!);

        // Act - LoadPlugins internally uses MapToResponse
        List<PluginInfoResponse> plugins = pluginLoader.LoadPlugins();

        // Assert - Tests MapToResponse coverage
        Assert.IsTrue(plugins.Any());
        PluginInfoResponse plugin = plugins.First();
        Assert.IsInstanceOfType(plugin, typeof(PluginInfoResponse));
        Assert.AreEqual("PuntuacionPorHora", plugin.Name);
        Assert.IsNotNull(plugin.Description);
        Assert.IsNotNull(plugin.Author);
        Assert.IsNotNull(plugin.Version);
    }

    private string CreateTempPluginDirectory()
    {
        string tempPath = Path.Combine(Path.GetTempPath(), $"PluginTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempPath);
        return tempPath;
    }

    private void CopyExamplePluginDll(string targetDir)
    {
        // Find ExamplePlugin.dll - try different paths (Release first for CI, then Debug for local)
        string[] possiblePaths = {
            // Release configuration (used by CI/CD)
            "../../../../ExamplePlugin/bin/Release/net8.0/ExamplePlugin.dll",
            "../../../ExamplePlugin/bin/Release/net8.0/ExamplePlugin.dll",
            "../../ExamplePlugin/bin/Release/net8.0/ExamplePlugin.dll",
            "../ExamplePlugin/bin/Release/net8.0/ExamplePlugin.dll",
            // Debug configuration (local development)
            "../../../../ExamplePlugin/bin/Debug/net8.0/ExamplePlugin.dll",
            "../../../ExamplePlugin/bin/Debug/net8.0/ExamplePlugin.dll",
            "../../ExamplePlugin/bin/Debug/net8.0/ExamplePlugin.dll",
            "../ExamplePlugin/bin/Debug/net8.0/ExamplePlugin.dll"
        };

        string? sourceDll = null;
        foreach (var path in possiblePaths)
        {
            var fullPath = Path.GetFullPath(path);
            if (File.Exists(fullPath))
            {
                sourceDll = fullPath;
                break;
            }
        }

        if (sourceDll == null)
        {
            Assert.Fail("Could not find ExamplePlugin.dll. Make sure the ExamplePlugin project is built.");
        }

        string targetDll = Path.Combine(targetDir, "ExamplePlugin.dll");
        File.Copy(sourceDll, targetDll, overwrite: true);
    }
}
