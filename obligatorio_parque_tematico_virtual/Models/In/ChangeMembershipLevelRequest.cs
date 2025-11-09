using System.ComponentModel.DataAnnotations;

namespace Models.In;

public class ChangeMembershipLevelRequest
{
    [Required]
    public int MembershipLevel { get; set; }
}

