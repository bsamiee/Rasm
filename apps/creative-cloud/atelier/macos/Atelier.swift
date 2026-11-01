import AppleEvents
import ApplicationServices
import Applications
import CurrentProcess
import Encoding
import Foundation
import JSONRPC
import Sparkle
import os

// --- [ERRORS] --------------------------------------------------------------------------

struct AtelierFailure: LocalizedError {
    enum Reason: Sendable {
        case notAdobe
        case noProcess
        case unrendered
    }

    let subject: String
    let reason: Reason

    var errorDescription: String? {
        switch reason {
            case .notAdobe: "\(subject) has no bundle identifier starting \(adobePrefix)"
            case .noProcess: "No running application has Process ID \(subject)"
            case .unrendered: "ScreenCaptureKit returned no SDR image of process \(subject)"
        }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

@main
enum Atelier {
    static func main() async {
        let definitions: ScriptingDefinitions = ScriptingDefinitions()
        let logger: Logger = Logger(for: Self.self)
        let calls: DispatchQueue = DispatchQueue(label: "Atelier.calls", attributes: .concurrent)
        let output: DispatchSerialQueue = DispatchSerialQueue(label: "Atelier.output")
        do {
            try disclaimResponsibility()
            AXUIElementSetMessagingTimeout(AXUIElementCreateSystemWide(), Float(messagingTimeout / .seconds(1)))
            do {
                try checkAccessibilityTrust(prompting: true)
            } catch {
                logger.notice("Started without Accessibility trust, \(error, privacy: .public)")
            }
            let updaterDelegate: UpdaterDelegate = UpdaterDelegate()
            let updater: SPUStandardUpdaterController = SPUStandardUpdaterController(startingUpdater: true, updaterDelegate: updaterDelegate, userDriverDelegate: nil)
            defer { extendLifetime((updater, updaterDelegate)) }
            try await serve(
                reading: FileHandle.standardInput.bytes.jsonLines,
                writing: { line in try await withTaskExecutorPreference(output) { try FileHandle.standardOutput.write(contentsOf: line) } },
                dropped: { failure in logger.debug("Dropped \(failure, privacy: .public)") },
                { (request: Request) in try await withTaskExecutorPreference(calls) { try await request.perform(definitions: definitions) } },
            )
        } catch {
            logger.fault("Atelier stopped with \(error, privacy: .public)")
            exit(EXIT_FAILURE)
        }
    }
}

final class UpdaterDelegate: NSObject, SPUUpdaterDelegate {
    func updater(_: SPUUpdater, willInstallUpdateOnQuit _: SUAppcastItem, immediateInstallationBlock _: () -> Void) -> Bool { true }
}
