using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Data;
using Data.Configs;
using Data.Models;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Notification;
using Notification.Providers;
using WebApp.Services;
using WebApp.Services.Admin;

namespace WebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            // See https://github.com/dotnet/aspnetcore/issues/14160
            // Also https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/issues/415
            // JwtSecurityTokenHandler.DefaultInboundClaimTypeMap = new Dictionary<string, string>();
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove(JwtRegisteredClaimNames.Sub);
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDatabaseDeveloperPageExceptionFilter();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(Configuration.GetConnectionString("MySqlConnection"),
                    new MariaDbServerVersion(new Version(10, 5, 8)),
                    builder => { builder.MigrationsAssembly("WebApp"); }));

            services.AddDefaultIdentity<ApplicationUser>(options =>
                {
                    options.SignIn.RequireConfirmedEmail = false;
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddTransient<IUserValidator<ApplicationUser>, CustomUserValidator>();
            services.ConfigureApplicationCookie(options =>
            {
                options.Events.OnRedirectToLogin = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api")
                        && context.Response.StatusCode == StatusCodes.Status200OK)
                    {
                        context.Response.Clear();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    }

                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
            });

            services.AddIdentityServer()
                .AddApiAuthorization<ApplicationUser, ApplicationDbContext>()
                .AddProfileService<ProfileService>();

            // See https://stackoverflow.com/questions/52526186/net-core-identity
            // and https://stackoverflow.com/questions/60184703/net-core-3-1-403.
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
                    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
                })
                .AddIdentityServerJwt();
            services.AddAuthorization(options =>
            {
                options.AddPolicy("ManageUsers",
                    policy => { policy.RequireRole(ApplicationRoles.Administrator, ApplicationRoles.UserManager); });
                options.AddPolicy("ManageContests",
                    policy => { policy.RequireRole(ApplicationRoles.Administrator, ApplicationRoles.ContestManager); });
                options.AddPolicy("ManageProblems",
                    policy => { policy.RequireRole(ApplicationRoles.Administrator, ApplicationRoles.ContestManager); });
                options.AddPolicy("ManageSubmissions",
                    policy =>
                    {
                        policy.RequireRole(ApplicationRoles.Administrator, ApplicationRoles.SubmissionManager);
                    });
            });
            services.AddControllersWithViews().AddNewtonsoftJson();
            services.AddRazorPages();

            services.AddOptions();
            services.Configure<ApplicationConfig>(Configuration.GetSection("Application"));
            services.Configure<NotificationConfig>(Configuration.GetSection("Notification"));

            services.AddHttpClient(); // IHttpClientFactory

            services.AddScoped<IBulletinService, BulletinService>();
            services.AddScoped<IContestService, ContestService>();
            services.AddScoped<IProblemService, ProblemService>();
            services.AddScoped<ISubmissionService, SubmissionService>();

            services.AddScoped<IAdminUserService, AdminUserService>();
            services.AddScoped<IAdminBulletinService, AdminBulletinService>();
            services.AddScoped<IAdminContestService, AdminContestService>();
            services.AddScoped<IAdminProblemService, AdminProblemService>();
            services.AddScoped<IAdminSubmissionService, AdminSubmissionService>();

            services.AddScoped<IDingTalkNotification, DingTalkNotification>();
            services.AddScoped<INotificationBroadcaster, NotificationBroadcaster>();

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration => { configuration.RootPath = "ClientApp/dist"; });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider provider)
        {
            ConfigureDatabase(provider).Wait();

            app.Use(async (ctx, next) =>
            {
                ctx.SetIdentityServerOrigin(Configuration["Application:Host"]);
                await next();
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint(); // https://github.com/aspnet/Announcements/issues/432
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // app.UseHsts();
            }

            // app.UseHttpsRedirection();
            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseRouting();

            app.UseAuthentication()
                .UseCookiePolicy(new CookiePolicyOptions
                {
                    MinimumSameSitePolicy = SameSiteMode.Lax
                });
            app.UseIdentityServer();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    // Check https://github.com/dotnet/aspnetcore/issues/17277
                    // spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
                    spa.UseAngularCliServer(npmScript: "start:dotnet");
                }
            });
        }

        private async Task ConfigureDatabase(IServiceProvider provider)
        {
            var logger = provider.GetRequiredService<ILogger<Startup>>();
            var context = provider.GetRequiredService<ApplicationDbContext>();
            await context.Database.MigrateAsync();

            var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();

            foreach (var role in ApplicationRoles.RoleList)
            {
                var exists = await roleManager.RoleExistsAsync(role);
                if (!exists)
                {
                    logger.LogInformation($"Creating role {role}.");
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            if (await userManager.FindByEmailAsync(Configuration["Application:AdminUser:Email"].ToUpper()) == null &&
                await userManager.FindByNameAsync(Configuration["Application:AdminUser:ContestantId"]) == null)
            {
                logger.LogInformation($"Admin user not found. Creating admin user.");
                var adminUser = new ApplicationUser
                {
                    Email = Configuration["Application:AdminUser:Email"],
                    UserName = Configuration["Application:AdminUser:ContestantId"],
                    ContestantId = Configuration["Application:AdminUser:ContestantId"],
                    ContestantName = Configuration["Application:AdminUser:ContestantName"]
                };
                var password = Configuration["Application:AdminUser:Password"];
                var result = await userManager.CreateAsync(adminUser, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, ApplicationRoles.Administrator);
                }
            }

            logger.LogInformation("Database configured successfully");
        }
    }
}