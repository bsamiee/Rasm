import AppleEvents
import ApplicationServer
import Applications
import CoreServices
import EffectRPC
import Foundation

// --- [CONSTANTS] -----------------------------------------------------------------------

private let adobePrefix: String = "com.adobe."
private let activatingCommands: KeyValuePairs<String, [String]> = [
    "\(adobePrefix)InDesign": ["UIUIshow", "K2LKrvlO", "K2LKrvlB"],
    "\(adobePrefix)Acrobat.Pro": ["CARObfrt"],
    "\(adobePrefix)PremierePro": ["CR8RCRPr"],
]

// --- [MODELS] --------------------------------------------------------------------------

enum HelperRequest: Decodable, Sendable {
    private enum Own: String, CodingKey {
        case faces, face
    }

    private struct Named: Decodable {
        let name: String
    }

    case server(Request)
    case faces
    case face(name: String)

    init(from decoder: any Decoder) throws {
        let keys = try decoder.container(keyedBy: Own.self)
        self =
            switch keys.allKeys.first {
                case .faces: .faces
                case .face: .face(name: try keys.decode(Named.self, forKey: .face).name)
                case nil: .server(try Request(from: decoder))
            }
    }

    func perform(definitions: ScriptingDefinitions) async throws -> Response {
        switch self {
            case .server(let request):
                try await request.perform(
                    admitting: { NSPredicate(format: "%K BEGINSWITH %@", kMDItemCFBundleIdentifier as String, adobePrefix) },
                    refusing: { installation in Set(activatingCommands.first { stem, _ in installation.identifier.hasPrefix(stem) }?.value.compactMap(UInt64.init(sdef:)) ?? []) },
                    definitions: definitions,
                )
            case .faces: .value(FontFace.available())
            case .face(let name): try .value(FontFace(named: name))
        }
    }
}
