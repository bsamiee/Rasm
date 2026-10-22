import AdobeScripting
import AppKit
public import AppleEvents
public import Applications
public import CoreGraphics
import CoreServices
public import EffectRPC
import Fonts
public import Foundation
import UniformTypeIdentifiers

// --- [MODELS] --------------------------------------------------------------------------

enum Execute {
    struct Result: Encodable {
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
                Outcome(Swift.Result { try Artifact(uri: file, mimeType: file.resourceValues(forKeys: [.contentTypeKey]).contentType?.preferredMIMEType, data: Data(contentsOf: file)) })
            }
        }
    }
}

enum Screenshot {
    struct Result: Encodable {
        let image: Data
        let width: Int
        let height: Int
    }
}

struct RunningInstance: Encodable {
    let pid: pid_t
    let url: URL
    let activationPolicy: String
    let state: InstanceState

    init(_ instance: NSRunningApplication, at url: URL, state: InstanceState) {
        pid = instance.processIdentifier
        self.url = url
        activationPolicy =
            switch instance.activationPolicy {
                case .regular: "regular"
                case .accessory: "accessory"
                case .prohibited: "prohibited"
                @unknown default: String(instance.activationPolicy.rawValue)
            }
        self.state = state
    }
}

struct Launched: Encodable {
    let pid: pid_t
}

public enum Request: Decodable, Sendable {
    case installations
    case instances
    case applications(toOpen: URL)
    case status(pid: pid_t)
    case launch(url: URL)
    case perform(pid: pid_t, actions: [ElementAction])
    case terms(url: URL)
    case definition(url: URL, kind: ScriptingDictionary.Kind, name: String)
    case execute(pid: pid_t, command: String, arguments: [String: AppleEventValue], artifacts: [URL])
    case script(pid: pid_t, source: String)
    case screenshot(pid: pid_t, window: CGWindowID?, pixels: Int?)
    case faces
    case face(name: String)

    public func perform(definitions: ScriptingDefinitions) async -> Result<Response, any Error> {
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
                            states.compactMap { instance, state in instance.bundleURL.map { url in RunningInstance(instance, at: url, state: state) } }
                        }
                    )
                case .applications(let file):
                    return .value(NSWorkspace.shared.urlsForApplications(toOpen: file))
                case .status(let pid):
                    return try await .value(InstanceState(Adobe.running(pid).get().instance.status(walking: walked)))
                case .launch(let url):
                    return try await .value(Launched(pid: NSWorkspace.shared.launchApplication(Adobe.installation(at: url).get()).get().processIdentifier))
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
                        Execute.Result(reply: AppleEventValue(resolved.refusingActivation(of: event, among: installation.activating).get().sent().get(), in: dictionary), reading: artifacts)
                    )
                case .script(let pid, let source):
                    let installation: Installation = try Adobe.running(pid).get().installation
                    let dictionary: ScriptingDictionary = try await definitions.dictionary(of: installation).get()
                    guard let script: (command: ScriptingDictionary.Command, arguments: [String: AppleEventValue]) = dictionary.script(running: source) else {
                        throw RequestFailure.unscripted(installation)
                    }
                    let event: NSAppleEventDescriptor = try dictionary.event(script.command, arguments: script.arguments, target: NSAppleEventDescriptor(processIdentifier: pid)).get()
                    return try await .value(AppleEventValue(event.sent().get(), in: dictionary))
                case .screenshot(let pid, let window, let pixels):
                    let image: CGImage = try await Adobe.running(pid).get().instance.screenshot(of: window, longestSide: pixels).get().image
                    return try .value(Screenshot.Result(image: image.data(as: .png).get(), width: image.width, height: image.height))
                case .faces:
                    return .value(AvailableFace.available())
                case .face(let name):
                    return try .value(AvailableFace.named(name).get())
            }
        }
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

public enum RequestFailure: LocalizedError {
    case unscripted(Installation)

    public var errorDescription: String? {
        switch self {
            case .unscripted(let installation): "\(installation.name) declares no script command in its dictionary, nothing was sent"
        }
    }
}
