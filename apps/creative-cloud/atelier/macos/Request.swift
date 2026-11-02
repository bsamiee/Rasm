import AppKit
import AppleEvents
import ApplicationServices
import Applications
import CoreServices
import Foundation
import JSONRPC
import ScreenCaptureKit
import UniformTypeIdentifiers

// --- [CONSTANTS] -----------------------------------------------------------------------

let notifications: Set<String> = [
    kAXWindowCreatedNotification, kAXSheetCreatedNotification, kAXMenuOpenedNotification, kAXMenuClosedNotification, kAXUIElementDestroyedNotification,
    kAXFocusedWindowChangedNotification, kAXMainWindowChangedNotification, kAXTitleChangedNotification, kAXWindowMiniaturizedNotification,
    kAXWindowDeminiaturizedNotification, kAXApplicationHiddenNotification, kAXApplicationShownNotification, kAXValueChangedNotification,
]

let messagingTimeout: Duration = .seconds(1)

// --- [MODELS] --------------------------------------------------------------------------

struct Artifact: Encodable, Sendable {
    let uri: URL
    let mimeType: String?
    let data: Data
}

struct Executed: Encodable, Sendable {
    let reply: AppleEventValue
    let artifacts: [Outcome<Artifact>]

    init(reply: AppleEventValue, reading files: [URL]) {
        self.reply = reply
        artifacts = files.map { file in
            Outcome(Result { try Artifact(uri: file, mimeType: file.resourceValues(forKeys: [.contentTypeKey]).contentType?.preferredMIMEType, data: Data(contentsOf: file)) })
        }
    }
}

struct Captured: Encodable, Sendable {
    let image: Data
    let width: Int
    let height: Int
}

struct Launched: Encodable, Sendable {
    let pid: pid_t
}

struct RunningInstance: Encodable, Sendable {
    private enum Key: CodingKey {
        case pid, url, activationPolicy, state
    }

    let instance: NSRunningApplication
    let url: URL
    let state: InstanceState

    func encode(to encoder: any Encoder) throws {
        let activationPolicy: String =
            switch instance.activationPolicy {
                case .regular: "regular"
                case .accessory: "accessory"
                case .prohibited: "prohibited"
                @unknown default: String(instance.activationPolicy.rawValue)
            }
        var fields: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
        try fields.encode(instance.processIdentifier, forKey: .pid)
        try fields.encode(url, forKey: .url)
        try fields.encode(activationPolicy, forKey: .activationPolicy)
        try fields.encode(state, forKey: .state)
    }
}

enum Request: Decodable, Sendable {
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

    func perform(definitions: ScriptingDefinitions) async throws -> Response {
        switch self {
            case .installations:
                return await .stream(
                    Installation.search(
                        NSPredicate(format: "%K BEGINSWITH %@", kMDItemCFBundleIdentifier as String, adobePrefix),
                        in: FileManager.default.urls(for: .applicationDirectory, in: .allDomainsMask),
                    )
                    .map { listing -> any Encodable & Sendable in Outcome(listing) }
                )
            case .instances:
                return await .stream(
                    NSWorkspace.shared.instanceStates(
                        of: { instance in instance.bundleIdentifier?.hasPrefix(adobePrefix) == true },
                        walking: \.requiresAnswer,
                        notifications: notifications,
                        polling: messagingTimeout,
                    )
                    .map { states -> any Encodable & Sendable in
                        states.compactMap { instance, state in instance.bundleURL.map { url in RunningInstance(instance: instance, url: url, state: state) } }
                    }
                )
            case .applications(let file):
                return .value(NSWorkspace.shared.urlsForApplications(toOpen: file))
            case .status(let pid):
                return try await .value(running(pid).instance.state(walking: \.requiresAnswer))
            case .launch(let url):
                let instance: NSRunningApplication = try await NSWorkspace.shared.launch(adobeInstallation(at: url))
                try await instance.finishedLaunching()
                return .value(Launched(pid: instance.processIdentifier))
            case .perform(let pid, let actions):
                _ = try running(pid)
                for action: ElementAction in actions { try await action.perform(in: pid) }
                return .value(nil)
            case .terms(let url):
                return try await .value(definitions.dictionary(of: adobeInstallation(at: url)))
            case .definition(let url, let kind, let name):
                return try await .value(definitions.dictionary(of: adobeInstallation(at: url)).text(of: kind, named: name))
            case .execute(let pid, let command, let arguments, let artifacts):
                let installation: Installation = try running(pid).installation
                let dictionary: ScriptingDictionary = try await definitions.dictionary(of: installation)
                let event: NSAppleEventDescriptor = try dictionary.event(
                    dictionary.command(command, taking: Set(arguments.keys)),
                    arguments: arguments,
                    target: NSAppleEventDescriptor(processIdentifier: pid),
                    refusing: activating(of: installation),
                )
                return try await .value(Executed(reply: AppleEventValue(event.sent(), in: dictionary), reading: artifacts))
            case .script(let pid, let source):
                let dictionary: ScriptingDictionary = try await definitions.dictionary(of: running(pid).installation)
                let script: ScriptingDictionary.Command = try dictionary.command(handling: scriptEvents)
                let declared: [String: AppleEventValue] = scriptArguments.filter { key, _ in script.parameters.contains { term in term.code == FourCharCode(sdef: key) } }
                let event: NSAppleEventDescriptor = try dictionary.event(
                    script,
                    arguments: declared.merging(["direct": .text(source)]) { _, direct in direct },
                    target: NSAppleEventDescriptor(processIdentifier: pid),
                )
                return try await .value(AppleEventValue(event.sent(), in: dictionary))
            case .screenshot(let pid, let window, let pixels):
                let configuration: SCScreenshotConfiguration = SCScreenshotConfiguration()
                configuration.showsCursor = false
                guard let image: CGImage = try await running(pid).instance.screenshot(of: window, longestSide: pixels, configuration: configuration).output.sdrImage else {
                    throw AtelierFailure(subject: String(pid), reason: .unrendered)
                }
                return try .value(Captured(image: image.data(as: .png), width: image.width, height: image.height))
            case .faces:
                return .value(FontFace.available())
            case .face(let name):
                return try .value(FontFace(named: name))
        }
    }
}
