import AppleEvents
import ApplicationServices
import Applications
import CurrentProcess
import Encoding
import Foundation
import JSONRPC
import ServiceManagement
import Sparkle
import System
import os

// --- [MODELS] --------------------------------------------------------------------------

struct ServerAgent: Decodable {
    let environment: [String: String]

    private enum CodingKeys: String, CodingKey {
        case environment = "EnvironmentVariables"
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

struct ServerError: LocalizedError {
    enum Reason: Sendable {
        case unidentified
        case relocated(named: [String])
        case unreadable(any Error)
        case refused(any Error)
    }

    let subject: String
    let reason: Reason

    var errorDescription: String? { "\(reason): \(subject)" }
}

// --- [SERVICES] ------------------------------------------------------------------------

final class UpdaterDelegate: NSObject, SPUUpdaterDelegate {
    func updater(_: SPUUpdater, willInstallUpdateOnQuit _: SUAppcastItem, immediateInstallationBlock: () -> Void) -> Bool {
        immediateInstallationBlock()
        return true
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

@main
enum Atelier {
    static func main() async {
        let logger: Logger = Logger(for: Self.self)
        let input: Result<Stat, Errno> = Result { () throws(Errno) in try FileDescriptor.standardInput.stat() }
        guard case .success(let stat) = input, [FileType.fifo, .socket].contains(stat.type) else {
            do {
                try register(bundle: .main)
            } catch {
                logger.fault("Atelier left its server unregistered, \(error, privacy: .public)")
                exit(EXIT_FAILURE)
            }
            return
        }
        let definitions: ScriptingDefinitions = ScriptingDefinitions()
        let calls: DispatchQueue = DispatchQueue(label: "Atelier.calls", attributes: .concurrent)
        let output: DispatchSerialQueue = DispatchSerialQueue(label: "Atelier.output")
        do {
            try disclaimResponsibility()
            AXUIElementSetMessagingTimeout(AXUIElementCreateSystemWide(), Float(messagingTimeout / .seconds(1)))
            do {
                try checkAccessibilityTrust(prompting: true)
            } catch {
                logger.notice("Started without Accessibility trust, \(error, privacy: .public)")
            }
            let updaterDelegate: UpdaterDelegate = UpdaterDelegate()
            let updater: SPUStandardUpdaterController = SPUStandardUpdaterController(startingUpdater: true, updaterDelegate: updaterDelegate, userDriverDelegate: nil)
            defer { extendLifetime((updater, updaterDelegate)) }
            try await serve(
                reading: FileHandle.standardInput.bytes.jsonLines,
                writing: { line in try await withTaskExecutorPreference(output) { try FileHandle.standardOutput.write(contentsOf: line) } },
                dropped: { failure in logger.debug("Dropped \(failure, privacy: .public)") },
                { (request: Request) in try await withTaskExecutorPreference(calls) { try await request.perform(definitions: definitions) } },
            )
        } catch {
            logger.fault("Atelier stopped with \(error, privacy: .public)")
            exit(EXIT_FAILURE)
        }
    }

    static func register(bundle: Bundle) throws(ServerError) {
        guard let identifier: String = bundle.bundleIdentifier else {
            throw ServerError(subject: bundle.bundleURL.path(percentEncoded: false), reason: .unidentified)
        }
        let plist: String = "\(identifier).server.plist"
        let file: URL = bundle.bundleURL.appending(components: "Contents", "Library", "LaunchAgents", plist)
        let agent: ServerAgent
        do {
            agent = try PropertyListDecoder().decode(ServerAgent.self, from: Data(contentsOf: file))
        } catch {
            throw ServerError(subject: file.path(percentEncoded: false), reason: .unreadable(error))
        }
        guard let executable: String = bundle.executablePath, agent.environment.values.contains(executable) else {
            throw ServerError(subject: bundle.bundleURL.path(percentEncoded: false), reason: .relocated(named: agent.environment.values.sorted()))
        }
        do {
            try SMAppService.agent(plistName: plist).register()
        } catch {
            throw ServerError(subject: plist, reason: .refused(error))
        }
    }
}
