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
- `src/Zan/App.xaml(.cs)` — tray-only app lifecycle (no main window); opens
  Settings on demand and on first run when no key is configured.
- `src/Zan/TrayIconFactory.cs` — tray icon + right-click menu (Settings, Quit).
- `src/Zan/Models/` — `ActionCatalog`/`ActionItem`/`ActionsDocument`, `AppSettings`.
- `src/Zan/Services/` — `AppPaths` (%APPDATA%\Zan), `ActionStore`,
  `SettingsStore`, `CredentialStore` (Win32 Credential Manager P/Invoke),
  `KeyStore` (OpenAI/Anthropic keys).
- `src/Zan/Input/` — `HotkeyCombo` (parse/format + Win32 conversion),
  `HotkeyService` (RegisterHotKey via a message-only window), `HotkeyCoordinator`
  (binds each action + dictation hotkey to a handler).
- `src/Zan/Injection/` — `KeySynthesizer` (SendInput Ctrl+C/Ctrl+V; waits for
  modifier release), `ClipboardHelper` (snapshot/restore), `SelectionReader`,
  `TextInjector`.
- `src/Zan/Transform/` — `ITextTransformer` + `OpenAITextTransformer` /
  `AnthropicTextTransformer`, `TextEngineFactory`, `TransformController`
  (selection -> engine -> deliver), `ITransformUi`.
- `src/Zan/Views/SettingsWindow.xaml(.cs)` — keys, provider/model pickers,
  dictation cleanup, editable actions list, per-action + dictation hotkeys.
- `src/Zan/Views/HotkeyRecorder.xaml(.cs)` — control to capture a hotkey combo.
- `src/Zan/Views/TransformHud.xaml(.cs)` — non-activating "working" HUD.
- `src/Zan/Views/PopupWindow.xaml(.cs)` — read-only result/error popup.
- `src/Zan/Assets/zan.png` — tray icon.
- `shared/actions.json` (repo root) is copied next to the exe at build time and
  is the seed for built-in actions + the dictation cleanup prompt. User edits
  persist to `%APPDATA%\Zan\actions.json` and `settings.json`. See
  `windows/README.md` for the full milestone plan.

## Milestones
1. ✅ **Tray app shell** — icon + menu + Quit, no main window, loads the catalog.
2. ✅ **Settings** — keys (Credential Manager), provider/model pickers, editable
   actions persisted to `%APPDATA%\Zan`.
3. ✅ **Global hotkeys** — per-action + dictation hotkey registration (Win32
   RegisterHotKey), captured in Settings.
4. ✅ **Actions end to end** — read selection (synth Ctrl+C, wait for modifier
   release, restore clipboard) -> run engine (OpenAI/Anthropic text, or prefix)
   -> deliver (replace via Ctrl+V / popup / copy), with a working HUD.
5. Dictation (mic capture -> OpenAI transcription -> cleanup -> insert) + HUD.
6. On-device Whisper.
7. Anthropic text, history, launch at login.
8. Packaging / signing.
