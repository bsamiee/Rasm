import AppKit
import Applications
import ArgumentParser
import AsyncAlgorithms
import Carbon.HIToolbox
import CasePaths
import OrderedCollections
import Processes
import System

// --- [CONSTANTS] -----------------------------------------------------------------------

private let installedNames: ArgumentHelp = "Installed application paths, bundle identifiers, display names, or process IDs of running instances. Names ignore case and diacritics"

// --- [OPERATIONS] ----------------------------------------------------------------------

private func installed(_ names: [String], _ run: Run) async -> [(name: String, installations: [Installation])] {
    await OrderedSet(names).async.compactMap { name -> (name: String, installations: [Installation])? in
        do throws(ApplicationFailure) { return (name, try await Installation.installed(named: name)) } catch {
            run.fail(name, error)
            return nil
        }
    }
    .reduce(into: []) { found, entry in found.append(entry) }
}

private func act(_ action: ElementAction, key: String) async {
    await Run.execute { run in
        do throws(ApplicationFailure) {
            let (instance, state): (NSRunningApplication, InstanceState) = try await action.perform()
            run.show(instance, state)
        } catch {
            run.fail(key, error)
        }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

struct OutlineOptions: ParsableArguments {
    @Option(help: ArgumentHelp("Longest running line, cutting asking text, then options, then windows to fit", valueName: "characters"))
    var lineLength: Int?

    func validate() throws(ValidationError) {
        if let lineLength, lineLength <= 0 { throw ValidationError("\(lineLength) is not a positive integer") }
    }
}

struct Running: ParsableArguments {
    @Argument(help: "Running application paths, bundle identifiers, display names, or process IDs from a line. Names ignore case and diacritics")
    var names: [String]

    func each(_ run: Run, _ body: @escaping @Sendable (NSRunningApplication) async throws(ApplicationFailure) -> Void) async {
        let named: [[NSRunningApplication]] = names.map { name in
            pid_t(name).map { pid in [NSRunningApplication(processIdentifier: pid)].compactMap(\.self) } ?? NSWorkspace.shared.instances(named: name)
        }
        for (name, found): (String, [NSRunningApplication]) in zip(names, named) where found.isEmpty { run.emit(name, [String: String]()) }
        let instances = OrderedSet(named.joined())
        await withDiscardingTaskGroup { group in
            for instance: NSRunningApplication in instances {
                group.addTask {
                    do throws(ApplicationFailure) { try await body(instance) } catch { run.fail(instance.key, error) }
                }
            }
        }
    }
}

struct Find: AsyncParsableCommand {
    static let configuration: CommandConfiguration = .init(abstract: "Prints each name's installations, best match first, with no launch")

    @Argument(help: installedNames)
    var names: [String]

    func run() async {
        await Run.execute(accessibility: false) { run in
            for (name, installations): (String, [Installation]) in await installed(names, run) { run.emit(name, installations) }
        }
    }
}

struct Watch: AsyncParsableCommand {
    static let configuration: CommandConfiguration = .init(abstract: "Prints one line per instance change, per system prompt naming an instance, and per system pressure change above nominal")

    @Argument(help: installedNames)
    var names: [String]

    @OptionGroup var outline: OutlineOptions

    func run() async {
        await Run.execute(lineLength: outline.lineLength) { run in
            let applications: [Installation] = await installed(names, run).flatMap(\.installations)
            guard !applications.isEmpty else { return }
            for identifier: String in OrderedSet(applications.map(\.identifier)) where NSWorkspace.shared.instances(of: { instance in instance.bundleIdentifier == identifier }).isEmpty {
                run.emit(identifier, [String: String]())
            }
            let matching: @Sendable (NSRunningApplication) -> Bool = { [applications] instance in applications.contains { installation in installation.runs(instance) } }
            await withDiscardingTaskGroup { group in
                group.addTask {
                    var calm = true
                    for await outcome: Result<SystemPressure, Errno> in SystemPressure.changes() {
                        do throws(Errno) {
                            let pressure: SystemPressure = try outcome.get()
                            let normal: Bool = pressure.memory.isDisjoint(with: [.warning, .critical]) && pressure.thermal == .nominal
                            if !(calm && normal) { run.emit("system", pressure) }
                            calm = normal
                        } catch {
                            run.fail("system", ApplicationFailure(subject: "system", reason: .system(error)))
                        }
                    }
                }
                group.addTask { await run.follow(NSWorkspace.shared.instanceStates(of: matching)) }
                group.addTask { await run.follow(NSWorkspace.shared.systemPrompts(naming: applications.map(\.name))) }
            }
        }
    }
}

struct Launch: AsyncParsableCommand {
    static let configuration: CommandConfiguration = .init(
        abstract: "Opens applications or documents without activation, reusing a running instance. Prints states until each instance settles or asks, or until it exits with --wait"
    )

    @Argument(help: installedNames)
    var names: [String] = []

    @Flag(help: "Opens another instance when one runs, for applications that support several")
    var new: Bool = false

    @Flag(help: "Observes one named application until it exits and returns its exit status, or 128 plus its signal. Quits an instance it created when the run ends first")
    var wait: Bool = false

    @Option(parsing: .upToNextOption, help: ArgumentHelp("Files to open", valueName: "path"), transform: { path in URL(filePath: path) })
    var document: [URL] = []

    @Option(
        parsing: .upToNextOption,
        help: "NAME=VALUE environment variables of new instance",
        transform: { entry in
            let parts: [Substring] = entry.split(separator: "=", maxSplits: 1, omittingEmptySubsequences: false)
            guard parts.count == 2 else { throw ValidationError("\(entry) lacks =, pass NAME=VALUE") }
            return (String(parts[0]), String(parts[1]))
        },
    )
    var environment: [(String, String)] = []

    @OptionGroup var outline: OutlineOptions

    @Option(parsing: .remaining, help: ArgumentHelp("Command-line arguments of new instance, every input after it", valueName: "argument"))
    var arguments: [String] = []

    func validate() throws(ValidationError) {
        guard !names.isEmpty || !document.isEmpty else { throw ValidationError("Pass names or --document") }
        guard names.count < 2 || document.isEmpty else { throw ValidationError("--document belongs to one application") }
        guard !wait || names.count == 1 else { throw ValidationError("--wait belongs to one named application") }
    }

    func run() async {
        let variables: [String: String] = Dictionary(environment) { _, last in last }
        let (names, document, arguments, new, wait): ([String], [URL], [String], Bool, Bool) = (names, document, arguments, new, wait)
        await Run.execute(lineLength: outline.lineLength, waiting: wait) { run in
            let resolved: [(key: String, outcome: Result<(Installation, [URL]), ApplicationFailure>)] =
                if names.isEmpty {
                    document.map { file in (file.path(percentEncoded: false), Result { () throws(ApplicationFailure) in (try Installation(opening: file), [file]) }) }
                } else {
                    await installed(names, run).map { name, found in (name, Result { () throws(ApplicationFailure) in (try Installation.newest(of: name, among: found), document) }) }
                }
            for case (let key, .failure(let failure)) in resolved { run.fail(key, failure) }
            let applications: OrderedDictionary<Installation, [URL]> = OrderedDictionary(
                resolved.compactMap { entry -> (Installation, [URL])? in if case .success(let application) = entry.outcome { application } else { nil } }
            ) { first, next in
                names.isEmpty ? first + next : first
            }
            await withDiscardingTaskGroup { group in
                for (installation, documents): (Installation, [URL]) in applications {
                    group.addTask {
                        await run.follow(
                            launching: NSWorkspace.shared.launching(installation, opening: documents, arguments: arguments, environment: variables, newInstance: new, waiting: wait),
                            as: installation.identifier,
                        )
                    }
                }
            }
        }
    }
}

struct Read: AsyncParsableCommand {
    static let configuration: CommandConfiguration = .init(abstract: "Prints each instance's whole state with element ids once, --window walks one window")

    @OptionGroup var running: Running

    @Option(help: "Window number from a line")
    var window: CGWindowID?

    func run() async {
        await Run.execute { run in
            await running.each(run) { instance in run.show(instance, await instance.reading(window: window), whole: true) }
        }
    }
}

struct Capture: AsyncParsableCommand {
    static let configuration: CommandConfiguration = .init(abstract: "Writes one PNG per presented window, or of --window, with no activation. Prints one line per file")

    @OptionGroup var running: Running

    @Option(help: "Window number from a line")
    var window: CGWindowID?

    @Option(help: ArgumentHelp("Longest side in pixels, smaller captures keep native size", valueName: "pixels"))
    var longestSide: Int?

    func validate() throws(ValidationError) {
        if let longestSide, longestSide <= 0 { throw ValidationError("\(longestSide) is not a positive integer") }
    }

    func run() async {
        await Run.execute { run throws(ApplicationFailure) in
            let directory: URL = .temporaryDirectory.appending(component: program, directoryHint: .isDirectory)
            do { try FileManager.default.createDirectory(at: directory, withIntermediateDirectories: true) } catch {
                throw ApplicationFailure(subject: directory.path(percentEncoded: false), reason: .host(error as NSError))
            }
            await running.each(run) { instance throws(ApplicationFailure) in
                let shots: [CGWindowID: Result<URL, ApplicationFailure>] = try await instance.capturing(into: directory, window: window, longestSide: longestSide)
                guard !shots.isEmpty else { throw ApplicationFailure(subject: instance.key, reason: .noWindow) }
                for (number, shot): (CGWindowID, Result<URL, ApplicationFailure>) in shots.sorted(by: { first, second in first.key < second.key }) {
                    do throws(ApplicationFailure) {
                        let file: URL = try shot.get()
                        run.emit(instance.key, [String(instance.processIdentifier): Event.captured(Screenshot(file: file.path(percentEncoded: false), number: number))])
                    } catch {
                        run.fail(instance.key, error)
                    }
                }
            }
        }
    }
}

struct ElementOperand: ParsableArguments {
    @Argument(
        help: "Element ID from a read line",
        transform: { id in
            guard let element = Element.ID(base64Encoded: id) else { throw ValidationError("\(id) is not base64") }
            return element
        },
    )
    var id: Element.ID
}

struct Perform: AsyncParsableCommand {
    static let configuration: CommandConfiguration = .init(abstract: "Performs one Accessibility action on an element. Prints its instance's next state")

    @OptionGroup var element: ElementOperand

    @Argument(help: "Accessibility action from element actions")
    var action: String

    func run() async { await act(.perform(element: element.id, action: action), key: element.id.description) }
}

struct SetValue: AsyncParsableCommand {
    static let configuration: CommandConfiguration = .init(commandName: "set", abstract: "Sets an element's \(kAXValueAttribute) to text. Prints its instance's next state")

    @OptionGroup var element: ElementOperand

    @Argument(help: "Text for element \(kAXValueAttribute)")
    var value: String

    func run() async { await act(.set(element: element.id, value: value), key: element.id.description) }
}

struct Key: AsyncParsableCommand {
    static let configuration: CommandConfiguration = .init(
        abstract: "Posts one key to a process whose window blocks its main thread, leaving the frontmost app and cursor unchanged. Prints its next state"
    )

    @Argument(help: "Process ID from a line with an asking row of kind blocking")
    var pid: pid_t

    @Argument(help: "Virtual key code, \(kVK_Return) Return, \(kVK_Escape) Escape")
    var code: CGKeyCode

    func run() async { await act(.key(process: pid, code: code), key: String(pid)) }
}

struct Quit: AsyncParsableCommand {
    static let configuration: CommandConfiguration = .init(abstract: "Sends each running instance the quit event. Prints states and exits 1 unless each instance exited")

    @OptionGroup var running: Running

    @Flag(help: "Kills each instance without asking. Use when no answer or quit frees it")
    var force: Bool = false

    func run() async {
        await Run.execute { run in
            await running.each(run) { instance in
                if await run.quit(instance, instance.quitting(force: force))?.is(\.exited) != true { run.fail() }
            }
        }
    }
}
