using System;
using MemberBalanceStatistics.Domain.Options;
using MemberBalanceStatistics.Domain.Provider;
using MemberBalanceStatistics.Provider.Redis;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MemberBalanceConfigurationExtensions
    {
        public static IServiceCollection AddMemberBalanceProvider(this IServiceCollection services, Action<RedisOptions, IServiceProvider> optionsConfigure)
        {
            return services
                .AddOptions<RedisOptions>()
                .Configure(optionsConfigure)
                .Services
                .AddSingleton<IMemberBalanceProvider, MemberBalanceProvider>();
        }
    }
}
