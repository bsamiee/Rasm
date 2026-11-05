public import Foundation
public import Results
import ServiceManagement

// --- [ERRORS] --------------------------------------------------------------------------

public struct AgentFailure: LocalizedError, CustomNSError {
    public let subject: String
    public let cause: any Error

    public var errorDescription: String? { "\(subject): \(cause.localizedDescription)" }
    public var errorUserInfo: [String: Any] { [NSUnderlyingErrorKey: cause] }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public func registerAgents() async throws(AggregateError<AgentFailure>) {
    let folder: String = Bundle.main.bundleURL.appending(components: "Contents", "Library", "LaunchAgents").path(percentEncoded: false)
    let plists: [String]
    do {
        plists = try FileManager.default.contentsOfDirectory(atPath: folder)
    } catch {
        throw AggregateError(first: AgentFailure(subject: folder, cause: error))
    }
    var failures: [AgentFailure] = []
    for plist: String in plists {
        let agent: SMAppService = SMAppService.agent(plistName: plist)
        do {
            if agent.status == .enabled { try await agent.unregister() }
            try agent.register()
            if agent.status == .requiresApproval { SMAppService.openSystemSettingsLoginItems() }
        } catch {
            failures.append(AgentFailure(subject: plist, cause: error))
        }
    }
    if let failure: AggregateError<AgentFailure> = AggregateError(failures) { throw failure }
}
