public import Applications
import CasePathsCore
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
    var context = InstrumentationContext.topLevel
    InstrumentationSystem.instrument.extract(ProcessInfo.processInfo.environment, into: &context, using: EnvironmentCarrier())
    let registration = ClosureService {
        do {
            var registered: [Result<Void, ApplicationFailure>] = []
            for agent in try Installation(at: Bundle.main.bundleURL).agents() { registered.append(await Result { () async throws(ApplicationFailure) in try await agent.register() }) }
            let outcome = traverse(registered) { outcome in outcome.aggregated(\.self) }
            if case .failure(let refused) = outcome, refused.errors.contains(where: { failure in failure.reason == .requiresApproval }) { SMAppService.openSystemSettingsLoginItems() }
            _ = try outcome.get()
        } catch {
            logger.critical("Server agent registration failed", error: error)
            throw error
        }
    }
    let requests = ClosureService { @MainActor in
        do {
            try disclaimed.get()
            let claim = try Installation(at: Bundle.main.bundleURL).claim
            do { try Program.current.require() } catch { logger.notice("Started without every privacy grant", error: error) }
            do { try PermittedFrontList.recover() } catch { logger.error("Permitted front list recovery failed", error: error) }
            let updater = Updater()
            defer { extendLifetime(updater) }
            try await cancelWhenGracefulShutdown {
                try await serve(
                    reading: FileHandle.standardInput.bytes.jsonLines,
                    writing: { line in try FileDescriptor.standardOutput.writeAll(line) },
                    matching: { claim?.predicate ?? NSPredicate(value: true) },
                    cachedDefinition: cachedDefinition,
                )
            }
        } catch let failure as JSONRPCFailure where failure.reason.is(\.cancelled) {
            logger.info("Request serving canceled")
        } catch {
            logger.critical("Request serving failed", error: error)
            throw error
        }
    }
    let services =
        [ServiceGroupConfiguration.ServiceConfiguration(service: telemetry)] + (serving ? [.init(service: MetricKitReports(to: logger))] : [])
        + [.init(service: serving ? requests : registration, successTerminationBehavior: .gracefullyShutdownGroup, failureTerminationBehavior: .gracefullyShutdownGroup)]
    let group = ServiceGroup(configuration: ServiceGroupConfiguration(services: services, gracefulShutdownSignals: [.sigterm, .sigint], logger: logger))
    do { try await InstrumentationContext.withValue(context) { try await group.run() } } catch { exit(EXIT_FAILURE) }
}
