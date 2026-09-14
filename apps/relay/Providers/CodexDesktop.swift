import AppKit
import Foundation

enum CodexDesktop {
  static let bundleIdentifier: String = "com.openai.codex"
  private nonisolated static let quitDeadline: Duration = .seconds(10)

  static func applicationURL() -> Result<URL, CodexFailure> {
    NSWorkspace.shared.urlForApplication(withBundleIdentifier: bundleIdentifier)
      .map { application in .success(application) } ?? .failure(.applicationUnavailable)
  }

  static func runningInstances() -> [NSRunningApplication] {
    NSRunningApplication.runningApplications(withBundleIdentifier: bundleIdentifier)
      .filter { application in !application.isTerminated }
  }

  static func relaunchIfRunning() async -> Result<Void, CodexFailure> {
    let instances: [NSRunningApplication] = runningInstances()
    guard !instances.isEmpty else { return .success(()) }
    for instance: NSRunningApplication in instances {
      if case .failure = await quit(instance) { instance.forceTerminate() }
      guard !Task.isCancelled else { return .failure(.cancelled) }
    }
    return await open()
  }

  private static func quit(_ instance: NSRunningApplication) async -> Result<Void, CodexFailure> {
    let pid: pid_t = instance.processIdentifier
    return await ProcessRun.withDeadline(quitDeadline, timedOut: .timedOut, cancelled: .cancelled) {
      @MainActor in
      let terminations: AsyncCompactMapSequence<NotificationCenter.Notifications, pid_t> =
        terminatedProcessIdentifiers()
      return switch (instance.terminate(), instance.isTerminated) {
      case (false, _): .failure(.desktopQuitRefused)
      case (true, true): .success(())
      case (true, false): await terminations.contains(pid) ? .success(()) : .failure(.cancelled)
      }
    }
  }

  private nonisolated static func terminatedProcessIdentifiers()
    -> AsyncCompactMapSequence<NotificationCenter.Notifications, pid_t>
  {
    NSWorkspace.shared.notificationCenter
      .notifications(named: NSWorkspace.didTerminateApplicationNotification)
      .compactMap { notification in
        (notification.userInfo?[NSWorkspace.applicationUserInfoKey] as? NSRunningApplication)?
          .processIdentifier
      }
  }

  static func open() async -> Result<Void, CodexFailure> {
    let configuration: NSWorkspace.OpenConfiguration = NSWorkspace.OpenConfiguration()
    configuration.activates = true
    return await applicationURL().bind { application in
      await Result {
        try await NSWorkspace.shared.openApplication(at: application, configuration: configuration)
      }
      .mapError(CodexFailure.desktopLaunch)
      .map { _ in () }
    }
  }

  static func openSignIn(_ url: URL) -> Result<Void, CodexFailure> {
    NSWorkspace.shared.open(url) ? .success(()) : .failure(.signInPageUnopened)
  }
}
