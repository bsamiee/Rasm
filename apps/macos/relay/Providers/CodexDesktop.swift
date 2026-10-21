import AppKit
import Foundation

// --- [MODELS] --------------------------------------------------------------------------

private nonisolated struct CodexPackage: Decodable {
    let entrypoint: String
}

// --- [OPERATIONS] ----------------------------------------------------------------------

enum CodexDesktop {
    private static let bundleIdentifier: String = "com.openai.codex"

    static func serverExecutable() throws(CodexFailure) -> URL {
        guard let application: URL = NSWorkspace.shared.urlForApplication(withBundleIdentifier: bundleIdentifier) else { throw .applicationUnavailable }
        let package: URL = application.appending(path: "Contents/Resources/codex-cli", directoryHint: .isDirectory)
        let manifest: CodexPackage = try Result { try JSONDecoder().decode(CodexPackage.self, from: Data(contentsOf: package.appending(path: "codex-package.json"))) }
            .mapError(CodexFailure.storage)
            .get()
        return package.appending(path: manifest.entrypoint)
    }

    static func refuseWhileRunning() throws(CodexFailure) {
        guard NSRunningApplication.runningApplications(withBundleIdentifier: bundleIdentifier).allSatisfy(\.isTerminated) else { throw .desktopRunning }
    }

    static func openSignIn(_ url: URL) throws(CodexFailure) {
        guard NSWorkspace.shared.open(url) else { throw .signInPageUnopened }
    }
}
