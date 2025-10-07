
namespace Models.In
{
    public class AddRolesRequest
    {
        public Guid UserId { get; set; }

        public string Role { get; set; }
    }
}