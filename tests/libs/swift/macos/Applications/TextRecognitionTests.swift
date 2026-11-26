import AppKit
import Applications
import CustomDump
import Foundation
import Testing

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite(.timeLimit(.minutes(1)))
struct TextRecognitionTests {
    static let drawings: [(language: String, scale: Int, labels: [(text: String, rect: CGRect)])] = [
        (
            "en",
            1,
            [
                "Selection Tool", "Direct Selection Tool", "Group Selection Tool", "Magic Wand Tool", "Lasso Tool", "Artboard Tool", "Pen Tool", "Curvature Tool", "Line Segment Tool",
                "Rectangular Grid Tool", "Rectangle Tool", "Ellipse Tool", "Polygon Tool", "Star Tool", "Paintbrush Tool", "Shaper Tool",
            ].enumerated().map { row, text in (text: text, rect: CGRect(x: 6, y: 4 + 30 * row, width: 230, height: 24)) },
        ),
        ("ja", 2, [("ファイル", CGRect(x: 6, y: 18, width: 120, height: 24)), ("編集", CGRect(x: 130, y: 240, width: 100, height: 24)), ("表示", CGRect(x: 30, y: 438, width: 100, height: 24))]),
    ]

    @Test(arguments: drawings)
    func readsPanelLabelsAtTheirScreenFrames(language: String, scale: Int, labels: [(text: String, rect: CGRect)]) async throws {
        let frame = CGRect(x: 731, y: 212, width: 250, height: 500)
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
        context.setFillColor(CGColor(gray: 0.27, alpha: 1))
        context.fill(CGRect(x: 0, y: 0, width: context.width, height: context.height))
        context.translateBy(x: 0, y: CGFloat(context.height))
        context.scaleBy(x: CGFloat(scale), y: -CGFloat(scale))
        context.setStrokeColor(CGColor(gray: 0.36, alpha: 1))
        NSGraphicsContext.saveGraphicsState()
        NSGraphicsContext.current = NSGraphicsContext(cgContext: context, flipped: true)
        for (row, label) in labels.enumerated() {
            context.stroke(label.rect.insetBy(dx: 0.5, dy: 0.5))
            NSAttributedString(
                string: label.text,
                attributes: [.font: NSFont.systemFont(ofSize: NSFont.smallSystemFontSize), .foregroundColor: NSColor(white: row.isMultiple(of: 2) ? 0.55 : 0.85, alpha: 1)],
            )
            .draw(at: CGPoint(x: label.rect.minX + 32, y: label.rect.minY + 5))
        }
        NSGraphicsContext.restoreGraphicsState()
        let image: CGImage = try #require(context.makeImage())
        let found: [(text: String, frame: CGRect)] = try await image.recognizeText(at: frame, in: Locale.Language(identifier: language), customWords: [])
        expectNoDifference(Set(found.map(\.text)), Set(labels.map(\.text)))
        let drawn: [String: CGRect] = Dictionary(uniqueKeysWithValues: labels.map { label in (label.text, label.rect.offsetBy(dx: frame.minX, dy: frame.minY)) })
        let centers: [String: CGPoint] = Dictionary(found.map { recognized in (recognized.text, CGPoint(x: recognized.frame.midX, y: recognized.frame.midY)) }) { first, _ in first }
        let misplaced: [String: CGPoint?] = Dictionary(
            uniqueKeysWithValues: drawn.compactMap { (text: String, rect: CGRect) -> (String, CGPoint?)? in centers[text].map(rect.contains) == true ? nil : (text, centers[text]) }
        )
        expectNoDifference(misplaced, [:], "Each label's recognized center falls inside its drawn row \(drawn)")
    }
}
