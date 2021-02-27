using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(Server.Areas.Identity.IdentityHostingStartup))]
namespace Server.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
            });
        }
    }
}