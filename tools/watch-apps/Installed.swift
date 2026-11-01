import AppKit
import Applications
import ArgumentParser
import AsyncAlgorithms
import OrderedCollections
import Results

// --- [COMPOSITION] ---------------------------------------------------------------------

struct Installed: ParsableArguments {
    @Argument(help: "Bundle identifiers or display names of installed applications, case and diacritics ignored")
    var names: [String]
}

struct Watch: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(
        abstract: "Prints dialogs and sheets requiring an answer, hangs, and exits of running instances at start and on each change"
    )

    @OptionGroup var installed: Installed

    func run() async throws {
        try await WatchApps.run(installed.names) { installations, output in
            do {
                try checkAccessibilityTrust()
            } catch {
                for id: String in installations.keys { output.yield((id, .fail(reason: error.localizedDescription))) }
                return
            }
            await withDiscardingTaskGroup { group in
                for id: String in installations.keys {
                    group.addTask {
                        for await states: OrderedDictionary<NSRunningApplication, InstanceState> in await NSWorkspace.shared.instanceStates(
                            of: { instance in instance.bundleIdentifier == id },
                            walking: \.requiresAnswer,
                            notifications: watched,
                            polling: messagingTimeout,
                        ) {
                            output.yield((id, .answering(states)))
                        }
                    }
                }
            }
        }
    }
}

struct Launch: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(
        abstract: "Opens each application in background and prints its state once launched. Document windows open only for --document files"
    )

    @OptionGroup var installed: Installed

    @Flag(help: "Opens another instance when one runs")
    var new: Bool = false

    @Flag(help: "Opens new instance hidden with windows and dialogs off screen")
    var hides: Bool = false

    @Option(help: "File to open, one per flag")
    var document: [String] = []

    @Option(parsing: .unconditionalSingleValue, help: "Command-line argument of new instance, one per flag")
    var arguments: [String] = []

    @Option(
        help: "NAME=VALUE environment variable of new instance, one per flag",
        transform: { entry in
            guard let split: String.Index = entry.firstIndex(of: "=") else { throw ValidationError("\(entry) is no NAME=VALUE variable") }
            return (String(entry[..<split]), String(entry[entry.index(after: split)...]))
        },
    )
    var environment: [(String, String)] = []

    func run() async throws {
        let (new, hides, documents, arguments, environment): (Bool, Bool, [URL], [String], [String: String]) = (
            new, hides, document.map { path in URL(filePath: path) }, arguments,
            Dictionary(environment) { _, last in last },
        )
        try await WatchApps.run(installed.names) { installations, output in
            await withDiscardingTaskGroup { group in
                for (id, copies): (key: String, value: [Installation]) in installations {
                    group.addTask {
                        let launched: Result<NSRunningApplication, ApplicationFailure> = await Result { () async throws(ApplicationFailure) in
                            let configuration: NSWorkspace.OpenConfiguration = NSWorkspace.OpenConfiguration()
                            configuration.createsNewApplicationInstance = new
                            configuration.hides = hides
                            configuration.arguments = arguments
                            configuration.environment = environment
                            return try await NSWorkspace.shared.launch(Installation.single(of: id, among: copies), opening: documents, configuration: configuration)
                        }
                        switch launched {
                            case .success(let instance): output.yield((id, .answering([instance: await instance.state(walking: \.requiresAnswer)])))
                            case .failure(let failure): output.yield((id, .fail(reason: failure.localizedDescription)))
                        }
                    }
                }
            }
        }
    }
}

struct Read: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(
        abstract: "Prints full state of each running instance once"
    )

    @OptionGroup var installed: Installed

    @Option(help: "Process ID of one instance to read")
    var pid: pid_t?

    func run() async throws {
        let pid: pid_t? = pid
        try await WatchApps.run(installed.names) { installations, output in
            for id: String in installations.keys {
                let running: [NSRunningApplication] = NSWorkspace.shared.instances { instance in instance.bundleIdentifier == id }
                let chosen: Result<[NSRunningApplication], Unmatched> =
                    switch pid {
                        case .some(let pid):
                            Unmatched.single(
                                running.filter { instance in instance.processIdentifier == pid },
                                subject: "running instance of \(id)",
                                query: String(pid),
                                candidates: running.map { instance in String(instance.processIdentifier) },
                            ) { instance in String(instance.processIdentifier) }
                            .map { instance in [instance] }
                        case .none: .success(running)
                    }
                switch chosen {
                    case .success(let chosen):
                        let states: [(NSRunningApplication, InstanceState)] = await Array(chosen.async.map { instance in (instance, await instance.state(walking: { _ in true })) })
                        output.yield((id, .states(OrderedDictionary(uniqueKeysWithValues: states))))
                    case .failure(let miss): output.yield((id, .fail(reason: miss.localizedDescription)))
                }
            }
        }
    }
}
