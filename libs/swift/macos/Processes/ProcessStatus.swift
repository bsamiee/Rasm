public import Darwin
public import System

// --- [MODELS] --------------------------------------------------------------------------

public enum ProcessStatus: String, Encodable, Sendable {
    case running
    case sleeping
    case stopped
}

// --- [OPERATIONS] ----------------------------------------------------------------------

func information<Info: BitwiseCopyable>(of process: pid_t, _ flavor: Int32, _ empty: Info) throws(Errno) -> Info {
    var info: Info = empty
    guard unsafe proc_pidinfo(process, flavor, 0, &info, Int32(MemoryLayout<Info>.size)) > 0 else { throw Errno(rawValue: errno) }
    return info
}

extension ProcessStatus {
    public init(of process: pid_t) throws(Errno) {
        switch Int32(try information(of: process, PROC_PIDTBSDINFO, proc_bsdinfo()).pbi_status) {
            case SSTOP: self = .stopped
            case SZOMB: throw .noSuchProcess
            default: self = try information(of: process, PROC_PIDTASKINFO, proc_taskinfo()).pti_numrunning > 0 ? .running : .sleeping
        }
    }
}
