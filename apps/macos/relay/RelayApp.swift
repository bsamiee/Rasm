import AppKit
import CasePathsCore
import CurrentProcess
import Logging
import Observation
import Processes
import ServiceLifecycle
import Sparkle
import SwiftUI
import Telemetry

// --- [COMPOSITION] ---------------------------------------------------------------------

@main
struct RelayApp: App {
    static let settingsWindow: String = "settings"

    @NSApplicationDelegateAdaptor(AppDelegate.self) private var delegate: AppDelegate
    @SwiftUI.Environment(\.openWindow) private var openWindow: OpenWindowAction
    private let userDriverDelegate: UserDriverDelegate = .init()
    private let updater: SPUStandardUpdaterController

    init() {
        updater = SPUStandardUpdaterController(startingUpdater: true, updaterDelegate: nil, userDriverDelegate: userDriverDelegate)
    }

    var body: some Scene {
        MenuBarExtra {
            MenuBarExtraContent(store: delegate.store, updater: updater.updater)
        } label: {
            Image(.relaySymbol)
                .accessibilityLabel("Relay")
        }
        .menuBarExtraStyle(.window)

        Window("Settings", id: Self.settingsWindow) {
            switch delegate.store {
                case .success(let store): SettingsView(store: store)
                case .failure: EmptyView()
                case .none: ProgressView().controlSize(.small)
            }
        }
        .defaultSize(width: 680, height: 460)
        .windowResizability(.contentMinSize)
        .windowToolbarStyle(.unified)
        .commands {
            CommandGroup(replacing: .appSettings) {
                Button("Settings") { openWindow(id: Self.settingsWindow) }
                    .keyboardShortcut(",", modifiers: .command)
            }
        }
    }
}

@Observable
final class AppDelegate: NSObject, NSApplicationDelegate {
    private(set) var store: Result<AccountStore, ProcessFailure>?
    private let logger: Logger
    private let services: ServiceGroup
    @ObservationIgnored private var launch: Task<Void, Never>?
    @ObservationIgnored private var running: Task<Void, Never>?

    override init() {
        let telemetry = Program.current.telemetry(scopes: [AppDelegate.self])
        logger = Logger(label: String(reflecting: AppDelegate.self))
        services = ServiceGroup(services: [telemetry, MetricKitReports(to: logger)], logger: logger)
        super.init()
    }

    func applicationDidFinishLaunching(_: Notification) {
        running = Task(name: "Telemetry") { [services, logger] in
            do { try await services.run() } catch { logger.error("Telemetry services failed", error: error) }
        }
        launch = Task(name: "Launch Relay") { [self] in
            switch await Result(catching: { () async throws(ProcessFailure) in try await loginShellEnvironment(within: .seconds(5)) }).map(AccountStore.init(environment:)) {
                case .failure(let failure) where failure.reason.is(\.cancelled): logger.info("Quit canceled launch during login shell read")
                case let launched:
                    store = launched
                    switch launched {
                        case .success(let started): started.start()
                        case .failure(let error): logger.error("Login shell environment read failed", error: error)
                    }
            }
        }
    }

    func applicationShouldTerminate(_ sender: NSApplication) -> NSApplication.TerminateReply {
        Task(name: "Quit Relay") { [self] in
            launch?.cancel()
            await launch?.value
            if case .success(let launched) = store { await launched.stop() }
            await services.triggerGracefulShutdown()
            await running?.value
            sender.reply(toApplicationShouldTerminate: true)
        }
        return .terminateLater
    }
}

final class UserDriverDelegate: NSObject, SPUStandardUserDriverDelegate {
    var supportsGentleScheduledUpdateReminders: Bool { true }

    func standardUserDriverWillHandleShowingUpdate(_: Bool, forUpdate _: SUAppcastItem, state: SPUUserUpdateState) {
        NSApplication.shared.setActivationPolicy(.regular)
        if !state.userInitiated { NSApplication.shared.dockTile.badgeLabel = "1" }
    }

    func standardUserDriverWillFinishUpdateSession() {
        NSApplication.shared.dockTile.badgeLabel = nil
        NSApplication.shared.setActivationPolicy(.accessory)
    }
}
