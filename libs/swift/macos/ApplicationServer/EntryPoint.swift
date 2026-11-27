package import Applications
import CasePathsCore
import CurrentProcess
import Encoding
package import Foundation
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

package func main(cachedDefinition: @escaping @Sendable (Installation) -> URL? = { _ in nil }) async {
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
            _ = try traverse(registered) { outcome in outcome.aggregated(\.self) }.get()
        } catch let refused as AggregateError<ApplicationFailure> where refused.errors.allSatisfy({ failure in failure.reason.is(\.requiresApproval) }) {
            SMAppService.openSystemSettingsLoginItems()
            logger.notice("Server agent awaits approval", error: refused)
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
            let (updater, dictionaries, watches) = (Updater(), ScriptingDictionaries(cachedDefinition: cachedDefinition), Watches())
            defer { extendLifetime(updater) }
            let (calls, output) = (DispatchQueue(label: "\(Request.self).calls", attributes: .concurrent), DispatchSerialQueue(label: "\(Request.self).output"))
            let matching: @Sendable () -> NSPredicate = { claim?.predicate ?? NSPredicate(value: true) }
            let served = await Result {
                try await cancelWhenGracefulShutdown {
                    try await JSONRPC.serve(
                        reading: FileHandle.standardInput.bytes.jsonLines,
                        writing: { line in _ = try await withTaskExecutorPreference(output) { try FileDescriptor.standardOutput.writeAll(line) } },
                        onDrop: { failure in logger.debug("Dropped request", error: failure) },
                        { (request: Request) in try await withTaskExecutorPreference(calls) { try await request.perform(matching: matching, dictionaries: dictionaries, watches: watches) } },
                    )
                }
            }
            await withTaskCancellationShield { await watches.finish() }
            try served.get()
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
