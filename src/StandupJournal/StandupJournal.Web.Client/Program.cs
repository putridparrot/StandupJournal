using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using StandupJournal.Core;
using StandupJournal.Shared.Services;
using StandupJournal.Web.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Add device-specific services used by the StandupJournal.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();
builder.Services.AddScoped<ThemeService>();
builder.Services.AddScoped<AppPreferencesService>(sp =>
{
	var service = new AppPreferencesService();
	service.SetDatabaseLocation("In-memory store (WebAssembly)");
	return service;
});
builder.Services.AddFluentUIComponents();
builder.Services.AddStandupCore();
builder.Services.AddInMemoryDataStore();
builder.Services.AddApiDataStore(options =>
{
	options.BaseAddress = builder.HostEnvironment.BaseAddress;
}, setAsPrimaryDataStore: true);

await builder.Build().RunAsync();
