import AppKit
import Foundation

nonisolated struct CodexDesktopInstance: Codable, Sendable {
  let accountID: UUID
  let processID: Int32
  let launchedAt: Date
}

nonisolated struct CodexDesktopState: Codable, Sendable {
  let instances: [CodexDesktopInstance]
  let selectedAccountID: UUID?
}

enum CodexDesktop {
  static func applicationURL() -> Result<URL, CodexFailure> {
    return NSWorkspace.shared.urlForApplication(withBundleIdentifier: "com.openai.codex")
      .map { application in .success(application) } ?? .failure(.applicationUnavailable)
  }

  static func isRunning(_ instance: CodexDesktopInstance) -> Bool {
    return NSRunningApplication(processIdentifier: instance.processID).map { application in
      application.bundleIdentifier == "com.openai.codex"
        && !application.isTerminated
        && application.launchDate == instance.launchedAt
    } ?? false
  }

  static func frontmostAccount(in instances: [CodexDesktopInstance]) -> UUID? {
    return NSWorkspace.shared.frontmostApplication.flatMap { frontmost in
      instances.first { instance in
        instance.processID == frontmost.processIdentifier && isRunning(instance)
      }?.accountID
    }
  }

  static func open(
    accountID: UUID,
    home: URL,
    desktopDirectory: URL,
    environment: [String: String],
    existing: CodexDesktopInstance?
  ) async -> Result<CodexDesktopInstance, CodexFailure> {
    if let existing, isRunning(existing),
      let application: NSRunningApplication = NSRunningApplication(
        processIdentifier: existing.processID)
    {
      return application.activate(options: [.activateAllWindows])
        ? .success(existing) : .failure(.desktopUnavailable)
    }
    var variables: [String: String] = environment
    variables["CODEX_HOME"] = home.path
    variables["CODEX_ELECTRON_USER_DATA_PATH"] = desktopDirectory.path
    let configuration: NSWorkspace.OpenConfiguration = NSWorkspace.OpenConfiguration()
    configuration.createsNewApplicationInstance = true
    configuration.activates = true
    configuration.environment = variables
    configuration.arguments = ["--user-data-dir=\(desktopDirectory.path)"]
    return await applicationURL().bind { application in
      await Result {
        try await NSWorkspace.shared.openApplication(at: application, configuration: configuration)
      }
      .mapError(CodexFailure.desktopLaunch)
      .flatMap { running in
        guard running.bundleIdentifier == "com.openai.codex", !running.isTerminated,
          let launchedAt: Date = running.launchDate
        else { return .failure(.desktopUnavailable) }
        return .success(
          CodexDesktopInstance(
            accountID: accountID,
            processID: running.processIdentifier,
            launchedAt: launchedAt
          ))
      }
    }
  }

  static func openSignIn(_ url: URL) -> Result<Void, CodexFailure> {
    return NSWorkspace.shared.open(url) ? .success(()) : .failure(.signInPageUnopened)
  }
}
