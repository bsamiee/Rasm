import Darwin
import Foundation
import IOKit.pwr_mgt
import Results
package import System

// --- [MODELS] --------------------------------------------------------------------------

package struct ResourceUsage: Encodable, Equatable, Sendable {
    package struct Interval: Encodable, Equatable, Sendable {
        package enum Cause: String, Encodable, Sendable {
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

        package let start: ResourceUsage
        let end: ResourceUsage

        var duration: Duration { start.at.duration(to: end.at) }
        var cpu: Duration { end.cpu - start.cpu }
        var runnable: Duration { end.runnable - start.runnable }
        var pageWait: Duration { end.pageWait - start.pageWait }
        var diskBytesRead: UInt64 { end.diskBytesRead - start.diskBytesRead }
        var diskBytesWritten: UInt64 { end.diskBytesWritten - start.diskBytesWritten }

        package var cause: Cause {
            let spent: [(cause: Cause, time: Duration)] = [(.running, cpu), (.runnable, runnable), (.paging, pageWait)]
            return switch (spent.max { left, right in left.time < right.time }, diskBytesRead, diskBytesWritten) {
                case (.some(let longest), _, _) where longest.time > .zero && longest.time * 2 >= duration: longest.cause
                case (_, 0, 0): .idle
                case (_, let read, let written): read >= written ? .reading : .writing
            }
        }

        package func encode(to encoder: any Encoder) throws {
            var fields = encoder.container(keyedBy: CodingKeys.self)
            try fields.encode([
                .cause: cause, .duration: duration / .milliseconds(1), .cpu: cpu / .milliseconds(1), .runnable: runnable / .milliseconds(1),
                .pageWait: pageWait / .milliseconds(1), .diskBytesRead: diskBytesRead, .diskBytesWritten: diskBytesWritten, .end: end,
            ])
        }
    }

    private enum CodingKeys: CodingKey {
        case cpu, runnable, pageWait, diskBytesRead, diskBytesWritten, footprint, powerAssertions
    }

    let cpu: Duration
    let runnable: Duration
    let pageWait: Duration
    let diskBytesRead: UInt64
    let diskBytesWritten: UInt64
    let footprint: Int
    let threads: Int
    let runningThreads: Int
    let powerAssertions: Result<[String], ProcessFailure>
    let at: SuspendingClock.Instant

    package func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: CodingKeys.self)
        try fields.encode([
            .cpu: cpu / .milliseconds(1), .runnable: runnable / .milliseconds(1), .pageWait: pageWait / .milliseconds(1), .diskBytesRead: diskBytesRead,
            .diskBytesWritten: diskBytesWritten, .footprint: footprint, .powerAssertions: EncodedResult(powerAssertions),
        ])
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension KeyedEncodingContainer {
    fileprivate mutating func encode(_ fields: KeyValuePairs<Key, any Encodable>) throws {
        for (key, value) in fields { try encode(value, forKey: key) }
    }
}

extension ResourceUsage {
    private static let timebase: mach_timebase_info_data_t = {
        var timebase = mach_timebase_info_data_t()
        _ = unsafe mach_timebase_info(&timebase)
        return timebase
    }()

    private static func assertions() -> Result<[NSNumber: AnyObject], ProcessFailure> {
        var table: Unmanaged<CFDictionary>?
        let read = unsafe IOPMCopyAssertionsByProcess(&table)
        let held = unsafe table?.takeRetainedValue() as? [NSNumber: AnyObject]
        return read == kIOReturnSuccess ? .success(held ?? [:]) : .failure(ProcessFailure(subject: "Power assertion read", reason: .mach(ProcessFailure.Status(kernel: read))))
    }

    package init(of instance: ProcessInstance) throws(Errno) {
        try self.init(of: instance, among: Self.assertions())
    }

    package static func all(of instances: some Sequence<ProcessInstance>) -> [ProcessInstance: Result<Self, Errno>] {
        let held = assertions()
        return Dictionary(instances.map { instance in (instance, Result { () throws(Errno) in try Self(of: instance, among: held) }) }) { first, _ in first }
    }

    private init(of instance: ProcessInstance, among assertions: Result<[NSNumber: AnyObject], ProcessFailure>) throws(Errno) {
        var usage = rusage_info_current()
        let status = withUnsafeMutablePointer(to: &usage) { usage in
            unsafe usage.withMemoryRebound(to: rusage_info_t?.self, capacity: 1) { buffer in unsafe proc_pid_rusage(instance.pid, RUSAGE_INFO_CURRENT, buffer) }
        }
        let failure = Errno(rawValue: errno)
        let task = try information(of: instance.pid, PROC_PIDTASKALLINFO, proc_taskallinfo())
        guard ProcessInstance(task.pbsd) == instance else { throw .noSuchProcess }
        guard status == 0 else { throw failure }
        let elapsed = { (ticks: UInt64) -> Duration in .nanoseconds(ticks) * Self.timebase.numer / Self.timebase.denom }
        self.init(
            cpu: elapsed(usage.ri_user_time + usage.ri_system_time),
            runnable: elapsed(usage.ri_runnable_time),
            pageWait: elapsed(usage.ri_page_wait_time_mach),
            diskBytesRead: usage.ri_diskio_bytesread,
            diskBytesWritten: usage.ri_diskio_byteswritten,
            footprint: Int(usage.ri_phys_footprint),
            threads: Int(task.ptinfo.pti_threadnum),
            runningThreads: Int(task.ptinfo.pti_numrunning),
            powerAssertions: assertions.map { table in
                Set(
                    (table[NSNumber(value: instance.pid)] as? [[String: Any]] ?? []).compactMap { (assertion: [String: Any]) -> String? in
                        switch (assertion[kIOPMAssertionTypeKey], assertion[kIOPMAssertionNameKey], assertion[kIOPMAssertionLevelKey]) {
                            case (.some(let type as String), .some(let name as String), .some(let level as Int)) where level == kIOPMAssertionLevelOn: "\(type) named \(name)"
                            case _: nil
                        }
                    }
                )
                .sorted()
            },
            at: .now,
        )
    }

    package func since(_ earlier: Self) -> Interval { Interval(start: earlier, end: self) }
}
