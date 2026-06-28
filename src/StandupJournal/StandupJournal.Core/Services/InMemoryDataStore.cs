using System.Collections.Concurrent;
using StandupJournal.Core.Contracts;
using StandupJournal.Core.Models;

namespace StandupJournal.Core.Services;

public sealed class InMemoryDataStore : ILocalDataStore
{
    private readonly ConcurrentDictionary<Guid, StandupEntry> _entries = new();

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<StandupEntry>> GetEntriesAsync(CancellationToken cancellationToken = default)
    {
        var entries = _entries.Values
            .Select(Clone)
            .OrderByDescending(entry => entry.Date)
            .ToList()
            .AsReadOnly();

        return Task.FromResult<IReadOnlyList<StandupEntry>>(entries);
    }

    public Task<StandupEntry?> GetEntryByDateAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var entry = _entries.Values.FirstOrDefault(item => item.Date == date);
        return Task.FromResult(entry is null ? null : Clone(entry));
    }

    public Task SaveEntryAsync(StandupEntry entry, CancellationToken cancellationToken = default)
    {
        var normalizedEntry = Clone(entry);
        normalizedEntry.Id = normalizedEntry.Id == Guid.Empty ? Guid.NewGuid() : normalizedEntry.Id;
        _entries[normalizedEntry.Id] = normalizedEntry;

        return Task.CompletedTask;
    }

    public Task DeleteEntryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _entries.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    private static StandupEntry Clone(StandupEntry source)
    {
        return new StandupEntry
        {
            Id = source.Id,
            Date = source.Date,
            Yesterday = source.Yesterday,
            Today = source.Today,
            Blockers = source.Blockers,
            Notes = source.Notes,
            UpdatedAtUtc = source.UpdatedAtUtc
        };
    }
}
