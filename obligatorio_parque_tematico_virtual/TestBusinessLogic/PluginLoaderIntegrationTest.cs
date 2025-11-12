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
            }
        }
    }

    [TestMethod]
    public void LoadPlugins_WithValidPluginDll_DiscoversPluginSuccessfully()
    {
        CopyExamplePluginDll(_tempPluginDirectory!);
        PluginLoader pluginLoader = new PluginLoader(_tempPluginDirectory!);

        List<PluginInfoResponse> plugins = pluginLoader.LoadPlugins();

        Assert.AreEqual(3, plugins.Count);
        Assert.IsTrue(plugins.Any(p => p.Name == "PuntuacionPorHora"));
    }

    [TestMethod]
    public void CreateStrategyInstance_WithLoadedPlugin_CreatesValidStrategy()
    {
        CopyExamplePluginDll(_tempPluginDirectory!);
        PluginLoader pluginLoader = new PluginLoader(_tempPluginDirectory!);
        pluginLoader.LoadPlugins();

        IConcreteStrategy strategy = pluginLoader.CreateStrategyInstance("PuntuacionPorHora");

        Assert.IsNotNull(strategy);
        Assert.IsInstanceOfType(strategy, typeof(IConcreteStrategy));
        Assert.AreEqual("PuntuacionPorHora", strategy.Name);
    }

    [TestMethod]
    public void DiscoverPlugins_ReadsPluginDescriptionAttribute()
    {
        CopyExamplePluginDll(_tempPluginDirectory!);
        PluginLoader pluginLoader = new PluginLoader(_tempPluginDirectory!);

        List<PluginInfoResponse> plugins = pluginLoader.LoadPlugins();

        Assert.IsTrue(plugins.Any());
        PluginInfoResponse plugin = plugins.First();
    }

    [TestMethod]
    public void DiscoverPlugins_ReadsPluginAuthorAttribute()
    {
        CopyExamplePluginDll(_tempPluginDirectory!);
        PluginLoader pluginLoader = new PluginLoader(_tempPluginDirectory!);

        List<PluginInfoResponse> plugins = pluginLoader.LoadPlugins();

        Assert.IsTrue(plugins.Any());
        PluginInfoResponse plugin = plugins.First();
    }

    [TestMethod]
    public void MapToResponse_ConvertsPluginInfoToResponse()
    {
        CopyExamplePluginDll(_tempPluginDirectory!);
        PluginLoader pluginLoader = new PluginLoader(_tempPluginDirectory!);

        List<PluginInfoResponse> plugins = pluginLoader.LoadPlugins();

        Assert.IsTrue(plugins.Any());
        PluginInfoResponse? plugin = plugins.FirstOrDefault(p => p.Name == "PuntuacionPorHora");
        Assert.IsNotNull(plugin);
        Assert.IsInstanceOfType(plugin, typeof(PluginInfoResponse));
        Assert.AreEqual("PuntuacionPorHora", plugin.Name);
    }

    private string CreateTempPluginDirectory()
    {
        string tempPath = Path.Combine(Path.GetTempPath(), $"PluginTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempPath);
        return tempPath;
    }

    private void CopyExamplePluginDll(string targetDir)
    {
        string[] possiblePaths =
        {
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
        foreach (String path in possiblePaths)
        {
            String fullPath = Path.GetFullPath(path);
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