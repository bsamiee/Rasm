import AsyncAlgorithms
@unsafe @preconcurrency public import Dispatch
public import Foundation
public import System

// --- [MODELS] --------------------------------------------------------------------------

public struct SystemPressure: Encodable, Equatable, Sendable {
    private enum CodingKeys: CodingKey {
        case memory, thermal
    }

    public let memory: DispatchSource.MemoryPressureEvent
    public let thermal: ProcessInfo.ThermalState

    public func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: CodingKeys.self)
        let memory =
            switch memory {
                case .critical: "critical"
                case .warning: "warning"
                case _: "normal"
            }
        try fields.encode(memory, forKey: .memory)
        let thermal =
            switch thermal {
                case .nominal: "nominal"
                case .fair: "fair"
                case .serious: "serious"
                case .critical: "critical"
                @unknown default: "unknown"
            }
        try fields.encode(thermal, forKey: .thermal)
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension SystemPressure {
    public static func current() throws(Errno) -> Self {
        var level: Int32 = 0
        var size = MemoryLayout<Int32>.size
        guard unsafe sysctlbyname("kern.memorystatus_vm_pressure_level", &level, &size, nil, 0) == 0 else { throw Errno(rawValue: errno) }
        return Self(memory: DispatchSource.MemoryPressureEvent(rawValue: UInt(level)), thermal: ProcessInfo.processInfo.thermalState)
    }

    public static func changes() -> some AsyncSequence<Result<Self, Errno>, Never> & Sendable {
        let (memory, signal) = AsyncStream<Void>.makeStream(bufferingPolicy: .bufferingNewest(1))
        let source = DispatchSource.makeMemoryPressureSource(eventMask: [.normal, .warning, .critical], queue: .global())
        source.setEventHandler { signal.yield(()) }
        signal.onTermination = { _ in source.cancel() }
        source.activate()
        return merge(chain([()].async, memory), NotificationCenter.default.messages(of: ProcessInfo.processInfo, for: .thermalStateDidChange).map { _ in () })
            .map { _ in Result { () throws(Errno) in try current() } }
            .removeDuplicates()
    }
}
