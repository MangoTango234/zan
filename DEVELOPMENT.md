# Development & handoff

Context for picking up work on Zan (macOS) in a new session or by a new
contributor. The macOS app is the reference implementation; a Windows port is
planned (see `windows/README.md`).

## What Zan is
Native macOS menu-bar app (SwiftUI, menu-bar agent, no Dock icon, non-sandboxed)
for voice-to-text dictation and AI text actions, at the cursor, in any app.
OpenAI or on-device Whisper for transcription; OpenAI or Anthropic for text.
Keys in the Keychain. No telemetry.

## Repo map
- `project.yml` — XcodeGen spec (default build: no WhisperKit, lightweight).
- `project-whisperkit.yml` — overlay that adds on-device WhisperKit. Generate
  with `xcodegen generate --spec project-whisperkit.yml`.
- `Zan/Sources/`
  - `App.swift` — `@main`, creates stores, MenuBarExtra, registers hotkeys at
    launch, start-at-login on first run, shows the welcome window.
  - `MenuContentView.swift` — the dropdown: header, SectionCard layout, footer.
  - `Models/Action.swift` — the unified `Action` (name, detail, hotkey,
    engine ai|prefix, prompt/prefix, output replace|popup|copy) + `defaults`.
  - `Stores/` — `ActionStore` (actions.json + migration), `AppSettings`
    (UserDefaults: providers, models, modes, cleanup prompt), `HistoryStore`,
    `KeychainStore` (OpenAI + Anthropic keys), `AppPaths`, `LoginItem`.
  - `Audio/` — `AudioRecorder` (AVAudioRecorder + metering), `DictationController`
    (hotkey -> record -> transcribe -> optional cleanup -> insert).
  - `Transcription/` — `Transcriber` protocol, `OpenAITranscriber`,
    `WhisperKitTranscriber` (guarded by `#if canImport(WhisperKit)`),
    `TranscriberFactory`.
  - `Transform/` — `TextTransformer` protocol, `OpenAITransformer`,
    `AnthropicTransformer`, `TextEngineFactory`, `TransformController`
    (per-action hotkeys -> read selection -> run -> deliver).
  - `Injection/` — `SelectionReader` (Cmd+C), `TextInjector` (Cmd+V),
    `PasteboardHelper`, `KeySynthesizer` (waits for modifier release).
  - `Permissions/` — `AccessibilityPermission`, `PermissionsManager`.
  - `Views/` — `ActionsSectionView`, `VoiceSectionView`, `DictationStatusView`,
    `RecordingOverlay`, `TransformHUD`, `InfoPopup`, `SystemSectionView`,
    `WelcomeWindow`, `ApiKeySectionView`.
  - `Resources/Assets.xcassets/AppIcon.appiconset` — app icon.
- `shared/actions.json` — canonical built-in catalog (keep in sync with
  `Action.defaults` + `AppSettings.defaultCleanupPrompt`).
- `windows/README.md` — Windows (.NET/C#) build plan.
- `screenshots/` — README images/GIFs.

## Build & run
```sh
brew install xcodegen          # once
xcodegen generate              # default (OpenAI transcription)
open Zan.xcodeproj             # Cmd+R
# on-device Whisper instead:
xcodegen generate --spec project-whisperkit.yml
```
`Zan.xcodeproj` is gitignored (regenerated). Default committed signing is ad-hoc
so anyone can build.

## Release (notarized) recipe
1. `xcodegen generate --spec project-whisperkit.yml`
2. Build Release signed with the project's Developer ID (FUTODI, UAB, team
   `8YBTCAGG78`), hardened runtime + entitlements + secure timestamp:
   ```sh
   xcodebuild -project Zan.xcodeproj -scheme Zan -configuration Release \
     -derivedDataPath build-release build \
     CODE_SIGN_STYLE=Manual CODE_SIGN_IDENTITY="Developer ID Application" \
     DEVELOPMENT_TEAM=8YBTCAGG78 PROVISIONING_PROFILE_SPECIFIER="" \
     ENABLE_HARDENED_RUNTIME=YES OTHER_CODE_SIGN_FLAGS="--timestamp"
   ```
   (If multiple Developer ID certs exist, pass the FUTODI cert's SHA-1 hash as
   `CODE_SIGN_IDENTITY` to disambiguate.)
3. `ditto -c -k --keepParent Zan.app Zan.zip`
4. `xcrun notarytool submit Zan.zip --key <p8> --key-id <id> --issuer <id> --wait`
   using the App Store Connect API key on the FUTODI account. **These
   credentials are NOT in the repo** (kept privately).
5. `xcrun stapler staple Zan.app`, re-zip, `gh release upload <tag> Zan.zip`.

Bump version in `project.yml` AND `CFBundleShortVersionString`/`CFBundleVersion`
in the `info.properties` (XcodeGen does not auto-link `MARKETING_VERSION`).

## Conventions / gotchas
- **Synthetic Cmd+C/Cmd+V waits for hotkey modifiers to release**
  (`KeySynthesizer.afterModifiersReleased`) or the keystroke is contaminated.
- **Clipboard is snapshotted and restored** around copy/paste.
- **`.confirmationDialog`/`.alert` are unreliable from a MenuBarExtra window** —
  use inline confirmations (see delete/reset).
- **Keychain service** is `dev.local.zan` (a stable label, unchanged by the
  bundle-id rebrand so saved keys persist). Config lives in
  `~/Library/Application Support/Zan/`.
- Anthropic has **no speech-to-text** API — transcription is OpenAI or on-device.
- Bundle id `com.futodi.zan`; menu-bar agent (`LSUIElement`).

## Current state
- v0.1.1 shipped: public repo, notarized downloadable release (FUTODI, UAB),
  source build, README with screenshots/demos, feedback at hello@futodi.com.

## Ideas / next
- Use it, collect friction, polish prompts/UX.
- Optional: README polish, a one-command release script, app-icon refinements.
- Windows version per `windows/README.md` (separate effort).
