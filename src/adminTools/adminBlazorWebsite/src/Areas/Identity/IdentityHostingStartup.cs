using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(adminBlazorWebsite.Areas.Identity.IdentityHostingStartup))]
namespace adminBlazorWebsite.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
            });
        }
    }
}