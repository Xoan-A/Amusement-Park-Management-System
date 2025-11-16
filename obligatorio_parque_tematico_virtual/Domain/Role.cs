namespace Domain;

public class Role
{
    public const string Administrator = "Administrator";
    public const string Operator = "Operator";
    public const string Visitor = "Visitor";

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
