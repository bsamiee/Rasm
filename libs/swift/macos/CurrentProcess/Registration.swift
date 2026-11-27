package import Applications
import Configuration
import Encoding
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
        let key = ConfigKey("agents.registered.\(label)")
        let replaced = Program.current.settings.bytes(forKey: key).map(Data.init) != installed
        let stopped = keepAlive == .bool(true) ? try !runsInstalledProgram() : false
        let service = service
        let again = service.status == .enabled && (replaced || stopped)
        let registered = await Result { () async throws in
            if again {
                let state: Logger.Metadata = ["label": "\(label, privacy: .public)", "replaced": .stringConvertible(replaced), "stopped": .stringConvertible(stopped)]
                Logger(label: String(reflecting: Self.self)).info("Agent enabled from other code or not running its installed program, registering it again", metadata: state)
                try await service.unregister()
            }
            if again || [.notRegistered, .notFound].contains(service.status) { try service.register() }
        }
        switch (service.status, registered) {
            case (.requiresApproval, _): throw ApplicationFailure(subject: label, reason: .requiresApproval)
            case (_, .failure(let error)): throw ApplicationFailure(subject: label, reason: .foundation(error as NSError))
            case (_, .success): Program.current.store(installed, forKey: key)
        }
    }
}
