public import Darwin
import Foundation
import Security
public import System

// --- [MODELS] --------------------------------------------------------------------------

public struct ProcessInstance: Codable, Hashable, Sendable {
    public let pid: pid_t
    public let started: UInt64

    public init(pid: pid_t, started: UInt64) { (self.pid, self.started) = (pid, started) }
}

extension ProcessInstance: RawRepresentable, CustomStringConvertible {
    public var rawValue: String { "\(pid)@\(started)" }
    public var description: String { rawValue }

    public init?(rawValue: String) {
        let parts = rawValue.split(separator: "@", omittingEmptySubsequences: false)
        guard parts.count == 2, let pid = pid_t(parts[0]), let started = UInt64(parts[1]) else { return nil }
        self.init(pid: pid, started: started)
    }
}

public enum ProcessStatus: String, Encodable, Sendable {
    case running
    case runnable
    case sleeping
    case stopped
}

public struct MainThread: Sendable {
    public let stopped: Bool
    public let runnable: Bool
    public let time: Duration
    public let at: SuspendingClock.Instant
}

// --- [OPERATIONS] ----------------------------------------------------------------------

func information<Info: BitwiseCopyable>(of process: pid_t, _ flavor: Int32, _ empty: Info, argument: UInt64 = 0) throws(Errno) -> Info {
    var info = empty
    guard unsafe proc_pidinfo(process, flavor, argument, &info, Int32(MemoryLayout<Info>.size)) > 0 else { throw Errno(rawValue: errno) }
    return info
}

func usage(of process: pid_t) throws(Errno) -> rusage_info_current {
    var usage = rusage_info_current()
    let read = withUnsafeMutablePointer(to: &usage) { usage in
        unsafe usage.withMemoryRebound(to: rusage_info_t?.self, capacity: 1) { buffer in unsafe proc_pid_rusage(process, RUSAGE_INFO_CURRENT, buffer) }
    }
    guard read == 0 else { throw Errno(rawValue: errno) }
    return usage
}

extension ProcessInstance {
    // --- [IDENTITY]

    public init(of pid: pid_t) throws(Errno) {
        let usage = try usage(of: pid)
        guard usage.ri_proc_exit_abstime == 0 else { throw .noSuchProcess }
        self.init(pid: pid, started: usage.ri_proc_start_abstime)
    }

    public init?(named name: String) throws(Errno) {
        switch (pid_t(name), Self(rawValue: name)) {
            case (.some(let pid), _): try self.init(of: pid)
            case (.none, .some(let named)):
                guard Result(catching: { () throws(Errno) in try Self(of: named.pid) }) == .success(named) else { throw .noSuchProcess }
                self = named
            case (.none, .none): return nil
        }
    }

    public var hasExited: Bool { Result(catching: { () throws(Errno) in try Self(of: pid) }) != .success(self) }

    public static func executing(_ matching: (FilePath) -> Bool) throws(Errno) -> [Self] {
        let capacity = proc_listallpids(nil, 0)
        guard capacity > 0 else { throw Errno(rawValue: errno) }
        let listed = unsafe [pid_t](unsafeUninitializedCapacity: Int(capacity)) { buffer, count in
            count = Int(max(unsafe proc_listallpids(buffer.baseAddress, Int32(buffer.count * MemoryLayout<pid_t>.size)), 0))
        }
        guard !listed.isEmpty else { throw Errno(rawValue: errno) }
        return try listed.filter { pid in
            var path = [CChar](repeating: 0, count: Int(MAXPATHLEN) * 4)
            return unsafe proc_pidpath(pid, &path, UInt32(path.count)) > 0 && matching(FilePath(platformString: path))
        }
        .map { pid throws(Errno) -> Self? in
            do throws(Errno) {
                let instance = try Self(of: pid)
                return matching(try instance.executable()) ? instance : nil
            } catch  where [.noSuchProcess, .notPermitted].contains(error) {
                return nil
            }
        }
        .compactMap(\.self)
    }

    // --- [KERNEL_TOKEN]

    private func auditToken() throws(Errno) -> audit_token_t {
        var name: mach_port_name_t = 0
        guard unsafe task_name_for_pid(mach_task_self_, pid, &name) == KERN_SUCCESS else { throw hasExited ? .noSuchProcess : .notPermitted }
        defer { _ = unsafe mach_port_deallocate(mach_task_self_, name) }
        var token = audit_token_t()
        var count = mach_msg_type_number_t(MemoryLayout<audit_token_t>.size / MemoryLayout<natural_t>.size)
        let read = withUnsafeMutablePointer(to: &token) { token in
            unsafe token.withMemoryRebound(to: integer_t.self, capacity: Int(count)) { words in unsafe task_info(name, task_flavor_t(TASK_AUDIT_TOKEN), words, &count) }
        }
        guard !hasExited else { throw .noSuchProcess }
        guard read == KERN_SUCCESS else { throw .notPermitted }
        return token
    }

    public func executable() throws(Errno) -> FilePath {
        var token = try auditToken()
        var path = [CChar](repeating: 0, count: Int(MAXPATHLEN) * 4)
        guard unsafe proc_pidpath_audittoken(&token, &path, UInt32(path.count)) > 0 else { throw Errno(rawValue: errno) }
        return FilePath(platformString: path)
    }

    public func signal(_ signal: Int32) throws(Errno) {
        var token = try auditToken()
        let status = unsafe proc_signal_with_audittoken(&token, signal)
        guard status == 0 else { throw Errno(rawValue: status) }
    }

    public func runsInstalledCode() throws(Errno) -> Bool {
        let token = try auditToken()
        var code: SecCode?
        let attributes = [kSecGuestAttributeAudit: withUnsafeBytes(of: token) { bytes in unsafe Data(bytes) }] as CFDictionary
        let found = unsafe SecCodeCopyGuestWithAttributes(nil, attributes, [], &code)
        return code.map { code in SecCodeCheckValidity(code, [], nil) } ?? found == errSecSuccess
    }
}

extension MainThread {
    public init(of instance: ProcessInstance) throws(Errno) {
        let process = instance.pid
        let read = Result { () throws(Errno) in
            (
                state: Int32(try information(of: process, PROC_PIDTBSDINFO, proc_bsdinfo()).pbi_status),
                thread: try information(of: process, PROC_PIDTHREADINFO, proc_threadinfo(), argument: information(of: process, PROC_PIDLISTTHREADS, UInt64.zero)),
            )
        }
        guard !instance.hasExited else { throw .noSuchProcess }
        let (state, thread) = try read.get()
        self.init(stopped: state == SSTOP, runnable: thread.pth_run_state == TH_STATE_RUNNING, time: .nanoseconds(thread.pth_user_time + thread.pth_system_time), at: .now)
    }

    public func status(since earlier: Self) -> ProcessStatus {
        switch (stopped, runnable, time - earlier.time) {
            case (true, _, _): .stopped
            case (false, false, .zero): .sleeping
            case (false, true, let ran) where ran * 2 < earlier.at.duration(to: at): .runnable
            case (false, _, _): .running
        }
    }
}
