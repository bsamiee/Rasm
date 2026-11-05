import CoreText
import Foundation
import Results

// --- [MODELS] --------------------------------------------------------------------------

struct FontFace: Codable, Hashable, Sendable {
    let postscriptName: String
    let family: String
    let style: String
    let displayName: String
    let file: URL
}

extension FontFace {
    static func available() -> [Self] {
        let collection: CTFontCollection = CTFontCollectionCreateFromAvailableFonts([kCTFontCollectionRemoveDuplicatesOption as String: true] as CFDictionary)
        return (CTFontCollectionCreateMatchingFontDescriptors(collection) as? [CTFontDescriptor] ?? []).compactMap(Self.init(_:))
    }

    init(named name: String) throws(Unmatched) {
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
