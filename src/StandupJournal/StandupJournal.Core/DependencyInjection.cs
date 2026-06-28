using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StandupJournal.Core.Contracts;
using StandupJournal.Core.Options;
using StandupJournal.Core.Services;

namespace StandupJournal.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddStandupCore(this IServiceCollection services)
    {
        services.AddScoped<IStandupJournalService, StandupJournalService>();
        services.AddScoped<IStandupSyncService, StandupSyncService>();
        services.AddScoped<IStandupSeedService, StandupSeedService>();
        return services;
    }

    public static IServiceCollection AddInMemoryDataStore(this IServiceCollection services)
    {
        services.TryAddSingleton<InMemoryDataStore>();
        services.TryAddSingleton<ILocalDataStore>(serviceProvider => serviceProvider.GetRequiredService<InMemoryDataStore>());
        services.TryAddSingleton<IDataStore>(serviceProvider => serviceProvider.GetRequiredService<InMemoryDataStore>());
        return services;
    }

    public static IServiceCollection AddApiDataStore(this IServiceCollection services, Action<ApiDataStoreOptions>? configure = null, bool setAsPrimaryDataStore = false)
    {
        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.AddHttpClient<ApiDataStore>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<ApiDataStoreOptions>>().Value;
            if (!string.IsNullOrWhiteSpace(options.BaseAddress))
            {
                client.BaseAddress = new Uri(options.BaseAddress, UriKind.Absolute);
            }
        });

        services.TryAddScoped<IRemoteDataStore>(serviceProvider => serviceProvider.GetRequiredService<ApiDataStore>());

        if (setAsPrimaryDataStore)
        {
            services.AddScoped<IDataStore>(serviceProvider => serviceProvider.GetRequiredService<ApiDataStore>());
        }

        return services;
    }
}
