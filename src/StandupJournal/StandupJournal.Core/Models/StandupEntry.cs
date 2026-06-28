namespace StandupJournal.Core.Models;

public sealed class StandupEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);

    public string Yesterday { get; set; } = string.Empty;

    public string Today { get; set; } = string.Empty;

    public string Blockers { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
