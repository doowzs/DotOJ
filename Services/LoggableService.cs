using System;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using Judge1.Data;
using Judge1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Judge1.Services
{
    public class LoggableService<T> where T : class
    {
        protected readonly ApplicationDbContext Context;
        protected readonly IHttpContextAccessor Accessor;
        protected readonly UserManager<ApplicationUser> Manager;
        protected readonly ILogger<T> Logger;

        private ApplicationUser _user;

        public LoggableService(IServiceProvider provider)
        {
            Context = provider.GetRequiredService<ApplicationDbContext>();
            Accessor = provider.GetRequiredService<IHttpContextAccessor>();
            Manager = provider.GetRequiredService<UserManager<ApplicationUser>>();
            Logger = provider.GetRequiredService<ILogger<T>>();
        }

        public async Task GetCurrentLoggedInUser()
        {
            if (_user != null) return;

            var context = Accessor.HttpContext;
            if (context == null || !context.User.IsAuthenticated())
            {
                _user = null;
            }
            else
            {
                _user = await Manager.FindByIdAsync(context.User.GetSubjectId());
            }
        }

        public async Task LogDebug(string message, params object[] args)
        {
            await GetCurrentLoggedInUser();
            Logger.LogDebug($"{typeof(T)} {message} User={_user?.Email}", args);
        }

        public async Task LogInformation(string message, params object[] args)
        {
            await GetCurrentLoggedInUser();
            Logger.LogInformation($"{typeof(T)} {message} User={_user?.Email}", args);
        }

        public async Task LogError(string message, params object[] args)
        {
            await GetCurrentLoggedInUser();
            Logger.LogError($"{typeof(T)} {message} User={_user?.Email}", args);
        }

        public async Task LogCritical(string message, params object[] args)
        {
            await GetCurrentLoggedInUser();
            Logger.LogCritical($"{typeof(T)} {message} User={_user?.Email}", args);
        }
    }
}