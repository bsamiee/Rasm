import CustomDump
import Encoding
import Foundation
import Results
import Testing

@testable import Applications

// --- [COMPOSITION] ---------------------------------------------------------------------

struct FontFaces {
    @Test
    func everyAvailableFaceResolvesByItsPostscriptName() throws {
        let faces: [FontFace] = FontFace.available()
        let byName: [String: [FontFace]] = Dictionary(grouping: faces, by: \.postscriptName)
        let unresolved: [String] = byName.compactMap { name, sharing in
            let resolved: Result<FontFace, Unmatched> = Result { () throws(Unmatched) in try FontFace(named: name) }
            return switch resolved {
                case .success(let face) where sharing.contains(face): nil
                case .success, .failure: name
            }
        }
        expectNoDifference(unresolved.sorted(), [])
        expectNoDifference(faces.filter { face in !face.file.isFileURL || !FileManager.default.fileExists(atPath: face.file.path(percentEncoded: false)) }.map(\.postscriptName), [])
    }

    @Test
    func faceEncodesWireFaceShapeAndRoundTrips() throws {
        let face: FontFace = FontFace(postscriptName: "Rasm-Bold", family: "Rasm", style: "Bold", displayName: "Rasm Bold", file: URL(filePath: "/Library/Fonts/Rasm.otf"))
        let expected: JSONValue = try json(#"{"postscriptName":"Rasm-Bold","family":"Rasm","style":"Bold","displayName":"Rasm Bold","file":"file:///Library/Fonts/Rasm.otf"}"#)
        expectNoDifference(try wire(face), expected)
        expectNoDifference(try JSONDecoder().decode(FontFace.self, from: JSONEncoder().encodeLine(face)), face)
    }

    @Test(arguments: ["Rasm-Absent-Face", "Helvetica-Boldest"])
    func unknownNameIsUnmatched(name: String) throws {
        let miss: Unmatched = try #require(throws: Unmatched.self) { try FontFace(named: name) }
        expectNoDifference(miss.query, name)
    }
}
