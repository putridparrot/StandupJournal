using SQLite;

namespace StandupJournal.Infrastructure.Sqlite;

[Table("standup_entries")]
internal sealed class SqliteStandupRecord
{
    [PrimaryKey]
    public string Id { get; set; } = Guid.NewGuid().ToString("D");

    [Indexed]
    public string DateIso { get; set; } = DateOnly.FromDateTime(DateTime.Now).ToString("yyyy-MM-dd");

    public string Yesterday { get; set; } = string.Empty;

    public string Today { get; set; } = string.Empty;

    public string Blockers { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public string UpdatedAtUtcIso { get; set; } = DateTimeOffset.UtcNow.ToString("O");
}
