public import AppKit
public import Encoding
import Synchronization
import UniformTypeIdentifiers

// --- [MODELS] --------------------------------------------------------------------------

public struct Pasteboard: Encodable, Sendable {
    public enum Content: CaseTagged, Equatable, Sendable {
        case text(value: String)
        case withheld
    }

    private enum CodingKeys: CodingKey {
        case locator, changeCount, types, access, patterns, contentType, content
    }

    public let locator: Locator
    public let changeCount: Int
    public let types: [String]
    public let access: NSPasteboard.AccessBehavior
    public let patterns: [String]
    public let contentType: String?
    public let content: Content?

    public func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: CodingKeys.self)
        try fields.encode(locator, forKey: .locator)
        try fields.encode(changeCount, forKey: .changeCount)
        try fields.encode(types, forKey: .types)
        switch access {
            case .default: try fields.encode("default", forKey: .access)
            case .ask: try fields.encode("ask", forKey: .access)
            case .alwaysAllow: try fields.encode("alwaysAllow", forKey: .access)
            case .alwaysDeny: try fields.encode("alwaysDeny", forKey: .access)
            @unknown default: try fields.encode(access.rawValue, forKey: .access)
        }
        try fields.encode(patterns, forKey: .patterns)
        try fields.encodeIfPresent(contentType, forKey: .contentType)
        try fields.encodeIfPresent(content, forKey: .content)
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Pasteboard {
    private static let reads: Mutex<[NSPasteboard.Name: Pasteboard]> = Mutex([:])

    static func locator(of name: NSPasteboard.Name) -> Locator { Locator(steps: [Locator.Step(role: String(describing: NSPasteboard.self), key: .identifier(value: name.rawValue))]) }

    public static func write(_ text: String, to name: NSPasteboard.Name) throws(ApplicationFailure) {
        let board = NSPasteboard(name: name)
        board.clearContents()
        guard board.setString(text, forType: .string) else { throw ApplicationFailure(subject: "\(name.rawValue) text \(String(reflecting: text))", reason: .refused) }
    }

    public init(_ name: NSPasteboard.Name) async throws(ApplicationFailure) {
        let board = NSPasteboard(name: name)
        let (changeCount, access): (Int, NSPasteboard.AccessBehavior) = (board.changeCount, board.accessBehavior)
        if let last = Self.reads.withLock({ reads in reads[name] }), last.changeCount == changeCount, last.access == access {
            self = last
            return
        }
        let types: [NSPasteboard.PasteboardType] = board.types ?? []
        let detected: Set<__NSPasteboardDetectionPattern>
        let contentType: UTType?
        do {
            detected = try await board.__detectPatterns(forPatterns: [
                .__probableWebURL, .__probableWebSearch, .__number, .__link, .__phoneNumber, .__emailAddress, .__postalAddress, .__calendarEvent,
                .__shipmentTrackingNumber, .__flightNumber, .__moneyAmount,
            ])
        } catch let error as CocoaError where error.code.rawValue == NSPasteboardContentsNotAvailableError {
            detected = []
        } catch {
            throw ApplicationFailure(subject: name.rawValue, reason: .foundation(error as NSError))
        }
        do {
            contentType = try await types.contains(.fileURL) ? board.detectedMetadata(for: [\.contentType]).contentType : nil
        } catch let error as CocoaError where error.code.rawValue == NSPasteboardContentsNotAvailableError {
            contentType = nil
        } catch {
            throw ApplicationFailure(subject: name.rawValue, reason: .foundation(error as NSError))
        }
        let content: Content? =
            switch (types.contains(.string), access) {
                case (false, _): nil
                case (true, .alwaysAllow): board.string(forType: .string).map(Content.text(value:))
                case (true, _): .withheld
            }
        self.init(
            locator: Self.locator(of: name),
            changeCount: changeCount,
            types: types.map(\.rawValue),
            access: access,
            patterns: detected.map(\.rawValue).sorted(),
            contentType: contentType?.identifier,
            content: content,
        )
        Self.reads.withLock { reads in reads[name] = self }
    }
}
