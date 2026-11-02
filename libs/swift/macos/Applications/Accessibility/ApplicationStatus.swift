import AppKit
import ApplicationServices
import AsyncAlgorithms
public import CasePaths
public import CoreGraphics
public import Processes
import Synchronization

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
    case launching
    case responding(elements: [Element], windows: [Window])
    case unresponsive(process: ProcessStatus, windows: [Window])
}

// --- [SERVICES] ------------------------------------------------------------------------

final class Observer: Sendable {
    let instance: NSRunningApplication
    let ticks: AsyncStream<Void>.Continuation
    let observed: Mutex<(observer: AXObserver, unregistered: [String], launch: NSKeyValueObservation?)>

    init(_ instance: NSRunningApplication, notifications: Set<String>, ticks: AsyncStream<Void>.Continuation) throws(ApplicationFailure) {
        let element: AXUIElement = AXUIElementCreateApplication(instance.processIdentifier)
        var created: AXObserver?
        let creation: AXError = unsafe AXObserverCreate(
            instance.processIdentifier,
            { _, _, _, refcon in
                guard let refcon: UnsafeMutableRawPointer = unsafe refcon else { return }
                unsafe Unmanaged<Observer>.fromOpaque(refcon).takeUnretainedValue().ticks.yield(())
            },
            &created,
        )
        guard let observer: AXObserver = created else { throw element.failure(creation, reading: ["AXObserverCreate"]) }
        CFRunLoopAddSource(CFRunLoopGetMain(), AXObserverGetRunLoopSource(observer), .defaultMode)
        self.instance = instance
        self.ticks = ticks
        observed = Mutex((observer, Array(notifications), instance.observe(\.isFinishedLaunching) { _, _ in ticks.yield(()) }))
    }

    func register() throws(ApplicationFailure) {
        let element: AXUIElement = AXUIElementCreateApplication(instance.processIdentifier)
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

extension AXUIElement {
    func windows() throws(ApplicationFailure) -> [Data?] {
        let read: [String: CFTypeRef] = try values([kAXWindowsAttribute, kAXFocusedWindowAttribute])
        return (read[kAXWindowsAttribute] as? [CFTypeRef] ?? []).compactMap(Self.checked).map(\.token) + [read[kAXFocusedWindowAttribute].flatMap(Self.checked)?.token]
    }
}

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
        await withTaskExecutorPreference(accessibility) {
            let owned: [Window] = Window.owned(by: processIdentifier)
            let responding: ([Element]) -> ApplicationStatus = { elements in
                let described: Set<CGWindowID> = Set(elements.compactMap(\.window))
                return .responding(elements: elements, windows: owned.filter { window in !described.contains(window.number) })
            }
            let read: Result<(status: ApplicationStatus, windows: [Data?]?), ApplicationFailure> = Result { () throws(ApplicationFailure) in
                guard try processStatus() != .stopped else { return (.unresponsive(process: .stopped, windows: owned), nil) }
                try observer?.get().register()
                let application: AXUIElement = AXUIElementCreateApplication(processIdentifier)
                let windows: [Data?] = try application.windows()
                if let last, last.windows == windows { return (responding(last.elements), windows) }
                let elements: [Element] = try application.candidates().map { candidate throws(ApplicationFailure) in try candidate.described(walking: walked) }.compactMap(\.self)
                return (isFinishedLaunching || elements.contains(where: \.requiresAnswer) ? responding(elements) : .launching, windows)
            }
            let status: Result<ApplicationStatus, ApplicationFailure> = read.map(\.status).flatMapError { failure in
                failure.reason.is(\.unanswered)
                    ? Result { () throws(ApplicationFailure) in isFinishedLaunching ? .unresponsive(process: try processStatus(), windows: owned) : .launching } : .failure(failure)
            }
            return switch read {
                case .success(let read): (status, read.windows)
                case .failure: (status, nil)
            }
        }
    }

    func statusChanges(
        walking walked: @escaping @Sendable (Element) -> Bool,
        observer: Result<Observer, ApplicationFailure>,
        ticks: AsyncStream<Void>,
        polling interval: Duration,
        clock: some Clock<Duration> & Sendable,
    ) -> some AsyncSequence<Result<ApplicationStatus, ApplicationFailure>, Never> {
        let reason: @Sendable (ApplicationFailure) -> PartialCaseKeyPath<ApplicationFailure.Reason> = { failure in ApplicationFailure.Reason.allCasePaths[failure.reason] }
        return merge(chain([()].async, ticks).map { _ in false }, AsyncTimerSequence(interval: interval, clock: clock).map { _ in true })
            .reductions((status: Result<ApplicationStatus, ApplicationFailure>.success(.launching), windows: [Data?]?.none)) { last, beat in
                let seen: (elements: [Element], windows: [Data?])? =
                    switch (beat, last.status, last.windows) {
                        case (true, .success(.responding(let elements, _)), .some(let windows)): (elements, windows)
                        case (_, _, _): nil
                    }
                return await self.status(walking: walked, observer: observer, after: seen)
            }
            .map(\.status)
            .prefix { read in read[case: \.failure]?.reason.is(\.noProcess) != true }
            .removeDuplicates { last, next in
                switch (last, next) {
                    case (.success(let last), .success(let next)): last == next
                    case (.failure(let last), .failure(let next)): last.subject == next.subject && reason(last) == reason(next)
                    case (_, _): false
                }
            }
    }
}
