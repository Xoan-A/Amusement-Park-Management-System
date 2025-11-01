namespace Domain;

public class PluginInfo
{
    private string _name = string.Empty;
    private string _description = string.Empty;
    private string _author = string.Empty;
    private string _version = string.Empty;

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

    public string Description
    {
        get => _description;
        set => _description = value ?? string.Empty;
    }

    public string Author
    {
        get => _author;
        set => _author = value ?? string.Empty;
    }

    public string Version
    {
        get => _version;
        set => _version = value ?? string.Empty;
    }

    public string AssemblyPath { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
}
