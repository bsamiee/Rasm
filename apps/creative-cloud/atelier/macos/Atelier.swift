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

struct Scripted: Encodable {
    let runtime: String
    let reply: AppleEventValue
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
    case launch(URL)
    case perform(pid_t, actions: [ElementAction])
    case terms(URL)
    case definition(URL, kind: ScriptingDictionary.Kind, name: String)
    case execute(pid_t, command: String, arguments: [String: AppleEventValue], artifacts: [URL])
    case script(pid_t, source: String)
    case screenshot(pid_t, window: CGWindowID?, pixels: Int?)
    case faces
    case face(String)

    private enum Method: String, Decodable { case installations, instances, launch, perform, terms, definition, execute, script, screenshot, faces, face }
    private enum Key: CodingKey { case method, params }
    private enum Field: CodingKey { case url, pid, actions, kind, name, command, arguments, artifacts, source, window, pixels }

    init(from decoder: any Decoder) throws {
        let call: KeyedDecodingContainer<Key> = try decoder.container(keyedBy: Key.self)
        let method: Method = try call.decode(Method.self, forKey: .method)
        let params: () throws -> KeyedDecodingContainer<Field> = { try call.nestedContainer(keyedBy: Field.self, forKey: .params) }
        self =
            switch method {
                case .installations: .installations
                case .instances: .instances
                case .launch: try .launch(params().decode(URL.self, forKey: .url))
                case .perform: try .perform(params().decode(pid_t.self, forKey: .pid), actions: params().decode([ElementAction].self, forKey: .actions))
                case .terms: try .terms(params().decode(URL.self, forKey: .url))
                case .definition:
                    try .definition(params().decode(URL.self, forKey: .url), kind: params().decode(ScriptingDictionary.Kind.self, forKey: .kind), name: params().decode(String.self, forKey: .name))
                case .execute:
                    try .execute(
                        params().decode(pid_t.self, forKey: .pid),
                        command: params().decode(String.self, forKey: .command),
                        arguments: params().decode([String: AppleEventValue].self, forKey: .arguments),
                        artifacts: params().decode([URL].self, forKey: .artifacts),
                    )
                case .script: try .script(params().decode(pid_t.self, forKey: .pid), source: params().decode(String.self, forKey: .source))
                case .screenshot:
                    try .screenshot(
                        params().decode(pid_t.self, forKey: .pid),
                        window: params().decodeIfPresent(CGWindowID.self, forKey: .window),
                        pixels: params().decodeIfPresent(Int.self, forKey: .pixels),
                    )
                case .faces: .faces
                case .face: try .face(params().decode(String.self, forKey: .name))
            }
    }

    func perform(definitions: ScriptingDefinitions) async -> Result<Response, any Error> {
        let timeout: Duration = .seconds(60)
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
                        NSWorkspace.shared.instanceStates(of: Adobe.scripted, walking: walked).map { states -> any Encodable & Sendable in
                            states.compactMap { instance, state in instance.bundleURL.map { url in Instance(pid: instance.processIdentifier, url: url, state: state) } }
                        }
                    )
                case .launch(let url):
                    return try await .value(["pid": NSWorkspace.shared.launchApplication(at: Adobe.installation(at: url).get().url, within: timeout).get().processIdentifier])
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
                    if let interference: Interference = installation.interference(of: resolved, sending: event) { throw Refusal.interferes(resolved, interference) }
                    return try await .value(Executed(reply: AppleEventValue(event.sent(within: timeout).get(), in: dictionary), reading: artifacts))
                case .script(let pid, let source):
                    let installation: Installation = try Adobe.running(pid).get().installation
                    guard let script: (code: String, runtime: String) = installation.script else { throw Refusal.unscripted(installation) }
                    let dictionary: ScriptingDictionary = try await definitions.dictionary(of: installation).get()
                    let event: NSAppleEventDescriptor = try dictionary.event(
                        dictionary.command(script.code, taking: ["direct"]).get(),
                        arguments: ["direct": .text(source)],
                        target: NSAppleEventDescriptor(processIdentifier: pid),
                    )
                    .get()
                    return try await .value(Scripted(runtime: script.runtime, reply: AppleEventValue(event.sent(within: timeout).get(), in: dictionary)))
                case .screenshot(let pid, let window, let pixels):
                    let image: CGImage = try await Adobe.running(pid).get().instance.screenshot(of: window, longestSide: pixels).get().image
                    return try .value(Captured(image: image.data(as: .png).get(), width: image.width, height: image.height))
                case .faces:
                    return .value(InstalledFace.installed())
                case .face(let name):
                    return try .value(InstalledFace.named(name).get())
            }
        }
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

enum Refusal: LocalizedError {
    case notAdobe(URL)
    case noProcess(pid_t)
    case interferes(ScriptingDictionary.Command, Interference)
    case unscripted(Installation)

    var errorDescription: String? {
        switch self {
            case .notAdobe(let url): "\(url.path(percentEncoded: false)) has no bundle identifier starting \(Adobe.identifierPrefix)"
            case .noProcess(let pid): "No running application bundle has pid \(pid)"
            case .interferes(let command, let interference):
                ["\(command.name) (\(command.code.sdef)) \(interference)", Adobe.routes[interference], "nothing was sent"].compactMap(\.self).joined(separator: ", ")
            case .unscripted(let installation): "\(installation.name) runs no script through Apple Events without user interaction, nothing was sent"
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

enum Adobe {
    static let identifierPrefix: String = "com.adobe."

    static let routes: [Interference: String] = [
        .menuAction: "the command tool's Menu form runs one a linked extension lists, execute_script runs any",
        .script: "execute_script runs it",
    ]

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

    static func scripted(_ instance: NSRunningApplication) -> Bool {
        switch instance.bundleURL.map(installation(at:)) {
            case .some(.success(let installation)): installation.scriptable
            case .some(.failure), .none: false
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
