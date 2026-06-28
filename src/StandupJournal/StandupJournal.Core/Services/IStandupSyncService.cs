using StandupJournal.Core.Models;

namespace StandupJournal.Core.Services;

public interface IStandupSyncService
{
    bool CanSync { get; }

    Task<StandupSyncResult> SyncAsync(CancellationToken cancellationToken = default);
}
