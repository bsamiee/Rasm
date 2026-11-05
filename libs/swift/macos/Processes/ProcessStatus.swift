public import Darwin
public import System

// --- [MODELS] --------------------------------------------------------------------------

public enum ProcessStatus: String, Encodable, Sendable {
    case running
    case sleeping
    case stopped
}

// --- [OPERATIONS] ----------------------------------------------------------------------

func information<Info: BitwiseCopyable>(of process: pid_t, _ flavor: Int32, _ empty: Info, argument: UInt64 = 0) throws(Errno) -> Info {
    var info: Info = empty
    guard unsafe proc_pidinfo(process, flavor, argument, &info, Int32(MemoryLayout<Info>.size)) > 0 else { throw Errno(rawValue: errno) }
    return info
}

extension ProcessStatus {
    public init(of process: pid_t, mainThread: Bool = false) throws(Errno) {
        switch (Int32(try information(of: process, PROC_PIDTBSDINFO, proc_bsdinfo()).pbi_status), mainThread) {
            case (SSTOP, _): self = .stopped
            case (SZOMB, _): throw .noSuchProcess
            case (_, true):
                let main: UInt64 = try information(of: process, PROC_PIDLISTTHREADS, UInt64.zero)
                self = try information(of: process, PROC_PIDTHREADINFO, proc_threadinfo(), argument: main).pth_run_state == TH_STATE_RUNNING ? .running : .sleeping
            case (_, false): self = try information(of: process, PROC_PIDTASKINFO, proc_taskinfo()).pti_numrunning > 0 ? .running : .sleeping
        }
    }
}

public func mainThreadUserTime(of process: pid_t) throws(Errno) -> Duration {
    let main: UInt64 = try information(of: process, PROC_PIDLISTTHREADS, UInt64.zero)
    return .nanoseconds(try information(of: process, PROC_PIDTHREADINFO, proc_threadinfo(), argument: main).pth_user_time)
}

public func processes(executing path: String) -> Set<pid_t> {
    let capacity: Int = Int(proc_listallpids(nil, 0))
    let listed: [pid_t] = unsafe [pid_t](unsafeUninitializedCapacity: capacity) { buffer, count in
        count = Int(unsafe proc_listallpids(buffer.baseAddress, Int32(capacity * MemoryLayout<pid_t>.size)))
    }
    return Set(
        listed.filter { process in
            var name: [UInt8] = [UInt8](repeating: 0, count: Int(MAXPATHLEN) * 4)
            return unsafe proc_pidpath(process, &name, UInt32(name.count)) > 0 && name.prefix { byte in byte != 0 }.elementsEqual(path.utf8)
        }
    )
}
