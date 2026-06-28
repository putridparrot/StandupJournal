using StandupJournal.Core.Models;

namespace StandupJournal.Core.Services;

public interface IStandupJournalService
{
    Task<IReadOnlyList<StandupEntry>> GetRecentEntriesAsync(int take = 14, CancellationToken cancellationToken = default);

    Task<StandupEntry> GetOrCreateEntryForDateAsync(DateOnly date, CancellationToken cancellationToken = default);

    Task SaveAsync(StandupEntry entry, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
