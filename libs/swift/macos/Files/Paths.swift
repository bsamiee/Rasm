public import Foundation
public import System

// --- [OPERATIONS] ----------------------------------------------------------------------

public func ifPresent<Value>(_ operation: () throws -> Value) -> Result<Value?, any Error> {
    Result { try operation() }.map(Optional.some).flatMapError { error in
        switch error {
            case CocoaError.fileNoSuchFile, CocoaError.fileReadNoSuchFile, .noSuchFileOrDirectory as Errno: .success(nil)
            default: .failure(error)
        }
    }
}

extension URL {
    public func canonical() throws(Errno) -> URL {
        guard let resolved: UnsafeMutablePointer<CChar> = unsafe realpath(path(percentEncoded: false), nil) else { throw Errno(rawValue: errno) }
        defer { unsafe free(resolved) }
        return URL(filePath: unsafe String(cString: resolved))
    }
}
