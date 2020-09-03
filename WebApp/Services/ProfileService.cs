using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using WebApp.Models;

namespace WebApp.Services
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _manager;

        public ProfileService(UserManager<ApplicationUser> manager)
        {
            _manager = manager;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var user = await _manager.GetUserAsync(context.Subject);
            var roles = await _manager.GetRolesAsync(user);
            context.IssuedClaims.Add(new Claim(JwtClaimTypes.Id, user.ContestantId));
            context.IssuedClaims.Add(new Claim(JwtClaimTypes.Name, user.ContestantName));
            context.IssuedClaims.AddRange(roles.Select(r => new Claim(JwtClaimTypes.Role, r)));
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            return Task.CompletedTask;
        }
    }
}
