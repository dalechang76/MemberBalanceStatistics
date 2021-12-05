using System;
using MemberBalanceStatistics.Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MemberBalanceStatistics
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var service = serviceProvider.GetService<IMemberBalanceStaiisticsService>();
            service.Excute();
        }

        static void ConfigureServices(IServiceCollection services)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
                .Build();

            services.AddMemberBalanceProvider((options, service) =>
            {
                configuration.GetSection("RedisConfig").Bind(options);
            });
        }
    }
}
