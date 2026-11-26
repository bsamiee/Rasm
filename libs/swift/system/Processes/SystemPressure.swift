import AsyncAlgorithms
@unsafe @preconcurrency public import Dispatch
public import Foundation
import IOKit
import IOKit.graphics
import IOKit.pwr_mgt
import IOKit.storage
public import System
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
    public let advisory: [String: Int]

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
        let advisory: [String: String] = advisory.mapValues { level in
            switch level {
                case kIOSystemLoadAdvisoryLevelBad: "bad"
                case kIOSystemLoadAdvisoryLevelOK: "ok"
                case kIOSystemLoadAdvisoryLevelGreat: "great"
                default: String(level)
            }
        }
        try fields.encode(advisory, forKey: .advisory)
    }
}

public struct SystemLoad: Encodable, Sendable {
    private enum CodingKeys: CodingKey {
        case idle, pageins, swapins, swapouts, decompressions, deviceUtilization, diskBytesRead, diskBytesWritten, diskTime, availableCapacityForImportantUsage
    }

    public let processor: SIMD4<UInt32>
    public let pageins: UInt64
    public let swapins: UInt64
    public let swapouts: UInt64
    public let decompressions: UInt64
    public let diskBytesRead: UInt64
    public let diskBytesWritten: UInt64
    public let diskTime: Duration
    public let deviceUtilization: Int?
    public let availableCapacityForImportantUsage: Int64?

    public func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: CodingKeys.self)
        let ticks = SIMD4<UInt64>(truncatingIfNeeded: processor)
        let total: UInt64 = ticks.wrappedSum()
        let idle: Double? = total == 0 ? nil : Double(ticks[Int(CPU_STATE_IDLE)]) / Double(total)
        try fields.encodeIfPresent(idle, forKey: .idle)
        for (key, count) in [
            (CodingKeys.pageins, pageins), (.swapins, swapins), (.swapouts, swapouts), (.decompressions, decompressions), (.diskBytesRead, diskBytesRead), (.diskBytesWritten, diskBytesWritten),
        ] {
            try fields.encode(count, forKey: key)
        }
        try fields.encode(diskTime / .seconds(1), forKey: .diskTime)
        try fields.encodeIfPresent(deviceUtilization, forKey: .deviceUtilization)
        try fields.encodeIfPresent(availableCapacityForImportantUsage, forKey: .availableCapacityForImportantUsage)
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

private func copyProperty(_ key: String, matching service: String) throws(NSError) -> [[String: Any]] {
    var iterator: io_iterator_t = 0
    let matched = unsafe IOServiceGetMatchingServices(kIOMainPortDefault, IOServiceMatching(service), &iterator)
    guard matched == KERN_SUCCESS else { throw NSError(domain: NSMachErrorDomain, code: Int(matched)) }
    defer { _ = IOObjectRelease(iterator) }
    return sequence(state: iterator) { iterator -> io_object_t? in
        let entry: io_object_t = IOIteratorNext(iterator)
        return entry == 0 ? nil : entry
    }
    .compactMap { (entry: io_object_t) -> [String: Any]? in
        defer { _ = IOObjectRelease(entry) }
        let property: CFTypeRef? = unsafe IORegistryEntryCreateCFProperty(entry, key as CFString, kCFAllocatorDefault, 0)?.takeRetainedValue()
        return property as? [String: Any]
    }
}

extension SystemPressure {
    public static func current() throws(Errno) -> Self {
        let levels: [UInt32] = try ["kern.memorystatus_vm_pressure_level", "kern.memorystatus_level"].map { name throws(Errno) -> UInt32 in
            var level: UInt32 = 0
            var size = MemoryLayout<UInt32>.size
            guard unsafe sysctlbyname(name, &level, &size, nil, 0) == 0 else { throw Errno(rawValue: errno) }
            return level
        }
        let advisory: CFDictionary? = unsafe IOCopySystemLoadAdvisoryDetailed()?.takeRetainedValue()
        return Self(
            memory: DispatchSource.MemoryPressureEvent(rawValue: UInt(levels[0])),
            available: Int(levels[1]),
            thermal: ProcessInfo.processInfo.thermalState,
            lowPower: ProcessInfo.processInfo.isLowPowerModeEnabled,
            advisory: advisory as? [String: Int] ?? [:],
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
        .map { _ in Result { () throws(Errno) in try current() }.mapError { error in ProcessFailure(subject: "system pressure", reason: .system(error)) } }
        return chain(refused.async, readings).removeDuplicates()
    }
}

extension SystemLoad {
    public static func current() throws(NSError) -> Self {
        let host = mach_host_self()
        defer { _ = unsafe mach_port_deallocate(mach_task_self_, host) }
        var processor = host_cpu_load_info()
        var memory = vm_statistics64()
        let reads: [kern_return_t] = [
            withUnsafeMutablePointer(to: &processor) { info in
                var count = mach_msg_type_number_t(MemoryLayout<host_cpu_load_info>.size / MemoryLayout<integer_t>.size)
                return unsafe info.withMemoryRebound(to: integer_t.self, capacity: Int(count)) { words in unsafe host_statistics(host, HOST_CPU_LOAD_INFO, words, &count) }
            },
            withUnsafeMutablePointer(to: &memory) { info in
                var count = mach_msg_type_number_t(MemoryLayout<vm_statistics64>.size / MemoryLayout<integer_t>.size)
                return unsafe info.withMemoryRebound(to: integer_t.self, capacity: Int(count)) { words in unsafe host_statistics64(host, HOST_VM_INFO64, words, &count) }
            },
        ]
        if let failed = reads.first(where: { read in read != KERN_SUCCESS }) { throw NSError(domain: NSMachErrorDomain, code: Int(failed)) }
        let disks: [[String: Any]] = try copyProperty(kIOBlockStorageDriverStatisticsKey, matching: kIOBlockStorageDriverClass)
        let disk = { (key: String) -> UInt64 in disks.reduce(0) { total, driver in total &+ (driver[key] as? UInt64 ?? 0) } }
        let accelerators: [[String: Any]] = try copyProperty("PerformanceStatistics", matching: kIOAcceleratorClassName)
        let volume: URLResourceValues
        do { volume = try URL(filePath: "/").resourceValues(forKeys: [.volumeAvailableCapacityForImportantUsageKey]) } catch { throw error as NSError }
        let ticks = processor.cpu_ticks
        return Self(
            processor: SIMD4(ticks.0, ticks.1, ticks.2, ticks.3),
            pageins: memory.pageins,
            swapins: memory.swapins,
            swapouts: memory.swapouts,
            decompressions: memory.decompressions,
            diskBytesRead: disk(kIOBlockStorageDriverStatisticsBytesReadKey),
            diskBytesWritten: disk(kIOBlockStorageDriverStatisticsBytesWrittenKey),
            diskTime: .nanoseconds(disk(kIOBlockStorageDriverStatisticsTotalReadTimeKey) &+ disk(kIOBlockStorageDriverStatisticsTotalWriteTimeKey)),
            deviceUtilization: accelerators.compactMap { statistics in statistics["Device Utilization %"] as? Int }.max(),
            availableCapacityForImportantUsage: volume.volumeAvailableCapacityForImportantUsage,
        )
    }

    public func since(_ earlier: Self) -> Self {
        Self(
            processor: processor &- earlier.processor,
            pageins: pageins &- earlier.pageins,
            swapins: swapins &- earlier.swapins,
            swapouts: swapouts &- earlier.swapouts,
            decompressions: decompressions &- earlier.decompressions,
            diskBytesRead: diskBytesRead &- earlier.diskBytesRead,
            diskBytesWritten: diskBytesWritten &- earlier.diskBytesWritten,
            diskTime: diskTime - earlier.diskTime,
            deviceUtilization: deviceUtilization,
            availableCapacityForImportantUsage: availableCapacityForImportantUsage,
        )
    }
}
