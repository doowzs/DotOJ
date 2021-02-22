using System;
using Data;
using Data.Configs;
using Data.RabbitMQ;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Notification;
using Notification.Providers;
using Worker.RabbitMQ;

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
                        options.UseMySql(hostContext.Configuration.GetConnectionString("MySqlConnection"),
                            new MariaDbServerVersion(new Version(10, 5, 8)))
                    );

                    services.AddOptions();
                    services.Configure<JudgingConfig>(hostContext.Configuration.GetSection("Judging"));
                    services.Configure<RabbitMqConfig>(hostContext.Configuration.GetSection("RabbitMQ"));
                    services.Configure<NotificationConfig>(hostContext.Configuration.GetSection("Notification"));

                    services.AddSingleton<JobRequestConsumer>();
                    services.AddSingleton<JobCompleteProducer>();
                    services.AddSingleton<WorkerHeartbeatProducer>();

                    services.AddScoped<INotificationBroadcaster, NotificationBroadcaster>();
                    services.AddScoped<IDingTalkNotification, DingTalkNotification>();
                });
    }
}