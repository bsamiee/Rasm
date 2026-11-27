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
        case memory, thermal, lowPower, advisory
    }

    let memory: DispatchSource.MemoryPressureEvent
    let thermal: ProcessInfo.ThermalState
    let lowPower: Bool
    let advisory: [String: Int]?

    package func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: CodingKeys.self)
        try fields.encode(memory.description, forKey: .memory)
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
            case idle, pageins, swapins, swapouts, decompressions, deviceUtilization, diskBytesRead, diskBytesWritten, diskTime, availableCapacity, availableMemory
        }

        let start: SystemLoad
        let end: SystemLoad
        let deviceUtilization: Int?
        let availableCapacity: Int
        let availableMemory: Int

        var idle: Double? {
            let ticks = SIMD4<UInt64>(truncatingIfNeeded: end.processor &- start.processor)
            let total = ticks.wrappedSum()
            return total == 0 ? nil : Double(ticks[Int(CPU_STATE_IDLE)]) / Double(total)
        }

        var disk: (bytesRead: UInt64, bytesWritten: UInt64, time: Duration) {
            end.disks.reduce(into: (bytesRead: UInt64.zero, bytesWritten: UInt64.zero, time: Duration.zero)) { sum, driver in
                guard let held = start.disks[driver.key] else { return }
                sum = (sum.bytesRead &+ driver.value.bytesRead &- held.bytesRead, sum.bytesWritten &+ driver.value.bytesWritten &- held.bytesWritten, sum.time + driver.value.time - held.time)
            }
        }

        func growth(_ counter: KeyPath<SystemLoad, UInt64>) -> UInt64 {
            end[keyPath: counter] &- start[keyPath: counter]
        }

        func paging(_ counter: KeyPath<SystemLoad, UInt64>) -> UInt64? {
            growth(\.memory.faults) == 0 ? nil : growth(counter)
        }

        package func encode(to encoder: any Encoder) throws {
            var fields = encoder.container(keyedBy: CodingKeys.self)
            let disk = self.disk
            let values: [(CodingKeys, (any Encodable)?)] = [
                (.idle, idle), (.pageins, paging(\.memory.pageins)), (.swapins, paging(\.memory.swapins)), (.swapouts, paging(\.memory.swapouts)),
                (.decompressions, paging(\.memory.decompressions)), (.deviceUtilization, deviceUtilization), (.diskBytesRead, disk.bytesRead),
                (.diskBytesWritten, disk.bytesWritten), (.diskTime, disk.time / .seconds(1)), (.availableCapacity, availableCapacity), (.availableMemory, availableMemory),
            ]
            for case (let key, let value?) in values { try fields.encode(value, forKey: key) }
        }
    }

    let processor: SIMD4<UInt32>
    let memory: vm_statistics64
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

private func statistics<Info: BitwiseCopyable>(of host: host_t, _ flavor: host_flavor_t, _ empty: Info) throws(ProcessFailure) -> Info {
    var info = empty
    let status: kern_return_t = withUnsafeMutableBytes(of: &info) { bytes in
        unsafe bytes.withMemoryRebound(to: integer_t.self) { words in
            var count = mach_msg_type_number_t(words.count)
            return unsafe host_statistics64(host, flavor, words.baseAddress, &count)
        }
    }
    guard status == KERN_SUCCESS else { throw ProcessFailure(subject: "host_statistics64 \(Info.self)", reason: .mach(status)) }
    return info
}

extension UInt32 {
    fileprivate init(sysctlbyname name: String) throws(ProcessFailure) {
        var value: UInt32 = 0
        var size = MemoryLayout<UInt32>.size
        guard unsafe sysctlbyname(name, &value, &size, nil, 0) == 0 else { throw ProcessFailure(subject: name, reason: .system(Errno(rawValue: errno))) }
        self = value
    }
}

extension SystemPressure {
    init() throws(ProcessFailure) {
        try self.init(
            memory: DispatchSource.MemoryPressureEvent(rawValue: UInt(UInt32(sysctlbyname: "kern.memorystatus_vm_pressure_level"))),
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
        let first = Result { () throws(ProcessFailure) in try Self() }
        let process = ProcessInfo.processInfo
        let readings = merge(
            pushed,
            NotificationCenter.default.messages(of: process, for: .thermalStateDidChange).map { _ in () },
            NotificationCenter.default.messages(of: process, for: .powerStateDidChange).map { _ in () },
        )
        .map { _ in Result { () throws(ProcessFailure) in try Self() } }
        return chain((refused + [first]).async, readings).removeDuplicates()
    }
}

extension SystemLoad {
    package init() throws(ProcessFailure) {
        let host = mach_host_self()
        defer { _ = unsafe mach_port_deallocate(mach_task_self_, host) }
        let ticks = try statistics(of: host, HOST_CPU_LOAD_INFO, host_cpu_load_info()).cpu_ticks
        try self.init(
            processor: SIMD4(ticks.0, ticks.1, ticks.2, ticks.3),
            memory: statistics(of: host, HOST_VM_INFO64, vm_statistics64()),
            disks: copyProperty(kIOBlockStorageDriverStatisticsKey, matching: kIOBlockStorageDriverClass).mapValues { statistics in
                let count = { (key: String) -> UInt64 in statistics[key] as? UInt64 ?? 0 }
                return (
                    bytesRead: count(kIOBlockStorageDriverStatisticsBytesReadKey), bytesWritten: count(kIOBlockStorageDriverStatisticsBytesWrittenKey),
                    time: .nanoseconds(count(kIOBlockStorageDriverStatisticsTotalReadTimeKey) &+ count(kIOBlockStorageDriverStatisticsTotalWriteTimeKey)),
                )
            },
        )
    }
}

extension SystemLoad.Interval {
    package init(since start: SystemLoad) throws(ProcessFailure) {
        let end = try SystemLoad()
        let volume: URLResourceValues
        do { volume = try URL(filePath: "/").resourceValues(forKeys: [.volumeAvailableCapacityKey]) } catch { throw ProcessFailure(subject: "/", error) }
        guard let availableCapacity = volume.volumeAvailableCapacity else { throw ProcessFailure(subject: "available capacity of /", reason: .notFound) }
        try self.init(
            start: start,
            end: end,
            deviceUtilization: copyProperty("PerformanceStatistics", matching: kIOAcceleratorClassName).values.compactMap { statistics in statistics["Device Utilization %"] as? Int }.max(),
            availableCapacity: availableCapacity,
            availableMemory: Int(UInt32(sysctlbyname: "kern.memorystatus_level")),
        )
    }
}
