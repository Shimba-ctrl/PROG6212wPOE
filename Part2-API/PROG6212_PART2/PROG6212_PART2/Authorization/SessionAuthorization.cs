
using Microsoft.AspNetCore.Http;

namespace PROG6212_PART2.Authorization
{
    public static class SessionAuthorization
    {
        public static bool IsAuthenticated(HttpContext context)
        {
            return context.Session.GetInt32("UserId") != null;
        }

        public static bool HasRole(
            HttpContext context,
            string requiredRole)
        {
            int? userId = context.Session.GetInt32("UserId");
            string? role = context.Session.GetString("UserRole");

            return userId != null &&
                   role == requiredRole;
        }
    }
}
