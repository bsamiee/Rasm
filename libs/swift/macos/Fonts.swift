import CoreText
import Foundation

// --- [MODELS] --------------------------------------------------------------------------

public struct InstalledFace: Codable, Hashable, Sendable {
    public let postscriptName: String
    public let family: String
    public let style: String
    public let fullName: String
    public let file: String

    public static func installed() -> [Self] {
        let collection: CTFontCollection = CTFontCollectionCreateFromAvailableFonts([kCTFontCollectionRemoveDuplicatesOption as String: true] as CFDictionary)
        return (CTFontCollectionCreateMatchingFontDescriptors(collection) as? [CTFontDescriptor] ?? []).compactMap(Self.init)
    }

    public static func named(_ name: String) -> Result<Self, Unmatched> {
        let query: CTFontDescriptor = CTFontDescriptorCreateWithAttributes([kCTFontNameAttribute: name] as CFDictionary)
        return CTFontDescriptorCreateMatchingFontDescriptor(query, Set([kCTFontNameAttribute as String]) as CFSet).flatMap(Self.init).map(Result.success)
            ?? .failure(Unmatched(subject: "installed face", query: name, candidates: installed().map(\.postscriptName)))
    }
}

extension InstalledFace {
    init?(_ descriptor: CTFontDescriptor) {
        switch (
            CTFontDescriptorCopyAttribute(descriptor, kCTFontNameAttribute) as? String, CTFontDescriptorCopyAttribute(descriptor, kCTFontFamilyNameAttribute) as? String,
            CTFontDescriptorCopyAttribute(descriptor, kCTFontStyleNameAttribute) as? String, CTFontDescriptorCopyAttribute(descriptor, kCTFontDisplayNameAttribute) as? String,
            CTFontDescriptorCopyAttribute(descriptor, kCTFontURLAttribute) as? URL,
        ) {
            case (.some(let postscriptName), .some(let family), .some(let style), .some(let fullName), .some(let file)):
                self.init(postscriptName: postscriptName, family: family, style: style, fullName: fullName, file: file.path(percentEncoded: false))
            default: return nil
        }
    }
}
