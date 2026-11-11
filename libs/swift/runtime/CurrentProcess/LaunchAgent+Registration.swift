public import Applications
import Foundation
import Logging
import Security
public import ServiceManagement
import Telemetry

// --- [OPERATIONS] ----------------------------------------------------------------------

extension LaunchAgent {
    public var service: SMAppService { .agent(plistName: plist.lastPathComponent) }

    @concurrent
    public func register() async throws(ApplicationFailure) {
        var code: SecStaticCode?
        var signing: CFDictionary?
        let found = unsafe SecStaticCodeCreateWithPath(Bundle.main.bundleURL as CFURL, [], &code)
        let read = code.map { code in unsafe SecCodeCopySigningInformation(code, [], &signing) } ?? found
        guard read == errSecSuccess else { throw ApplicationFailure(subject: label, reason: .host(NSError(domain: NSOSStatusErrorDomain, code: Int(read)))) }
        let installed = (signing as? [CFString: Any])?[kSecCodeInfoUnique] as? Data
        let registered = UserDefaults.standard.dictionary(forKey: "agents.registered") ?? [:]
        let replaced = registered[label] as? Data != installed
        let stopped = keepAlive ? try !runsInstalledProgram() : false
        let service = service
        let again = service.status == .enabled && (replaced || stopped)
        do {
            if again {
                let state: Logger.Metadata = ["label": "\(label, privacy: .public)", "replaced": .stringConvertible(replaced), "stopped": .stringConvertible(stopped)]
                Logger(label: String(reflecting: Self.self)).info("Agent enabled from other code or not running its installed program, registering it again", metadata: state)
                try await service.unregister()
            }
            if again || [.notRegistered, .notFound].contains(service.status) { try service.register() }
        } catch {
            throw ApplicationFailure(subject: label, reason: .host(error as NSError))
        }
        UserDefaults.standard.set(registered.merging([label: installed].compactMapValues(\.self)) { _, installed in installed }, forKey: "agents.registered")
    }
}
