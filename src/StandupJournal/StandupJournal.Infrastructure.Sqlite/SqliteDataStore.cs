using Microsoft.Extensions.Options;
using SQLite;
using StandupJournal.Core.Contracts;
using StandupJournal.Core.Models;
using StandupJournal.Infrastructure.Sqlite.Options;

namespace StandupJournal.Infrastructure.Sqlite;

public sealed class SqliteDataStore(IOptions<SqliteDataStoreOptions> options) : ILocalDataStore
{
    private readonly SqliteDataStoreOptions _options = options.Value;
    private readonly SemaphoreSlim _initializeLock = new(1, 1);
    private SQLiteAsyncConnection? _connection;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_connection is not null)
        {
            return;
        }

        await _initializeLock.WaitAsync(cancellationToken);
        try
        {
            if (_connection is not null)
            {
                return;
            }

            var directory = Path.GetDirectoryName(_options.DatabasePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            _connection = new SQLiteAsyncConnection(_options.DatabasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
            await _connection.CreateTableAsync<SqliteStandupRecord>();
        }
        finally
        {
            _initializeLock.Release();
        }
    }

    public async Task<IReadOnlyList<StandupEntry>> GetEntriesAsync(CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);

        var items = await _connection!
            .Table<SqliteStandupRecord>()
            .OrderByDescending(item => item.DateIso)
            .ToListAsync();

        return items.Select(ToDomain).ToList();
    }

    public async Task<StandupEntry?> GetEntryByDateAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);

        var dateIso = date.ToString("yyyy-MM-dd");
        var item = await _connection!
            .Table<SqliteStandupRecord>()
            .Where(entry => entry.DateIso == dateIso)
            .FirstOrDefaultAsync();

        return item is null ? null : ToDomain(item);
    }

    public async Task SaveEntryAsync(StandupEntry entry, CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);

        var normalizedId = entry.Id == Guid.Empty ? Guid.NewGuid() : entry.Id;
        entry.Id = normalizedId;

        var normalizedIdText = normalizedId.ToString("D");
        var existingRecord = await _connection!
            .Table<SqliteStandupRecord>()
            .Where(item => item.Id == normalizedIdText)
            .FirstOrDefaultAsync();

        var record = ToRecord(entry);
        if (existingRecord is null)
        {
            await _connection.InsertAsync(record);
            return;
        }

        await _connection.UpdateAsync(record);
    }

    public async Task DeleteEntryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);
        await _connection!.DeleteAsync<SqliteStandupRecord>(id.ToString("D"));
    }

    private static StandupEntry ToDomain(SqliteStandupRecord record)
    {
        return new StandupEntry
        {
            Id = Guid.Parse(record.Id),
            Date = DateOnly.ParseExact(record.DateIso, "yyyy-MM-dd"),
            Yesterday = record.Yesterday,
            Today = record.Today,
            Blockers = record.Blockers,
            Notes = record.Notes,
            UpdatedAtUtc = DateTimeOffset.Parse(record.UpdatedAtUtcIso)
        };
    }

    private static SqliteStandupRecord ToRecord(StandupEntry entry)
    {
        return new SqliteStandupRecord
        {
            Id = entry.Id.ToString("D"),
            DateIso = entry.Date.ToString("yyyy-MM-dd"),
            Yesterday = entry.Yesterday,
            Today = entry.Today,
            Blockers = entry.Blockers,
            Notes = entry.Notes,
            UpdatedAtUtcIso = entry.UpdatedAtUtc.ToString("O")
        };
    }
}
