import Darwin
import Foundation
import IOKit.pwr_mgt
public import System

// --- [MODELS] --------------------------------------------------------------------------

public struct ResourceUsage: Encodable, Equatable, Sendable {
    public struct Interval: Encodable, Sendable {
        public enum Cause: String, Encodable, Sendable {
            case running
            case runnable
            case paging
            case reading
            case writing
            case idle
        }

        private enum CodingKeys: CodingKey {
            case cause, duration, cpu, runnable, pageWait, diskBytesRead, diskBytesWritten, end
        }

        public let duration: Duration
        public let cpu: Duration
        public let runnable: Duration
        public let pageWait: Duration
        public let diskBytesRead: UInt64
        public let diskBytesWritten: UInt64
        public let end: ResourceUsage

        public var cause: Cause {
            let spent: [(cause: Cause, time: Duration)] = [(.running, cpu), (.runnable, runnable), (.paging, pageWait)]
            return switch (spent.max { left, right in left.time < right.time }, diskBytesRead, diskBytesWritten) {
                case (.some(let longest), _, _) where longest.time > .zero && longest.time * 2 >= duration: longest.cause
                case (_, 0, 0): .idle
                case (_, let read, let written): read >= written ? .reading : .writing
            }
        }

        public func encode(to encoder: any Encoder) throws {
            var fields = encoder.container(keyedBy: CodingKeys.self)
            try fields.encode(cause, forKey: .cause)
            for (key, time) in [(CodingKeys.duration, duration), (.cpu, cpu), (.runnable, runnable), (.pageWait, pageWait)] { try fields.encode(time / .seconds(1), forKey: key) }
            try fields.encode(diskBytesRead, forKey: .diskBytesRead)
            try fields.encode(diskBytesWritten, forKey: .diskBytesWritten)
            try fields.encode(end, forKey: .end)
        }
    }

    private enum CodingKeys: CodingKey {
        case cpu, runnable, pageWait, diskBytesRead, diskBytesWritten, footprint, threads, assertions, assertionsFailure
    }

    public let cpu: Duration
    public let runnable: Duration
    public let pageWait: Duration
    public let diskBytesRead: UInt64
    public let diskBytesWritten: UInt64
    public let footprint: Int
    public let threads: Int
    public let assertions: Result<[String], ProcessFailure>
    public let at: SuspendingClock.Instant

    public func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: CodingKeys.self)
        for (key, time) in [(CodingKeys.cpu, cpu), (.runnable, runnable), (.pageWait, pageWait)] { try fields.encode(time / .seconds(1), forKey: key) }
        try fields.encode(diskBytesRead, forKey: .diskBytesRead)
        try fields.encode(diskBytesWritten, forKey: .diskBytesWritten)
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
    private static let timebase: mach_timebase_info_data_t = {
        var timebase = mach_timebase_info_data_t()
        _ = unsafe mach_timebase_info(&timebase)
        return timebase
    }()

    public init(of instance: ProcessInstance) throws(Errno) {
        let threads = Result { () throws(Errno) in try information(of: instance.pid, PROC_PIDTASKINFO, proc_taskinfo()).pti_threadnum }
        var usage = rusage_info_current()
        let status = withUnsafeMutablePointer(to: &usage) { usage in
            unsafe usage.withMemoryRebound(to: rusage_info_t?.self, capacity: 1) { buffer in unsafe proc_pid_rusage(instance.pid, RUSAGE_INFO_CURRENT, buffer) }
        }
        let failure = Errno(rawValue: errno)
        guard !instance.hasExited else { throw .noSuchProcess }
        guard status == 0 else { throw failure }
        let elapsed = { (ticks: UInt64) -> Duration in .nanoseconds(ticks * UInt64(Self.timebase.numer) / UInt64(Self.timebase.denom)) }
        var table: Unmanaged<CFDictionary>?
        let read = unsafe IOPMCopyAssertionsByProcess(&table)
        let held: [[String: Any]] = unsafe (table?.takeRetainedValue() as? [NSNumber: [[String: Any]]])?[NSNumber(value: instance.pid)] ?? []
        let names: Set<String> = Set(held.compactMap { assertion in assertion[kIOPMAssertionNameKey as String] as? String })
        self.init(
            cpu: elapsed(usage.ri_user_time + usage.ri_system_time),
            runnable: elapsed(usage.ri_runnable_time),
            pageWait: elapsed(usage.ri_page_wait_time_mach),
            diskBytesRead: usage.ri_diskio_bytesread,
            diskBytesWritten: usage.ri_diskio_byteswritten,
            footprint: Int(usage.ri_phys_footprint),
            threads: Int(try threads.get()),
            assertions: read == kIOReturnSuccess ? .success(names.sorted()) : .failure(ProcessFailure(subject: "IOPMCopyAssertionsByProcess", reason: .ioKit(read))),
            at: .now,
        )
    }

    public func since(_ earlier: Self) -> Interval {
        Interval(
            duration: earlier.at.duration(to: at),
            cpu: cpu - earlier.cpu,
            runnable: runnable - earlier.runnable,
            pageWait: pageWait - earlier.pageWait,
            diskBytesRead: diskBytesRead - earlier.diskBytesRead,
            diskBytesWritten: diskBytesWritten - earlier.diskBytesWritten,
            end: self,
        )
    }
}
