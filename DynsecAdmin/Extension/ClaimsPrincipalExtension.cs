using System.Security.Claims;

namespace DynsecAdmin
{
    public static class ClaimsPrincipalExtension
    {
        public static bool IsSelf(this ClaimsPrincipal principal, string username)
        {
            var self = principal.FindFirstValue(ClaimTypes.Upn);
            return self == username;
        }
    }
}
