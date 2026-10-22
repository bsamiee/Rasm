import AdobeScripting
import AppKit
import AppleEvents
import Applications
import CoreServices
import CurrentProcess
import EffectRPC
import Fonts
import Foundation
import UniformTypeIdentifiers
import os

// --- [MODELS] --------------------------------------------------------------------------

struct Executed: Encodable {
    struct Artifact: Encodable, Sendable {
        let uri: URL
        let mimeType: String?
        let data: Data
    }

    let reply: AppleEventValue
    let artifacts: [Outcome<Artifact>]

    init(reply: AppleEventValue, reading files: [URL]) {
        self.reply = reply
        artifacts = files.map { file in
            Outcome(Result { try Artifact(uri: file, mimeType: file.resourceValues(forKeys: [.contentTypeKey]).contentType?.preferredMIMEType, data: Data(contentsOf: file)) })
        }
    }
}

struct Captured: Encodable {
    let image: Data
    let width: Int
    let height: Int
}

struct Instance: Encodable {
    let pid: pid_t
    let url: URL
    let state: InstanceState
}

enum Request: Decodable {
    case installations
    case instances
    case launch(url: URL)
    case perform(pid: pid_t, actions: [ElementAction])
    case terms(url: URL)
    case definition(url: URL, kind: ScriptingDictionary.Kind, name: String)
    case execute(pid: pid_t, command: String, arguments: [String: AppleEventValue], artifacts: [URL])
    case script(pid: pid_t, source: String)
    case screenshot(pid: pid_t, window: CGWindowID?, pixels: Int?)
    case faces
    case face(name: String)

    func perform(definitions: ScriptingDefinitions) async -> Result<Response, any Error> {
        let walked: @Sendable (Element) -> Bool = { element in element.requiresAnswer || element.role == kAXMenuRole }
        return await Result { () async throws in
            switch self {
                case .installations:
                    return await .stream(
                        NSWorkspace.shared.applications(
                            matching: NSPredicate(format: "%K BEGINSWITH %@", kMDItemCFBundleIdentifier as String, Adobe.identifierPrefix),
                            in: FileManager.default.urls(for: .applicationDirectory, in: .allDomainsMask),
                        )
                        .map { listing -> any Encodable & Sendable in Outcome(listing) }
                    )
                case .instances:
                    return await .stream(
                        NSWorkspace.shared.instanceStates(of: { instance in instance.bundleIdentifier?.hasPrefix(Adobe.identifierPrefix) == true }, walking: walked).map {
                            states -> any Encodable & Sendable in
                            states.compactMap { instance, state in instance.bundleURL.map { url in Instance(pid: instance.processIdentifier, url: url, state: state) } }
                        }
                    )
                case .launch(let url):
                    return try await .value(["pid": NSWorkspace.shared.launchApplication(at: Adobe.installation(at: url).get().url).get().processIdentifier])
                case .perform(let pid, let actions):
                    let instance: NSRunningApplication = try Adobe.running(pid).get().instance
                    for action: ElementAction in actions { try await instance.perform(action).get() }
                    return .value(nil)
                case .terms(let url):
                    return try await .value(definitions.dictionary(of: Adobe.installation(at: url).get()).get())
                case .definition(let url, let kind, let name):
                    return try await .value(definitions.dictionary(of: Adobe.installation(at: url).get()).get().text(of: kind, named: name).get())
                case .execute(let pid, let command, let arguments, let artifacts):
                    let installation: Installation = try Adobe.running(pid).get().installation
                    let dictionary: ScriptingDictionary = try await definitions.dictionary(of: installation).get()
                    let resolved: ScriptingDictionary.Command = try dictionary.command(command, taking: Set(arguments.keys)).get()
                    let event: NSAppleEventDescriptor = try dictionary.event(resolved, arguments: arguments, target: NSAppleEventDescriptor(processIdentifier: pid)).get()
                    return try await .value(
                        Executed(reply: AppleEventValue(resolved.refusingActivation(of: event, among: installation.activating).get().sent().get(), in: dictionary), reading: artifacts)
                    )
                case .script(let pid, let source):
                    let installation: Installation = try Adobe.running(pid).get().installation
                    let dictionary: ScriptingDictionary = try await definitions.dictionary(of: installation).get()
                    guard let script: (command: ScriptingDictionary.Command, arguments: [String: AppleEventValue]) = dictionary.script(running: source) else { throw Refusal.unscripted(installation) }
                    let event: NSAppleEventDescriptor = try dictionary.event(script.command, arguments: script.arguments, target: NSAppleEventDescriptor(processIdentifier: pid)).get()
                    return try await .value(AppleEventValue(event.sent().get(), in: dictionary))
                case .screenshot(let pid, let window, let pixels):
                    let image: CGImage = try await Adobe.running(pid).get().instance.screenshot(of: window, longestSide: pixels).get().image
                    return try .value(Captured(image: image.data(as: .png).get(), width: image.width, height: image.height))
                case .faces:
                    return .value(AvailableFace.available())
                case .face(let name):
                    return try .value(AvailableFace.named(name).get())
            }
        }
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

enum Refusal: LocalizedError {
    case notAdobe(URL)
    case noProcess(pid_t)
    case unscripted(Installation)

    var errorDescription: String? {
        switch self {
            case .notAdobe(let url): "\(url.path(percentEncoded: false)) has no bundle identifier starting \(Adobe.identifierPrefix)"
            case .noProcess(let pid): "No running application bundle has pid \(pid)"
            case .unscripted(let installation): "\(installation.name) declares no script command in its dictionary, nothing was sent"
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

enum Adobe {
    static let identifierPrefix: String = "com.adobe."

    static func installation(at url: URL) -> Result<Installation, any Error> {
        Installation.at(url)
            .mapError { error -> any Error in error }
            .flatMap { installation in installation.identifier.hasPrefix(identifierPrefix) ? .success(installation) : .failure(Refusal.notAdobe(url)) }
    }

    static func running(_ pid: pid_t) -> Result<(instance: NSRunningApplication, installation: Installation), any Error> {
        switch NSRunningApplication(processIdentifier: pid).flatMap({ instance in instance.bundleURL.map { url in (instance, url) } }) {
            case .some((let instance, let url)): installation(at: url).map { installation in (instance, installation) }
            case .none: .failure(Refusal.noProcess(pid))
        }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

@main
enum Atelier {
    static func main() async {
        let definitions: ScriptingDefinitions = ScriptingDefinitions()
        do {
            try disclaimResponsibility()
            AXIsProcessTrustedWithOptions(["AXTrustedCheckOptionPrompt": true] as CFDictionary)
            try await EffectRPC.serve(reading: .standardInput, writing: .standardOutput) { (request: Request) in await request.perform(definitions: definitions) }
        } catch {
            Logger(category: "Atelier").fault("Stopped reading requests: \(error, privacy: .public)")
            exit(EXIT_FAILURE)
        }
    }
}
