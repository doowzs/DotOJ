using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Shared.Configs;
using Shared.Models;

namespace Shared.Generics
{
    public class LoggableService<T> where T : class
    {
        protected readonly IOptions<ApplicationConfig> Config;
        protected readonly ApplicationDbContext Context;
        protected readonly IHttpContextAccessor Accessor;
        protected readonly UserManager<ApplicationUser> Manager;
        protected readonly ILogger<T> Logger;
        protected readonly IServiceProvider Provider;

        private bool _noUser;
        private ApplicationUser _user;

        public LoggableService(IServiceProvider provider, bool noUser = false)
        {
            Config = provider.GetRequiredService<IOptions<ApplicationConfig>>();
            Context = provider.GetRequiredService<ApplicationDbContext>();
            Accessor = provider.GetRequiredService<IHttpContextAccessor>();
            Manager = provider.GetRequiredService<UserManager<ApplicationUser>>();
            Logger = provider.GetRequiredService<ILogger<T>>();
            Provider = provider;
            _noUser = noUser;
        }

        public async Task GetCurrentLoggedInUser()
        {
            if (_user != null) return;

            var context = Accessor.HttpContext;
            if (context == null || context.User.Identity == null || !context.User.Identity.IsAuthenticated)
            {
                _user = null;
            }
            else
            {
                var user = await Manager.GetUserAsync(Accessor.HttpContext.User);
                _user = await Manager.FindByIdAsync(user.Id);
            }
        }

        public async Task LogDebug(string message, params object[] args)
        {
            await GetCurrentLoggedInUser();
            if (_noUser)
            {
                Logger.LogDebug($"{message}", args);
            }
            else
            {
                Logger.LogDebug($"{message} User={_user?.Email}", args);
            }
        }

        public async Task LogInformation(string message, params object[] args)
        {
            await GetCurrentLoggedInUser();
            if (_noUser)
            {
                Logger.LogInformation($"{message}", args);
            }
            else
            {
                Logger.LogInformation($"{message} User={_user?.Email}", args);
            }
        }

        public async Task LogError(string message, params object[] args)
        {
            await GetCurrentLoggedInUser();
            if (_noUser)
            {
                Logger.LogError($"{message}", args);
            }
            else
            {
                Logger.LogError($"{message} User={_user?.Email}", args);
            }
        }

        public async Task LogCritical(string message, params object[] args)
        {
            await GetCurrentLoggedInUser();
            if (_noUser)
            {
                Logger.LogCritical($"{message}", args);
            }
            else
            {
                Logger.LogCritical($"{message} User={_user?.Email}", args);
            }
        }
    }
}