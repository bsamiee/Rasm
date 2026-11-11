public import Foundation
import Synchronization
import System

// --- [SERVICES] ------------------------------------------------------------------------

public struct RotatingFile: Sendable {
    private final class Held: Sendable {
        let state: Mutex<(descriptor: FileDescriptor?, failure: FileFailure?)> = .init((nil, nil))
    }

    public let url: URL
    public var size: Int = 8 << 20
    public var age: Duration = .seconds(86_400)
    public var files: Int = 7
    private let held: Held = .init()

    public init(_ url: URL) { self.url = url }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension RotatingFile {
    public var urls: [URL] { stride(from: files, through: 0, by: -1).map(file) }

    public var failure: FileFailure? { held.state.withLock { held in held.failure } }

    private func file(_ index: Int) -> URL { index == 0 ? url : url.appendingPathExtension(String(index)) }

    private func open(_ path: FilePath, _ options: FileDescriptor.OpenOptions = []) throws -> FileDescriptor {
        try FileManager.default.createDirectory(at: url.deletingLastPathComponent(), withIntermediateDirectories: true, attributes: [.posixPermissions: 0o700])
        return try FileDescriptor.open(path, .writeOnly, options: options.union([.append, .create]), permissions: .ownerReadWrite)
    }

    private func spent(_ stat: Stat, before expiry: Duration) -> Bool { stat.size >= size || Duration(stat.st_birthtim) <= expiry }

    private func rotate(_ path: FilePath, before expiry: Duration) throws {
        let locked = try open(path, .exclusiveLock)
        try locked.closeAfter {
            guard try spent(locked.stat(), before: expiry), try ifPresent({ () throws(Errno) in try path.stat().inode }) == locked.stat().inode else { return }
            for index in stride(from: files, to: 0, by: -1) {
                let (source, target) = (file(index - 1).path(percentEncoded: false), file(index))
                if unsafe rename(source, target.path(percentEncoded: false)) != 0, errno != ENOENT { throw Errno(rawValue: errno) }
                let modified = try ifPresent { () throws(Errno) in try FilePath(target.path(percentEncoded: false)).stat().st_mtim }
                if let modified, Duration(modified) <= expiry { try FileManager.default.removeItem(at: target) }
            }
            if files <= 0 { try FileManager.default.removeItem(at: url) }
        }
    }

    public func append(_ data: Data, at now: Date = .now) throws(FileFailure) {
        let path = FilePath(url.path(percentEncoded: false))
        let expiry: Duration = .seconds(now.timeIntervalSince1970) - age
        try held.state.withLock { held throws(FileFailure) in
            do {
                let inode = try ifPresent { () throws(Errno) in try path.stat().inode }
                if let stale = held.descriptor, try stale.stat().inode != inode {
                    held.descriptor = nil
                    try stale.close()
                }
                let opened = try held.descriptor ?? open(path)
                let full = try spent(opened.stat(), before: expiry)
                if full {
                    held.descriptor = nil
                    try opened.close()
                    try rotate(path, before: expiry)
                }
                let current = full ? try open(path) : opened
                held.descriptor = current
                try current.writeAll(data)
                held.failure = nil
            } catch {
                let failure = FileFailure(file: url, reason: .filesystem(error))
                held.failure = failure
                throw failure
            }
        }
    }
}
