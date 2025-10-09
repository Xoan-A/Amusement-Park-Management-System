using Domain;

namespace Models.Out;

public class TopTenResponse
{
    public List<UserResponse> TopTenUsers { get; set; } = new List<UserResponse>();
}