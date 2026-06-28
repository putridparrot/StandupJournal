using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using StandupJournal.Core.Contracts;
using StandupJournal.Core.Models;
using StandupJournal.Core.Options;

namespace StandupJournal.Core.Services;

public sealed class ApiDataStore(HttpClient httpClient, IOptions<ApiDataStoreOptions> options) : IRemoteDataStore
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ApiDataStoreOptions _options = options.Value;

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<StandupEntry>> GetEntriesAsync(CancellationToken cancellationToken = default)
    {
        var entries = await _httpClient.GetFromJsonAsync<List<StandupEntry>>(_options.EntriesPath, cancellationToken);
        return entries ?? [];
    }

    public async Task<StandupEntry?> GetEntryByDateAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var path = BuildByDatePath(date);
        using var response = await _httpClient.GetAsync(path, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<StandupEntry>(cancellationToken);
    }

    public async Task SaveEntryAsync(StandupEntry entry, CancellationToken cancellationToken = default)
    {
        var id = entry.Id == Guid.Empty ? Guid.NewGuid() : entry.Id;
        entry.Id = id;

        using var response = await _httpClient.PutAsJsonAsync(BuildByIdPath(id), entry, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteEntryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync(BuildByIdPath(id), cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private string BuildByDatePath(DateOnly date)
    {
        return $"{_options.EntriesPath.TrimEnd('/')}/by-date/{Uri.EscapeDataString(date.ToString("yyyy-MM-dd"))}";
    }

    private string BuildByIdPath(Guid id)
    {
        return $"{_options.EntriesPath.TrimEnd('/')}/{id:D}";
    }
}
