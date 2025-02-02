using Newtonsoft.Json;
using System.Security.Claims;

namespace Presentation.Utils
{
    public static class OidcUtils
    {
        public static string GetUserIdentifier(ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        }

        public static bool IsUserHasRole(ClaimsPrincipal user, string role)
        {
            // try to extract roles from the default prop
            var roleClaim = user.FindFirst(ClaimTypes.Role);
            if (roleClaim != null)
            {
                return roleClaim.Value == role;
            }

            // try to extract roles from the keycloak claims
            var realm_access = user.FindFirst("realm_access");
            if (realm_access != null)
            {
                dynamic rolesObj = JsonConvert.DeserializeObject<object>(realm_access.Value);
                string[] roles = rolesObj.roles.ToObject<string[]>();
                return roles.Contains(role);
            }

            return false;
        }
    }
}
