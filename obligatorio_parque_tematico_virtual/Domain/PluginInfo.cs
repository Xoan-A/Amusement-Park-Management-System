namespace Domain;

public class PluginInfo
{
    private string _name = string.Empty;

    public string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Plugin name cannot be empty");
            _name = value;
        }
    }

    public string AssemblyPath { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
}