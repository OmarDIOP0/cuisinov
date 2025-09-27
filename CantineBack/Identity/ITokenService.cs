using System.Security.Claims;

namespace CantineBack.Identity
{
    public interface ITokenService
    {
     
        Tuple<string,DateTime> GenerateAccessToken(IEnumerable<Claim> claims);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
