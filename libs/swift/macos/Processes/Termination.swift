public import Foundation
public import Subprocess
public import System

// --- [MODELS] --------------------------------------------------------------------------

public struct ExitDetail: OptionSet, Sendable {
    public static let decryptFail: Self = Self(rawValue: NOTE_EXIT_DECRYPTFAIL)
    public static let memory: Self = Self(rawValue: NOTE_EXIT_MEMORY)
    public static let csError: Self = Self(rawValue: NOTE_EXIT_CSERROR)

    public let rawValue: Int32

    public init(rawValue: Int32) { self.rawValue = rawValue }
}

public struct ProcessExit: Equatable, Sendable {
    public let status: TerminationStatus
    public let detail: ExitDetail
}

extension TerminationStatus {
    init(waitStatus status: Int32) {
        self = status & _WSTOPPED == 0 ? .exited(status >> 8 & 0xff) : .signaled(status & _WSTOPPED)
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public func termination(of process: pid_t) -> AsyncStream<Result<ProcessExit, Errno>> {
    AsyncStream { continuation in
        let registered: Result<FileHandle, Errno> = Result { () throws(Errno) in
            let descriptor: Int32 = kqueue()
            guard descriptor >= 0 else { throw Errno(rawValue: errno) }
            let queue: FileHandle = FileHandle(fileDescriptor: descriptor, closeOnDealloc: true)
            var exit: kevent = unsafe kevent(
                ident: UInt(process),
                filter: Int16(EVFILT_PROC),
                flags: UInt16(EV_ADD | EV_ONESHOT),
                fflags: NOTE_EXIT | UInt32(NOTE_EXITSTATUS) | UInt32(NOTE_EXIT_DETAIL),
                data: 0,
                udata: nil,
            )
            guard unsafe kevent(descriptor, &exit, 1, nil, 0, nil) == 0 else { throw Errno(rawValue: errno) }
            return queue
        }
        switch registered {
            case .success(let queue):
                queue.readabilityHandler = { queue in
                    var exited: kevent = unsafe kevent()
                    var now: timespec = timespec()
                    let read: Int32 = unsafe kevent(queue.fileDescriptor, nil, 0, &exited, 1, &now)
                    guard read != 0 else { return }
                    let data: Int32 = Int32(truncatingIfNeeded: unsafe exited.data)
                    continuation.yield(
                        read < 0 ? .failure(Errno(rawValue: errno)) : .success(ProcessExit(status: TerminationStatus(waitStatus: data), detail: ExitDetail(rawValue: data & NOTE_EXIT_DETAIL_MASK)))
                    )
                    continuation.finish()
                }
                continuation.onTermination = { _ in queue.readabilityHandler = nil }
            case .failure(let error):
                continuation.yield(.failure(error))
                continuation.finish()
        }
    }
}
