import Foundation
public import Subprocess
public import System

// --- [MODELS] --------------------------------------------------------------------------

public struct ExitDetail: OptionSet, Sendable {
    public static let decryptFail: Self = .init(rawValue: NOTE_EXIT_DECRYPTFAIL)
    public static let memory: Self = .init(rawValue: NOTE_EXIT_MEMORY)
    public static let csError: Self = .init(rawValue: NOTE_EXIT_CSERROR)

    public let rawValue: Int32

    public init(rawValue: Int32) { self.rawValue = rawValue }
}

extension ExitDetail: Encodable {
    public func encode(to encoder: any Encoder) throws {
        var names = encoder.singleValueContainer()
        try names.encode([(Self.memory, "memory"), (Self.csError, "codeSigning"), (Self.decryptFail, "decryption")].filter { option, _ in contains(option) }.map(\.1))
    }
}

public struct ProcessExit: Equatable, Sendable {
    private static let reportingSignals: Set<TerminationStatus> = Set([SIGQUIT, SIGILL, SIGTRAP, SIGABRT, SIGEMT, SIGFPE, SIGBUS, SIGSEGV, SIGSYS, SIGXCPU, SIGXFSZ].map(TerminationStatus.signaled))

    public let status: TerminationStatus
    public let detail: ExitDetail

    public var reports: Bool { detail.contains(.csError) || Self.reportingSignals.contains(status) }
}

extension TerminationStatus {
    init(waitStatus status: Int32) {
        self = status & _WSTOPPED == 0 ? .exited(status >> 8 & 0xff) : .signaled(status & _WSTOPPED)
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension ProcessInstance {
    public func termination() -> AsyncStream<Result<ProcessExit, Errno>> {
        AsyncStream { continuation in
            let registered = Result { () throws(Errno) in
                guard !hasExited else { throw .noSuchProcess }
                let descriptor = kqueue()
                guard descriptor >= 0 else { throw Errno(rawValue: errno) }
                let queue = FileHandle(fileDescriptor: descriptor, closeOnDealloc: true)
                var exit = unsafe kevent(
                    ident: UInt(pid),
                    filter: Int16(EVFILT_PROC),
                    flags: UInt16(EV_ADD | EV_ONESHOT),
                    fflags: NOTE_EXIT | UInt32(NOTE_EXITSTATUS) | UInt32(NOTE_EXIT_DETAIL),
                    data: 0,
                    udata: nil,
                )
                guard unsafe kevent(descriptor, &exit, 1, nil, 0, nil) == 0 else { throw Errno(rawValue: errno) }
                return queue
            }
            @Sendable func drained(_ descriptor: Int32) -> Result<ProcessExit, Errno>? {
                var exited = unsafe kevent()
                var now = timespec()
                let read = unsafe kevent(descriptor, nil, 0, &exited, 1, &now)
                guard read != 0 else { return nil }
                let data = Int32(truncatingIfNeeded: unsafe exited.data)
                return read < 0 ? .failure(Errno(rawValue: errno)) : .success(ProcessExit(status: TerminationStatus(waitStatus: data), detail: ExitDetail(rawValue: data & NOTE_EXIT_DETAIL_MASK)))
            }
            switch registered {
                case .success(let queue) where hasExited:
                    continuation.yield(drained(queue.fileDescriptor) ?? .failure(.noSuchProcess))
                    continuation.finish()
                case .success(let queue):
                    queue.readabilityHandler = { queue in
                        guard let outcome = drained(queue.fileDescriptor) else { return }
                        continuation.yield(outcome)
                        continuation.finish()
                    }
                    continuation.onTermination = { _ in queue.readabilityHandler = nil }
                case .failure(let error):
                    continuation.yield(.failure(error))
                    continuation.finish()
            }
        }
    }
}
