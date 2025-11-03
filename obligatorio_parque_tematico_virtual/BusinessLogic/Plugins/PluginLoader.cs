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
        LoadPlugins();
    }

    public List<PluginInfoResponse> LoadPlugins()
    {
        _availablePlugins.Clear();

        if (!Directory.Exists(_pluginsPath))
        {
            return new List<PluginInfoResponse>();
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
                            Description = type.GetCustomAttribute<PluginDescriptionAttribute>()?.Description ?? string.Empty,
                            Author = type.GetCustomAttribute<PluginAuthorAttribute>()?.Author ?? string.Empty,
                            Version = assembly.GetName().Version?.ToString() ?? "1.0.0",
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

    public PluginInfoResponse? GetPluginByName(string name)
    {
        PluginInfo pluginInfo = _availablePlugins.GetValueOrDefault(name);
        return pluginInfo != null ? MapToResponse(pluginInfo) : null;
    }

    public IConcreteStrategy CreateStrategyInstance(string name)
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

        object? instance = Activator.CreateInstance(type);
        if (instance is not IConcreteStrategy strategy)
        {
            throw new InvalidOperationException($"Type '{pluginInfo.TypeName}' does not implement IConcreteStrategy");
        }

        return strategy;
    }

    public List<string> GetAvailablePluginNames()
    {
        return _availablePlugins.Keys.ToList();
    }

    private PluginInfoResponse MapToResponse(PluginInfo pluginInfo)
    {
        return new PluginInfoResponse
        {
            Name = pluginInfo.Name,
            Description = pluginInfo.Description,
            Author = pluginInfo.Author,
            Version = pluginInfo.Version
        };
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class PluginDescriptionAttribute : Attribute
{
    public string Description { get; }
    public PluginDescriptionAttribute(string description)
    {
        Description = description;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class PluginAuthorAttribute : Attribute
{
    public string Author { get; }
    public PluginAuthorAttribute(string author)
    {
        Author = author;
    }
}
