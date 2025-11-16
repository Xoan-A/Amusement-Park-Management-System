
namespace Models.In;

public class CreateUserRequest
{
    public string Name { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public DateTime? BirthDate { get; set; }
    public int? MembershipLevel { get; set; }
    public List<string> Roles { get; set; }
}