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
                self =
                    switch try information(of: process, PROC_PIDTHREADINFO, proc_threadinfo(), argument: main).pth_run_state {
                        case TH_STATE_RUNNING: .running
                        case TH_STATE_STOPPED: .stopped
                        case _: .sleeping
                    }
            case (_, false): self = try information(of: process, PROC_PIDTASKINFO, proc_taskinfo()).pti_numrunning > 0 ? .running : .sleeping
        }
    }
}
