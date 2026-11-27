import AsyncAlgorithms
@unsafe @preconcurrency import Dispatch
import Foundation
import IOKit.pwr_mgt
import System
import notify

// --- [MODELS] --------------------------------------------------------------------------

package struct SystemPressure: Encodable, Equatable, Sendable {
    private enum CodingKeys: CodingKey {
        case memory, thermal, advisory
    }

    let memory: DispatchSource.MemoryPressureEvent
    let thermal: ProcessInfo.ThermalState
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
        let processorIdle: Double?
        let swapins: UInt64?
        let swapouts: UInt64?
    }

    let processor: SIMD4<UInt32>
    let memory: vm_statistics64
}

// --- [OPERATIONS] ----------------------------------------------------------------------

private func statistics<Info: BitwiseCopyable>(of host: host_t, _ flavor: host_flavor_t, _ empty: Info) throws(ProcessFailure) -> Info {
    var info = empty
    let status: kern_return_t = withUnsafeMutableBytes(of: &info) { bytes in
        unsafe bytes.withMemoryRebound(to: integer_t.self) { words in
            var count = mach_msg_type_number_t(words.count)
            return unsafe host_statistics64(host, flavor, words.baseAddress, &count)
        }
    }
    guard status == KERN_SUCCESS else { throw ProcessFailure(subject: "host_statistics64 \(Info.self)", reason: .mach(ProcessFailure.Status(kernel: status))) }
    return info
}

extension SystemPressure {
    init() throws(ProcessFailure) {
        let name = "kern.memorystatus_vm_pressure_level"
        var level: UInt32 = 0
        var size = MemoryLayout<UInt32>.size
        guard unsafe sysctlbyname(name, &level, &size, nil, 0) == 0 else { throw ProcessFailure(subject: name, reason: .system(Errno(rawValue: errno))) }
        self.init(
            memory: DispatchSource.MemoryPressureEvent(rawValue: UInt(level)),
            thermal: ProcessInfo.processInfo.thermalState,
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
        let readings = merge(pushed, NotificationCenter.default.messages(of: ProcessInfo.processInfo, for: .thermalStateDidChange).map { _ in () })
            .map { _ in Result { () throws(ProcessFailure) in try Self() } }
        return chain((refused + [first]).async, readings).removeDuplicates()
    }
}

extension SystemLoad {
    package init() throws(ProcessFailure) {
        let host = mach_host_self()
        defer { _ = unsafe mach_port_deallocate(mach_task_self_, host) }
        let ticks = try statistics(of: host, HOST_CPU_LOAD_INFO, host_cpu_load_info()).cpu_ticks
        try self.init(processor: SIMD4(ticks.0, ticks.1, ticks.2, ticks.3), memory: statistics(of: host, HOST_VM_INFO64, vm_statistics64()))
    }
}

extension SystemLoad.Interval {
    init(start: SystemLoad, end: SystemLoad) {
        let ticks = SIMD4<UInt64>(truncatingIfNeeded: end.processor &- start.processor)
        let total: UInt64 = ticks.wrappedSum()
        let faulted: Bool = end.memory.faults != start.memory.faults
        self.init(
            processorIdle: total == 0 ? nil : Double(ticks[Int(CPU_STATE_IDLE)]) / Double(total),
            swapins: faulted ? end.memory.swapins &- start.memory.swapins : nil,
            swapouts: faulted ? end.memory.swapouts &- start.memory.swapouts : nil,
        )
    }

    package init(since start: SystemLoad) throws(ProcessFailure) {
        try self.init(start: start, end: SystemLoad())
    }
}
