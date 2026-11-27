import Foundation
package import Subprocess
package import System

// --- [MODELS] --------------------------------------------------------------------------

package struct ExitDetail: OptionSet, Sendable {
    static let decryption = Self(rawValue: NOTE_EXIT_DECRYPTFAIL)
    static let memory = Self(rawValue: NOTE_EXIT_MEMORY)
    static let codeSigning = Self(rawValue: NOTE_EXIT_CSERROR)

    package let rawValue: Int32

    package init(rawValue: Int32) { self.rawValue = rawValue }
}

extension ExitDetail: Encodable {
    package func encode(to encoder: any Encoder) throws {
        var names = encoder.singleValueContainer()
        try names.encode([(Self.memory, "memory"), (Self.codeSigning, "codeSigning"), (Self.decryption, "decryption")].filter { option, _ in contains(option) }.map(\.1))
    }
}

package struct ProcessExit: Equatable, Sendable {
    private static let reportingSignals: Set<TerminationStatus> = Set([SIGQUIT, SIGILL, SIGTRAP, SIGABRT, SIGEMT, SIGFPE, SIGBUS, SIGSEGV, SIGSYS, SIGXCPU, SIGXFSZ].map(TerminationStatus.signaled))

    package let status: TerminationStatus
    package let detail: ExitDetail

    package var producesReport: Bool { detail.contains(.codeSigning) || Self.reportingSignals.contains(status) }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension ProcessInstance {
    package func exits() -> AsyncStream<Result<ProcessExit, Errno>> {
        let (exits, exit) = AsyncStream<Result<ProcessExit, Errno>>.makeStream()
        let deliver: @Sendable (FileHandle) -> Void = { queue in
            var event = unsafe kevent()
            var now = timespec()
            let read = unsafe kevent(queue.fileDescriptor, nil, 0, &event, 1, &now)
            let data = Int32(truncatingIfNeeded: unsafe event.data)
            let status: TerminationStatus = data & _WSTOPPED == 0 ? .exited(data >> 8 & 0xff) : .signaled(data & _WSTOPPED)
            let outcome: Result<ProcessExit, Errno> =
                switch read {
                    case 1: .success(ProcessExit(status: status, detail: ExitDetail(rawValue: data & NOTE_EXIT_DETAIL_MASK)))
                    case 0: .failure(.noSuchProcess)
                    default: .failure(Errno(rawValue: errno))
                }
            exit.yield(outcome)
            exit.finish()
        }
        let registered = Result { () throws(Errno) -> FileHandle in
            let descriptor = kqueue()
            guard descriptor >= 0 else { throw Errno(rawValue: errno) }
            let queue = FileHandle(fileDescriptor: descriptor, closeOnDealloc: true)
            var change = unsafe kevent(
                ident: UInt(pid),
                filter: Int16(EVFILT_PROC),
                flags: UInt16(EV_ADD | EV_ONESHOT),
                fflags: NOTE_EXIT | UInt32(NOTE_EXITSTATUS) | UInt32(NOTE_EXIT_DETAIL),
                data: 0,
                udata: nil,
            )
            guard unsafe kevent(descriptor, &change, 1, nil, 0, nil) == 0 else { throw Errno(rawValue: errno) }
            return queue
        }
        switch registered {
            case .success(let queue) where hasExited: deliver(queue)
            case .success(let queue):
                exit.onTermination = { _ in queue.readabilityHandler = nil }
                queue.readabilityHandler = deliver
            case .failure(let error):
                exit.yield(.failure(error))
                exit.finish()
        }
        return exits
    }
}
