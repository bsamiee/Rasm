import AppKit

// --- [OPERATIONS] ----------------------------------------------------------------------

private func watch(owner: pid_t) -> any DispatchSourceProcess {
    let source: any DispatchSourceProcess = DispatchSource.makeProcessSource(identifier: owner, eventMask: .exit, queue: .global())
    source.setEventHandler { @Sendable in exit(EXIT_SUCCESS) }
    source.activate()
    if kill(owner, 0) != 0 { exit(EXIT_SUCCESS) }
    return source
}

// --- [COMPOSITION] ---------------------------------------------------------------------

@main
enum FixtureApplication {
    @MainActor
    static func main() {
        UserDefaults.standard.register(defaults: ["ApplePersistenceIgnoreState": true])
        let settings: Settings = Settings(defaults: .standard)
        let watchdog: any DispatchSourceProcess? = settings.owner.map(watch)
        switch settings.role {
            case .helper: withExtendedLifetime(watchdog) { dispatchMain() }
            case .application:
                let application: NSApplication = NSApplication.shared
                let delegate: ApplicationDelegate = ApplicationDelegate(settings: settings)
                application.setActivationPolicy(.regular)
                application.delegate = delegate
                withExtendedLifetime((watchdog, delegate)) { application.run() }
        }
    }
}
