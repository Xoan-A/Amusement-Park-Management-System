using System.Security.Claims;
using IBusinessLogic;

namespace BusinessLogic;

public class ClaimsLogic : IClaimsLogic
{
    public Guid GetCurrentUserId(ClaimsPrincipal user)
    {
        string? userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }

        return Guid.Parse(userIdClaim);
    }
}

