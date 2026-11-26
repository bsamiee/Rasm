import AppKit
public import Applications
import CurrentProcess
import Encoding
public import Foundation
import JSONRPC
import Logging
import Results
import ServiceLifecycle
import ServiceManagement
import System
import Telemetry
import Tracing
import UnixSignals
import Updater

// --- [COMPOSITION] ---------------------------------------------------------------------

public func main(cachedDefinition: @escaping @Sendable (Installation) -> URL?) async {
    let serving = Result { () throws(Errno) in try FileDescriptor.standardInput.stat() }.map { stat in [FileType.fifo, .socket].contains(stat.type) } == .success(true)
    let disclaimed = Result { () throws(Errno) in if serving { try disclaimResponsibility() } }
    let telemetry = Program.current.telemetry(scopes: [Request.self])
    let logger = Logger(label: String(reflecting: Request.self))
    let work = ClosureService { @MainActor in
        do {
            try disclaimed.get()
            let installation = try Installation(at: Bundle.main.bundleURL)
            guard serving else {
                let agents = try installation.agents()
                var registered: [Result<Void, ApplicationFailure>] = []
                for agent in agents where agent.service.status != .requiresApproval {
                    registered.append(await Result { () async throws(ApplicationFailure) in try await agent.register() })
                }
                _ = try traverse(registered) { outcome in outcome.aggregated(\.self) }.get()
                if agents.contains(where: { agent in agent.service.status == .requiresApproval }) { SMAppService.openSystemSettingsLoginItems() }
                return
            }
            do { try Program.current.require() } catch { logger.notice("Started without every privacy grant", error: error) }
            let updater = Updater(activationPolicy: .prohibited)
            defer { extendLifetime(updater) }
            var context = InstrumentationContext.topLevel
            InstrumentationSystem.instrument.extract(ProcessInfo.processInfo.environment, into: &context, using: EnvironmentCarrier())
            let claimed = installation.claim?.prefixes
            try await cancelWhenGracefulShutdown { [context] in
                try await InstrumentationContext.withValue(context) {
                    try await serve(
                        reading: FileHandle.standardInput.bytes.jsonLines,
                        writing: { line in try FileDescriptor.standardOutput.writeAll(line) },
                        matching: {
                            claimed.map { prefixes in NSCompoundPredicate(orPredicateWithSubpredicates: prefixes.map { prefix in NSPredicate(format: "%K BEGINSWITH %@", identifierKey, prefix) }) }
                                ?? NSPredicate(value: true)
                        },
                        cachedDefinition: cachedDefinition,
                    )
                }
            }
        } catch {
            guard case .cancelled? = (error as? JSONRPCFailure)?.reason else {
                logger.critical(serving ? "Request serving failed" : "Server agent registration failed", error: error)
                throw error
            }
            logger.info("Request serving canceled")
        }
    }
    let services =
        [ServiceGroupConfiguration.ServiceConfiguration(service: telemetry)] + (serving ? [.init(service: MetricKitReports(to: logger))] : [])
        + [.init(service: work, successTerminationBehavior: .gracefullyShutdownGroup, failureTerminationBehavior: .gracefullyShutdownGroup)]
    do { try await ServiceGroup(configuration: ServiceGroupConfiguration(services: services, gracefulShutdownSignals: [.sigterm, .sigint], logger: logger)).run() } catch { exit(EXIT_FAILURE) }
}
