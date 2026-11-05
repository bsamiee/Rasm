@unsafe @preconcurrency import Dispatch
import Foundation

// --- [OPERATIONS] ----------------------------------------------------------------------

public func processEndings() -> AsyncStream<Int32?> {
    AsyncStream { ending in
        let sources: [any DispatchSourceProtocol] =
            [SIGTERM, SIGINT, SIGHUP].map { number in
                signal(number, SIG_IGN)
                let source: any DispatchSourceSignal = DispatchSource.makeSignalSource(signal: number)
                source.setEventHandler { ending.yield(number) }
                return source
            } + [DispatchSource.makeProcessSource(identifier: getppid(), eventMask: .exit)]
        sources.last?.setEventHandler { ending.yield(nil) }
        for source: any DispatchSourceProtocol in sources {
            source.activate()
        }
        ending.onTermination = { _ in
            for source: any DispatchSourceProtocol in sources {
                source.cancel()
            }
        }
    }
}
