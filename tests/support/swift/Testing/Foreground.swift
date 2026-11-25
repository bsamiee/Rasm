@unsafe @preconcurrency public import ApplicationServices
public import CoreGraphics
import CustomDump
public import Foundation
public import Testing

// --- [CONSTANTS] ----------------------------------------------------------------------

private let skyLight = "/System/Library/PrivateFrameworks/SkyLight.framework/SkyLight"

// --- [MODELS] --------------------------------------------------------------------------

public struct Foreground: Equatable, Sendable {
    public let application: pid_t?
    public let window: CGWindowID?
    public let order: [CGWindowID]
    public let pointer: CGPoint?

    public init(application: pid_t?, window: CGWindowID?, order: [CGWindowID], pointer: CGPoint?) {
        (self.application, self.window, self.order, self.pointer) = (application, window, order, pointer)
    }

    public static func sampled() throws(ForegroundFailure) -> Self {
        guard unsafe AXIsProcessTrustedWithOptions([kAXTrustedCheckOptionPrompt.takeUnretainedValue(): true] as CFDictionary) else {
            throw ForegroundFailure(subject: CommandLine.arguments[0], reason: .ungranted)
        }
        let application = try frontProcess()
        let window = try application.flatMap { process throws(ForegroundFailure) in try focusedWindow(of: AXUIElementCreateApplication(process)) }
        let windows = CGWindowListCopyWindowInfo([.optionOnScreenOnly, .excludeDesktopElements], kCGNullWindowID) as? [[String: Any]] ?? []
        return Self(
            application: application,
            window: window,
            order: windows.filter { row in row[kCGWindowLayer as String] as? Int == 0 }.compactMap { row in row[kCGWindowNumber as String] as? CGWindowID },
            pointer: CGEvent(source: nil)?.location,
        )
    }

    public func unchanged(in later: Self) -> Bool {
        (application, window, pointer) == (later.application, later.window, later.pointer) && later.order.starts(with: order.filter(Set(later.order).contains))
    }

    private static func frontProcess() throws(ForegroundFailure) -> pid_t? {
        let library = unsafe dlopen(skyLight, RTLD_LAZY)
        let front = try unsafe symbol("SLPSGetFrontProcess", in: library, as: (@convention(c) (UnsafeMutablePointer<ProcessSerialNumber>) -> OSStatus).self)
        let main = try unsafe symbol("SLSMainConnectionID", in: library, as: (@convention(c) () -> Int32).self)
        let connection = try unsafe symbol(
            "SLSGetConnectionIDForPSN",
            in: library,
            as: (@convention(c) (Int32, UnsafeMutablePointer<ProcessSerialNumber>, UnsafeMutablePointer<Int32>) -> CGError).self,
        )
        let owner = try unsafe symbol("SLSConnectionGetPID", in: library, as: (@convention(c) (Int32, UnsafeMutablePointer<pid_t>) -> CGError).self)
        var (serial, client, process) = (ProcessSerialNumber(), Int32(0), pid_t(0))
        return unsafe front(&serial) == noErr && connection(main(), &serial, &client) == .success && owner(client, &process) == .success ? process : nil
    }

    private static func symbol<Function>(_ name: String, in library: UnsafeMutableRawPointer?, as type: Function.Type) throws(ForegroundFailure) -> Function {
        guard let address = unsafe dlsym(library, name) else { throw ForegroundFailure(subject: name, reason: .unresolved(skyLight)) }
        return unsafe unsafeBitCast(address, to: type)
    }

    private static func focusedWindow(of application: AXUIElement) throws(ForegroundFailure) -> CGWindowID? {
        var (value, number): (CFTypeRef?, CGWindowID) = (nil, 0)
        switch unsafe AXUIElementCopyAttributeValue(application, kAXFocusedWindowAttribute as CFString, &value) {
            case .success:
                return value.flatMap { value in
                    unsafe CFGetTypeID(value) == AXUIElementGetTypeID() && windowNumber(unsafeDowncast(value, to: AXUIElement.self), &number) == .success ? number : nil
                }
            case .noValue, .attributeUnsupported: return nil
            case let error: throw ForegroundFailure(subject: kAXFocusedWindowAttribute, reason: .accessibility(error))
        }
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

public struct ForegroundFailure: LocalizedError, CustomStringConvertible, Sendable {
    public enum Reason: Sendable {
        case ungranted
        case unresolved(String)
        case accessibility(AXError)
    }

    public let subject: String
    public let reason: Reason

    public var description: String {
        switch reason {
            case .ungranted: "Accessibility is not granted to \(subject)"
            case .unresolved(let library): "\(subject) is not exported by \(library)"
            case .accessibility(let error): "AXError \(error.rawValue) reading \(subject)"
        }
    }

    public var errorDescription: String? { description }
}

// --- [SERVICES] ------------------------------------------------------------------------

public struct ForegroundTrait: TestTrait, SuiteTrait, TestScoping {
    public var isRecursive: Bool { true }

    @concurrent
    public func provideScope(for test: Test, testCase _: Test.Case?, performing function: @concurrent @Sendable () async throws -> Void) async throws {
        let start = ContinuousClock.now
        let before = try Foreground.sampled()
        try await function()
        let after = try Foreground.sampled()
        guard !before.unchanged(in: after) else { return }
        let touched = CGEventType(rawValue: ~0).map { input in CGEventSource.secondsSinceLastEventType(.hidSystemState, eventType: input) < (ContinuousClock.now - start) / .seconds(1) } ?? false
        Issue.record(
            "Foreground changed\(touched ? " while the owner used the keyboard or pointer" : "")\n\(diff(before, after) ?? "")",
            severity: touched ? .warning : .error,
            sourceLocation: test.sourceLocation,
        )
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Trait where Self == ForegroundTrait {
    public static var foregroundUnchanged: Self { Self() }
}

@_silgen_name("_AXUIElementGetWindow")
private func windowNumber(_: AXUIElement, _: UnsafeMutablePointer<CGWindowID>) -> AXError
