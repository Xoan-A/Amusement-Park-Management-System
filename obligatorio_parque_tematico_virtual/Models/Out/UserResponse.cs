
namespace Models.Out;

public class UserResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public DateTime? BirthDate { get; set; }
    public int? MembershipLevel { get; set; }
    public ICollection<string> UserRoles { get; set; }
    public int Score { get; set; }
}