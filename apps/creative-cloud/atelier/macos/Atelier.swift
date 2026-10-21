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

// --- [TYPES] ---------------------------------------------------------------------------

enum Interference: String {
    case activation = "activates the application or opens another one"
    case interface = "opens a window, dialog, or panel with these arguments"
    case menuAction = "runs a menu action, the Menu form runs it"
    case script = "runs a script, execute_script runs it"
}

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

struct InstanceStatus: Encodable {
    let pid: pid_t
    let status: Outcome<ApplicationStatus>
}

enum Request: Decodable {
    case installations
    case statuses
    case launch(URL)
    case status(pid_t)
    case press(pid_t, element: CGWindowID, actions: [ElementAction])
    case terms(URL)
    case definition(URL, kind: ScriptingDictionary.Kind, name: String)
    case execute(pid_t, command: String, arguments: [String: AppleEventValue], artifacts: [URL])
    case screenshot(pid_t, window: CGWindowID?, pixels: Int?)
    case faces
    case face(String)

    private enum Method: String, Decodable { case installations, statuses, launch, status, press, terms, definition, execute, screenshot, faces, face }
    private enum Key: CodingKey { case method, params }
    private enum Field: CodingKey { case url, pid, element, actions, kind, name, command, arguments, artifacts, window, pixels }

    init(from decoder: any Decoder) throws {
        let call: KeyedDecodingContainer<Key> = try decoder.container(keyedBy: Key.self)
        let method: Method = try call.decode(Method.self, forKey: .method)
        let params: () throws -> KeyedDecodingContainer<Field> = { try call.nestedContainer(keyedBy: Field.self, forKey: .params) }
        self =
            switch method {
                case .installations: .installations
                case .statuses: .statuses
                case .launch: try .launch(params().decode(URL.self, forKey: .url))
                case .status: try .status(params().decode(pid_t.self, forKey: .pid))
                case .press:
                    try .press(
                        params().decode(pid_t.self, forKey: .pid),
                        element: params().decode(CGWindowID.self, forKey: .element),
                        actions: params().decode([ElementAction].self, forKey: .actions),
                    )
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
                case .statuses:
                    return await .stream(
                        NSWorkspace.shared.instanceEvents(of: Adobe.scripted).compactMap { event -> (any Encodable & Sendable)? in
                            if case .status(let instance, let read) = event { InstanceStatus(pid: instance.processIdentifier, status: Outcome(read)) } else { nil }
                        }
                    )
                case .launch(let url):
                    return try await .value(["pid": NSWorkspace.shared.launchApplication(at: Adobe.installation(at: url).get().url, within: timeout).get().processIdentifier])
                case .status(let pid):
                    return try await .value(Adobe.running(pid).get().instance.status().get())
                case .press(let pid, let element, let actions):
                    let instance: NSRunningApplication = try Adobe.running(pid).get().instance
                    for action: ElementAction in actions { try await instance.perform(action, on: element).get() }
                    return .value(nil)
                case .terms(let url):
                    return try await .value(definitions.dictionary(of: Adobe.installation(at: url).get(), within: timeout).get())
                case .definition(let url, let kind, let name):
                    return try await .value(definitions.dictionary(of: Adobe.installation(at: url).get(), within: timeout).get().text(of: kind, named: name).get())
                case .execute(let pid, let command, let arguments, let artifacts):
                    let installation: Installation = try Adobe.running(pid).get().installation
                    let dictionary: ScriptingDictionary = try await definitions.dictionary(of: installation, within: timeout).get()
                    let resolved: ScriptingDictionary.Command = try dictionary.command(command, taking: Set(arguments.keys)).get()
                    let event: NSAppleEventDescriptor = try dictionary.event(resolved, arguments: arguments, target: NSAppleEventDescriptor(processIdentifier: pid)).get()
                    if let interference: Interference = resolved.reason(
                        for: event,
                        among: Adobe.routes + (Adobe.refusals.first { stem, _ in installation.identifier.hasPrefix(stem) }?.value ?? []),
                    ) {
                        throw Refusal.interferes(resolved, interference)
                    }
                    return try await .value(Executed(reply: AppleEventValue(event.sent(within: timeout).get(), in: dictionary), reading: artifacts))
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

    var errorDescription: String? {
        switch self {
            case .notAdobe(let url): "\(url.path(percentEncoded: false)) is no application with an identifier starting \(Adobe.identifierPrefix)"
            case .noProcess(let pid): "No running application bundle has pid \(pid)"
            case .interferes(let command, let interference): "\(command.name) (\(command.code.sdef)) \(interference.rawValue), nothing was sent"
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

enum Adobe {
    static let identifierPrefix: String = "com.adobe."

    static let routes: [(code: String, clause: EventClause, reason: Interference)] =
        ["UIUIInvk", "CAROexec"].map { code in (code, .always, .menuAction) } + ["miscDoAc", "miscfile"].map { code in (code, .always, .script) }

    static let refusals: KeyValuePairs<String, [(code: String, clause: EventClause, reason: Interference)]> = [
        "com.adobe.InDesign": [
            ("UIUIshow", .always, .activation),
            ("K2LKrvlO", .always, .activation),
            ("K2LKrvlB", .always, .activation),
            ("TeXTSTed", .always, .interface),
            ("K2  TpsV", .always, .interface),
            ("K2  casc", .always, .interface),
            ("K2  tile", .always, .interface),
            ("K2  rstr", .direct(of: ["cwin", "Lwin", "Swin"]), .interface),
            ("aevtpdoc", .parameter("prno", .among(["true"])), .interface),
            ("K2  ePrB", .parameter("BPBD", .among(["true"])), .interface),
            ("CoReclos", .parameter("svop", .among([nil, "ask "])), .interface),
            ("aevtquit", .parameter("savo", .among([nil, "ask "])), .interface),
            ("coresetd", .property("pvis", of: ["CPNS", "pgsp", "cLbp"], .among(["true"])), .interface),
            ("coresetd", .property("UIAc", of: nil, .except("eNvr")), .interface),
        ]
            + [
                "K2  expt", "InBKexpt", "InESexpt", "K2PIexpt", "K2XLexpt", "TeXTexpt", "K2  exAs", "InESexAs", "K2PIexAs", "K2XLexAs", "TeXTexAs", "K2PIplac", "InESplac", "TeXTplac",
                "K2  LCDr", "K2  LPGn", "K2PIcopl", "InEScopl", "TeXTcopl", "K2PIplli", "TeXTplli",
            ]
            .map { code in (code, .parameter("imot", .among(["true"])), .interface) },
        "com.adobe.illustrator": [
            ("misckCPk", .always, .interface),
            ("ART5kLWC", .always, .interface),
            ("aevtodoc", .parameter("DLOG", .among([nil, "true"])), .interface),
            ("coresetd", .property("pUIL", of: nil, .except("e45a")), .interface),
        ],
        "com.adobe.Photoshop": [
            ("8BIMpsop", .always, .interface),
            ("8BIMptmg", .always, .interface),
            ("PsCSodoc", .parameter("ShwD", .except("Nevr")), .interface),
            ("PsCSclos", .parameter("savo", .among([nil, "ask "])), .interface),
            ("coresetd", .property("AUiL", of: nil, .except("Nevr")), .interface),
        ],
        "com.adobe.Acrobat.Pro": [
            ("coresetd", .property("pisf", of: nil, .any), .activation),
            ("CARObfrt", .always, .activation),
            ("coreclos", .parameter("savo", .among(["ask "])), .interface),
            ("aevtquit", .parameter("savo", .among(["ask "])), .interface),
            ("coresetd", .property("pskw", of: nil, .among(["fals"])), .interface),
        ],
        "com.adobe.AfterEffects": [
            ("aevtpdoc", .parameter("pdlg", .among(["true"])), .interface),
            ("coresetd", .property("pvis", of: nil, .among(["true"])), .interface),
            ("coresetd", .property("pmnd", of: nil, .among(["fals"])), .interface),
        ],
        "com.adobe.PremierePro": [
            ("CR8RCPAE", .always, .interface),
            ("CR8RCRPr", .always, .activation),
        ],
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
