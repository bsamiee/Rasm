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

        public let start: ResourceUsage
        public let end: ResourceUsage

        public var duration: Duration { start.at.duration(to: end.at) }
        public var cpu: Duration { end.cpu - start.cpu }
        public var runnable: Duration { end.runnable - start.runnable }
        public var pageWait: Duration { end.pageWait - start.pageWait }
        public var diskBytesRead: UInt64 { end.diskBytesRead - start.diskBytesRead }
        public var diskBytesWritten: UInt64 { end.diskBytesWritten - start.diskBytesWritten }

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
            let values: [(CodingKeys, any Encodable)] = [
                (.cause, cause), (.duration, duration / .seconds(1)), (.cpu, cpu / .seconds(1)), (.runnable, runnable / .seconds(1)), (.pageWait, pageWait / .seconds(1)),
                (.diskBytesRead, diskBytesRead), (.diskBytesWritten, diskBytesWritten), (.end, end),
            ]
            for (key, value) in values { try fields.encode(value, forKey: key) }
        }
    }

    private enum CodingKeys: CodingKey {
        case cpu, runnable, pageWait, diskBytesRead, diskBytesWritten, footprint, threads, runningThreads, assertions, assertionsFailure
    }

    public let cpu: Duration
    public let runnable: Duration
    public let pageWait: Duration
    public let diskBytesRead: UInt64
    public let diskBytesWritten: UInt64
    public let footprint: Int
    public let threads: Int
    public let runningThreads: Int
    public let assertions: Result<[String], ProcessFailure>
    public let at: SuspendingClock.Instant

    public func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: CodingKeys.self)
        let held: (CodingKeys, any Encodable) =
            switch assertions {
                case .success(let names): (.assertions, names)
                case .failure(let error): (.assertionsFailure, error.localizedDescription)
            }
        let values: [(CodingKeys, any Encodable)] = [
            (.cpu, cpu / .seconds(1)), (.runnable, runnable / .seconds(1)), (.pageWait, pageWait / .seconds(1)), (.diskBytesRead, diskBytesRead),
            (.diskBytesWritten, diskBytesWritten), (.footprint, footprint), (.threads, threads), (.runningThreads, runningThreads), held,
        ]
        for (key, value) in values { try fields.encode(value, forKey: key) }
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
        var usage = rusage_info_current()
        let status = withUnsafeMutablePointer(to: &usage) { usage in
            unsafe usage.withMemoryRebound(to: rusage_info_t?.self, capacity: 1) { buffer in unsafe proc_pid_rusage(instance.pid, RUSAGE_INFO_CURRENT, buffer) }
        }
        let failure = Errno(rawValue: errno)
        let task = try information(of: instance.pid, PROC_PIDTASKALLINFO, proc_taskallinfo())
        guard ProcessInstance(task.pbsd) == instance else { throw .noSuchProcess }
        guard status == 0 else { throw failure }
        let elapsed = { (ticks: UInt64) -> Duration in .nanoseconds(ticks) * Self.timebase.numer / Self.timebase.denom }
        var table: Unmanaged<CFDictionary>?
        let read = unsafe IOPMCopyAssertionsByProcess(&table)
        let held = unsafe (table?.takeRetainedValue() as? [NSNumber: AnyObject])?[NSNumber(value: instance.pid)] as? [[String: Any]]
        let names: Set<String> = Set((held ?? []).compactMap { assertion in assertion[kIOPMAssertionNameKey] as? String })
        self.init(
            cpu: elapsed(usage.ri_user_time + usage.ri_system_time),
            runnable: elapsed(usage.ri_runnable_time),
            pageWait: elapsed(usage.ri_page_wait_time_mach),
            diskBytesRead: usage.ri_diskio_bytesread,
            diskBytesWritten: usage.ri_diskio_byteswritten,
            footprint: Int(usage.ri_phys_footprint),
            threads: Int(task.ptinfo.pti_threadnum),
            runningThreads: Int(task.ptinfo.pti_numrunning),
            assertions: read == kIOReturnSuccess ? .success(names.sorted()) : .failure(ProcessFailure(subject: "IOPMCopyAssertionsByProcess", reason: .mach(read))),
            at: .now,
        )
    }

    public func since(_ earlier: Self) -> Interval { Interval(start: earlier, end: self) }
}
