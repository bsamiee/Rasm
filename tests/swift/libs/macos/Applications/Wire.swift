import Encoding
import Foundation

// --- [CONSTANTS] -----------------------------------------------------------------------

let crashReports: URL = URL(filePath: #filePath).deletingLastPathComponent().appending(components: "..", "..", "..", "fixtures", "crash-reports", directoryHint: .isDirectory).standardizedFileURL

// --- [OPERATIONS] ----------------------------------------------------------------------

func wire(_ value: some Encodable) throws -> JSONValue {
    try JSONDecoder().decode(JSONValue.self, from: JSONEncoder().encodeLine(value))
}

func json(_ text: String) throws -> JSONValue {
    try JSONDecoder().decode(JSONValue.self, from: Data(text.utf8))
}
