public import Foundation
public import System

// --- [OPERATIONS] ----------------------------------------------------------------------

public func ifPresent<Value, Failure: Error>(_ read: () throws(Failure) -> Value) throws(Failure) -> Value? {
    do {
        return try read()
    } catch {
        return switch error as any Error {
            case CocoaError.fileNoSuchFile, CocoaError.fileReadNoSuchFile, .noSuchFileOrDirectory as Errno: nil
            default: throw error
        }
    }
}

func ifPresent<Value>(_ file: URL, _ operation: () throws -> Value) throws(FileFailure) -> Value? {
    do { return try ifPresent(operation) } catch { throw FileFailure(file: file, reason: .filesystem(error)) }
}

extension URL {
    public func canonical() throws(Errno) -> URL {
        guard let resolved = unsafe realpath(path(percentEncoded: false), nil) else { throw Errno(rawValue: errno) }
        defer { unsafe free(resolved) }
        return URL(filePath: unsafe String(cString: resolved))
    }
}
