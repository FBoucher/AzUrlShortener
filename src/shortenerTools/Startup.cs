using Cloud5mins.domain;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using shortenerTools;
using shortenerTools.Abstractions;
using shortenerTools.Implementations;
using System;
using System.Diagnostics.CodeAnalysis;

[assembly: FunctionsStartup(typeof(Startup))]
namespace shortenerTools
{
    [ExcludeFromCodeCoverage]
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            var configuration = configurationBuilder.Build();

            builder.Services.AddSingleton<IConfiguration>(configuration);

            builder.Services.AddHttpClient<IUserIpLocationService, UserIpLocationService>(client =>
            {
                client.BaseAddress = new Uri(configuration.GetSection("IpLocationService:Url").Value);
            });

            builder.Services.AddSingleton<IStorageTableHelper, StorageTableHelper>(provider =>
            {
                var config = provider.GetService<IConfiguration>();
                return new StorageTableHelper(config.GetSection("UlsDataStorage").Value);
            });
        }
    }
}