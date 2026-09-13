import AppKit
import Foundation

struct CodexDesktopInstance: Codable, Sendable {
  let accountID: UUID
  let processID: Int32
  let launchedAt: Date
}

struct CodexDesktopState: Codable, Sendable {
  let instances: [CodexDesktopInstance]
  let selectedAccountID: UUID?
}

@MainActor
enum CodexDesktop {
  static func applicationURL() -> Result<URL, CodexFailure> {
    guard
      let application: URL = NSWorkspace.shared.urlForApplication(
        withBundleIdentifier: "com.openai.codex")
    else {
      return .failure(.applicationUnavailable)
    }
    return .success(application)
  }

  static func isRunning(_ instance: CodexDesktopInstance) -> Bool {
    guard
      let application: NSRunningApplication = NSRunningApplication(
        processIdentifier: instance.processID)
    else {
      return false
    }
    return application.bundleIdentifier == "com.openai.codex"
      && !application.isTerminated
      && application.launchDate == instance.launchedAt
  }

  static func foregroundAccount(in instances: [CodexDesktopInstance]) -> UUID? {
    guard let frontmost: NSRunningApplication = NSWorkspace.shared.frontmostApplication else {
      return nil
    }
    return instances.first { instance in
      instance.processID == frontmost.processIdentifier && isRunning(instance)
    }?.accountID
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
      guard application.activate(options: [.activateAllWindows]) else {
        return .failure(.desktopUnavailable)
      }
      return .success(existing)
    }
    let application: URL
    switch applicationURL() {
    case .success(let value): application = value
    case .failure(let error): return .failure(error)
    }
    // The desktop's own multi-instance launcher passes this pair with the matching Chromium switch;
    // an explicit user data path is what makes it honor CODEX_HOME and take a per-path instance lock.
    var variables: [String: String] = environment
    variables["CODEX_HOME"] = home.path
    variables["CODEX_ELECTRON_USER_DATA_PATH"] = desktopDirectory.path
    let configuration: NSWorkspace.OpenConfiguration = NSWorkspace.OpenConfiguration()
    configuration.createsNewApplicationInstance = true
    configuration.activates = true
    configuration.environment = variables
    configuration.arguments = ["--user-data-dir=\(desktopDirectory.path)"]
    do {
      let running: NSRunningApplication = try await NSWorkspace.shared.openApplication(
        at: application,
        configuration: configuration
      )
      guard running.bundleIdentifier == "com.openai.codex", !running.isTerminated,
        let launchedAt: Date = running.launchDate
      else { return .failure(.desktopUnavailable) }
      return .success(
        CodexDesktopInstance(
          accountID: accountID,
          processID: running.processIdentifier,
          launchedAt: launchedAt
        ))
    } catch {
      return .failure(.desktopLaunch(error))
    }
  }

  static func openSignIn(_ url: URL) -> Result<Void, CodexFailure> {
    return NSWorkspace.shared.open(url) ? .success(()) : .failure(.signInPageUnopened)
  }
}
