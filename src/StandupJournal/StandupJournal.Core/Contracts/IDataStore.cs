using StandupJournal.Core.Models;

namespace StandupJournal.Core.Contracts;

public interface IDataStore
{
    Task InitializeAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StandupEntry>> GetEntriesAsync(CancellationToken cancellationToken = default);

    Task<StandupEntry?> GetEntryByDateAsync(DateOnly date, CancellationToken cancellationToken = default);

    Task SaveEntryAsync(StandupEntry entry, CancellationToken cancellationToken = default);

    Task DeleteEntryAsync(Guid id, CancellationToken cancellationToken = default);
}
