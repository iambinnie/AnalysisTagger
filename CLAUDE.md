# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Context

**AnalysisTagger** is a .NET MAUI video analysis and tagging application inspired by LongoMatch. The PoC phase is complete and the VideoView embedding decision is resolved — this repository is the full multi-project solution build.

**PoC learnings (in repo `AnalysisTaggerVideoPoC`):**
- `LibVLCSharp` `VideoView` was rejected — renders in a separate OS window, cannot be embedded in MAUI layout.
- `MediaElement` (CommunityToolkit.Maui) passed all embedding tests on Windows and is the selected video player.

## Environment

- **IDE**: Visual Studio 2026 Community
- **SDK**: .NET 10.0
- **Primary target**: Windows (`net10.0-windows10.0.19041.0`)
- **Future targets**: Android, iOS, macCatalyst

**Windows Long Path support must be enabled** (required for deep MAUI build paths):
```powershell
Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Control\FileSystem" -Name LongPathsEnabled -Value 1
```

## Build Commands

```bash
# Build (default platform)
dotnet build AnalysisTagger/AnalysisTagger.csproj

# Build specific platform
dotnet build -f net10.0-windows10.0.19041.0
dotnet build -f net10.0-android
dotnet build -f net10.0-ios

# Release build
dotnet build -c Release

# Run unit tests (once test projects exist)
dotnet test
```

Solution format: `.slnx` (Visual Studio 2026 / MSBuild 17.8+).

## Planned Solution Structure

Multi-project Clean Architecture. Build order:

1. **Solution scaffold** — multi-project layout, `global.json`, DI wiring
2. **Domain layer** — models, value objects, enums *(scaffolded in current commit)*
3. **Application layer** — interfaces, services, DTOs, exceptions
4. **Infrastructure — Database** — EF Core + SQLite, repositories, Unit of Work
5. **Infrastructure — Video** — `IVideoPlayer` implemented with `MediaElement`
6. **Infrastructure — Export** — `IVideoExporter` implemented with `Xabe.FFmpeg`
7. **UI — Shell & Navigation** — `AppShell`, `MauiProgram.cs`, navigation routes
8. **UI — Tagging Panel & Timeline** — `TaggingPanelView`, `TimelineView`, ViewModels
9. **UI — Pitch Overlay** — `PitchOverlayView`, `PitchDrawable`, `PitchOverlayViewModel`
10. **Testing** — domain and application unit tests

## Architecture

Clean Architecture with strict layer boundaries. Dependency direction: UI → Application → Domain; Infrastructure implements Application interfaces.

### Domain Layer (`Domain/`)

**Entities** (`Models/`):
- `Project` — top-level container for a match/analysis session. Holds teams, a `TagTemplate`, a video file path, and a list of `EventTag`s. Contains domain query methods: `GetEventsByCategory`, `GetEventsByPlayer`, `GetEventsInWindow`, `AddEvent`.
- `EventTag` — a tagged moment in video. References a `VideoSegment`, a `Category`, zero or more `Player`s, notes, and a list of `DrawingAnnotation`s.
- `TagTemplate` — reusable set of `Category` definitions for a sport; built-in templates exist for Football and Generic.
- `Category` — a tagging button definition (name, color, default lead/lag times, subcategories).
- `Playlist` / `PlaylistEntry` — ordered collection of `EventTag` references with optional per-entry segment overrides.
- `Team` / `Player` — squad data with photos and numbers.
- `DrawingAnnotation` — serialized drawing overlaid on a specific video frame (stored as JSON + thumbnail).

**Value Objects** (`ValueObjects/`):
- `Timecode` — strongly-typed `TimeSpan` wrapper. Use everywhere instead of raw numbers; provides `IsAfter()`, `IsBefore()`, and formatting helpers.
- `VideoSegment` — start/end window of video with duration calculation and lead/lag adjustment.

**Enums** (`Enums/`):
- `SportType`: Generic, Football, Basketball, Rugby, Hockey, Tennis, Custom
- `TaggingMode`: LiveTagging (real-time) vs PostTagging (post-match review)

### Application Layer (`Application/`) — *not yet implemented*

Will contain interfaces (e.g. `IVideoPlayer`, `IVideoExporter`, repository contracts), application services, DTOs, and domain exceptions.

### Infrastructure Layer — *not yet implemented*

- **Database**: EF Core + SQLite. Repository pattern with Unit of Work.
- **Video**: `MediaElement`-based `IVideoPlayer` implementation.
- **Export**: `Xabe.FFmpeg`-based `IVideoExporter` implementation.

### UI / Presentation Layer

- `MauiProgram.cs` — composition root; MAUI DI, fonts, logging.
- `AppShell.xaml` — navigation shell; add new routes here.
- `App.xaml` — merges `Resources/Styles/Colors.xaml` and `Resources/Styles/Styles.xaml`.
- `MainPage.xaml` — placeholder; will be replaced during UI build phase.

Planned UI components: `TaggingPanelView`, `TimelineView`, `PitchOverlayView` (with `PitchDrawable`).

### Data Flow

`Project` owns all session data. User tags a video moment → `EventTag` (with `VideoSegment` + `Category` + optional players/drawings) is added to the project. Tags are grouped into `Playlist`s for review/export. `TagTemplate`s supply reusable `Category` sets per sport.

## Key Conventions

- Domain logic (filtering, querying) lives on domain entities, not in services. Follow the existing pattern on `Project` for new queries.
- `Timecode` is the canonical type for any point in time within a video — never use raw `TimeSpan` or `double`.
- Platform-specific entry points (`MainActivity.cs`, `AppDelegate.cs`, `Program.cs`) stay thin; all initialisation goes in `MauiProgram.cs`.
- Global colors and control styles live in `Resources/Styles/` — extend there, not inline.
- Do not introduce `LibVLCSharp` or any other video library — `MediaElement` is the decided choice.
