using StandupJournal.Core.Contracts;
using StandupJournal.Core.Models;

namespace StandupJournal.Core.Services;

public sealed class StandupJournalService(IDataStore dataStore) : IStandupJournalService
{
    public async Task<IReadOnlyList<StandupEntry>> GetRecentEntriesAsync(int take = 14, CancellationToken cancellationToken = default)
    {
        await dataStore.InitializeAsync(cancellationToken);

        var entries = await dataStore.GetEntriesAsync(cancellationToken);
        return entries
            .OrderByDescending(entry => entry.Date)
            .Take(Math.Max(1, take))
            .ToList();
    }

    public async Task<StandupEntry> GetOrCreateEntryForDateAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        await dataStore.InitializeAsync(cancellationToken);

        var existingEntry = await dataStore.GetEntryByDateAsync(date, cancellationToken);
        return existingEntry ?? new StandupEntry { Date = date };
    }

    public async Task SaveAsync(StandupEntry entry, CancellationToken cancellationToken = default)
    {
        await dataStore.InitializeAsync(cancellationToken);
        entry.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await dataStore.SaveEntryAsync(entry, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await dataStore.InitializeAsync(cancellationToken);
        await dataStore.DeleteEntryAsync(id, cancellationToken);
    }
}
