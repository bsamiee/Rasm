public import CoreGraphics
public import Foundation
import Vision

// --- [OPERATIONS] ----------------------------------------------------------------------

extension CGImage {
    public func recognizeText(at frame: CGRect, in language: Locale.Language, customWords: [String]) async throws(ApplicationFailure) -> [(text: String, frame: CGRect)] {
        var request = RecognizeTextRequest()
        let supported: Bool = request.supportedRecognitionLanguages.contains { listed in listed.languageCode == language.languageCode }
        (request.recognitionLanguages, request.automaticallyDetectsLanguage) = (supported ? [language] : request.recognitionLanguages, !supported)
        (request.customWords, request.minimumTextHeightFraction) = (customWords, 0)
        let subject = "text recognition at \(frame)"
        let observations: [RecognizedTextObservation]
        do { observations = try await request.perform(on: self) } catch VisionError.requestCancelled, is CancellationError {
            throw ApplicationFailure(subject: subject, reason: .cancelled)
        } catch {
            throw ApplicationFailure(subject: subject, reason: .foundation(error as NSError))
        }
        let known: Set<String> = Set(customWords)
        return observations.map { observation in
            (
                observation.topCandidates(10).first { candidate in known.contains(candidate.string) }?.string ?? observation.transcript,
                observation.boundingBox.toImageCoordinates(frame.size, origin: .upperLeft).offsetBy(dx: frame.minX, dy: frame.minY),
            )
        }
    }
}

extension Watch {
    public nonisolated func recognizeText(in windows: [CGWindowID], customWords: [String]) async throws(ApplicationFailure) -> [CGWindowID: [(text: String, frame: CGRect)]] {
        guard !windows.isEmpty else { return [:] }
        var found: [CGWindowID: [(text: String, frame: CGRect)]] = [:]
        for (window, captured) in zip(windows, try await capture(.windows(numbers: windows))) {
            let screenshot: Screenshot = try captured.get()
            found[window] = try await screenshot.image.recognizeText(at: screenshot.frame, in: language, customWords: customWords)
        }
        return found
    }
}
