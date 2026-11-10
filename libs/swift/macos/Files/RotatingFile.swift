public import Foundation
import System

// --- [MODELS] --------------------------------------------------------------------------

public struct RotatingFile: Sendable {
    public let url: URL
    public let limit: Int64
    public let kept: UInt

    public init(_ url: URL, limit: Int64, kept: UInt) {
        (self.url, self.limit, self.kept) = (url, limit, kept)
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension RotatingFile {
    public var files: [URL] { stride(from: Int(kept), through: 0, by: -1).map(file) }

    private func file(_ index: Int) -> URL { index == 0 ? url : url.appendingPathExtension(String(index)) }

    public func append(_ data: Data) throws(FileFailure) {
        let path = FilePath(url.path(percentEncoded: false))
        func open(_ options: FileDescriptor.OpenOptions = []) throws -> FileDescriptor {
            try .open(path, .writeOnly, options: options.union([.append, .create]), permissions: [.ownerReadWrite, .groupRead, .otherRead])
        }
        do {
            let current = try open()
            let written = try current.closeAfter {
                guard try current.stat().size < limit else { return false }
                try current.writeAll(data)
                return true
            }
            guard !written else { return }
            let full = try open(.exclusiveLock)
            try full.closeAfter {
                guard try full.stat().size >= limit, try ifPresent({ () throws(Errno) in try path.stat().inode }) == full.stat().inode else { return }
                for index in stride(from: Int(kept), to: 0, by: -1) {
                    _ = try ifPresent { () throws(Errno) in
                        guard unsafe rename(file(index - 1).path(percentEncoded: false), file(index).path(percentEncoded: false)) == 0 else { throw Errno(rawValue: errno) }
                    }
                }
                if kept == 0 { try FileManager.default.removeItem(at: url) }
            }
            let emptied = try open()
            _ = try emptied.closeAfter { try emptied.writeAll(data) }
        } catch {
            throw FileFailure(file: url, reason: .filesystem(error))
        }
    }
}
