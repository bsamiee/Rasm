import AppKit
import Foundation

// --- [MODELS] --------------------------------------------------------------------------

private nonisolated struct CodexPackage: Decodable {
    let entrypoint: String
}

// --- [OPERATIONS] ----------------------------------------------------------------------

enum CodexDesktop {
    private static let bundleIdentifier: String = "com.openai.codex"

    static func applicationURL() -> Result<URL, CodexFailure> {
        NSWorkspace.shared.urlForApplication(withBundleIdentifier: bundleIdentifier)
            .map(Result.success) ?? .failure(.applicationUnavailable)
    }

    static func serverExecutable() -> Result<URL, CodexFailure> {
        applicationURL().flatMap { application in
            let package: URL = application.appending(path: "Contents/Resources/codex-cli", directoryHint: .isDirectory)
            return Result {
                try JSONDecoder().decode(CodexPackage.self, from: Data(contentsOf: package.appending(path: "codex-package.json")))
            }
            .map { manifest in package.appending(path: manifest.entrypoint) }
            .mapError { _ in .applicationUnavailable }
        }
    }

    static func relaunchIfRunning() async -> Result<Void, CodexFailure> {
        let instances: [NSRunningApplication] = NSRunningApplication.runningApplications(
            withBundleIdentifier: bundleIdentifier
        )
        .filter { application in !application.isTerminated }
        guard !instances.isEmpty else { return .success(()) }
        for instance: NSRunningApplication in instances {
            if case .failure = await quit(instance) { instance.forceTerminate() }
            guard !Task.isCancelled else { return .failure(.cancelled) }
        }
        return await applicationURL().bind { application in
            await Result {
                try await NSWorkspace.shared.openApplication(at: application, configuration: NSWorkspace.OpenConfiguration())
            }
            .mapError(CodexFailure.desktopLaunch)
            .map { _ in () }
        }
    }

    static func openSignIn(_ url: URL) -> Result<Void, CodexFailure> {
        NSWorkspace.shared.open(url) ? .success(()) : .failure(.signInPageUnopened)
    }

    private static func quit(_ instance: NSRunningApplication) async -> Result<Void, CodexFailure> {
        let pid: pid_t = instance.processIdentifier
        return await ProcessRun.withDeadline(.seconds(10)) {
            @MainActor in
            let terminations: AsyncCompactMapSequence<NotificationCenter.Notifications, pid_t> =
                NSWorkspace.shared.notificationCenter
                .notifications(named: NSWorkspace.didTerminateApplicationNotification)
                .compactMap { notification in
                    (notification.userInfo?[NSWorkspace.applicationUserInfoKey] as? NSRunningApplication)?
                        .processIdentifier
                }
            return switch (instance.terminate(), instance.isTerminated) {
                case (false, _): .failure(.desktopQuitRefused)
                case (true, true): .success(())
                case (true, false): await terminations.contains(pid) ? .success(()) : .failure(.cancelled)
            }
        }
    }
}
