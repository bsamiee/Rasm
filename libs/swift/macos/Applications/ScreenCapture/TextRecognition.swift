import AppKit
import CasePathsCore
import CoreGraphics
import CoreImage.CIFilterBuiltins
import Foundation
import Vision

// --- [MODELS] --------------------------------------------------------------------------

struct RecognizedLine: Equatable, Sendable {
    let text: String
    let frame: CGRect
    let keyEquivalentTranscript: String?
    let isTitle: Bool
    let enabled: Bool
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Screenshot {
    func recognizeText(in language: Locale.Language, labels: Set<String>) async throws(ApplicationFailure) -> [RecognizedLine] {
        var request = RecognizeTextRequest()
        let languages: [Locale.Language] = request.supportedRecognitionLanguages
        let supported: [String] = languages.map(\.minimalIdentifier)
        let listed: [String: Locale.Language] = Dictionary(zip(supported, languages)) { first, _ in first }
        let matched: Locale.Language? = Bundle.preferredLocalizations(from: supported, forPreferences: [language.minimalIdentifier]).first
            .flatMap { identifier in listed[identifier] }
            .flatMap { match in match.languageCode == language.languageCode ? match : nil }
        if let matched { request.recognitionLanguages = [matched] } else { request.automaticallyDetectsLanguage = true }
        request.minimumTextHeightFraction = 0
        let subject = "text recognition in the window at \(SIMD2<Double>(frame.minX, frame.minY).coordinates)"
        let observations: [RecognizedTextObservation]
        do { observations = try await request.perform(on: image) } catch VisionError.requestCancelled, is CancellationError {
            throw ApplicationFailure(subject: subject, reason: .cancelled)
        } catch {
            throw ApplicationFailure(subject: subject, reason: .foundation(error as NSError))
        }
        guard !observations.isEmpty else { return [] }
        let lines: [[RecognizedTextObservation]] =
            observations
            .sorted { first, second in first.boundingBox.cgRect.midY > second.boundingBox.cgRect.midY }
            .reduce(into: []) { lines, observation in
                let band: ClosedRange<CGFloat>? = lines.last?.first.map { leading in leading.boundingBox.cgRect.minY...leading.boundingBox.cgRect.maxY }
                if band?.contains(observation.boundingBox.cgRect.midY) == true { lines[lines.count - 1].append(observation) } else { lines.append([observation]) }
            }
        let rows: [(title: RecognizedTextObservation, label: String?, trailing: [String])] = lines.map { line in
            let ordered: [RecognizedTextObservation] = line.sorted { first, second in first.boundingBox.cgRect.minX < second.boundingBox.cgRect.minX }
            let labelled: [String?] = ordered.map { observation in observation.topCandidates(10).first { candidate in labels.contains(candidate.string) }?.string }
            let title: Int = labelled.firstIndex { label in label != nil } ?? 0
            return (ordered[title], labelled[title], ordered[(title + 1)...].map(\.transcript))
        }
        let tone = CIFilter.colorControls()
        (tone.inputImage, tone.saturation) = (CIImage(cgImage: image), 0)
        let gray: CIImage? = tone.outputImage
        let pixels = CGSize(width: image.width, height: image.height)
        let reductions: CIImage = rows.enumerated().reduce(CIImage.empty()) { joined, row in
            let range = CIFilter.areaMinMaxRed()
            (range.inputImage, range.extent) = (gray, row.element.title.boundingBox.toImageCoordinates(pixels))
            let reduced: CIImage = range.outputImage ?? CIImage.empty()
            return reduced.transformed(by: CGAffineTransform(translationX: CGFloat(row.offset) - reduced.extent.minX, y: -reduced.extent.minY)).composited(over: joined)
        }
        var extremes: [Float] = Array(repeating: 0, count: 4 * rows.count)
        unsafe rendering.render(
            reductions,
            toBitmap: &extremes,
            rowBytes: MemoryLayout<Float>.stride * extremes.count,
            bounds: CGRect(x: 0, y: 0, width: rows.count, height: 1),
            format: .RGBAf,
            colorSpace: nil,
        )
        let contrasts: [Float] = rows.indices.map { index in extremes[4 * index + 1] - extremes[4 * index] }
        let full: CGFloat = NSColor.controlTextColor.alphaComponent
        let threshold: Float = (contrasts.max() ?? 0) * Float((full + NSColor.disabledControlTextColor.alphaComponent) / (2 * full))
        return zip(rows, contrasts).map { row, contrast in
            RecognizedLine(
                text: row.label ?? row.title.transcript,
                frame: row.title.boundingBox.toImageCoordinates(frame.size, origin: .upperLeft).offsetBy(dx: frame.minX, dy: frame.minY),
                keyEquivalentTranscript: row.trailing.isEmpty ? nil : row.trailing.joined(separator: " "),
                isTitle: row.title.isTitle,
                enabled: contrast >= threshold,
            )
        }
    }
}

extension Watch {
    nonisolated func recognizeText(
        in windows: [CGWindowID],
        labels: Set<String>,
    ) async throws(ApplicationFailure) -> [CGWindowID: [RecognizedLine]] {
        guard !windows.isEmpty else { return [:] }
        var found: [CGWindowID: [RecognizedLine]] = [:]
        for (window, captured) in zip(windows, try await capture(.windows(numbers: windows))) {
            switch captured {
                case .success(let screenshot): found[window] = try await screenshot.recognizeText(in: language, labels: labels)
                case .failure(let failure) where failure.reason.is(\.absent) || failure.reason.is(\.noBackingStore): continue
                case .failure(let failure): throw failure
            }
        }
        return found
    }
}
