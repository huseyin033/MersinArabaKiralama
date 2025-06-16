using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading.Tasks;
using MersinArabaKiralama.Models;

namespace MersinArabaKiralama.Helpers
{
    public static class RoleHelper
{
        public static async Task<bool> IsAdminAsync(UserManager<ApplicationUser> userManager, ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true)
                return false;
                
            var currentUser = await userManager.GetUserAsync(user);
            if (currentUser == null)
                return false;
                
            return await userManager.IsInRoleAsync(currentUser, "Admin");
        }
    }
}
