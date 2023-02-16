using System.Security.Claims;

namespace SP23.P02.Web.Features
{
    public static class UserPrincipalExtensions
    {
        public static int? GetCurrentUderId(this ClaimsPrincipal claimsPrincipal)
        {
            var userIdClaimValue = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaimValue == null)
            {
                return null;
            }
            return int.Parse(userIdClaimValue);
        }

        public static string? GetCurrentUserName(this ClaimsPrincipal claimsPrincipal)
        {
            var userNameClaimValue = claimsPrincipal.FindFirstValue(ClaimTypes.Name);

            return claimsPrincipal.Identity?.Name;
        }
    }

}
