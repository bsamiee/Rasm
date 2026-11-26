import AppKit
import CustomDump
import Foundation
import ScreenCapture
import Testing

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite(.timeLimit(.minutes(1)))
struct TextRecognitionTests {
    static let drawings: [(language: String, labels: [(text: String, rect: CGRect)])] = [
        (
            "en",
            [
                ("Clean Up", CGRect(x: 12, y: 18, width: 236, height: 20)), ("Rectangle Frame Tool", CGRect(x: 12, y: 196, width: 236, height: 20)),
                ("Vqxlirn", CGRect(x: 96, y: 371, width: 150, height: 20)), ("Paragraph Styles", CGRect(x: 12, y: 522, width: 236, height: 20)),
            ],
        ),
        ("ja", [("ファイル", CGRect(x: 12, y: 18, width: 120, height: 20)), ("編集", CGRect(x: 140, y: 240, width: 100, height: 20)), ("表示", CGRect(x: 30, y: 498, width: 100, height: 20))]),
    ]

    @Test(arguments: drawings)
    func readsDrawnLabelsAtTheirScreenFrames(language: String, labels: [(text: String, rect: CGRect)]) async throws {
        let frame = CGRect(x: 731, y: 212, width: 260, height: 560)
        let scale = 2
        let context: CGContext = try #require(
            unsafe CGContext(
                data: nil,
                width: Int(frame.width) * scale,
                height: Int(frame.height) * scale,
                bitsPerComponent: 8,
                bytesPerRow: 0,
                space: CGColorSpaceCreateDeviceRGB(),
                bitmapInfo: CGBitmapInfo(rawValue: CGImageAlphaInfo.premultipliedLast.rawValue),
            )
        )
        context.setFillColor(.white)
        context.fill(CGRect(x: 0, y: 0, width: context.width, height: context.height))
        context.translateBy(x: 0, y: CGFloat(context.height))
        context.scaleBy(x: CGFloat(scale), y: -CGFloat(scale))
        NSGraphicsContext.saveGraphicsState()
        NSGraphicsContext.current = NSGraphicsContext(cgContext: context, flipped: true)
        for label in labels { NSAttributedString(string: label.text, attributes: [.font: NSFont.menuFont(ofSize: 13)]).draw(in: label.rect) }
        NSGraphicsContext.restoreGraphicsState()
        let image: CGImage = try #require(context.makeImage())
        let found: [(text: String, frame: CGRect)] = try await image.recognizeText(at: frame, in: Locale.Language(identifier: language), customWords: [])
        expectNoDifference(Set(found.map(\.text)), Set(labels.map(\.text)))
        let drawn: [String: CGRect] = Dictionary(uniqueKeysWithValues: labels.map { label in (label.text, label.rect.offsetBy(dx: frame.minX, dy: frame.minY)) })
        let centers: [String: CGPoint] = Dictionary(found.map { recognized in (recognized.text, CGPoint(x: recognized.frame.midX, y: recognized.frame.midY)) }) { first, _ in first }
        let misplaced: [String: CGPoint?] = Dictionary(
            uniqueKeysWithValues: drawn.compactMap { (text: String, rect: CGRect) -> (String, CGPoint?)? in centers[text].map(rect.contains) == true ? nil : (text, centers[text]) }
        )
        expectNoDifference(misplaced, [:], "Each label's recognized center falls inside its drawn rectangle \(drawn)")
    }
}
