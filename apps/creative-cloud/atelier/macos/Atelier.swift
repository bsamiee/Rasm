import Applications
import CoreGraphics
import CurrentProcess
import EffectRPC
import Encoding
import Foundation
import Logging
import Results
import ServiceLifecycle
import ServiceManagement
import Sparkle
import System
import Telemetry
import Tracing
import UniformTypeIdentifiers
import UnixSignals

// --- [SERVICES] ------------------------------------------------------------------------

final class UpdaterDelegate: NSObject, SPUUpdaterDelegate {
    func updater(_: SPUUpdater, willInstallUpdateOnQuit _: SUAppcastItem, immediateInstallationBlock: () -> Void) -> Bool {
        immediateInstallationBlock()
        return true
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

private func disclaimResponsibility() throws(Errno) {
    func check(_ status: Int32) throws(Errno) { if status != 0 { throw Errno(rawValue: status) } }
    guard responsiblePID(getpid()) != getpid() else { return }
    guard let executable = Bundle.main.executablePath else { throw .noSuchFileOrDirectory }
    var attributes: posix_spawnattr_t?
    try check(unsafe posix_spawnattr_init(&attributes))
    defer { _ = unsafe posix_spawnattr_destroy(&attributes) }
    try check(unsafe disclaim(&attributes, 1))
    try check(unsafe posix_spawnattr_setflags(&attributes, Int16(POSIX_SPAWN_SETEXEC)))
    try check(unsafe posix_spawn(nil, executable, nil, &attributes, CommandLine.unsafeArgv, environ))
}

@_silgen_name("responsibility_get_pid_responsible_for_pid")
private func responsiblePID(_: pid_t) -> pid_t
@_silgen_name("responsibility_spawnattrs_setdisclaim")
private func disclaim(_: UnsafeMutablePointer<posix_spawnattr_t?>, _: Int32) -> Int32

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
                    let agents = try FileManager.default.contentsOfDirectory(
                        at: Bundle.main.bundleURL.appending(components: "Contents", "Library", "LaunchAgents"),
                        includingPropertiesForKeys: nil,
                    )
                    .filter { file in UTType(filenameExtension: file.pathExtension)?.conforms(to: .propertyList) == true }
                    .map { file in SMAppService.agent(plistName: file.lastPathComponent) }
                    _ = try traverse(agents.filter { agent in agent.status != .requiresApproval }) { agent in Result { try agent.register() }.aggregated(\.self) }.get()
                    if agents.contains(where: { agent in agent.status == .requiresApproval }) { SMAppService.openSystemSettingsLoginItems() }
                    return
                }
                do { try checkAccessibilityTrust(prompting: true) } catch { logger.notice("Started without Accessibility trust", error: error) }
                if !CGRequestScreenCaptureAccess() { logger.notice("Started without Screen Recording grant") }
                let definitions = ScriptingDefinitions()
                let calls = DispatchQueue(label: "Atelier.calls", attributes: .concurrent)
                let output = DispatchSerialQueue(label: "Atelier.output")
                let updaterDelegate = UpdaterDelegate()
                let updater = SPUStandardUpdaterController(startingUpdater: true, updaterDelegate: updaterDelegate, userDriverDelegate: nil)
                defer { extendLifetime((updater, updaterDelegate)) }
                var context = InstrumentationContext.topLevel
                InstrumentationSystem.instrument.extract(ProcessInfo.processInfo.environment, into: &context, using: EnvironmentCarrier())
                try await cancelWhenGracefulShutdown { [context] in
                    try await InstrumentationContext.withValue(context) {
                        try await serve(
                            reading: FileHandle.standardInput.bytes.jsonLines,
                            writing: { line in try await withTaskExecutorPreference(output) { try FileHandle.standardOutput.write(contentsOf: line) } },
                            dropped: { failure in logger.debug("Dropped request", error: failure) },
                            { (request: HelperRequest) in try await withTaskExecutorPreference(calls) { try await request.perform(definitions: definitions) } },
                        )
                    }
                }
            } catch {
                guard case .cancelled? = (error as? EffectRPCFailure)?.reason else {
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
