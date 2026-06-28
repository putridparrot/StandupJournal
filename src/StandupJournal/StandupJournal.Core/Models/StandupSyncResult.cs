namespace StandupJournal.Core.Models;

public sealed class StandupSyncResult
{
    public bool IsConfigured { get; init; }

    public int LocalUpdates { get; init; }

    public int RemoteUpdates { get; init; }

    public int ConflictsResolved { get; init; }

    public DateTimeOffset SyncedAtUtc { get; init; }
}
