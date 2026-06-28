using StandupJournal.Web.Components;
using StandupJournal.Core.Contracts;
using StandupJournal.Core;
using StandupJournal.Core.Models;
using Microsoft.FluentUI.AspNetCore.Components;
using StandupJournal.Shared.Services;
using StandupJournal.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();
builder.Services.AddFluentUIComponents();
builder.Services.AddStandupCore();
builder.Services.AddInMemoryDataStore();
builder.Services.AddScoped<ThemeService>();
builder.Services.AddScoped<AppPreferencesService>(sp =>
{
    var service = new AppPreferencesService();
    service.SetDatabaseLocation("In-memory store (web host)");
    return service;
});

// Add device-specific services used by the StandupJournal.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(
        typeof(StandupJournal.Shared._Imports).Assembly,
        typeof(StandupJournal.Web.Client._Imports).Assembly);

var standupApi = app.MapGroup("/api/standups");

standupApi.MapGet("/", async (IDataStore dataStore, CancellationToken cancellationToken) =>
{
    await dataStore.InitializeAsync(cancellationToken);
    var entries = await dataStore.GetEntriesAsync(cancellationToken);
    return Results.Ok(entries.OrderByDescending(item => item.Date));
});

standupApi.MapGet("/by-date/{dateIso}", async (IDataStore dataStore, string dateIso, CancellationToken cancellationToken) =>
{
    if (!DateOnly.TryParse(dateIso, out var date))
    {
        return Results.BadRequest("Invalid date. Use yyyy-MM-dd format.");
    }

    await dataStore.InitializeAsync(cancellationToken);
    var entry = await dataStore.GetEntryByDateAsync(date, cancellationToken);
    return entry is null ? Results.NotFound() : Results.Ok(entry);
});

standupApi.MapPut("/{id:guid}", async (IDataStore dataStore, Guid id, StandupEntry entry, CancellationToken cancellationToken) =>
{
    await dataStore.InitializeAsync(cancellationToken);
    entry.Id = id;
    entry.UpdatedAtUtc = DateTimeOffset.UtcNow;
    await dataStore.SaveEntryAsync(entry, cancellationToken);
    return Results.NoContent();
});

standupApi.MapDelete("/{id:guid}", async (IDataStore dataStore, Guid id, CancellationToken cancellationToken) =>
{
    await dataStore.InitializeAsync(cancellationToken);
    await dataStore.DeleteEntryAsync(id, cancellationToken);
    return Results.NoContent();
});

app.Run();
