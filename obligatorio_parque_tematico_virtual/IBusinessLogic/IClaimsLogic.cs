using System.Security.Claims;

namespace IBusinessLogic;

public interface IClaimsLogic
{
    Guid GetCurrentUserId(ClaimsPrincipal user);
}

