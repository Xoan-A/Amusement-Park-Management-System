using System.Reflection;
using IBusinessLogic.Strategy;
using IBusinessLogic;
using Domain;
using Models.Out;

namespace BusinessLogic.Plugins;

public class PluginLoader : IPluginLoader
{
    private readonly string _pluginsPath;
    private readonly Dictionary<string, PluginInfo> _availablePlugins;

    public PluginLoader(string pluginsPath)
    {
        _pluginsPath = pluginsPath;
        _availablePlugins = new Dictionary<string, PluginInfo>();
        DiscoverPluginsInCurrentAssembly();
        LoadPlugins();
    }

    public List<PluginInfoResponse> LoadPlugins()
    {
        string currentAssemblyPath = Assembly.GetExecutingAssembly().Location;
        Dictionary<string, PluginInfo> builtInPlugins = _availablePlugins
            .Where(p => p.Value.AssemblyPath == currentAssemblyPath)
            .ToDictionary(p => p.Key, p => p.Value);

        _availablePlugins.Clear();

        foreach (KeyValuePair<string, PluginInfo> plugin in builtInPlugins)
        {
            _availablePlugins[plugin.Key] = plugin.Value;
        }

        if (!Directory.Exists(_pluginsPath))
        {
            return _availablePlugins.Values.Select(MapToResponse).ToList();
        }

        string[] dllFiles = Directory.GetFiles(_pluginsPath, "*.dll");

        foreach (string dllFile in dllFiles)
        {
            try
            {
                Assembly assembly = Assembly.LoadFrom(dllFile);
                DiscoverPlugins(assembly, dllFile);
            }
            catch (Exception)
            {
                continue;
            }
        }

        return _availablePlugins.Values.Select(MapToResponse).ToList();
    }

    private void DiscoverPlugins(Assembly assembly, string assemblyPath)
    {
        Type[] types;
        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException)
        {
            return;
        }

        foreach (Type type in types)
        {
            if (type.IsClass && !type.IsAbstract && typeof(IConcreteStrategy).IsAssignableFrom(type))
            {
                try
                {
                    IConcreteStrategy? instance = Activator.CreateInstance(type) as IConcreteStrategy;
                    if (instance != null)
                    {
                        PluginInfo pluginInfo = new PluginInfo
                        {
                            Name = instance.Name,
                            AssemblyPath = assemblyPath,
                            TypeName = type.FullName ?? type.Name
                        };

                        _availablePlugins[instance.Name] = pluginInfo;
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }
    }

    private void DiscoverPluginsInCurrentAssembly()
    {
        Assembly currentAssembly = Assembly.GetExecutingAssembly();
        DiscoverPlugins(currentAssembly, currentAssembly.Location);
    }

    public IConcreteStrategy CreateStrategyInstance(string name, Dictionary<string, object>? parameters = null)
    {
        if (!_availablePlugins.TryGetValue(name, out PluginInfo? pluginInfo))
        {
            throw new KeyNotFoundException($"Plugin '{name}' not found");
        }

        Assembly assembly = Assembly.LoadFrom(pluginInfo.AssemblyPath);
        Type? type = assembly.GetType(pluginInfo.TypeName);

        if (type == null)
        {
            throw new InvalidOperationException($"Type '{pluginInfo.TypeName}' not found in assembly");
        }

        object? instance = null;

        if (parameters != null && parameters.Count > 0)
        {
            ConstructorInfo[] constructors = type.GetConstructors();
            foreach (ConstructorInfo constructor in constructors)
            {
                ParameterInfo[] ctorParams = constructor.GetParameters();
                if (ctorParams.Length == parameters.Count)
                {
                    object[] args = new object[ctorParams.Length];
                    bool matched = true;

                    for (int i = 0; i < ctorParams.Length; i++)
                    {
                        string paramName = ctorParams[i].Name?.ToLower() ?? "";
                        if (parameters.ContainsKey(paramName))
                        {
                            args[i] = parameters[paramName];
                        }
                        else
                        {
                            matched = false;
                            break;
                        }
                    }

                    if (matched)
                    {
                        instance = constructor.Invoke(args);
                        break;
                    }
                }
            }
        }

        if (instance == null)
        {
            instance = Activator.CreateInstance(type);
        }

        if (instance is not IConcreteStrategy strategy)
        {
            throw new InvalidOperationException($"Type '{pluginInfo.TypeName}' does not implement IConcreteStrategy");
        }

        return strategy;
    }

    public void AddPlugin(Stream dllStream, string fileName)
    {
        if (dllStream == null || !dllStream.CanRead)
        {
            throw new ArgumentException("Invalid stream provided");
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File name cannot be empty");
        }

        if (!fileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Only .dll files are allowed");
        }

        if (!Directory.Exists(_pluginsPath))
        {
            Directory.CreateDirectory(_pluginsPath);
        }

        string filePath = Path.Combine(_pluginsPath, fileName);

        using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        {
            dllStream.CopyTo(fileStream);
        }

        LoadPlugins();
    }

    private PluginInfoResponse MapToResponse(PluginInfo pluginInfo)
    {
        return new PluginInfoResponse
        {
            Name = pluginInfo.Name,
        };
    }
}