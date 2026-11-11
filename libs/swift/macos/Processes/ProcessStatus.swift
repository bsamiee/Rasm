public import Darwin
public import System

// --- [MODELS] --------------------------------------------------------------------------

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

public func hasExited(_ process: pid_t) -> Bool {
    do throws(Errno) { return Int32(try information(of: process, PROC_PIDTBSDINFO, proc_bsdinfo()).pbi_status) == SZOMB } catch { return error == .noSuchProcess }
}

extension MainThread {
    public init(of process: pid_t) throws(Errno) {
        let state = Int32(try information(of: process, PROC_PIDTBSDINFO, proc_bsdinfo()).pbi_status)
        guard state != SZOMB else { throw .noSuchProcess }
        let thread = try information(of: process, PROC_PIDTHREADINFO, proc_threadinfo(), argument: information(of: process, PROC_PIDLISTTHREADS, UInt64.zero))
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

public func processes(executing path: String) throws(Errno) -> Set<pid_t> {
    let capacity = proc_listallpids(nil, 0)
    guard capacity > 0 else { throw Errno(rawValue: errno) }
    let listed = unsafe [pid_t](unsafeUninitializedCapacity: Int(capacity)) { buffer, count in
        count = Int(max(unsafe proc_listallpids(buffer.baseAddress, Int32(buffer.count * MemoryLayout<pid_t>.size)), 0))
    }
    guard !listed.isEmpty else { throw Errno(rawValue: errno) }
    return Set(
        listed.filter { process in
            var name = [UInt8](repeating: 0, count: Int(MAXPATHLEN) * 4)
            return unsafe proc_pidpath(process, &name, UInt32(name.count)) > 0 && name.prefix { byte in byte != 0 }.elementsEqual(path.utf8)
        }
    )
}
