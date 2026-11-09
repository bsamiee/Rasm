@unsafe @preconcurrency import Dispatch
import Foundation

// --- [OPERATIONS] ----------------------------------------------------------------------

public func processEndings() -> AsyncStream<Int32?> {
    AsyncStream { ending in
        let signals: [(number: Int32, previous: sig_t?, source: any DispatchSourceSignal)] = [SIGTERM, SIGINT, SIGHUP].map { number in
            let source: any DispatchSourceSignal = DispatchSource.makeSignalSource(signal: number)
            source.setEventHandler { ending.yield(number) }
            return (number, signal(number, SIG_IGN), source)
        }
        let parent: any DispatchSourceProcess = DispatchSource.makeProcessSource(identifier: getppid(), eventMask: .exit)
        parent.setEventHandler { ending.yield(nil) }
        let sources: [any DispatchSourceProtocol] = signals.map(\.source) + [parent]
        for source: any DispatchSourceProtocol in sources {
            source.activate()
        }
        ending.onTermination = { _ in
            for source: any DispatchSourceProtocol in sources {
                source.cancel()
            }
            for (number, previous, _): (Int32, sig_t?, any DispatchSourceSignal) in signals {
                signal(number, previous)
            }
        }
    }
}
