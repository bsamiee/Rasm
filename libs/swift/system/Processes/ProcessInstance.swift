package import Darwin
package import Foundation
import Security
import Subprocess
package import System

// --- [MODELS] --------------------------------------------------------------------------

package struct ProcessInstance: Codable, Hashable, Sendable {
    package let pid: pid_t
    package let started: UInt64
}

extension ProcessInstance: RawRepresentable, CustomStringConvertible {
    package var rawValue: String { "\(pid)@\(started)" }
    package var description: String { rawValue }

    package init?(rawValue: String) {
        let parts = rawValue.split(separator: "@", omittingEmptySubsequences: false)
        guard parts.count == 2, let pid = pid_t(parts[0]), let started = UInt64(parts[1]) else { return nil }
        self.init(pid: pid, started: started)
    }
}

package enum ProcessStatus: String, Encodable, Sendable {
    case running
    case runnable
    case throttled
    case uninterruptible
    case sleeping
    case stopped
    case traced
}

package struct MainThread: Sendable {
    package let state: ProcessStatus
    let time: Duration
    let at: SuspendingClock.Instant
}

// --- [OPERATIONS] ----------------------------------------------------------------------

func information<Info: BitwiseCopyable>(of process: pid_t, _ flavor: Int32, _ empty: Info, argument: UInt64 = 0) throws(Errno) -> Info {
    var info = empty
    let size = Int32(MemoryLayout<Info>.size)
    guard unsafe proc_pidinfo(process, flavor, argument, &info, size) == size else { throw Errno(rawValue: errno) }
    return info
}

extension ProcessInstance {
    // --- [IDENTITY]

    package init(of pid: pid_t) throws(Errno) {
        var name: [Int32] = [CTL_KERN, KERN_PROC, KERN_PROC_PID, pid]
        var process = unsafe kinfo_proc()
        var size = unsafe MemoryLayout<kinfo_proc>.stride
        guard unsafe sysctl(&name, u_int(name.count), &process, &size, nil, 0) == 0 else { throw Errno(rawValue: errno) }
        let (status, start) = unsafe (process.kp_proc.p_stat, process.kp_proc.p_starttime)
        guard size > 0, Int32(status) != SZOMB else { throw .noSuchProcess }
        self.init(pid: pid, started: UInt64(start.tv_sec) * USEC_PER_SEC + UInt64(start.tv_usec))
    }

    init(_ process: proc_bsdinfo) {
        self.init(pid: pid_t(bitPattern: process.pbi_pid), started: process.pbi_start_tvsec * USEC_PER_SEC + process.pbi_start_tvusec)
    }

    func bsdInfo() throws(Errno) -> proc_bsdinfo {
        let process = try information(of: pid, PROC_PIDTBSDINFO, proc_bsdinfo())
        guard Self(process) == self else { throw .noSuchProcess }
        return process
    }

    package var hasExited: Bool { Result(catching: { () throws(Errno) in try Self(of: pid) }) != .success(self) }

    package static func all(executing matching: (FilePath) -> Bool) throws(Errno) -> [Self] {
        let capacity = proc_listallpids(nil, 0)
        guard capacity > 0 else { throw Errno(rawValue: errno) }
        let listed = unsafe [pid_t](unsafeUninitializedCapacity: Int(capacity)) { buffer, count in
            count = Int(max(unsafe proc_listallpids(buffer.baseAddress, Int32(buffer.count * MemoryLayout<pid_t>.size)), 0))
        }
        guard !listed.isEmpty else { throw Errno(rawValue: errno) }
        var path = [CChar](repeating: 0, count: Int(MAXPATHLEN) * 4)
        return try listed.map { pid throws(Errno) -> Self? in
            guard unsafe proc_pidpath(pid, &path, UInt32(path.count)) > 0, matching(FilePath(platformString: path)) else { return nil }
            do throws(Errno) {
                let instance = try Self(of: pid)
                return matching(try instance.executable()) ? instance : nil
            } catch  where [.noSuchProcess, .noSuchFileOrDirectory].contains(error) {
                return nil
            }
        }
        .compactMap(\.self)
    }

    package func executable() throws(Errno) -> FilePath {
        var path = [CChar](repeating: 0, count: Int(MAXPATHLEN) * 4)
        let read = unsafe proc_pidpath(pid, &path, UInt32(path.count))
        let failure = Errno(rawValue: errno)
        guard !hasExited else { throw .noSuchProcess }
        guard read > 0 else { throw failure }
        return FilePath(platformString: path)
    }

    // --- [KERNEL_TOKEN]

    private func auditToken() throws(Errno) -> audit_token_t {
        var name = mach_port_name_t()
        guard unsafe task_name_for_pid(mach_task_self_, pid, &name) == KERN_SUCCESS else {
            _ = try bsdInfo()
            throw .notPermitted
        }
        let port = Mach.Port<Mach.SendRight>(name: name)
        var token = audit_token_t()
        var count = mach_msg_type_number_t(MemoryLayout<audit_token_t>.size / MemoryLayout<natural_t>.size)
        let read = port.withBorrowedName { name in
            withUnsafeMutablePointer(to: &token) { token in
                unsafe token.withMemoryRebound(to: integer_t.self, capacity: Int(count)) { words in unsafe task_info(name, task_flavor_t(TASK_AUDIT_TOKEN), words, &count) }
            }
        }
        _ = try bsdInfo()
        guard read == KERN_SUCCESS else { throw .notPermitted }
        return token
    }

    package func signal(_ signal: Int32) throws(Errno) {
        var token = try auditToken()
        let status = unsafe proc_signal_with_audittoken(&token, signal)
        guard status == 0 else { throw Errno(rawValue: status) }
    }

    package func listeningPorts() throws(Errno) -> [in_port_t] {
        let capacity = proc_pidinfo(pid, PROC_PIDLISTFDS, 0, nil, 0)
        guard capacity > 0 else { throw Errno(rawValue: errno) }
        let descriptors = unsafe [proc_fdinfo](unsafeUninitializedCapacity: Int(capacity) / MemoryLayout<proc_fdinfo>.stride) { buffer, count in
            count = Int(max(unsafe proc_pidinfo(pid, PROC_PIDLISTFDS, 0, buffer.baseAddress, Int32(buffer.count * MemoryLayout<proc_fdinfo>.stride)), 0)) / MemoryLayout<proc_fdinfo>.stride
        }
        let ports = descriptors.compactMap { descriptor -> in_port_t? in
            var socket = socket_fdinfo()
            let size = Int32(MemoryLayout<socket_fdinfo>.size)
            guard descriptor.proc_fdtype == PROX_FDTYPE_SOCKET, unsafe proc_pidfdinfo(pid, descriptor.proc_fd, PROC_PIDFDSOCKETINFO, &socket, size) == size else { return nil }
            let tcp = unsafe socket.psi.soi_proto.pri_tcp
            return socket.psi.soi_kind == SOCKINFO_TCP && tcp.tcpsi_state == TSI_S_LISTEN ? in_port_t(bigEndian: in_port_t(truncatingIfNeeded: tcp.tcpsi_ini.insi_lport)) : nil
        }
        _ = try bsdInfo()
        return Set(ports).sorted()
    }

    package func runsInstalledCode() throws(ProcessFailure) -> Bool {
        let subject = "Code signature of process \(rawValue)"
        var code: SecCode?
        let validity: OSStatus
        do throws(Errno) {
            let guest: CFDictionary? = try pid == getpid() ? nil : [kSecGuestAttributeAudit: withUnsafeBytes(of: auditToken()) { bytes in unsafe Data(bytes) }] as CFDictionary
            let found: OSStatus = if let guest { unsafe SecCodeCopyGuestWithAttributes(nil, guest, [], &code) } else { unsafe SecCodeCopySelf([], &code) }
            validity = code.map { code in SecCodeCheckValidity(code, [], nil) } ?? found
            _ = try bsdInfo()
        } catch {
            throw ProcessFailure(subject: subject, reason: .system(error))
        }
        switch validity {
            case errSecSuccess: return true
            case errSecCSStaticCodeChanged: return false
            default:
                let message: [String: Any] = SecCopyErrorMessageString(validity, nil).map { message in [NSLocalizedDescriptionKey: message as String] } ?? [:]
                throw ProcessFailure(subject: subject, reason: .foundation(NSError(domain: NSOSStatusErrorDomain, code: Int(validity), userInfo: message)))
        }
    }

    // --- [PROFILE]

    private var reports: URL { URL.temporaryDirectory.appending(components: "sample", rawValue, directoryHint: .isDirectory) }

    package func samples(since earliest: Date) throws(ProcessFailure) -> [URL] {
        let written: [(report: URL, at: Date)]
        do {
            written = try FileManager.default.contentsOfDirectory(at: reports, includingPropertiesForKeys: [.contentModificationDateKey]).map { (report: URL) throws -> (report: URL, at: Date) in
                (report, try report.resourceValues(forKeys: [.contentModificationDateKey]).contentModificationDate ?? .distantPast)
            }
        } catch CocoaError.fileReadNoSuchFile {
            return []
        } catch {
            throw ProcessFailure(subject: reports.path(percentEncoded: false), error)
        }
        return written.filter { sample in sample.at >= earliest }.sorted { first, second in first.at > second.at }.map(\.report)
    }

    package func sample(for duration: Duration) async throws(ProcessFailure) -> URL {
        guard !hasExited else { throw ProcessFailure(subject: "sample", reason: .system(.noSuchProcess)) }
        let right = "system.privilege.taskport"
        let granted: OSStatus = right.withCString { name in
            var item = unsafe AuthorizationItem(name: name, valueLength: 0, value: nil, flags: 0)
            return withUnsafeMutablePointer(to: &item) { item in
                var rights = unsafe AuthorizationRights(count: 1, items: item)
                return unsafe AuthorizationCreate(&rights, nil, [.extendRights, .destroyRights], nil)
            }
        }
        guard granted == errAuthorizationSuccess else { throw ProcessFailure(subject: "sample", reason: .authorization(right: right, ProcessFailure.Status(security: granted))) }
        let report: URL = reports.appending(component: "\(Date.now.formatted(.iso8601)).sample.txt")
        do { try FileManager.default.createDirectory(at: reports, withIntermediateDirectories: true) } catch { throw ProcessFailure(subject: reports.path(percentEncoded: false), error) }
        try await Configuration(executable: .name("sample"), arguments: [String(pid), String(Int((duration / .seconds(1)).rounded(.up))), "-mayDie", "-file", report.path(percentEncoded: false)])
            .run(input: .none, output: .discarded)
        return report
    }
}

extension MainThread {
    package init(of instance: ProcessInstance) throws(Errno) {
        let thread = Result { () throws(Errno) in
            try information(of: instance.pid, PROC_PIDTHREADINFO, proc_threadinfo(), argument: information(of: instance.pid, PROC_PIDLISTTHREADS, UInt64.zero))
        }
        let process = try instance.bsdInfo()
        self.init(process, try thread.get(), at: .now)
    }

    init(_ process: proc_bsdinfo, _ thread: proc_threadinfo, at instant: SuspendingClock.Instant) {
        let traced: Bool = process.pbi_flags & UInt32(PROC_FLAG_TRACED) != 0
        let starved: Bool = process.pbi_flags & UInt32(PROC_FLAG_PA_THROTTLE | PROC_FLAG_PA_SUSP) != 0
        let state: ProcessStatus =
            switch (Int32(process.pbi_status), traced, starved, thread.pth_run_state) {
                case (SSTOP, true, _, _), (_, true, false, TH_STATE_STOPPED): .traced
                case (SSTOP, false, _, _), (_, false, false, TH_STATE_STOPPED): .stopped
                case (_, _, true, _): .throttled
                case (_, _, false, TH_STATE_UNINTERRUPTIBLE): .uninterruptible
                case (_, _, false, TH_STATE_RUNNING): .running
                case (_, _, false, _): .sleeping
            }
        self.init(state: state, time: .nanoseconds(thread.pth_user_time + thread.pth_system_time), at: instant)
    }

    package func status(since earlier: Self) -> ProcessStatus {
        switch (state, time - earlier.time) {
            case (.running, let ran) where ran * 2 < earlier.at.duration(to: at): .runnable
            case (.sleeping, let ran) where ran > .zero: .running
            case (let state, _): state
        }
    }
}
