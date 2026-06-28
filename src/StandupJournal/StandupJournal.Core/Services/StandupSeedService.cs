using StandupJournal.Core.Contracts;
using StandupJournal.Core.Models;

namespace StandupJournal.Core.Services;

public sealed class StandupSeedService(IDataStore dataStore) : IStandupSeedService
{
    public async Task<int> SeedSampleEntriesAsync(CancellationToken cancellationToken = default)
    {
        await dataStore.InitializeAsync(cancellationToken);

        var existing = await dataStore.GetEntriesAsync(cancellationToken);
        if (existing.Count > 0)
        {
            return 0;
        }

        var today = DateOnly.FromDateTime(DateTime.Now);
        var entries = new[]
        {
            new StandupEntry
            {
                Date = today.AddDays(-2),
                Yesterday = "Set up initial MAUI + WASM scaffold.",
                Today = "Wire Fluent UI components and create Home page.",
                Blockers = "None",
                Notes = "Kick-off sample entry."
            },
            new StandupEntry
            {
                Date = today.AddDays(-1),
                Yesterday = "Added SQLite local storage and abstraction.",
                Today = "Prototype online datastore API integration.",
                Blockers = "Need backend endpoint URL for mobile sync.",
                Notes = "Testing data for history timeline."
            },
            new StandupEntry
            {
                Date = today,
                Yesterday = "Implemented sync and seed scaffolding.",
                Today = "Review UX and finalize online deployment settings.",
                Blockers = "Waiting for hosting/auth decisions.",
                Notes = "Auto-generated starter seed."
            }
        };

        foreach (var entry in entries)
        {
            entry.UpdatedAtUtc = DateTimeOffset.UtcNow;
            await dataStore.SaveEntryAsync(entry, cancellationToken);
        }

        return entries.Length;
    }
}
