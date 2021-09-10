using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using Shared.Configs;
using Shared.Generics;
using Shared.Models;
using Shared.RabbitMQ;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Notification;
using Notification.Providers;
using Server.RabbitMQ;
using Server.Services;
using Server.Services.Admin;
using Server.Services.Background.Cron;
using Server.Services.Background.Queue;
using Server.Services.Singleton;

namespace Server
{
    public class Startup
    {
        private IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDatabaseDeveloperPageExceptionFilter();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(Configuration.GetConnectionString("MySqlConnection"),
                    new MariaDbServerVersion(new Version(10, 5, 8)),
                    builder => { builder.MigrationsAssembly("Server"); }));

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

            var jwtTokenConfig = Configuration.GetSection("jwtToken").Get<JwtTokenConfig>();
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(builder =>
                {
                    builder.RequireHttpsMetadata = false;
                    builder.SaveToken = true;
                    builder.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtTokenConfig.Issuer,
                        ValidAudience = jwtTokenConfig.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtTokenConfig.Secret))
                    };
                });
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

            services.AddOptions();
            services.Configure<ApplicationConfig>(Configuration.GetSection("Application"));
            services.Configure<JwtTokenConfig>(Configuration.GetSection("jwtToken"));
            services.Configure<RabbitMqConfig>(Configuration.GetSection("RabbitMQ"));
            services.Configure<NotificationConfig>(Configuration.GetSection("Notification"));

            services.AddHttpClient(); // IHttpClientFactory

            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IBulletinService, BulletinService>();
            services.AddScoped<IContestService, ContestService>();
            services.AddScoped<IProblemService, ProblemService>();
            services.AddScoped<ISubmissionService, SubmissionService>();
            services.AddScoped<ISubmissionReviewService, SubmissionReviewService>();

            services.AddScoped<IAdminUserService, AdminUserService>();
            services.AddScoped<IAdminBulletinService, AdminBulletinService>();
            services.AddScoped<IAdminContestService, AdminContestService>();
            services.AddScoped<IAdminProblemService, AdminProblemService>();
            services.AddScoped<IAdminSubmissionService, AdminSubmissionService>();

            services.AddSingleton<JobRequestProducer>();
            services.AddSingleton<JobCompleteConsumer>();
            services.AddSingleton<WorkerHeartbeatConsumer>();

            services.AddSingleton<ProblemStatisticsService>();
            services.AddSingleton<WorkerStatisticsService>();
            services.AddSingleton<QueueStatisticsService>();
            services.AddSingleton<TestKitLabSubmitTokenService>();

            // TODO: Broadcasters can be made singleton.
            services.AddScoped<IDingTalkNotification, DingTalkNotification>();
            services.AddScoped<INotificationBroadcaster, NotificationBroadcaster>();

            // Background cron job services. Note that these services cannot start until DB is migrated.
            services.AddHostedService<WorkerStatisticsBackgroundService>();
            services.AddHostedService<PlagiarismCleanerBackgroundService>();

            // Background task queue services.
            services.AddSingleton<BackgroundTaskQueue<JobRequestMessage>>();
            services.AddHostedService<JobRequestBackgroundService>();

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration => { configuration.RootPath = "wwwroot/dist"; });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider provider,
            IHostApplicationLifetime lifetime)
        {
            ConfigureDatabase(provider).Wait();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint(); // https://github.com/aspnet/Announcements/issues/432
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles(); // wwwroot

            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseRouting();
            app.UseCors(builder => { builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); });

            app.UseAuthentication();
            app.UseAuthorization();

            // Handle plagiarism report static files.
            // This should come after UseAuthorization() for auth.
            var options = provider.GetRequiredService<IOptions<ApplicationConfig>>();
            var plagiarismsFolder = Path.Combine(options.Value.DataPath, "plagiarisms");
            if (!Directory.Exists(plagiarismsFolder)) Directory.CreateDirectory(plagiarismsFolder);
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(plagiarismsFolder),
                RequestPath = "/plagiarisms",
                OnPrepareResponse = async (ctx) =>
                {
                    var authorized = ctx.Context.User.IsInRole(ApplicationRoles.Administrator)
                                     || ctx.Context.User.IsInRole(ApplicationRoles.ContestManager)
                                     || ctx.Context.User.IsInRole(ApplicationRoles.SubmissionManager);
                    if (!authorized)
                    {
                        const string unauthorizedBody = "Unauthorized";
                        ctx.Context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        ctx.Context.Response.Headers["Cache-Control"] = "no-store";
                        ctx.Context.Response.Headers["Content-Length"] = unauthorizedBody.Length.ToString();
                        ctx.Context.Response.Headers["Content-Type"] = "text/html";
                        await ctx.Context.Response.WriteAsync(unauthorizedBody);
                    }
                }
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });

            app.UseSpa(spa =>
            {
                if (env.IsDevelopment())
                {
                    // Check https://github.com/dotnet/aspnetcore/issues/17277
                    // spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
                    spa.Options.SourcePath = "../Client";
                    spa.UseAngularCliServer(npmScript: "start:dotnet");
                }
            });

            lifetime.ApplicationStarted.Register(() =>
            {
                var factory = new RabbitMqConnectionFactory(app.ApplicationServices);
                var connection = factory.GetConnection();
                app.ApplicationServices.GetRequiredService<JobRequestProducer>().Start(connection);
                app.ApplicationServices.GetRequiredService<JobCompleteConsumer>().Start(connection);
                app.ApplicationServices.GetRequiredService<WorkerHeartbeatConsumer>().Start(connection);
            });

            lifetime.ApplicationStopping.Register(() =>
            {
                var factory = new RabbitMqConnectionFactory(app.ApplicationServices);
                app.ApplicationServices.GetRequiredService<WorkerHeartbeatConsumer>().Stop();
                app.ApplicationServices.GetRequiredService<JobCompleteConsumer>().Stop();
                app.ApplicationServices.GetRequiredService<JobRequestProducer>().Stop();
                factory.CloseConnection();
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
            else
            {
                var adminUser = await userManager.FindByNameAsync(Configuration["Application:AdminUser:ContestantId"]);
                var password = Configuration["Application:AdminUser:Password"];
                if (!await userManager.CheckPasswordAsync(adminUser, password))
                {
                    logger.LogInformation("Resetting password for admin user.");
                    var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
                    var result = await userManager.ResetPasswordAsync(adminUser, token, password);
                    if (!result.Succeeded)
                    {
                        throw new Exception(string.Join(',', result.Errors.Select(e => e.Description)));
                    }
                }
            }

            logger.LogInformation("Database configured successfully");
        }
    }
}