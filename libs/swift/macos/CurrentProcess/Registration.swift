package import Applications
import Configuration
import Foundation
import Logging
import ServiceManagement
import Telemetry

// --- [OPERATIONS] ----------------------------------------------------------------------

extension LaunchAgent {
    var service: SMAppService { .agent(plistName: plist.lastPathComponent) }

    @concurrent
    package func register() async throws(ApplicationFailure) {
        let installed = try Program.current.signature.unique
        let keys = (plist: ConfigKey("agents.registered.\(label).plist"), program: ConfigKey("agents.registered.\(label).program"))
        let rewritten = Program.current.settings.bytes(forKey: keys.plist).map(Data.init) != contents
        let replaced = Program.current.settings.bytes(forKey: keys.program).map(Data.init) != installed
        let running = keepAlive == .always ? Result { () throws(ApplicationFailure) in try runsInstalledProgram() } : .success(true)
        let stopped = running == .success(false)
        let service = service
        let restarting = service.status == .enabled && keepAlive == .always && !rewritten && (replaced || stopped)
        let logger = Logger(label: String(reflecting: Self.self))
        let state: Logger.Metadata = [
            "label": "\(label, privacy: .public)", "rewritten": .stringConvertible(rewritten), "replaced": .stringConvertible(replaced), "stopped": .stringConvertible(stopped),
            "terminated": .stringConvertible(restarting),
        ]
        if case .failure(let unchecked) = running { logger.notice("Agent job could not be checked against its installed program", error: unchecked, metadata: state) }
        if restarting {
            logger.info("Agent runs replaced code or stopped, terminating it so launchd starts the installed program on the kept registration", metadata: state)
            try await terminate()
            do { try await Task.sleep(for: throttleInterval) } catch { throw ApplicationFailure(subject: label, reason: .cancelled) }
        }
        let restarted = restarting && Result { () throws(ApplicationFailure) in try runsInstalledProgram() } == .success(true)
        let again = service.status == .enabled && !restarted && (rewritten || replaced || stopped)
        let registered = await Result { () async throws in
            if again {
                logger.info("Agent declaration changed or its job runs no installed program, registering it again", metadata: state)
                try await service.unregister()
            }
            if again || [.notRegistered, .notFound].contains(service.status) { try service.register() }
        }
        switch (service.status, registered) {
            case (.requiresApproval, _): throw ApplicationFailure(subject: label, reason: .requiresApproval)
            case (_, .failure(let error)): throw ApplicationFailure(subject: label, reason: .foundation(error as NSError))
            case (_, .success):
                Program.current.store(contents, forKey: keys.plist)
                Program.current.store(installed, forKey: keys.program)
        }
    }
}
