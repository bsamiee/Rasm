import AsyncAlgorithms
@unsafe @preconcurrency import Dispatch
import Foundation
import IOKit
import IOKit.graphics
import IOKit.pwr_mgt
import IOKit.storage
import System
import notify

// --- [MODELS] --------------------------------------------------------------------------

package struct SystemPressure: Encodable, Equatable, Sendable {
    private enum CodingKeys: CodingKey {
        case memory, available, thermal, lowPower, advisory
    }

    let memory: DispatchSource.MemoryPressureEvent
    let available: Int
    let thermal: ProcessInfo.ThermalState
    let lowPower: Bool
    let advisory: [String: Int]?

    package func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: CodingKeys.self)
        try fields.encode(memory.description, forKey: .memory)
        try fields.encode(available, forKey: .available)
        let thermal: String =
            switch thermal {
                case .nominal: "nominal"
                case .fair: "fair"
                case .serious: "serious"
                case .critical: "critical"
                @unknown default: String(thermal.rawValue)
            }
        try fields.encode(thermal, forKey: .thermal)
        try fields.encode(lowPower, forKey: .lowPower)
        let advisory: [String: String]? = advisory?.mapValues { level in
            switch level {
                case kIOSystemLoadAdvisoryLevelBad: "bad"
                case kIOSystemLoadAdvisoryLevelOK: "ok"
                case kIOSystemLoadAdvisoryLevelGreat: "great"
                default: String(level)
            }
        }
        try fields.encodeIfPresent(advisory, forKey: .advisory)
    }
}

package struct SystemLoad: Sendable {
    package struct Interval: Encodable, Sendable {
        private enum CodingKeys: CodingKey {
            case idle, pageins, swapins, swapouts, decompressions, deviceUtilization, diskBytesRead, diskBytesWritten, diskTime, availableCapacityForImportantUsage
        }

        let growth: SystemLoad
        let deviceUtilization: Int?
        let availableCapacityForImportantUsage: Int64?

        var idle: Double? {
            let ticks = SIMD4<UInt64>(truncatingIfNeeded: growth.processor)
            let total: UInt64 = ticks.wrappedSum()
            return total == 0 ? nil : Double(ticks[Int(CPU_STATE_IDLE)]) / Double(total)
        }

        var stale: Bool { growth.faults == 0 }

        package func encode(to encoder: any Encoder) throws {
            var fields = encoder.container(keyedBy: CodingKeys.self)
            let disk = growth.disks.values.reduce((bytesRead: UInt64.zero, bytesWritten: UInt64.zero, time: Duration.zero)) { total, driver in
                (total.bytesRead &+ driver.bytesRead, total.bytesWritten &+ driver.bytesWritten, total.time + driver.time)
            }
            let paging: [(CodingKeys, UInt64)] = stale ? [] : [(.pageins, growth.pageins), (.swapins, growth.swapins), (.swapouts, growth.swapouts), (.decompressions, growth.decompressions)]
            try fields.encodeIfPresent(idle, forKey: .idle)
            for (key, count) in paging + [(.diskBytesRead, disk.bytesRead), (.diskBytesWritten, disk.bytesWritten)] { try fields.encode(count, forKey: key) }
            try fields.encode(disk.time / .seconds(1), forKey: .diskTime)
            try fields.encodeIfPresent(deviceUtilization, forKey: .deviceUtilization)
            try fields.encodeIfPresent(availableCapacityForImportantUsage, forKey: .availableCapacityForImportantUsage)
        }
    }

    let processor: SIMD4<UInt32>
    let pageins: UInt64
    let swapins: UInt64
    let swapouts: UInt64
    let decompressions: UInt64
    let faults: UInt64
    let disks: [UInt64: (bytesRead: UInt64, bytesWritten: UInt64, time: Duration)]
}

// --- [OPERATIONS] ----------------------------------------------------------------------

private func copyProperty(_ key: String, matching service: String) throws(ProcessFailure) -> [UInt64: [String: Any]] {
    var iterator: io_iterator_t = 0
    let matched = unsafe IOServiceGetMatchingServices(kIOMainPortDefault, IOServiceMatching(service), &iterator)
    guard matched == KERN_SUCCESS else { throw ProcessFailure(subject: service, reason: .mach(matched)) }
    defer { _ = IOObjectRelease(iterator) }
    let entries = sequence(state: iterator) { iterator -> io_object_t? in
        let entry: io_object_t = IOIteratorNext(iterator)
        return entry == 0 ? nil : entry
    }
    return try Dictionary(
        uniqueKeysWithValues: entries.map { (entry: io_object_t) throws(ProcessFailure) -> (UInt64, [String: Any]?) in
            defer { _ = IOObjectRelease(entry) }
            var id: UInt64 = 0
            let identified = unsafe IORegistryEntryGetRegistryEntryID(entry, &id)
            guard identified == KERN_SUCCESS else { throw ProcessFailure(subject: service, reason: .mach(identified)) }
            return unsafe (id, IORegistryEntryCreateCFProperty(entry, key as CFString, kCFAllocatorDefault, 0)?.takeRetainedValue() as? [String: Any])
        }
    )
    .compactMapValues(\.self)
}

extension SystemPressure {
    init() throws(ProcessFailure) {
        let level = { (name: String) throws(ProcessFailure) -> UInt32 in
            var level: UInt32 = 0
            var size = MemoryLayout<UInt32>.size
            guard unsafe sysctlbyname(name, &level, &size, nil, 0) == 0 else { throw ProcessFailure(subject: name, reason: .system(Errno(rawValue: errno))) }
            return level
        }
        try self.init(
            memory: DispatchSource.MemoryPressureEvent(rawValue: UInt(level("kern.memorystatus_vm_pressure_level"))),
            available: Int(level("kern.memorystatus_level")),
            thermal: ProcessInfo.processInfo.thermalState,
            lowPower: ProcessInfo.processInfo.isLowPowerModeEnabled,
            advisory: unsafe IOCopySystemLoadAdvisoryDetailed()?.takeRetainedValue() as? [String: Int],
        )
    }

    package static func changes() -> some AsyncSequence<Result<Self, ProcessFailure>, Never> & Sendable {
        let (pushed, signal) = AsyncStream<Void>.makeStream(bufferingPolicy: .bufferingNewest(1))
        let source = DispatchSource.makeMemoryPressureSource(eventMask: .all, queue: .global())
        source.setEventHandler { signal.yield() }
        var token: Int32 = NOTIFY_TOKEN_INVALID
        let registered: UInt32 = unsafe notify_register_dispatch(kIOSystemLoadAdvisoryNotifyName, &token, .global()) { _ in signal.yield() }
        signal.onTermination = { [token] _ in
            source.cancel()
            _ = notify_cancel(token)
        }
        source.activate()
        let refused: [Result<Self, ProcessFailure>] =
            registered == NOTIFY_STATUS_OK ? [] : [.failure(ProcessFailure(subject: "notify_register_dispatch \(kIOSystemLoadAdvisoryNotifyName)", reason: .notify(registered)))]
        let process = ProcessInfo.processInfo
        let readings = merge(
            chain([()].async, pushed),
            NotificationCenter.default.messages(of: process, for: .thermalStateDidChange).map { _ in () },
            NotificationCenter.default.messages(of: process, for: .powerStateDidChange).map { _ in () },
        )
        .map { _ in Result { () throws(ProcessFailure) in try Self() } }
        return chain(refused.async, readings).removeDuplicates()
    }
}

extension SystemLoad {
    package init() throws(ProcessFailure) {
        let host = mach_host_self()
        defer { _ = unsafe mach_port_deallocate(mach_task_self_, host) }
        var (processors, info, length): (natural_t, processor_info_array_t?, mach_msg_type_number_t) = (0, nil, 0)
        let loaded: kern_return_t = unsafe host_processor_info(host, PROCESSOR_CPU_LOAD_INFO, &processors, &info, &length)
        guard loaded == KERN_SUCCESS, let loads = unsafe info else { throw ProcessFailure(subject: "host_processor_info PROCESSOR_CPU_LOAD_INFO", reason: .mach(loaded)) }
        let size: vm_size_t = vm_size_t(length) * vm_size_t(MemoryLayout<integer_t>.stride)
        defer { _ = unsafe vm_deallocate(mach_task_self_, vm_address_t(bitPattern: loads), size) }
        let processor: SIMD4<UInt32> = unsafe loads.withMemoryRebound(to: processor_cpu_load_info.self, capacity: Int(processors)) { cores in
            unsafe UnsafeBufferPointer(start: cores, count: Int(processors)).reduce(SIMD4<UInt32>.zero) { (sum: SIMD4<UInt32>, core: processor_cpu_load_info) -> SIMD4<UInt32> in
                sum &+ SIMD4(core.cpu_ticks.0, core.cpu_ticks.1, core.cpu_ticks.2, core.cpu_ticks.3)
            }
        }
        var memory = vm_statistics64()
        let counted: kern_return_t = withUnsafeMutableBytes(of: &memory) { info in
            unsafe info.withMemoryRebound(to: integer_t.self) { words in
                var count = mach_msg_type_number_t(words.count)
                return unsafe host_statistics64(host, HOST_VM_INFO64, words.baseAddress, &count)
            }
        }
        guard counted == KERN_SUCCESS else { throw ProcessFailure(subject: "host_statistics64 HOST_VM_INFO64", reason: .mach(counted)) }
        try self.init(
            processor: processor,
            pageins: memory.pageins,
            swapins: memory.swapins,
            swapouts: memory.swapouts,
            decompressions: memory.decompressions,
            faults: memory.faults,
            disks: copyProperty(kIOBlockStorageDriverStatisticsKey, matching: kIOBlockStorageDriverClass).mapValues { statistics in
                let count = { (key: String) -> UInt64 in statistics[key] as? UInt64 ?? 0 }
                return (
                    bytesRead: count(kIOBlockStorageDriverStatisticsBytesReadKey), bytesWritten: count(kIOBlockStorageDriverStatisticsBytesWrittenKey),
                    time: .nanoseconds(count(kIOBlockStorageDriverStatisticsTotalReadTimeKey) &+ count(kIOBlockStorageDriverStatisticsTotalWriteTimeKey)),
                )
            },
        )
    }

    func since(_ earlier: Self) -> Self {
        Self(
            processor: processor &- earlier.processor,
            pageins: pageins &- earlier.pageins,
            swapins: swapins &- earlier.swapins,
            swapouts: swapouts &- earlier.swapouts,
            decompressions: decompressions &- earlier.decompressions,
            faults: faults &- earlier.faults,
            disks: Dictionary(
                uniqueKeysWithValues: disks.map { id, later in
                    let held = earlier.disks[id] ?? (0, 0, .zero)
                    return (id, (bytesRead: later.bytesRead &- held.bytesRead, bytesWritten: later.bytesWritten &- held.bytesWritten, time: later.time - held.time))
                }
            ),
        )
    }
}

extension SystemLoad.Interval {
    package init(since earlier: SystemLoad) throws(ProcessFailure) {
        let growth = try SystemLoad().since(earlier)
        let volume: URLResourceValues
        do { volume = try URL(filePath: "/").resourceValues(forKeys: [.volumeAvailableCapacityForImportantUsageKey]) } catch { throw ProcessFailure(subject: "/", error) }
        try self.init(
            growth: growth,
            deviceUtilization: copyProperty("PerformanceStatistics", matching: kIOAcceleratorClassName).values.compactMap { statistics in statistics["Device Utilization %"] as? Int }.max(),
            availableCapacityForImportantUsage: volume.volumeAvailableCapacityForImportantUsage,
        )
    }
}
