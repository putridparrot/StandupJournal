using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;

namespace StandupJournal.Shared.Services;

public sealed class ThemeService
{
    private const string ThemeModeStorageKey = "standup.theme.mode";
    private bool _loaded;

    public event Action? ThemeChanged;

    public DesignThemeModes Mode { get; private set; } = DesignThemeModes.System;

    public string ModeLabel => Mode switch
    {
        DesignThemeModes.Light => "Light",
        DesignThemeModes.Dark => "Dark",
        _ => "System"
    };

    public async Task EnsureLoadedAsync(IJSRuntime jsRuntime)
    {
        if (_loaded)
        {
            return;
        }

        try
        {
            var storedMode = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", ThemeModeStorageKey);
            if (Enum.TryParse<DesignThemeModes>(storedMode, true, out var mode))
            {
                Mode = mode;
            }

            _loaded = true;
            ThemeChanged?.Invoke();
        }
        catch
        {
            // JS runtime might not be available yet (for example during prerender).
            // Keep _loaded as false so we can retry once interactive.
        }
    }

    public async Task SetModeAsync(DesignThemeModes mode, IJSRuntime jsRuntime)
    {
        await EnsureLoadedAsync(jsRuntime);

        if (Mode == mode)
        {
            return;
        }

        Mode = mode;

        try
        {
            await jsRuntime.InvokeVoidAsync("localStorage.setItem", ThemeModeStorageKey, Mode.ToString());
        }
        catch
        {
        }

        ThemeChanged?.Invoke();
    }

    public async Task CycleModeAsync(IJSRuntime jsRuntime)
    {
        await EnsureLoadedAsync(jsRuntime);

        var next = Mode switch
        {
            DesignThemeModes.System => DesignThemeModes.Light,
            DesignThemeModes.Light => DesignThemeModes.Dark,
            _ => DesignThemeModes.System
        };

        await SetModeAsync(next, jsRuntime);
    }
}
