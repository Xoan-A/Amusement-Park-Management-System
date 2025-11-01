using Domain;
using IBusinessLogic.Strategy;

namespace IBusinessLogic;

public interface IPluginLoader
{
    List<PluginInfo> LoadPlugins();
    PluginInfo? GetPluginByName(string name);
    IConcreteStrategy CreateStrategyInstance(string name);
    List<string> GetAvailablePluginNames();
}
