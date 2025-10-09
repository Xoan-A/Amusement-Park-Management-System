namespace Domain;

public class Role
{
    public const string ADMINISTRATOR = "Administrator";
    public const string OPERATOR = "Operator";
    public const string VISITOR = "Visitor";

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
