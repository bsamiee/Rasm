import CoreServices
import CustomDump
import Files
import Foundation
import TestSupport
import Testing

// --- [MODELS] --------------------------------------------------------------------------

private struct Change: Equatable {
    let path: String
    let created: Bool
    let modified: Bool
    let removed: Bool
    let rootChanged: Bool

    init(_ event: FileEvent) {
        path = event.url.path(percentEncoded: false)
        created = event.flags & FSEventStreamEventFlags(kFSEventStreamEventFlagItemCreated) != 0
        modified = event.flags & FSEventStreamEventFlags(kFSEventStreamEventFlagItemModified) != 0
        removed = event.flags & FSEventStreamEventFlags(kFSEventStreamEventFlagItemRemoved) != 0
        rootChanged = event.flags & FSEventStreamEventFlags(kFSEventStreamEventFlagRootChanged) != 0
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

@Suite(.testDirectory, .timeLimit(.minutes(1)))
struct FileEvents {
    @Test
    func reportsWatchedFileAloneWithItsFlags() async throws {
        let directory: URL = try #require(TestDirectory.current)
        let file: URL = directory.appending(path: "watched.json")
        var paths: Set<String> = []
        var steps: [String] = []
        for await batch: Result<[FileEvent], FileFailure> in FileEvent.events(of: file) {
            let reported: [Change] = try batch.get().map(Change.init)
            paths.formUnion(reported.map(\.path))
            let current: [Change] = reported.filter { change in change.path == file.path(percentEncoded: false) }
            let appeared: Bool = current.contains { change in change.created || change.rootChanged }
            let vanished: Bool = current.contains { change in change.removed || change.rootChanged }
            switch (steps.count, appeared, current.contains(where: \.modified), vanished) {
                case (0, _, _, _):
                    steps.append("watching")
                    try Data("{}".utf8).write(to: directory.appending(path: "sibling.json"))
                    try Data("{}".utf8).write(to: file)
                case (1, true, _, _):
                    steps.append("created")
                    try Data("{\"a\":1}".utf8).write(to: file)
                case (2, _, true, _):
                    steps.append("modified")
                    try FileManager.default.removeItem(at: file)
                case (3, _, _, true): steps.append("removed")
                default: break
            }
            guard steps.count < 4 else { break }
        }
        expectNoDifference(steps, ["watching", "created", "modified", "removed"])
        expectNoDifference(paths, [file.path(percentEncoded: false)])
    }

    @Test
    func presenceStartsFromCurrentStateAndFollowsCreationAndRemoval() async throws {
        let file: URL = try #require(TestDirectory.current).appending(path: "presence.json")
        var observed: [Bool] = []
        for await presence: Result<Bool, FileFailure> in FileEvent.presence(of: file) {
            let present: Bool = try presence.get()
            observed.append(present)
            switch (observed.count, present) {
                case (1, false): try Data("{}".utf8).write(to: file)
                case (2, true):
                    try Data("{\"a\":1}".utf8).write(to: file)
                    try FileManager.default.removeItem(at: file)
                default: break
            }
            guard observed.count < 3 else { break }
        }
        expectNoDifference(observed, [false, true, false])
    }

    @Test
    func presenceOfExistingFileStartsTrue() async throws {
        let file: URL = try #require(TestDirectory.current).appending(path: "present.json")
        try Data("{}".utf8).write(to: file)
        var observed: [Bool] = []
        for await presence: Result<Bool, FileFailure> in FileEvent.presence(of: file) {
            observed.append(try presence.get())
            switch observed.count {
                case 1: try FileManager.default.removeItem(at: file)
                default: break
            }
            guard observed.count < 2 else { break }
        }
        expectNoDifference(observed, [true, false])
    }
}
