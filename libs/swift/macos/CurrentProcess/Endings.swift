import Dispatch
import Foundation
import Synchronization

// --- [OPERATIONS] ----------------------------------------------------------------------

public func processEndings() -> AsyncStream<Int32?> {
    AsyncStream { ending in
        let parent = DispatchSource.makeProcessSource(identifier: getppid(), eventMask: .exit)
        parent.setEventHandler { ending.yield(nil) }
        let signals = [SIGTERM, SIGINT, SIGHUP].map { number in
            let source = DispatchSource.makeSignalSource(signal: number)
            source.setEventHandler { ending.yield(number) }
            return (number: number, previous: signal(number, SIG_IGN), source: source)
        }
        for source: any DispatchSourceProtocol in signals.map(\.source) + [parent] { source.activate() }
        let sources = Mutex((parent: parent, signals: signals))
        ending.onTermination = { _ in
            sources.withLock { held in
                held.parent.cancel()
                for (number, previous, source) in held.signals {
                    source.cancel()
                    signal(number, previous)
                }
            }
        }
    }
}
