import Foundation

nonisolated struct ClaudePendingSwitch: Codable, Equatable, Sendable {
  enum Phase: String, Codable, Sendable {
    case parking
    case installing
  }

  let incoming: UUID
  let outgoing: UUID?
  var phase: Phase
}

nonisolated struct ClaudeSelectionState: Codable, Sendable {
  var pending: ClaudePendingSwitch?

  static func read(at url: URL) -> Result<ClaudeSelectionState, ClaudeFailure> {
    Result { try Data(contentsOf: url) }.map(Optional.some)
      .flatMapError { error -> Result<Data?, ClaudeFailure> in
        (error as? CocoaError)?.code == .fileReadNoSuchFile
          ? .success(nil) : .failure(.filesystem(error))
      }
      .flatMap { data in
        data.map { data in
          Result { try JSONDecoder().decode(Self.self, from: data) }
            .mapError(ClaudeFailure.filesystem)
        } ?? .success(ClaudeSelectionState())
      }
  }

  func write(to url: URL) -> Result<Void, ClaudeFailure> {
    Result {
      try FileManager.default.createDirectory(
        at: url.deletingLastPathComponent(), withIntermediateDirectories: true)
      try JSONEncoder().encode(self).write(to: url, options: .atomic)
    }
    .mapError(ClaudeFailure.filesystem)
  }
}
