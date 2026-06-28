namespace StandupJournal.Core.Services;

public interface IStandupSeedService
{
    Task<int> SeedSampleEntriesAsync(CancellationToken cancellationToken = default);
}
