import CoreText
public import Foundation
public import Results

// --- [MODELS] --------------------------------------------------------------------------

public struct FontFace: Codable, Hashable, Sendable {
    public let postscriptName: String
    public let family: String
    public let style: String
    public let displayName: String
    public let file: URL
}

extension FontFace {
    public static func available() -> [Self] {
        let collection: CTFontCollection = CTFontCollectionCreateFromAvailableFonts([kCTFontCollectionRemoveDuplicatesOption as String: true] as CFDictionary)
        return (CTFontCollectionCreateMatchingFontDescriptors(collection) as? [CTFontDescriptor] ?? []).compactMap(Self.init(_:))
    }

    public init(named name: String) throws(Unmatched) {
        let query: CTFontDescriptor = CTFontDescriptorCreateWithAttributes([kCTFontNameAttribute: name] as CFDictionary)
        guard let face: Self = CTFontDescriptorCreateMatchingFontDescriptor(query, nil).flatMap(Self.init(_:)) else {
            throw Unmatched(
                subject: "font",
                query: name,
                candidates: (CTFontManagerCopyAvailablePostScriptNames() as? [String] ?? []).filter { candidate in candidate.localizedStandardContains(name) },
            )
        }
        self = face
    }

    private init?(_ descriptor: CTFontDescriptor) {
        switch (
            CTFontDescriptorCopyAttribute(descriptor, kCTFontNameAttribute) as? String, CTFontDescriptorCopyAttribute(descriptor, kCTFontFamilyNameAttribute) as? String,
            CTFontDescriptorCopyAttribute(descriptor, kCTFontStyleNameAttribute) as? String, CTFontDescriptorCopyAttribute(descriptor, kCTFontDisplayNameAttribute) as? String,
            CTFontDescriptorCopyAttribute(descriptor, kCTFontURLAttribute) as? URL,
        ) {
            case (.some(let postscriptName), .some(let family), .some(let style), .some(let displayName), .some(let file)):
                self.init(postscriptName: postscriptName, family: family, style: style, displayName: displayName, file: file)
            default: return nil
        }
    }
}
