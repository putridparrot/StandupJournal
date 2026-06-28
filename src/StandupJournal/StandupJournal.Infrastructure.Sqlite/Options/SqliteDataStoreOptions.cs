namespace StandupJournal.Infrastructure.Sqlite.Options;

public sealed class SqliteDataStoreOptions
{
    public string DatabasePath { get; set; } = Path.Combine(AppContext.BaseDirectory, "standup-journal.db3");
}
