import Applications
import CoreGraphics
import CurrentProcess
import Encoding
import Foundation
import JSONRPC
import Sparkle
import System
import os

// --- [SERVICES] ------------------------------------------------------------------------

final class UpdaterDelegate: NSObject, SPUUpdaterDelegate {
    func updater(_: SPUUpdater, willInstallUpdateOnQuit _: SUAppcastItem, immediateInstallationBlock: () -> Void) -> Bool {
        immediateInstallationBlock()
        return true
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

@main
enum Atelier {
    static func main() async {
        let logger: Logger = Logger(for: Self.self)
        let input: Result<Stat, Errno> = Result { () throws(Errno) in try FileDescriptor.standardInput.stat() }
        guard case .success(let stat) = input, [FileType.fifo, .socket].contains(stat.type) else {
            do {
                try await registerAgents()
            } catch {
                logger.fault("Atelier left its server unregistered, \(error, privacy: .public)")
                exit(EXIT_FAILURE)
            }
            return
        }
        let definitions: ScriptingDefinitions = ScriptingDefinitions()
        let calls: DispatchQueue = DispatchQueue(label: "Atelier.calls", attributes: .concurrent)
        let output: DispatchSerialQueue = DispatchSerialQueue(label: "Atelier.output")
        let managed: Bool = ProcessInfo.processInfo.environment["MCP_LISTEN"] != nil
        do {
            try disclaimResponsibility()
            do {
                try checkAccessibilityTrust(prompting: managed)
            } catch {
                guard managed else { throw error }
                logger.notice("Started without Accessibility trust, \(error, privacy: .public)")
            }
            let captureGranted: Bool = managed ? CGRequestScreenCaptureAccess() : CGPreflightScreenCaptureAccess()
            if !captureGranted { logger.notice("Started without Screen Recording grant") }
            let updaterDelegate: UpdaterDelegate = UpdaterDelegate()
            let updater: SPUStandardUpdaterController? = managed ? SPUStandardUpdaterController(startingUpdater: true, updaterDelegate: updaterDelegate, userDriverDelegate: nil) : nil
            defer { extendLifetime((updater, updaterDelegate)) }
            try await serve(
                reading: FileHandle.standardInput.bytes.jsonLines,
                writing: { line in try await withTaskExecutorPreference(output) { try FileHandle.standardOutput.write(contentsOf: line) } },
                dropped: { failure in logger.debug("Dropped \(failure, privacy: .public)") },
                { (request: HelperRequest) in try await withTaskExecutorPreference(calls) { try await request.perform(definitions: definitions) } },
            )
        } catch {
            logger.fault("Atelier stopped with \(error, privacy: .public)")
            exit(EXIT_FAILURE)
        }
    }
}
