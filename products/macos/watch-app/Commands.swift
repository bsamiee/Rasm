import AppKit
import Applications
import ArgumentParser
import AsyncAlgorithms
import Carbon.HIToolbox
import CasePaths
import CurrentProcess
import Encoding
import Files
import OrderedCollections
import Processes
import ScreenCapture
import System
import Telemetry
import Tracing
import UniformTypeIdentifiers

// --- [CONSTANTS] -----------------------------------------------------------------------

private let installedNames: ArgumentHelp =
    "Installed application paths, file: URLs, bundle identifiers, whole file or display names, or process IDs of running instances. Names ignore case and diacritics"

// --- [OPERATIONS] ----------------------------------------------------------------------

private func elementID(_ id: String) throws(ValidationError) -> Element.ID {
    guard let element = Element.ID(base64Encoded: id) else { throw ValidationError("\(id) is not base64") }
    return element
}

private func installed(_ names: [String], _ run: Run) async -> [(name: String, installations: [Installation])] {
    await OrderedSet(names).async.compactMap { name -> (name: String, installations: [Installation])? in
        do throws(ApplicationFailure) { return (name, try await Installation.installed(named: name)) } catch {
            run.fail(name, error)
            return nil
        }
    }
    .reduce(into: []) { found, entry in found.append(entry) }
}

private func act(
    _ command: (some ParsableCommand).Type,
    _ action: ElementAction,
    on target: @escaping @Sendable () async throws(ApplicationFailure) -> Instance,
    key: String,
) async {
    await Run.execute(command) { run in
        do throws(ApplicationFailure) {
            let instance = try await target()
            let acted = try await instance.act([action])
            if case .failure(let failure)? = acted.outcomes.first { throw failure }
            run.show(instance, acted.state)
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
    @Argument(help: "Process IDs from a line, or installed application paths, file: URLs, bundle identifiers, or whole file or display names. Names ignore case and diacritics")
    var names: [String]

    func each(_ run: Run, _ body: @escaping @Sendable (Instance) async throws(ApplicationFailure) -> Void) async {
        var instances: OrderedSet<Instance> = []
        for name in names {
            do throws(ApplicationFailure) {
                let found = try await NSWorkspace.shared.instances(named: name)
                if found.isEmpty { run.emit(name, [String: String]()) }
                instances.formUnion(found)
            } catch {
                run.fail(name, error)
            }
        }
        await withDiscardingTaskGroup { [instances] group in
            for instance in instances {
                group.addTask {
                    do throws(ApplicationFailure) { try await body(instance) } catch { run.fail(instance.key, error) }
                }
            }
        }
    }
}

struct Find: AsyncParsableCommand {
    static let configuration = CommandConfiguration(abstract: "Prints each name's installations with no launch")

    @Argument(help: installedNames)
    var names: [String]

    func run() async {
        await Run.execute(Self.self, accessibility: false) { run in
            for (name, installations) in await installed(names, run) { run.emit(name, installations) }
        }
    }
}

struct Watch: AsyncParsableCommand {
    static let configuration = CommandConfiguration(
        abstract: "Prints one line per instance change, per system prompt naming an instance, and per system pressure change above nominal. Records standard windows and asking rows with --record"
    )

    @Argument(help: installedNames)
    var names: [String]

    @OptionGroup var outline: OutlineOptions

    @Option(
        help: ArgumentHelp(
            "Folder taking one movie per standard window and asking row, a clip and frame of the \(Recording.buffered.formatted(.units(width: .wide))) before each dialog, hang, and exit by signal, and timeline.jsonl",
            valueName: "folder",
        ),
        transform: { folder in URL(filePath: folder, directoryHint: .isDirectory) },
    )
    var record: URL?

    @Option(help: ArgumentHelp("Longest side of recorded frames in pixels, smaller windows keep native size", valueName: "pixels"))
    var longestSide: Int?

    func validate() throws(ValidationError) {
        if let longestSide, longestSide <= 0 { throw ValidationError("\(longestSide) is not a positive integer") }
        if longestSide != nil, record == nil { throw ValidationError("--longest-side belongs to --record") }
    }

    func run() async {
        await Run.execute(Self.self, lineLength: outline.lineLength, recording: record.map { folder in (folder, longestSide) }) { run in
            let applications = await installed(names, run).flatMap(\.installations)
            guard !applications.isEmpty else { return }
            for identifier in OrderedSet(applications.map(\.identifier)) where NSWorkspace.shared.instances(of: { instance in instance.bundleIdentifier == identifier }).isEmpty {
                run.emit(identifier, [String: String]())
            }
            let matching: @Sendable (NSRunningApplication) -> Bool = { [applications] instance in applications.contains { installation in installation.runs(instance) } }
            await withDiscardingTaskGroup { group in
                group.addTask {
                    var calm = true
                    for await outcome in SystemPressure.changes() {
                        do throws(Errno) {
                            let pressure = try outcome.get()
                            let normal = pressure.memory.isDisjoint(with: [.warning, .critical]) && pressure.thermal == .nominal
                            if !(calm && normal) { run.emit("system", pressure) }
                            calm = normal
                        } catch {
                            run.fail("system", ApplicationFailure(subject: "system", reason: .system(error)))
                        }
                    }
                }
                group.addTask { await run.follow(NSWorkspace.shared.instanceStates(of: matching)) }
                group.addTask { await run.follow(NSWorkspace.shared.systemPrompts(naming: applications.flatMap { installation in [installation.name, installation.bundleName].compactMap(\.self) })) }
                if let executable = Bundle.main.executableURL {
                    group.addTask {
                        switch await FileEvent.events(of: executable).first(where: { batch in batch[case: \.success]?.isEmpty != true }) {
                            case .success: await run.restart(into: executable)
                            case .failure(let failure): run.fail(Program.current.name, ApplicationFailure(subject: executable.path(percentEncoded: false), reason: .unwatched(failure)))
                            case nil: break
                        }
                    }
                }
            }
        }
    }
}

struct Launch: AsyncParsableCommand {
    static let configuration = CommandConfiguration(
        abstract: "Opens applications, files, or URLs without activation, reusing a running instance. Prints states until each instance settles or asks, or until it exits with --wait"
    )

    @Argument(help: installedNames)
    var names: [String] = []

    @Flag(help: "Opens another instance when one runs, for applications that support several")
    var new: Bool = false

    @Flag(help: "Observes one named application until it exits and returns its exit status, or 128 plus its signal. Quits an instance it created when the run ends first")
    var wait: Bool = false

    @Option(
        parsing: .upToNextOption,
        help: ArgumentHelp("Files or URLs to open", valueName: "path-or-url"),
        transform: { value in
            URL(string: value).flatMap { url in url.scheme == nil || FileManager.default.fileExists(atPath: value) ? nil : url } ?? URL(filePath: value)
        },
    )
    var document: [URL] = []

    @Option(
        parsing: .upToNextOption,
        help: "NAME=VALUE environment variables of new instance",
        transform: { entry in
            let parts = entry.split(separator: "=", maxSplits: 1, omittingEmptySubsequences: false)
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
        let (names, document, arguments, environment, new, wait) = (names, document, arguments, environment, new, wait)
        await Run.execute(Self.self, lineLength: outline.lineLength, waiting: wait) { run in
            var context: [String: String] = [:]
            InstrumentationSystem.instrument.inject(InstrumentationContext.current ?? .topLevel, into: &context, using: EnvironmentCarrier())
            let variables = context.merging(environment) { _, stated in stated }
            let resolved: [(key: String, outcome: Result<(Installation, [URL]), ApplicationFailure>)] =
                if names.isEmpty {
                    document.map { url in
                        (url.isFileURL ? url.path(percentEncoded: false) : url.absoluteString, Result { () throws(ApplicationFailure) in (try Installation(opening: url), [url]) })
                    }
                } else {
                    await installed(names, run).map { name, found in (name, Result { () throws(ApplicationFailure) in (try Installation.preferred(of: name, among: found), document) }) }
                }
            for case (let key, .failure(let failure)) in resolved { run.fail(key, failure) }
            let applications = OrderedDictionary(
                resolved.compactMap { entry -> (Installation, [URL])? in if case .success(let application) = entry.outcome { application } else { nil } }
            ) { first, next in
                names.isEmpty ? first + next : first
            }
            await withDiscardingTaskGroup { group in
                for (installation, documents) in applications {
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
    static let configuration = CommandConfiguration(abstract: "Prints each instance's whole state with element ids once, --window walks one window")

    @OptionGroup var running: Running

    @Option(help: "Window number from a line")
    var window: CGWindowID?

    func run() async {
        await Run.execute(Self.self) { run in
            await running.each(run) { instance in run.show(instance, await instance.reading(window: window), whole: true) }
        }
    }
}

struct Capture: AsyncParsableCommand {
    static let configuration = CommandConfiguration(
        abstract: "Writes one PNG per presented window, of --window, or of --element's frame inside its window, with no activation. Prints one line per file"
    )

    @OptionGroup var running: Running

    @Option(help: "Window number from a line")
    var window: CGWindowID?

    @Option(help: "Element ID from a read line, a pane, panel, or canvas", transform: elementID)
    var element: Element.ID?

    @Option(help: ArgumentHelp("Longest side in pixels, smaller captures keep native size", valueName: "pixels"))
    var longestSide: Int?

    func validate() throws(ValidationError) {
        if let longestSide, longestSide <= 0 { throw ValidationError("\(longestSide) is not a positive integer") }
        if window != nil, element != nil { throw ValidationError("Pass --window or --element") }
    }

    func run() async {
        let source: Source? = window.map { number in Source.windows(numbers: [number]) } ?? element.map(Source.element(id:))
        await Run.execute(Self.self) { run throws(ApplicationFailure) in
            try checkCaptureAccess(prompting: true)
            await running.each(run) { instance throws(ApplicationFailure) in
                let stills = try await instance.capture(source, to: Program.current.directory(.captures, of: instance.application.localizedName), longestSide: longestSide)
                guard !stills.isEmpty else { throw ApplicationFailure(subject: instance.key, reason: .noWindow) }
                for (number, still) in stills.sorted(by: { first, second in first.key < second.key }) {
                    do throws(ApplicationFailure) {
                        let file = try still.get().file
                        let footage = Footage(file: file.path(percentEncoded: false), pid: instance.process.pid, window: number)
                        run.emit(instance.key, [String(instance.process.pid): Tagged(instance: instance.process, value: Event.captured(footage))])
                    } catch {
                        run.fail(instance.key, error)
                    }
                }
            }
        }
    }
}

struct ElementOperand: ParsableArguments {
    @Argument(help: "Element ID from a read line", transform: elementID)
    var id: Element.ID
}

struct Perform: AsyncParsableCommand {
    static let configuration = CommandConfiguration(abstract: "Performs one Accessibility action on an element. Prints its instance's next state")

    @OptionGroup var element: ElementOperand

    @Argument(help: "Accessibility action from element actions")
    var action: String

    func run() async {
        let id = element.id
        await act(Self.self, .press(element: id, action: action), on: { () async throws(ApplicationFailure) in try await Instance.owning(id) }, key: id.description)
    }
}

struct SetValue: AsyncParsableCommand {
    static let configuration = CommandConfiguration(commandName: "set", abstract: "Sets an element's \(kAXValueAttribute) to text. Prints its instance's next state")

    @OptionGroup var element: ElementOperand

    @Argument(help: "Text for element \(kAXValueAttribute)")
    var value: String

    func run() async {
        let id = element.id
        await act(Self.self, .set(element: id, value: .string(value)), on: { () async throws(ApplicationFailure) in try await Instance.owning(id) }, key: id.description)
    }
}

struct Key: AsyncParsableCommand {
    static let configuration = CommandConfiguration(
        abstract: "Posts one key to a process whose window blocks its main thread, leaving the frontmost app and cursor unchanged. Prints its next state"
    )

    @Argument(help: "Process ID from a line with an asking row of kind blocking")
    var pid: pid_t

    @Argument(help: "Virtual key code, \(kVK_Return) Return, \(kVK_Escape) Escape")
    var code: CGKeyCode

    func run() async {
        let pid = pid
        await act(
            Self.self,
            .key(key: .code(code), modifiers: []),
            on: { () throws(ApplicationFailure) in
                guard let application = NSRunningApplication(processIdentifier: pid) else { throw ApplicationFailure(subject: String(pid), reason: .noProcess) }
                return try Instance(application)
            },
            key: String(pid),
        )
    }
}

struct Quit: AsyncParsableCommand {
    static let configuration = CommandConfiguration(abstract: "Sends each running instance the quit event. Prints states and exits 1 unless each instance exited")

    @OptionGroup var running: Running

    @Flag(help: "Kills each instance without asking. Use when no answer or quit frees it")
    var force: Bool = false

    func run() async {
        await Run.execute(Self.self) { run in
            await running.each(run) { instance in
                if await run.quit(instance, instance.quit(force: force))?.is(\.exited) != true { run.fail() }
            }
        }
    }
}

struct ProcessTable: AsyncParsableCommand {
    static let configuration = CommandConfiguration(commandName: "processes", abstract: "Prints each name's launch agents, running processes, and newest crash reports with no launch")

    @Argument(help: "Names as find takes, executable paths, process IDs, or instance values from a line, an exited instance included")
    var names: [String]

    @Option(help: ArgumentHelp("Newest crash reports printed per name", valueName: "count"))
    var reports: Int = 3

    func validate() throws(ValidationError) {
        if reports < 0 { throw ValidationError("\(reports) is negative") }
    }

    func run() async {
        await Run.execute(Self.self, accessibility: false) { run in
            for name in OrderedSet(names) {
                do throws(ApplicationFailure) { run.emit(name, try await Lifecycle(of: name, reports: reports)) } catch { run.fail(name, error) }
            }
        }
    }
}
