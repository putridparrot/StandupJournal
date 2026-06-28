using Microsoft.Extensions.DependencyInjection;
using StandupJournal.Core.Contracts;
using StandupJournal.Core.Models;

namespace StandupJournal.Core.Services;

public sealed class StandupSyncService(IServiceProvider serviceProvider) : IStandupSyncService
{
    private readonly ILocalDataStore? _localDataStore = serviceProvider.GetService<ILocalDataStore>();
    private readonly IRemoteDataStore? _remoteDataStore = serviceProvider.GetService<IRemoteDataStore>();

    public bool CanSync => _localDataStore is not null && _remoteDataStore is not null;

    public async Task<StandupSyncResult> SyncAsync(CancellationToken cancellationToken = default)
    {
        if (!CanSync)
        {
            return new StandupSyncResult
            {
                IsConfigured = false,
                SyncedAtUtc = DateTimeOffset.UtcNow
            };
        }

        await _localDataStore!.InitializeAsync(cancellationToken);
        await _remoteDataStore!.InitializeAsync(cancellationToken);

        var localEntries = await _localDataStore.GetEntriesAsync(cancellationToken);
        var remoteEntries = await _remoteDataStore.GetEntriesAsync(cancellationToken);

        var localByDate = localEntries
            .GroupBy(entry => entry.Date)
            .ToDictionary(group => group.Key, group => group.OrderByDescending(item => item.UpdatedAtUtc).First());
        var remoteByDate = remoteEntries
            .GroupBy(entry => entry.Date)
            .ToDictionary(group => group.Key, group => group.OrderByDescending(item => item.UpdatedAtUtc).First());

        var allDates = localByDate.Keys
            .Union(remoteByDate.Keys)
            .ToList();

        var localUpdates = 0;
        var remoteUpdates = 0;
        var conflictsResolved = 0;

        foreach (var date in allDates)
        {
            localByDate.TryGetValue(date, out var localEntry);
            remoteByDate.TryGetValue(date, out var remoteEntry);

            if (localEntry is null && remoteEntry is not null)
            {
                await _localDataStore.SaveEntryAsync(Clone(remoteEntry), cancellationToken);
                localUpdates++;
                continue;
            }

            if (remoteEntry is null && localEntry is not null)
            {
                await _remoteDataStore.SaveEntryAsync(Clone(localEntry), cancellationToken);
                remoteUpdates++;
                continue;
            }

            if (localEntry is null || remoteEntry is null)
            {
                continue;
            }

            if (Equivalent(localEntry, remoteEntry))
            {
                continue;
            }

            conflictsResolved++;
            var winner = localEntry.UpdatedAtUtc >= remoteEntry.UpdatedAtUtc ? localEntry : remoteEntry;

            if (!Equivalent(localEntry, winner))
            {
                await _localDataStore.SaveEntryAsync(Clone(winner), cancellationToken);
                localUpdates++;
            }

            if (!Equivalent(remoteEntry, winner))
            {
                await _remoteDataStore.SaveEntryAsync(Clone(winner), cancellationToken);
                remoteUpdates++;
            }
        }

        return new StandupSyncResult
        {
            IsConfigured = true,
            LocalUpdates = localUpdates,
            RemoteUpdates = remoteUpdates,
            ConflictsResolved = conflictsResolved,
            SyncedAtUtc = DateTimeOffset.UtcNow
        };
    }

    private static bool Equivalent(StandupEntry left, StandupEntry right)
    {
        return left.Date == right.Date
            && left.Yesterday == right.Yesterday
            && left.Today == right.Today
            && left.Blockers == right.Blockers
            && left.Notes == right.Notes
            && left.UpdatedAtUtc == right.UpdatedAtUtc;
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
