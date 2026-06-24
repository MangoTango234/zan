# Building Zan for Windows

.NET 8 / C# (WPF, tray-only). Build and run on **Windows** (WPF does not build on
macOS/Linux). Install the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

## Run (dev)
```sh
cd windows
dotnet run --project src/Zan
```
Zan starts in the system tray (no window). Right-click the tray icon for the menu.

## Build (single-file, self-contained)
Produces an exe that runs without installing .NET:
```sh
cd windows
dotnet publish src/Zan -c Release -r win-x64 ^
  -p:PublishSingleFile=true --self-contained true
```
Output: `src/Zan/bin/Release/net8.0-windows/win-x64/publish/Zan.exe`.

## Layout
- `Zan.sln`, `src/Zan/` — the WPF project.
- `src/Zan/App.xaml(.cs)` — tray-only app lifecycle (no main window).
- `src/Zan/TrayIconFactory.cs` — tray icon + right-click menu.
- `src/Zan/Models/ActionCatalog.cs` — schema for `shared/actions.json`.
- `src/Zan/Services/ActionStore.cs` — loads the shared catalog.
- `src/Zan/Assets/zan.png` — tray icon.
- `shared/actions.json` (repo root) is copied next to the exe at build time and
  is the source of truth for built-in actions. See `windows/README.md` for the
  full milestone plan.

## Milestones
1. **Tray app shell (this milestone)** — icon + menu + Quit, no main window,
   loads the shared catalog.
2. Settings window, keys, provider/model pickers, editable actions.
3. Global hotkeys.
4. Selection read -> action -> deliver.
5. Dictation (OpenAI) + HUD.
6. On-device Whisper.
7. Anthropic text, history, launch at login.
8. Packaging / signing.
