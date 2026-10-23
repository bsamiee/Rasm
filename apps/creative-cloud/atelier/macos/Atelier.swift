import AdobeAutomation
import AppleEvents
import ApplicationServices
import CurrentProcess
import EffectRPC
import Foundation
import Sparkle
import os

// --- [COMPOSITION] ---------------------------------------------------------------------

@main
enum Atelier {
    static func main() async {
        let definitions: ScriptingDefinitions = ScriptingDefinitions()
        do {
            try disclaimResponsibility()
            AXIsProcessTrustedWithOptions(["AXTrustedCheckOptionPrompt": true] as CFDictionary)
            let updaterDelegate: UpdaterDelegate = UpdaterDelegate()
            let updater: SPUStandardUpdaterController = SPUStandardUpdaterController(startingUpdater: true, updaterDelegate: updaterDelegate, userDriverDelegate: nil)
            defer { extendLifetime((updater, updaterDelegate)) }
            try await EffectRPC.serve(reading: .standardInput, writing: .standardOutput) { (request: Request) in await request.perform(definitions: definitions) }
        } catch {
            Logger(for: Self.self).fault("Atelier stopped with \(error, privacy: .public)")
            exit(EXIT_FAILURE)
        }
    }
}

final class UpdaterDelegate: NSObject, SPUUpdaterDelegate {
    func updater(_: SPUUpdater, willInstallUpdateOnQuit _: SUAppcastItem, immediateInstallationBlock _: () -> Void) -> Bool { true }
}
