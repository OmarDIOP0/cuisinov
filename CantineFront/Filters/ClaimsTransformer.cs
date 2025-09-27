using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Security.Principal;

namespace CantineFront.Filters
{
    public class ClaimsTransformer : IClaimsTransformation
    {
        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var wi = (WindowsIdentity)principal.Identity;
            if (wi.Groups != null)
            {
                foreach (var group in wi.Groups) //-- Getting all the AD groups that user belongs to---  
                {
                    try
                    {
                        var claim = new Claim(wi.RoleClaimType, group.Value);
                        wi.AddClaim(claim);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
            return Task.FromResult(principal);
        }

        //public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        //{
        //    // Clone current identity
        //    var clone = principal.Clone();
        //    var newIdentity = (ClaimsIdentity)clone.Identity;

        //    // Support AD and local accounts
        //    var nameId = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier ||
        //                                                      c.Type == ClaimTypes.Name);
        //    if (nameId == null)
        //    {
        //        return principal;
        //    }

        //    // Get user from database
        //    var user = await _userService.GetByUserName(nameId.Value);
        //    if (user == null)
        //    {
        //        return principal;
        //    }

        //    // Add role claims to cloned identity
        //    foreach (var role in user.Roles)
        //    {
        //        var claim = new Claim(newIdentity.RoleClaimType, role.Name);
        //        newIdentity.AddClaim(claim);
        //    }

        //    return clone;
        //}

    }
}
