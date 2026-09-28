import Foundation

// --- [MODELS] --------------------------------------------------------------------------

nonisolated struct ClaudePendingSwitch: Codable, Equatable, Sendable {
    let incoming: UUID
    let outgoing: UUID?
}

nonisolated struct ClaudeSelectionState: Codable, Sendable {
    let pending: ClaudePendingSwitch?

    static func read(at url: URL) -> Result<Self, ClaudeFailure> {
        ifPresent { try Data(contentsOf: url) }
            .flatMap { data in Result { try data.map { data in try JSONDecoder().decode(Self.self, from: data) } } }
            .map { state in state ?? Self(pending: nil) }
            .mapError(ClaudeFailure.filesystem)
    }

    func write(to url: URL) -> Result<Void, ClaudeFailure> {
        Result {
            try FileManager.default.createDirectory(
                at: url.deletingLastPathComponent(),
                withIntermediateDirectories: true,
            )
            try JSONEncoder().encode(self).write(to: url, options: .atomic)
        }
        .mapError(ClaudeFailure.filesystem)
    }
}
