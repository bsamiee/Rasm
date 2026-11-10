public import Darwin
public import System

// --- [MODELS] --------------------------------------------------------------------------

public struct ResourceUsage: Encodable, Equatable, Sendable {
    private enum CodingKeys: CodingKey {
        case cpu, footprint, threads
    }

    public let cpu: Duration
    public let footprint: Int
    public let threads: Int

    public func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: CodingKeys.self)
        try fields.encode(cpu / .seconds(1), forKey: .cpu)
        try fields.encode(footprint, forKey: .footprint)
        try fields.encode(threads, forKey: .threads)
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension ResourceUsage {
    public init(of process: pid_t) throws(Errno) {
        var usage = rusage_info_v6()
        let read = withUnsafeMutablePointer(to: &usage) { usage in
            unsafe usage.withMemoryRebound(to: rusage_info_t?.self, capacity: 1) { buffer in unsafe proc_pid_rusage(process, RUSAGE_INFO_V6, buffer) }
        }
        guard read == 0 else { throw Errno(rawValue: errno) }
        var timebase = mach_timebase_info_data_t()
        _ = unsafe mach_timebase_info(&timebase)
        self.init(
            cpu: .nanoseconds((usage.ri_user_time + usage.ri_system_time) * UInt64(timebase.numer) / UInt64(timebase.denom)),
            footprint: Int(usage.ri_phys_footprint),
            threads: Int(try information(of: process, PROC_PIDTASKINFO, proc_taskinfo()).pti_threadnum),
        )
    }
}
