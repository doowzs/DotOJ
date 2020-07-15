using System.Collections.Generic;
using System.Threading.Tasks;
using Judge1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Judge1.Controllers
{
    public class ApiController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _manager;

        private string user;
        
        public ApiController(UserManager<ApplicationUser> manager)
        {
            _manager = manager;
        }
        
        
        
        protected async Task<ApplicationUser> GetCurrentUserAsync()
        {
            return await _manager.GetUserAsync(HttpContext.User);
        }

        protected async Task<IList<string>> GetCurrentUserRolesAsync()
        {
            var user = await GetCurrentUserAsync();
            return await _manager.GetRolesAsync(user);
        }

        protected async Task<bool> IsCurrentUserInRoleAsync(string role)
        {
            var user = await GetCurrentUserAsync();
            return await _manager.IsInRoleAsync(user, role);
        }
    }
}