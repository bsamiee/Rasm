public import AppKit
import ApplicationServices
import Encoding
import Results

// --- [MODELS] --------------------------------------------------------------------------

public enum ApplicationStatus: Equatable, Sendable {
    case responding([Element])
    case unresponsive
}

extension ApplicationStatus: Encodable {
    private enum Key: CodingKey {
        case elements
    }

    public func encode(to encoder: any Encoder) throws {
        switch self {
            case .responding(let elements): try encoder.encode(case: "responding", keyedBy: Key.self) { fields in try fields.encode(elements, forKey: .elements) }
            case .unresponsive: try encoder.encode(case: "unresponsive", keyedBy: Key.self)
        }
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

public struct StatusChanges: AsyncSequence {
    public struct Iterator: AsyncIteratorProtocol {
        private enum Phase {
            case opening(Result<Observation, ElementFailure>)
            case reading(Observation, ApplicationStatus)
            case finished
        }

        let application: NSRunningApplication
        private var phase: Phase

        init(_ application: NSRunningApplication) {
            self.application = application
            phase = .opening(Result { () throws(ElementFailure) in try Observation(application) })
        }

        public mutating func next() async -> Result<ApplicationStatus, ElementFailure>? {
            let read: Result<(Observation, ApplicationStatus), ElementFailure>? =
                switch (phase, application.isTerminated) {
                    case (.finished, _), (_, true): nil
                    case (.opening(.failure(let error)), false): .failure(error)
                    case (.opening(.success(let observation)), false): await application.status().map { status in (observation, status) }
                    case (.reading(let observation, let last), false): await observation.change(from: last, of: application).map { read in read.map { status in (observation, status) } }
                }
            phase =
                switch read {
                    case .success((let observation, let status)): .reading(observation, status)
                    case .failure, .none: .finished
                }
            return read.map { read in read.map(\.1) }
        }
    }

    let application: NSRunningApplication

    public init(_ application: NSRunningApplication) {
        self.application = application
    }

    public func makeAsyncIterator() -> Iterator {
        Iterator(application)
    }
}

private final class Ticks: Sendable {
    let continuation: AsyncStream<Void>.Continuation

    init(continuation: AsyncStream<Void>.Continuation) {
        self.continuation = continuation
    }
}

private final class Observation {
    let observer: AXObserver
    let ticks: Ticks
    let lifetime: NSKeyValueObservation
    var pending: AsyncStream<Void>.Iterator

    init(_ application: NSRunningApplication) throws(ElementFailure) {
        let names: [String] = [
            kAXWindowCreatedNotification, kAXSheetCreatedNotification, kAXMenuOpenedNotification, kAXMenuClosedNotification, kAXUIElementDestroyedNotification,
            kAXFocusedWindowChangedNotification, kAXMainWindowChangedNotification, kAXTitleChangedNotification, kAXWindowMiniaturizedNotification,
            kAXWindowDeminiaturizedNotification, kAXApplicationHiddenNotification, kAXApplicationShownNotification, kAXValueChangedNotification,
        ]
        let element: AXUIElement = AXUIElementCreateApplication(application.processIdentifier)
        let (stream, tick): (AsyncStream<Void>, AsyncStream<Void>.Continuation) = AsyncStream.makeStream(bufferingPolicy: .bufferingNewest(1))
        var created: AXObserver?
        let creation: AXError = unsafe AXObserverCreate(application.processIdentifier, observed, &created)
        guard let observer: AXObserver = created else { throw .reading(creation, ["AXObserverCreate"], on: element) }
        self.observer = observer
        ticks = Ticks(continuation: tick)
        lifetime = application.observe(\.isTerminated, options: [.initial, .new]) { instance, _ in
            if instance.isTerminated { tick.finish() }
        }
        pending = stream.makeAsyncIterator()
        _ = try traverse(names) { name in
            let added: AXError = unsafe AXObserverAddNotification(observer, element, name as CFString, Unmanaged.passUnretained(ticks).toOpaque())
            return added == .success ? .success(()) : .failure(AggregateError(first: (notification: name, error: added)))
        }
        .mapError { refusals -> ElementFailure in refusals.first.error == .apiDisabled ? .untrusted : .unobservable(refusals) }.get()
        CFRunLoopAddSource(CFRunLoopGetMain(), AXObserverGetRunLoopSource(observer), .defaultMode)
    }

    deinit {
        CFRunLoopRemoveSource(CFRunLoopGetMain(), AXObserverGetRunLoopSource(observer), .defaultMode)
        lifetime.invalidate()
        ticks.continuation.finish()
    }

    func change(from last: ApplicationStatus, of application: NSRunningApplication) async -> Result<ApplicationStatus, ElementFailure>? {
        while await pending.next(isolation: #isolation) != nil, !application.isTerminated {
            switch await application.status() {
                case .success(last): continue
                case let read: return read
            }
        }
        return nil
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSRunningApplication {
    @concurrent
    public func status() async -> Result<ApplicationStatus, ElementFailure> {
        await withTaskExecutorPreference(accessibility) {
            let application: AXUIElement = AXUIElementCreateApplication(processIdentifier)
            let read: Result<ApplicationStatus, ElementFailure> = Result { () throws(ElementFailure) in
                try .responding(application.candidates().map { candidate throws(ElementFailure) in try candidate.described()?.element }.compactMap(\.self))
            }
            return switch read {
                case .failure(.unanswered): .success(.unresponsive)
                case .success, .failure: read
            }
        }
    }
}

private let observed: AXObserverCallback = { _, _, _, refcon in
    guard let refcon: UnsafeMutableRawPointer = unsafe refcon else { return }
    unsafe Unmanaged<Ticks>.fromOpaque(refcon).takeUnretainedValue().continuation.yield(())
}
