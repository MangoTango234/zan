# Zan for Windows (handoff / build plan)

This folder is for a **native Windows** version of Zan. The macOS app (Swift, at
the repo root) does not run on Windows, so this is a fresh implementation in
**.NET / C#** that mirrors the same features. Treat the macOS app as the
reference spec and `shared/actions.json` as the canonical built-in catalog.

> Start a fresh conversation pointed at this file to build it. Everything needed
> to begin is here; you do not need the macOS thread's history.

## What Zan is (feature spec to match)

A menu-bar / system-tray utility for fast voice + text AI, at the cursor, in any
app. No telemetry. Keys stay in OS secure storage.

**Voice (dictation)**
- One global hotkey, two modes: **toggle** (press to start/stop) and
  **hold-to-talk** (hold, release to stop).
- Records the mic to a temp file, transcribes, optionally runs an AI "cleanup"
  pass (editable prompt, default ON), then **types the text at the cursor**.
- A small on-screen recording HUD with a live waveform + Stop button.
- Transcription engine is selectable: **OpenAI (cloud)** or **on-device Whisper**.

**Text actions**
- A list of **actions**, each with: name, short description, a global hotkey, an
  **engine** (`ai` = LLM prompt, or `prefix` = prepend a fixed string), an
  editable **prompt**, and an **output mode**: `replaceSelection`, `popup`
  (read-only), or `copy`.
- Triggering an action: read the current selection (synthesized Ctrl+C), run it,
  then deliver per output mode (replace via Ctrl+V, show a popup, or copy).
- Ships with the catalog in `shared/actions.json` (Proofread, Make professional,
  Strip em dashes, Translate to English [popup], Summarize [popup], Open in
  r.jina.ai [prefix op]). Users can add/edit/delete their own.
- Text runs on **OpenAI** or **Anthropic Claude** (provider picker), per-provider
  API key + model.

**Also:** in-app history of past dictations/actions, a settings surface (keys,
providers, models, cleanup prompt), permissions/first-run guidance, start at
login, and an app that lives only in the tray.

## Stack

- **.NET 8, C#.** WPF is the simplest path for a tray utility (mature, easy
  windowing); WinUI 3 is the modern alternative, pick WPF unless you want WinUI.
- Single-file, self-contained publish (`dotnet publish -r win-x64`) so users can
  download and run without installing .NET.

### Platform mapping (macOS -> Windows)

| Concern | macOS (reference) | Windows approach |
|---|---|---|
| Tray icon + menu | NSStatusItem / MenuBarExtra | `Hardcodet.NotifyIcon.Wpf` (or H.NotifyIcon) |
| Global hotkeys | KeyboardShortcuts | `RegisterHotKey`/`UnregisterHotKey` (Win32) or `NHotkey.Wpf` |
| Read selection | synth Cmd+C | synth **Ctrl+C** via `SendInput`, read clipboard after a short delay, restore |
| Insert at cursor | synth Cmd+V | clipboard set + synth **Ctrl+V** via `SendInput`, restore clipboard |
| Mic capture | AVAudioRecorder | **NAudio** (`WaveInEvent`/WASAPI) -> 16 kHz mono WAV |
| On-device STT | WhisperKit | **Whisper.net** (whisper.cpp; downloads a ggml model) |
| OpenAI transcribe | /v1/audio/transcriptions | same REST via `HttpClient` (multipart) |
| Text (OpenAI) | /v1/chat/completions | same REST |
| Text (Anthropic) | /v1/messages (x-api-key, anthropic-version: 2023-06-01) | same REST |
| Secret storage | Keychain | **Windows Credential Manager** (e.g. `CredentialManagement`) or DPAPI (`ProtectedData`) |
| Config/history | ~/Library/Application Support/Zan | `%APPDATA%\Zan\` (actions.json, history.json) |
| Launch at login | SMAppService | `HKCU\...\Run` registry value or Startup shortcut |
| Distribution | Developer ID + notarize | Authenticode code-signing (optional for v0.1; unsigned triggers SmartScreen) |

### Gotchas carried over from macOS
- **Wait for hotkey modifiers to release before SendInput.** Same bug we hit on
  mac: if Ctrl/Alt are still physically held when you synthesize Ctrl+C/Ctrl+V,
  it's contaminated. Poll `GetAsyncKeyState` for Ctrl/Alt/Shift/Win until clear
  (with a timeout) before sending.
- **Clipboard save/restore** around both copy and paste so the user's clipboard
  is preserved (snapshot text + common formats, restore after a short delay).
- **No accessibility permission needed** for SendInput (simpler than macOS),
  but injecting into an **elevated** window requires the sender to be elevated.
- **Confirmations:** keep destructive confirms inline in the popup window (don't
  rely on modal dialogs from a tray popup).
- **Anthropic has no speech-to-text** — transcription is OpenAI or on-device only.

## Suggested milestones
1. Tray app shell (icon + menu + Quit), no main window, runs in tray.
2. Settings window: API keys (Cred Manager), provider/model pickers, load
   `shared/actions.json` into an editable actions list (persist to `%APPDATA%`).
3. Global hotkey registration per action + the dictation trigger.
4. Selection read (Ctrl+C) -> action engine -> deliver (replace/popup/copy),
   with the modifier-release + clipboard-restore handling.
5. Dictation: mic capture -> OpenAI transcription -> optional cleanup -> paste,
   with recording HUD.
6. On-device transcription via Whisper.net (engine picker).
7. Anthropic provider for text; history; launch at login.
8. Package: single-file publish; optional Authenticode signing.

## Keep in sync
`shared/actions.json` is the source of truth for built-in actions and the
dictation cleanup prompt. If you change defaults on one platform, update the JSON
(and ideally both apps read from it).
