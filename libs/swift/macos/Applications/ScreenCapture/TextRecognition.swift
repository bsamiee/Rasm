public import CoreGraphics
public import Foundation
import Vision

// --- [OPERATIONS] ----------------------------------------------------------------------

extension CGImage {
    public func recognizeText(at frame: CGRect, in language: Locale.Language, customWords: [String]) async throws(ApplicationFailure) -> [(text: String, frame: CGRect)] {
        var request = RecognizeTextRequest()
        request.recognitionLevel = .fast
        if !request.supportedRecognitionLanguages.contains(where: { supported in supported.languageCode == language.languageCode }) { request.recognitionLevel = .accurate }
        (request.recognitionLanguages, request.customWords, request.minimumTextHeightFraction) = ([language], customWords, 0)
        let observations: [RecognizedTextObservation]
        do { observations = try await request.perform(on: self) } catch VisionError.requestCancelled {
            throw ApplicationFailure(subject: String(describing: frame), reason: .cancelled)
        } catch {
            throw ApplicationFailure(subject: String(describing: frame), reason: .foundation(error as NSError))
        }
        return observations.map { observation in
            (observation.transcript, observation.boundingBox.toImageCoordinates(frame.size, origin: .upperLeft).offsetBy(dx: frame.minX, dy: frame.minY))
        }
    }
}

extension Watch {
    public nonisolated func recognizeText(in window: CGWindowID, customWords: [String]) async throws(ApplicationFailure) -> [(text: String, frame: CGRect)] {
        var found: [(text: String, frame: CGRect)] = []
        for captured in try await capture(.windows(numbers: [window])) {
            let screenshot: Screenshot = try captured.get()
            found += try await screenshot.image.recognizeText(at: screenshot.frame, in: language, customWords: customWords)
        }
        return found
    }
}
