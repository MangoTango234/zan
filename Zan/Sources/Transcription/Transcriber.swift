import Foundation

/// Turns an audio file into text. OpenAI (cloud) or WhisperKit (on-device).
/// Anthropic has no speech-to-text API, so Claude can't be a voice provider.
protocol Transcriber {
    func transcribe(fileURL: URL, model: String) async throws -> String
}

/// A human-readable API/transcription error surfaced to the UI.
struct TranscriptionError: LocalizedError {
    let message: String
    var errorDescription: String? { message }
}

/// A transcriber stand-in used when the selected engine isn't in this build.
struct UnavailableTranscriber: Transcriber {
    let message: String
    func transcribe(fileURL: URL, model: String) async throws -> String {
        throw TranscriptionError(message: message)
    }
}

/// Whether on-device transcription (WhisperKit) was compiled into this build.
enum BuildInfo {
    static var whisperKitAvailable: Bool {
        #if canImport(WhisperKit)
        true
        #else
        false
        #endif
    }
}

/// Returns the transcriber for the currently selected voice provider.
enum TranscriberFactory {
    @MainActor
    static func make() -> Transcriber {
        switch AppSettings.currentTranscriptionProvider() {
        case .openai:
            return OpenAITranscriber()
        case .local:
            #if canImport(WhisperKit)
            return WhisperKitTranscriber()
            #else
            return UnavailableTranscriber(
                message: "On-device transcription isn't in this build. Rebuild with WhisperKit (see README), or use the OpenAI engine.")
            #endif
        }
    }
}
