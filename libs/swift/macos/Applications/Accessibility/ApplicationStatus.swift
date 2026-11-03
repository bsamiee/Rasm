import AppKit
import ApplicationServices
import AsyncAlgorithms
public import CasePaths
public import CoreGraphics
public import Processes
import Synchronization

// --- [CONSTANTS] -----------------------------------------------------------------------

private let notifications: [String] = [
    kAXWindowCreatedNotification, kAXSheetCreatedNotification, kAXUIElementDestroyedNotification, kAXFocusedWindowChangedNotification, kAXTitleChangedNotification,
    kAXWindowMiniaturizedNotification, kAXWindowDeminiaturizedNotification,
]
private let beatAttributes: [String] = [kAXWindowsAttribute, kAXFocusedWindowAttribute]

// --- [MODELS] --------------------------------------------------------------------------

public struct Window: Encodable, Hashable, Sendable {
    public let number: CGWindowID
    public let title: String?
    public let frame: CGRect
    public let layer: Int
    public let onScreen: Bool
}

extension Window {
    init?(_ description: [String: Any]) {
        guard let number: CGWindowID = description[kCGWindowNumber as String] as? CGWindowID, let layer: Int = description[kCGWindowLayer as String] as? Int, layer >= 0,
            let frame: CGRect = (description[kCGWindowBounds as String] as? [String: Any]).flatMap({ bounds in CGRect(dictionaryRepresentation: bounds as CFDictionary) })
        else { return nil }
        self.init(
            number: number,
            title: (description[kCGWindowName as String] as? String).flatMap { title in title.isEmpty ? nil : title },
            frame: frame,
            layer: layer,
            onScreen: description[kCGWindowIsOnscreen as String] as? Bool ?? false,
        )
    }

    public static func owned(by process: pid_t) -> [Self] {
        (CGWindowListCopyWindowInfo(.optionAll, kCGNullWindowID) as? [[String: Any]] ?? []).filter { description in description[kCGWindowOwnerPID as String] as? pid_t == process }
            .compactMap(Self.init)
    }
}

@CasePathable
public enum ApplicationStatus: Encodable, Equatable, Sendable {
    case launching(windows: [Window])
    case responding(elements: [Element], windows: [Window])
    case unresponsive(process: ProcessStatus, windows: [Window])

    var onScreen: Self {
        switch self {
            case .launching(let windows): .launching(windows: windows.filter(\.onScreen))
            case .responding(let elements, let windows): .responding(elements: elements, windows: windows.filter(\.onScreen))
            case .unresponsive(let process, let windows): .unresponsive(process: process, windows: windows.filter(\.onScreen))
        }
    }

    static func changed(_ previous: Result<Self, ApplicationFailure>, _ next: Result<Self, ApplicationFailure>) -> Bool {
        switch (previous, next) {
            case (.success(.unresponsive(let held, let shown)), .success(.unresponsive(let process, let windows))): (held == .stopped) != (process == .stopped) || shown != windows
            case (_, _): previous != next
        }
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

final class Observer: Sendable {
    let instance: NSRunningApplication
    let ticks: AsyncStream<Void>.Continuation
    let observed: Mutex<(observer: AXObserver, unregistered: [String], launch: NSKeyValueObservation?)>

    init(_ instance: NSRunningApplication, ticks: AsyncStream<Void>.Continuation) throws(ApplicationFailure) {
        var created: AXObserver?
        let creation: AXError = unsafe AXObserverCreate(
            instance.processIdentifier,
            { _, _, _, refcon in
                guard let refcon: UnsafeMutableRawPointer = unsafe refcon else { return }
                unsafe Unmanaged<Observer>.fromOpaque(refcon).takeUnretainedValue().ticks.yield(())
            },
            &created,
        )
        guard let observer: AXObserver = created else { throw AXUIElement.application(instance.processIdentifier).failure(creation, reading: ["AXObserverCreate"]) }
        CFRunLoopAddSource(CFRunLoopGetMain(), AXObserverGetRunLoopSource(observer), .defaultMode)
        self.instance = instance
        self.ticks = ticks
        observed = Mutex((observer, notifications, instance.observe(\.isFinishedLaunching) { _, _ in ticks.yield(()) }))
    }

    func register(on element: AXUIElement) throws(ApplicationFailure) {
        try observed.withLock { observed throws(ApplicationFailure) in
            observed.unregistered = try observed.unregistered.map { notification throws(ApplicationFailure) -> String? in
                let added: AXError = unsafe AXObserverAddNotification(observed.observer, element, notification as CFString, Unmanaged.passUnretained(self).toOpaque())
                return switch added {
                    case .success, .notificationAlreadyRegistered, .notificationUnsupported: nil
                    case .apiDisabled, .cannotComplete: throw element.failure(added, reading: [notification])
                    default: notification
                }
            }
            .compactMap(\.self)
        }
    }

    func stop() {
        observed.withLock { observed in
            observed.launch?.invalidate()
            observed.launch = nil
            CFRunLoopRemoveSource(CFRunLoopGetMain(), AXObserverGetRunLoopSource(observed.observer), .defaultMode)
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSRunningApplication {
    func processStatus() throws(ApplicationFailure) -> ProcessStatus {
        do { return try ProcessStatus(of: processIdentifier, mainThread: true) } catch { throw ApplicationFailure(error, process: processIdentifier) }
    }

    @concurrent
    func status(
        walking walked: @Sendable (Element) -> Bool,
        observer: Result<Observer, ApplicationFailure>? = nil,
        after last: (elements: [Element], windows: [Data?])? = nil,
    ) async -> (status: Result<ApplicationStatus, ApplicationFailure>, windows: [Data?]?) {
        await withTaskExecutorPreference(accessibility) { () -> (status: Result<ApplicationStatus, ApplicationFailure>, windows: [Data?]?) in
            let owned: [Window] = Window.owned(by: processIdentifier).filter { window in !window.frame.isEmpty && (window.onScreen || isHidden) }
            let responding: ([Element]) -> ApplicationStatus = { elements in
                let described: Set<CGWindowID> = Set((elements + (last?.elements ?? [])).compactMap(\.window))
                return .responding(elements: elements, windows: owned.filter { window in !described.contains(window.number) })
            }
            do throws(ApplicationFailure) {
                let process: ProcessStatus = try processStatus()
                guard process != .stopped else { return (.success(.unresponsive(process: .stopped, windows: owned)), nil) }
                let application: AXUIElement = AXUIElement.application(processIdentifier)
                try observer?.get().register(on: application)
                let read: [String: CFTypeRef] = try application.values(last == nil ? beatAttributes + [kAXMenuBarAttribute] : beatAttributes)
                let windows: [Data?] =
                    (read[kAXWindowsAttribute] as? [CFTypeRef] ?? []).compactMap(AXUIElement.checked).map(\.token) + [read[kAXFocusedWindowAttribute].flatMap(AXUIElement.checked)?.token]
                if let last, last.windows == windows { return (.success(responding(last.elements)), windows) }
                let full: [String: CFTypeRef] = last == nil ? read : try read.merging(application.values([kAXMenuBarAttribute])) { _, menu in menu }
                let elements: [Element] = try AXUIElement.candidates(full).map { candidate throws(ApplicationFailure) in try candidate.described(walking: walked) }
                    .compactMap(\.self)
                return (.success(isFinishedLaunching || elements.contains(where: \.requiresAnswer) ? responding(elements) : .launching(windows: owned)), windows)
            } catch let failure where failure.reason.is(\.unanswered) {
                return (
                    Result { () throws(ApplicationFailure) in
                        let process: ProcessStatus = try processStatus()
                        return isFinishedLaunching ? .unresponsive(process: process, windows: owned) : .launching(windows: owned)
                    },
                    nil,
                )
            } catch {
                return (.failure(error), nil)
            }
        }
    }

    func statusChanges(
        walking walked: @escaping @Sendable (Element) -> Bool,
        observer: Result<Observer, ApplicationFailure>,
        ticks: AsyncStream<Void>,
        clock: some Clock<Duration> & Sendable,
    ) -> some AsyncSequence<Result<ApplicationStatus, ApplicationFailure>, Never> {
        merge(chain([()].async, ticks).map { _ in false }, AsyncTimerSequence(interval: messagingTimeout, clock: clock).map { _ in true })
            .reductions((status: Result<ApplicationStatus, ApplicationFailure>.success(.launching(windows: [])), windows: [Data?]?.none)) { last, beat in
                let seen: (elements: [Element], windows: [Data?])? =
                    switch (beat, last.status, last.windows) {
                        case (true, .success(.responding(let elements, _)), .some(let windows)): (elements, windows)
                        case (_, _, _): nil
                    }
                return await self.status(walking: walked, observer: observer, after: seen)
            }
            .map { read in read.status.map(\.onScreen) }
            .prefix { read in read[case: \.failure]?.reason.is(\.noProcess) != true }
            .removeDuplicates { previous, next in !ApplicationStatus.changed(previous, next) }
    }
}
