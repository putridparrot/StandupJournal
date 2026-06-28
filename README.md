# Standup Journal

NOTE: THIS IS IN DEVELOPMENT, USE AT YOUR OWN RISK

I often like to note down my stand-up information so that I can clearing recal and look back on my aims for the day, note any blockers etc. I often end up doing this in notepad which is fine, but wanting to make this more "formal" for myself, so I can check back on a date basis or search for information etc.

## Current scaffold

- .NET MAUI Blazor Hybrid + WebAssembly solution
- Fluent UI component stack wired in MAUI, Web host, and WebAssembly client
- Local datastore abstraction with SQLite implementation on MAUI
- Remote API datastore scaffold for online mode
- Sync service scaffold for local/remote reconciliation
- Seed helper for starter sample entries

## Datastore mode by host

- MAUI app uses local SQLite as primary store
- MAUI app can opt into remote sync by setting environment variable:
  - `STANDUP_REMOTE_BASE_URL=https://your-host`
- Web host uses in-memory store and exposes HTTP API endpoints:
  - `GET /api/standups`
  - `GET /api/standups/by-date/{yyyy-MM-dd}`
  - `PUT /api/standups/{id}`
  - `DELETE /api/standups/{id}`
- WebAssembly client uses API datastore as primary store (via Web host endpoints)

## Quick start

1. Build solution:
	- `dotnet build src/StandupJournal/StandupJournal.sln`
2. Run Web host:
	- `dotnet run --project src/StandupJournal/StandupJournal.Web`
3. Open app and use:
	- `Seed Sample Data` to add starter entries
	- `Sync Now` to reconcile local + remote stores when both are configured