import AppKit
import AppleEvents
import ApplicationServer
import Applications
import CoreServices
import CurrentProcess
import Encoding
import Fonts
import Foundation
import JSONRPC
import Logging
import Results
import ScreenCapture
import ServiceLifecycle
import ServiceManagement
import System
import Telemetry
import Tracing
import UnixSignals
import Updater

// --- [CONSTANTS] -----------------------------------------------------------------------

private let adobePrefix: String = "com.adobe."
private let activatingCommands: KeyValuePairs<String, [String]> = [
    "\(adobePrefix)InDesign": ["UIUIshow", "K2LKrvlO", "K2LKrvlB"],
    "\(adobePrefix)Acrobat.Pro": ["CARObfrt"],
    "\(adobePrefix)PremierePro": ["CR8RCRPr"],
]

// --- [MODELS] --------------------------------------------------------------------------

private enum FaceRequest: Decodable, Sendable {
    case faces
    case face(name: String)
}

// --- [COMPOSITION] ---------------------------------------------------------------------

@main
enum Atelier {
    static func main() async {
        let serving = Result { () throws(Errno) in try FileDescriptor.standardInput.stat() }.map { stat in [FileType.fifo, .socket].contains(stat.type) } == .success(true)
        let disclaimed = Result { () throws(Errno) in if serving { try disclaimResponsibility() } }
        let telemetry = Program(name: Bundle.main.bundleIdentifier).telemetry(scopes: [Self.self])
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
                do { try checkCaptureAccess(prompting: true) } catch { logger.notice("Started without Screen Recording grant", error: error) }
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
                            refusing: { installation in Set(activatingCommands.first { stem, _ in installation.identifier.hasPrefix(stem) }?.value.compactMap(UInt64.init(sdef:)) ?? []) },
                        ) { (request: FaceRequest) in
                            switch request {
                                case .faces: .value(FontFace.available())
                                case .face(let name): try .value(FontFace(named: name))
                            }
                        }
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
