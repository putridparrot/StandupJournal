namespace StandupJournal.Core.Options;

public sealed class ApiDataStoreOptions
{
    public string BaseAddress { get; set; } = string.Empty;

    public string EntriesPath { get; set; } = "/api/standups";
}
