using System.Security.Claims;

namespace ExpenseAnalyzer.API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int? GetUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(ClaimTypes.NameIdentifier);

            return int.TryParse(value, out var userId) ? userId : null;
        }
    }
}
