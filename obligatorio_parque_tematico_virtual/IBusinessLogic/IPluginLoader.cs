using IBusinessLogic.Strategy;
using Models.Out;

namespace IBusinessLogic;

public interface IPluginLoader
{
    List<PluginInfoResponse> LoadPlugins();
    IConcreteStrategy CreateStrategyInstance(string name, Dictionary<string, object>? parameters = null);
    void AddPlugin(Stream dllStream, string fileName);
}