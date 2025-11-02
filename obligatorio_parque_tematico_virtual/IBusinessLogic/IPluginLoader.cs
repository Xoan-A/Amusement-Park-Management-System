using IBusinessLogic.Strategy;
using Models.Out;

namespace IBusinessLogic;

public interface IPluginLoader
{
    List<PluginInfoResponse> LoadPlugins();
    PluginInfoResponse? GetPluginByName(string name);
    IConcreteStrategy CreateStrategyInstance(string name);
    List<string> GetAvailablePluginNames();
}
