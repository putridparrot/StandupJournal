using Microsoft.JSInterop;

namespace StandupJournal.Shared.Services;

public sealed class AppPreferencesService
{
    private const string AutoSaveStorageKey = "standup.preferences.autosave";
    private bool _loaded;

    public event Action? PreferencesChanged;

    public bool AutoSaveEnabled { get; private set; } = true;

    public string DatabaseLocation { get; private set; } = "Unknown";

    public async Task EnsureLoadedAsync(IJSRuntime jsRuntime)
    {
        if (_loaded)
        {
            return;
        }

        try
        {
            var storedValue = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", AutoSaveStorageKey);
            if (bool.TryParse(storedValue, out var autoSaveEnabled))
            {
                AutoSaveEnabled = autoSaveEnabled;
            }

            _loaded = true;
            PreferencesChanged?.Invoke();
        }
        catch
        {
            // JS runtime might not be available yet (for example during prerender).
            // Keep _loaded as false so we can retry once interactive.
        }
    }

    public async Task SetAutoSaveEnabledAsync(bool enabled, IJSRuntime jsRuntime)
    {
        await EnsureLoadedAsync(jsRuntime);

        if (AutoSaveEnabled == enabled)
        {
            return;
        }

        AutoSaveEnabled = enabled;

        try
        {
            await jsRuntime.InvokeVoidAsync("localStorage.setItem", AutoSaveStorageKey, enabled.ToString());
        }
        catch
        {
        }

        PreferencesChanged?.Invoke();
    }

    public void SetDatabaseLocation(string location)
    {
        if (string.IsNullOrWhiteSpace(location) || DatabaseLocation == location)
        {
            return;
        }

        DatabaseLocation = location;
        PreferencesChanged?.Invoke();
    }
}
