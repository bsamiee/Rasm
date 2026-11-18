import AppKit
import ApplicationServer
import Applications
import CoreServices
import CurrentProcess
import Encoding
import Foundation
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

// --- [CONSTANTS] -----------------------------------------------------------------------

private let adobePrefix: String = "com.adobe."

// --- [COMPOSITION] ---------------------------------------------------------------------

@main
enum Atelier {
    // --- [DICTIONARIES]

    @Sendable
    private static func cachedDictionary(of installation: Installation) -> URL? {
        let version = installation.version.map { version in version.split(separator: ".").prefix(2).joined(separator: ".") }
        let caches = URL.cachesDirectory.appending(component: "Adobe \(installation.bundleName ?? installation.name)", directoryHint: .isDirectory)
        return FileManager.default.enumerator(at: caches, includingPropertiesForKeys: nil)?.lazy.compactMap { item in item as? URL }
            .first { file in file.pathExtension == "sdef" && file.deletingLastPathComponent().lastPathComponent == version }
    }

    // --- [ENTRY]

    static func main() async {
        let serving = Result { () throws(Errno) in try FileDescriptor.standardInput.stat() }.map { stat in [FileType.fifo, .socket].contains(stat.type) } == .success(true)
        let disclaimed = Result { () throws(Errno) in if serving { try disclaimResponsibility() } }
        let telemetry = Program.current.telemetry(scopes: [Self.self])
        let logger = Logger(label: String(reflecting: Self.self))
        let work = ClosureService { @MainActor in
            do {
                try disclaimed.get()
                guard serving else {
                    let agents = try Installation(at: Bundle.main.bundleURL).agents()
                    var registered: [Result<Void, ApplicationFailure>] = []
                    for agent in agents where agent.service.status != .requiresApproval {
                        registered.append(await Result { () async throws(ApplicationFailure) in try await agent.register() })
                    }
                    _ = try traverse(registered) { outcome in outcome.aggregated(\.self) }.get()
                    if agents.contains(where: { agent in agent.service.status == .requiresApproval }) { SMAppService.openSystemSettingsLoginItems() }
                    return
                }
                do { try checkAccessibilityTrust(prompting: true) } catch { logger.notice("Started without Accessibility trust", error: error) }
                let updater = Updater(resting: .prohibited)
                defer { extendLifetime(updater) }
                var context = InstrumentationContext.topLevel
                InstrumentationSystem.instrument.extract(ProcessInfo.processInfo.environment, into: &context, using: EnvironmentCarrier())
                try await cancelWhenGracefulShutdown { [context] in
                    try await InstrumentationContext.withValue(context) {
                        try await serve(
                            reading: FileHandle.standardInput.bytes.jsonLines,
                            writing: { line in try FileDescriptor.standardOutput.writeAll(line) },
                            admitting: { NSPredicate(format: "%K BEGINSWITH %@", kMDItemCFBundleIdentifier as String, adobePrefix) },
                            refusing: { _ in [] },
                            cached: cachedDictionary,
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
}
