using Microsoft.Extensions.Logging;
using Microsoft.FluentUI.AspNetCore.Components;
using StandupJournal.Core;
using StandupJournal.Infrastructure.Sqlite;
using StandupJournal.Shared.Services;
using StandupJournal.Services;

namespace StandupJournal;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Add device-specific services used by the StandupJournal.Shared project
        builder.Services.AddSingleton<IFormFactor, FormFactor>();
        builder.Services.AddScoped<ThemeService>();
        builder.Services.AddScoped<AppPreferencesService>(sp =>
        {
            var service = new AppPreferencesService();
            service.SetDatabaseLocation(Path.Combine(FileSystem.AppDataDirectory, "standup-journal.db3"));
            return service;
        });
        builder.Services.AddFluentUIComponents();
        builder.Services.AddStandupCore();
        builder.Services.AddSqliteDataStore(options =>
        {
            options.DatabasePath = Path.Combine(FileSystem.AppDataDirectory, "standup-journal.db3");
        });

        var remoteBaseAddress = Environment.GetEnvironmentVariable("STANDUP_REMOTE_BASE_URL");
        if (!string.IsNullOrWhiteSpace(remoteBaseAddress))
        {
            builder.Services.AddApiDataStore(options =>
            {
                options.BaseAddress = remoteBaseAddress;
            });
        }

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
