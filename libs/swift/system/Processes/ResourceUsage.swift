import Darwin
public import Foundation
import IOKit.pwr_mgt
public import System

// --- [MODELS] --------------------------------------------------------------------------

public struct ResourceUsage: Encodable, Equatable, Sendable {
    private enum CodingKeys: CodingKey {
        case cpu, footprint, threads, assertions, assertionsFailure
    }

    public let cpu: Duration
    public let footprint: Int
    public let threads: Int
    public let assertions: Result<[String], NSError>

    public func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: CodingKeys.self)
        try fields.encode(cpu / .seconds(1), forKey: .cpu)
        try fields.encode(footprint, forKey: .footprint)
        try fields.encode(threads, forKey: .threads)
        switch assertions {
            case .success(let names): try fields.encode(names, forKey: .assertions)
            case .failure(let error): try fields.encode(error.localizedDescription, forKey: .assertionsFailure)
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension ResourceUsage {
    public init(of instance: ProcessInstance) throws(Errno) {
        let threads = Result { () throws(Errno) in try information(of: instance.pid, PROC_PIDTASKINFO, proc_taskinfo()).pti_threadnum }
        let usage = try usage(of: instance.pid)
        guard usage.ri_proc_start_abstime == instance.started, usage.ri_proc_exit_abstime == 0 else { throw .noSuchProcess }
        var timebase = mach_timebase_info_data_t()
        _ = unsafe mach_timebase_info(&timebase)
        var table: Unmanaged<CFDictionary>?
        let read = unsafe IOPMCopyAssertionsByProcess(&table)
        let held: [[String: Any]] = unsafe (table?.takeRetainedValue() as? [NSNumber: [[String: Any]]])?[NSNumber(value: instance.pid)] ?? []
        let names: Set<String> = Set(held.compactMap { assertion in assertion[kIOPMAssertionNameKey as String] as? String })
        self.init(
            cpu: .nanoseconds((usage.ri_user_time + usage.ri_system_time) * UInt64(timebase.numer) / UInt64(timebase.denom)),
            footprint: Int(usage.ri_phys_footprint),
            threads: Int(try threads.get()),
            assertions: read == kIOReturnSuccess ? .success(names.sorted()) : .failure(NSError(domain: NSMachErrorDomain, code: Int(read))),
        )
    }
}
