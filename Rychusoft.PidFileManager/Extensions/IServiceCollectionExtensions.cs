using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rychusoft.PidFileManager.HostedServices;
using System;

namespace Rychusoft.PidFileManager.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddPidFileHostedService(this IServiceCollection services, IConfiguration namedConfigurationSection) => services
            .Configure<PidFileOptions>(namedConfigurationSection)
            .AddHostedService<PidFileHostedService>();

        public static IServiceCollection AddPidFileHostedService(this IServiceCollection services, Action<PidFileOptions> configureOptions) => services
            .Configure(configureOptions)
            .AddHostedService<PidFileHostedService>();
    }
}