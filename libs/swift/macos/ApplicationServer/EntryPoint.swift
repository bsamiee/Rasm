import AppKit
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
    let installation = Result { () throws(ApplicationFailure) in try Installation(at: Bundle.main.bundleURL) }
    let agents = installation.flatMap { bundle in Result { () throws(ApplicationFailure) in try bundle.agents() } }
    let (lines, received) = AsyncThrowingStream<Data, any Error>.makeStream()
    let registration = ClosureService {
        do {
            var registered: [Result<Void, ApplicationFailure>] = []
            for agent in try agents.get() { registered.append(await Result { () async throws(ApplicationFailure) in try await agent.register() }) }
            _ = try traverse(registered) { outcome in outcome.aggregated(\.self) }.get()
        } catch let refused as AggregateError<ApplicationFailure> where refused.errors.allSatisfy({ failure in failure.reason.is(\.awaitsLoginItemApproval) }) {
            SMAppService.openSystemSettingsLoginItems()
            logger.notice("Server agent awaits approval", error: refused)
        }
    }
    let input = ClosureService {
        do {
            for try await line in FileHandle.standardInput.bytes.jsonLines.cancelOnGracefulShutdown() { received.yield(line) }
            received.finish()
        } catch {
            received.finish(throwing: error)
        }
    }
    let requests = ClosureService { @MainActor in
        do {
            try disclaimed.get()
            let claim = try installation.get().claim
            do { try Program.current.require() } catch { logger.notice("Started without every privacy grant", error: error) }
            NSApplication.loadApplication()
            do { try PermittedFrontList.recover() } catch { logger.error("Permitted front list recovery failed", error: error) }
            let (updater, dictionaries, watches) = (Updater(), ScriptingDictionaries(cachedDefinition: cachedDefinition), Watches())
            defer { extendLifetime(updater) }
            let (calls, output) = (DispatchQueue(label: "\(Request.self).calls", attributes: .concurrent), DispatchSerialQueue(label: "\(Request.self).output"))
            let matching: @Sendable () -> NSPredicate = { claim?.predicate ?? NSPredicate(value: true) }
            let served = await Result {
                try await JSONRPC.serve(
                    reading: lines,
                    writing: { line in _ = try await withTaskExecutorPreference(output) { try FileDescriptor.standardOutput.writeAll(line) } },
                    onDrop: { failure in logger.debug("Dropped request", error: failure) },
                    { (request: Request) in try await withTaskExecutorPreference(calls) { try await request.perform(matching: matching, dictionaries: dictionaries, watches: watches) } },
                )
            }
            await withTaskCancellationShield { await watches.finish() }
            try served.get()
        } catch let failure as JSONRPCFailure where failure.reason.is(\.cancelled) {
            logger.info("Request serving canceled")
        }
    }
    let services =
        [ServiceGroupConfiguration.ServiceConfiguration(service: telemetry)] + (serving ? [.init(service: MetricKitReports(to: logger))] : [])
        + (serving ? [requests, input] : [registration]).map { service in
            ServiceGroupConfiguration.ServiceConfiguration(service: service, successTerminationBehavior: .gracefullyShutdownGroup, failureTerminationBehavior: .gracefullyShutdownGroup)
        }
    do {
        var configuration = ServiceGroupConfiguration(services: services, gracefulShutdownSignals: [.sigterm, .sigint], logger: logger)
        configuration.maximumGracefulShutdownDuration = try agents.get().compactMap(\.exitTimeOut).min().map { (bound: Duration) -> Duration in bound / 2 }
        let group = ServiceGroup(configuration: configuration)
        try await InstrumentationContext.withValue(context) { try await group.run() }
    } catch {
        logger.critical("Server failed", error: error)
        exit(EXIT_FAILURE)
    }
}
