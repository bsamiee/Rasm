public import Darwin
import Foundation
import Security
import Subprocess
public import System

// --- [MODELS] --------------------------------------------------------------------------

public struct ProcessInstance: Codable, Hashable, Sendable {
    public let pid: pid_t
    public let started: UInt64
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
    case throttled
    case uninterruptible
    case sleeping
    case stopped
}

public struct MainThread: Sendable {
    public let state: ProcessStatus
    public let time: Duration
    public let at: SuspendingClock.Instant
}

// --- [OPERATIONS] ----------------------------------------------------------------------

func information<Info: BitwiseCopyable>(of process: pid_t, _ flavor: Int32, _ empty: Info, argument: UInt64 = 0) throws(Errno) -> Info {
    var info = empty
    guard unsafe proc_pidinfo(process, flavor, argument, &info, Int32(MemoryLayout<Info>.size)) > 0 else { throw Errno(rawValue: errno) }
    return info
}

extension ProcessInstance {
    // --- [IDENTITY]

    public init(of pid: pid_t) throws(Errno) {
        var name: [Int32] = [CTL_KERN, KERN_PROC, KERN_PROC_PID, pid]
        var process = unsafe kinfo_proc()
        var size = unsafe MemoryLayout<kinfo_proc>.stride
        guard unsafe sysctl(&name, u_int(name.count), &process, &size, nil, 0) == 0 else { throw Errno(rawValue: errno) }
        let (status, start) = unsafe (process.kp_proc.p_stat, process.kp_proc.p_starttime)
        guard size > 0, Int32(status) != SZOMB else { throw .noSuchProcess }
        self.init(pid: pid, started: UInt64(start.tv_sec) * USEC_PER_SEC + UInt64(start.tv_usec))
    }

    public var hasExited: Bool { Result(catching: { () throws(Errno) in try Self(of: pid) }) != .success(self) }

    public static func all(executing matching: (FilePath) -> Bool) throws(Errno) -> [Self] {
        let capacity = proc_listallpids(nil, 0)
        guard capacity > 0 else { throw Errno(rawValue: errno) }
        let listed = unsafe [pid_t](unsafeUninitializedCapacity: Int(capacity)) { buffer, count in
            count = Int(max(unsafe proc_listallpids(buffer.baseAddress, Int32(buffer.count * MemoryLayout<pid_t>.size)), 0))
        }
        guard !listed.isEmpty else { throw Errno(rawValue: errno) }
        return
            try listed
            .map { pid throws(Errno) -> Self? in
                do throws(Errno) {
                    let instance = try Self(of: pid)
                    return matching(try instance.executable()) ? instance : nil
                } catch  where [.noSuchProcess, .noSuchFileOrDirectory].contains(error) {
                    return nil
                }
            }
            .compactMap(\.self)
    }

    public func executable() throws(Errno) -> FilePath {
        var path = [CChar](repeating: 0, count: Int(MAXPATHLEN) * 4)
        let read = unsafe proc_pidpath(pid, &path, UInt32(path.count))
        let failure = Errno(rawValue: errno)
        guard !hasExited else { throw .noSuchProcess }
        guard read > 0 else { throw failure }
        return FilePath(platformString: path)
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

    public func signal(_ signal: Int32) throws(Errno) {
        var token = try auditToken()
        let status = unsafe proc_signal_with_audittoken(&token, signal)
        guard status == 0 else { throw Errno(rawValue: status) }
    }

    public func listeningPorts() throws(Errno) -> [in_port_t] {
        let capacity = proc_pidinfo(pid, PROC_PIDLISTFDS, 0, nil, 0)
        guard capacity > 0 else { throw Errno(rawValue: errno) }
        let descriptors = unsafe [proc_fdinfo](unsafeUninitializedCapacity: Int(capacity) / MemoryLayout<proc_fdinfo>.stride) { buffer, count in
            count = Int(max(unsafe proc_pidinfo(pid, PROC_PIDLISTFDS, 0, buffer.baseAddress, Int32(buffer.count * MemoryLayout<proc_fdinfo>.stride)), 0)) / MemoryLayout<proc_fdinfo>.stride
        }
        let ports = descriptors.filter { descriptor in descriptor.proc_fdtype == PROX_FDTYPE_SOCKET }.compactMap { descriptor -> in_port_t? in
            var socket = socket_fdinfo()
            let size = Int32(MemoryLayout<socket_fdinfo>.size)
            let read = unsafe proc_pidfdinfo(pid, descriptor.proc_fd, PROC_PIDFDSOCKETINFO, &socket, size) == size
            let tcp = unsafe socket.psi.soi_proto.pri_tcp
            return read && socket.psi.soi_kind == SOCKINFO_TCP && tcp.tcpsi_state == TSI_S_LISTEN ? in_port_t(bigEndian: in_port_t(truncatingIfNeeded: tcp.tcpsi_ini.insi_lport)) : nil
        }
        guard !hasExited else { throw .noSuchProcess }
        return Set(ports).sorted()
    }

    public func runsInstalledCode() throws(Errno) -> Bool {
        let token = try auditToken()
        var code: SecCode?
        let attributes = [kSecGuestAttributeAudit: withUnsafeBytes(of: token) { bytes in unsafe Data(bytes) }] as CFDictionary
        let found = unsafe SecCodeCopyGuestWithAttributes(nil, attributes, [], &code)
        let validity = code.map { code in SecCodeCheckValidity(code, [], nil) } ?? found
        guard !hasExited else { throw .noSuchProcess }
        return validity == errSecSuccess
    }

    // --- [PROFILE]

    public func sample(for duration: Duration, within timeout: Duration) async throws(ProcessFailure) -> String {
        guard !hasExited else { throw ProcessFailure(subject: rawValue, reason: .system(.noSuchProcess)) }
        return try await Configuration(executable: .name("sample"), arguments: [String(pid), String(Int((duration / .seconds(1)).rounded(.up))), "-mayDie", "-file", "/dev/stdout"])
            .run(output: .string(limit: .max), within: timeout)
    }
}

extension MainThread {
    public init(of instance: ProcessInstance) throws(Errno) {
        let process = instance.pid
        let read = Result { () throws(Errno) in
            (
                process: try information(of: process, PROC_PIDTBSDINFO, proc_bsdinfo()),
                thread: try information(of: process, PROC_PIDTHREADINFO, proc_threadinfo(), argument: information(of: process, PROC_PIDLISTTHREADS, UInt64.zero)),
            )
        }
        guard !instance.hasExited else { throw .noSuchProcess }
        let (bsd, thread) = try read.get()
        let starved: Bool = bsd.pbi_flags & UInt32(PROC_FLAG_PA_THROTTLE | PROC_FLAG_PA_SUSP) != 0
        let state: ProcessStatus =
            switch (Int32(bsd.pbi_status), starved, thread.pth_run_state) {
                case (SSTOP, _, _), (_, false, TH_STATE_STOPPED): .stopped
                case (_, true, _): .throttled
                case (_, false, TH_STATE_UNINTERRUPTIBLE): .uninterruptible
                case (_, false, TH_STATE_RUNNING): .running
                case (_, false, _): .sleeping
            }
        self.init(state: state, time: .nanoseconds(thread.pth_user_time + thread.pth_system_time), at: .now)
    }

    public func status(since earlier: Self) -> ProcessStatus {
        switch (state, time - earlier.time) {
            case (.running, let ran) where ran * 2 < earlier.at.duration(to: at): .runnable
            case (.sleeping, let ran) where ran > .zero: .running
            case (let state, _): state
        }
    }
}
