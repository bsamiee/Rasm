public import Foundation
import System
import os

// --- [SERVICES] ------------------------------------------------------------------------

public struct RotatingFile: Sendable {
    public let url: URL
    public var size: UInt = 8 << 20
    public var age: Duration = .seconds(86_400)
    public var backups: UInt = 7
    private let path: FilePath
    private let state = OSAllocatedUnfairLock<(writer: (descriptor: FileDescriptor, inode: Inode)?, failure: FileFailure?)>(initialState: (nil, nil))

    public init(_ url: URL) { (self.url, path) = (url, FilePath(url.path(percentEncoded: false))) }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension RotatingFile {
    public var urls: [URL] { stride(from: backups, through: 0, by: -1).map(file) }

    public var failure: FileFailure? { state.withLock { held in held.failure } }

    private func file(_ index: UInt) -> URL { index == 0 ? url : url.appendingPathExtension(String(index)) }

    private func open(_ options: FileDescriptor.OpenOptions = []) throws -> (descriptor: FileDescriptor, stat: Stat) {
        try FileManager.default.createDirectory(
            atPath: path.removingLastComponent().string,
            withIntermediateDirectories: true,
            attributes: [.posixPermissions: FilePermissions.ownerReadWriteExecute.rawValue],
        )
        let descriptor = try FileDescriptor.open(path, .writeOnly, options: options.union([.append, .create, .closeOnExec]), permissions: .ownerReadWrite)
        do { return (descriptor, try descriptor.stat()) } catch {
            try descriptor.close()
            throw error
        }
    }

    private func needsRotation(_ stat: Stat, adding count: Int, before expiry: Duration) -> Bool {
        stat.size > 0 && (stat.size + Int64(count) > size || Duration(stat.st_birthtim) <= expiry)
    }

    private func rotate(adding count: Int, before expiry: Duration) throws -> (descriptor: FileDescriptor, stat: Stat) {
        let locked = try open(.exclusiveLock)
        try locked.descriptor.closeAfter {
            guard needsRotation(locked.stat, adding: count, before: expiry), try ifPresent({ () throws(Errno) in try path.stat().inode }) == locked.stat.inode else { return }
            for index in stride(from: backups, through: 0, by: -1) {
                let source = file(index).path(percentEncoded: false)
                let kept = try index < backups && ifPresent({ () throws(Errno) in try FilePath(source).stat().st_mtim }).map { modified in Duration(modified) > expiry } == true
                if unsafe (kept ? rename(source, file(index + 1).path(percentEncoded: false)) : unlink(source)) != 0, errno != ENOENT { throw Errno(rawValue: errno) }
            }
        }
        return try open()
    }

    public func append(_ data: Data, at now: Date = .now) throws(FileFailure) {
        let expiry: Duration = .seconds(now.timeIntervalSince1970) - age
        try state.withLock { held -> Result<Void, FileFailure> in
            let outcome = Result { () throws in
                let found = try ifPresent { () throws(Errno) in try path.stat() }
                let opened: (descriptor: FileDescriptor, stat: Stat) =
                    switch (held.writer.take(), found) {
                        case (let writer?, let stat?) where writer.inode == stat.inode: (writer.descriptor, stat)
                        case (let writer?, _): try writer.descriptor.closeAfter { try open() }
                        case (nil, _): try open()
                    }
                let current =
                    if needsRotation(opened.stat, adding: data.count, before: expiry) {
                        try opened.descriptor.closeAfter { try rotate(adding: data.count, before: expiry) }
                    } else {
                        opened
                    }
                held.writer = (current.descriptor, current.stat.inode)
                try current.descriptor.writeAll(data)
            }
            .mapError { error in FileFailure(file: url, reason: .filesystem(error)) }
            held.failure = if case .failure(let failure) = outcome { failure } else { nil }
            return outcome
        }.get()
    }
}
