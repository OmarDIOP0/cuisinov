namespace CantineBack.Identity
{
    public class IdentityData
    {
        public const string AdminUserClaimName = "admin";
        public const string AdminUserPolicyName = "AdminPolicy";
        public const string GerantUserClaimName = "gerant";
        public const string GerantUserPolicyName = "GerantPolicy";

        public const string AdminOrGerantUserRoles = "admin,gerant";
        public const string AdminOrUserRoles = "admin,user";
    }
}
