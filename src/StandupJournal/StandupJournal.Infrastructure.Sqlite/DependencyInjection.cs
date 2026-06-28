using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StandupJournal.Core.Contracts;
using StandupJournal.Infrastructure.Sqlite.Options;

namespace StandupJournal.Infrastructure.Sqlite;

public static class DependencyInjection
{
    public static IServiceCollection AddSqliteDataStore(this IServiceCollection services, Action<SqliteDataStoreOptions>? configure = null)
    {
        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.TryAddSingleton<SqliteDataStore>();
        services.TryAddSingleton<ILocalDataStore>(serviceProvider => serviceProvider.GetRequiredService<SqliteDataStore>());
        services.TryAddSingleton<IDataStore>(serviceProvider => serviceProvider.GetRequiredService<SqliteDataStore>());
        return services;
    }
}
