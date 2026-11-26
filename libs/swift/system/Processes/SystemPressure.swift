import AsyncAlgorithms
@unsafe @preconcurrency public import Dispatch
public import Foundation
import IOKit
import IOKit.graphics
import IOKit.pwr_mgt
import IOKit.storage
import System
import notify

// --- [MODELS] --------------------------------------------------------------------------

public struct SystemPressure: Encodable, Equatable, Sendable {
    private enum CodingKeys: CodingKey {
        case memory, available, thermal, lowPower, advisory
    }

    public let memory: DispatchSource.MemoryPressureEvent
    public let available: Int
    public let thermal: ProcessInfo.ThermalState
    public let lowPower: Bool
    public let advisory: [String: Int]?

    public func encode(to encoder: any Encoder) throws {
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

public struct SystemLoad: Sendable {
    public struct Interval: Encodable, Sendable {
        private enum CodingKeys: CodingKey {
            case idle, pageins, swapins, swapouts, decompressions, deviceUtilization, diskBytesRead, diskBytesWritten, diskTime, availableCapacityForImportantUsage
        }

        public let growth: SystemLoad
        public let deviceUtilization: Int?
        public let availableCapacityForImportantUsage: Int64?

        public var idle: Double? {
            let ticks = SIMD4<UInt64>(truncatingIfNeeded: growth.processor)
            let total: UInt64 = ticks.wrappedSum()
            return total == 0 ? nil : Double(ticks[Int(CPU_STATE_IDLE)]) / Double(total)
        }

        public func encode(to encoder: any Encoder) throws {
            var fields = encoder.container(keyedBy: CodingKeys.self)
            let disk = growth.disks.values.reduce((bytesRead: UInt64.zero, bytesWritten: UInt64.zero, time: Duration.zero)) { total, driver in
                (total.bytesRead &+ driver.bytesRead, total.bytesWritten &+ driver.bytesWritten, total.time + driver.time)
            }
            try fields.encodeIfPresent(idle, forKey: .idle)
            for (key, count) in [
                (CodingKeys.pageins, growth.pageins), (.swapins, growth.swapins), (.swapouts, growth.swapouts), (.decompressions, growth.decompressions), (.diskBytesRead, disk.bytesRead),
                (.diskBytesWritten, disk.bytesWritten),
            ] {
                try fields.encode(count, forKey: key)
            }
            try fields.encode(disk.time / .seconds(1), forKey: .diskTime)
            try fields.encodeIfPresent(deviceUtilization, forKey: .deviceUtilization)
            try fields.encodeIfPresent(availableCapacityForImportantUsage, forKey: .availableCapacityForImportantUsage)
        }
    }

    public let processor: SIMD4<UInt32>
    public let pageins: UInt64
    public let swapins: UInt64
    public let swapouts: UInt64
    public let decompressions: UInt64
    public let disks: [UInt64: (bytesRead: UInt64, bytesWritten: UInt64, time: Duration)]
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
    public init() throws(ProcessFailure) {
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

    public static func changes() -> some AsyncSequence<Result<Self, ProcessFailure>, Never> & Sendable {
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
    public init() throws(ProcessFailure) {
        let host = mach_host_self()
        defer { _ = unsafe mach_port_deallocate(mach_task_self_, host) }
        let read = { (flavor: host_flavor_t, info: UnsafeMutableRawBufferPointer) throws(ProcessFailure) in
            let status = unsafe info.withMemoryRebound(to: integer_t.self) { words in
                var count = mach_msg_type_number_t(words.count)
                return unsafe host_statistics64(host, flavor, words.baseAddress, &count)
            }
            guard status == KERN_SUCCESS else { throw ProcessFailure(subject: "host_statistics64 flavor \(flavor)", reason: .mach(status)) }
        }
        var processor = host_cpu_load_info()
        var memory = vm_statistics64()
        try withUnsafeMutableBytes(of: &processor) { info throws(ProcessFailure) in try unsafe read(HOST_CPU_LOAD_INFO, info) }
        try withUnsafeMutableBytes(of: &memory) { info throws(ProcessFailure) in try unsafe read(HOST_VM_INFO64, info) }
        let ticks = processor.cpu_ticks
        try self.init(
            processor: SIMD4(ticks.0, ticks.1, ticks.2, ticks.3),
            pageins: memory.pageins,
            swapins: memory.swapins,
            swapouts: memory.swapouts,
            decompressions: memory.decompressions,
            disks: copyProperty(kIOBlockStorageDriverStatisticsKey, matching: kIOBlockStorageDriverClass).mapValues { statistics in
                let count = { (key: String) -> UInt64 in statistics[key] as? UInt64 ?? 0 }
                return (
                    bytesRead: count(kIOBlockStorageDriverStatisticsBytesReadKey), bytesWritten: count(kIOBlockStorageDriverStatisticsBytesWrittenKey),
                    time: .nanoseconds(count(kIOBlockStorageDriverStatisticsTotalReadTimeKey) &+ count(kIOBlockStorageDriverStatisticsTotalWriteTimeKey)),
                )
            },
        )
    }

    public func since(_ earlier: Self) -> Self {
        Self(
            processor: processor &- earlier.processor,
            pageins: pageins &- earlier.pageins,
            swapins: swapins &- earlier.swapins,
            swapouts: swapouts &- earlier.swapouts,
            decompressions: decompressions &- earlier.decompressions,
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
    public init(since earlier: SystemLoad) throws(ProcessFailure) {
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
