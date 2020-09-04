using AutoMapper.Configuration;
using Data;
using Data.Configs;
using Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Notification;
using Notification.Providers;
using Worker.Runners;

namespace Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHttpClient();
                    services.AddHostedService<Worker>();

                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseMySql(hostContext.Configuration.GetConnectionString("MySqlConnection"))
                    );

                    services.AddOptions();
                    services.Configure<JudgingConfig>(hostContext.Configuration.GetSection("Judging"));
                    services.Configure<NotificationConfig>(hostContext.Configuration.GetSection("Notification"));

                    services.AddScoped<ISubmissionRunner, SubmissionRunner>();

                    services.AddScoped<INotificationBroadcaster, NotificationBroadcaster>();
                    services.AddScoped<IDingTalkNotification, DingTalkNotification>();
                });
    }
}