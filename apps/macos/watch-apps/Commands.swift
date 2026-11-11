import AppKit
import Applications
import ArgumentParser
import AsyncAlgorithms
import Carbon.HIToolbox
import CasePaths
import CurrentProcess
import Files
import OrderedCollections
import Processes
import System
import Telemetry
import Tracing

// --- [CONSTANTS] -----------------------------------------------------------------------

private let installedNames: ArgumentHelp =
    "Installed application paths, file: URLs, bundle identifiers, whole file or display names, or process IDs of running instances. Names ignore case and diacritics"

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

private func lifecycle(of name: String, reports limit: Int) async throws(ApplicationFailure) -> Lifecycle {
    let exited = ProcessInstance(rawValue: name)
    let named: ProcessInstance?
    do throws(Errno) { named = try ProcessInstance(named: name) } catch  where error == .noSuchProcess && exited != nil { named = exited } catch {
        throw ApplicationFailure(subject: name, reason: error == .noSuchProcess ? .noProcess : .system(error))
    }
    let file: URL?
    do throws(Errno) { file = named == nil && !FilePath(name).removingLastComponent().isEmpty ? try ifPresent { () throws(Errno) in try URL(filePath: name).canonical() } : nil } catch {
        throw ApplicationFailure(subject: name, reason: .system(error))
    }
    let regular: Bool
    do { regular = try file?.resourceValues(forKeys: [.isRegularFileKey]).isRegularFile == true } catch { throw ApplicationFailure(subject: name, reason: .host(error as NSError)) }
    let executable = regular ? file.map { file in FilePath(file.path(percentEncoded: false)) } : nil
    let installations = named == nil && executable == nil ? try await Installation.installed(named: name) : []
    let agents = Array(try installations.map { installation throws(ApplicationFailure) in try installation.agents() }.joined())
    let bundles: [FilePath]
    do throws(Errno) { bundles = try installations.map { installation throws(Errno) in FilePath(try installation.url.canonical().path(percentEncoded: false)) } } catch {
        throw ApplicationFailure(subject: name, reason: .system(error))
    }
    let scoped: (FilePath) -> Bool = { path in path == executable || bundles.contains(where: path.starts(with:)) }
    let owned: (CrashReport) -> Bool = { report in
        report.executable.map { path in scoped(FilePath(path)) } == true || installations.contains { installation in installation.identifier == report.bundleIdentifier }
            || agents.contains { agent in agent.label == report.service }
    }
    let running: [ProcessInstance]
    do throws(Errno) {
        running =
            switch named {
                case .some(let named) where named.hasExited: []
                case .some(let named): [named]
                case .none: try ProcessInstance.executing(scoped)
            }
    } catch {
        throw ApplicationFailure(subject: name, reason: .system(error))
    }
    let found = CrashReport.reports { report in named.map { named in report.instance == named } ?? owned(report) }
    return Lifecycle(
        agents: agents,
        processes: running.map { instance in
            Lifecycle.Row(instance: instance, pid: instance.pid, executable: try? instance.executable().string, replaced: (try? instance.runsInstalledCode()).map { installed in !installed })
        },
        reports: Array(found.reports.prefix(limit)),
    )
}

private func act(_ command: (some ParsableCommand).Type, _ action: ElementAction, key: String) async {
    await Run.execute(command) { run in
        do throws(ApplicationFailure) {
            let (instance, state) = try await action.perform()
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
    static let configuration: CommandConfiguration = .init(abstract: "Prints each name's installations with no launch")

    @Argument(help: installedNames)
    var names: [String]

    func run() async {
        await Run.execute(Self.self, accessibility: false) { run in
            for (name, installations) in await installed(names, run) { run.emit(name, installations) }
        }
    }
}

struct Watch: AsyncParsableCommand {
    static let configuration: CommandConfiguration = .init(abstract: "Prints one line per instance change, per system prompt naming an instance, and per system pressure change above nominal")

    @Argument(help: installedNames)
    var names: [String]

    @OptionGroup var outline: OutlineOptions

    func run() async {
        await Run.execute(Self.self, lineLength: outline.lineLength) { run in
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
                group.addTask { await run.follow(NSWorkspace.shared.systemPrompts(naming: applications.map(\.name))) }
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
                    document.map { file in (file.path(percentEncoded: false), Result { () throws(ApplicationFailure) in (try Installation(opening: file), [file]) }) }
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
    static let configuration: CommandConfiguration = .init(abstract: "Prints each instance's whole state with element ids once, --window walks one window")

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
        await Run.execute(Self.self) { run throws(ApplicationFailure) in
            let directory = Program.current.directory(.runtime)
            do { try FileManager.default.createDirectory(at: directory, withIntermediateDirectories: true) } catch {
                throw ApplicationFailure(subject: directory.path(percentEncoded: false), reason: .host(error as NSError))
            }
            await running.each(run) { instance throws(ApplicationFailure) in
                let shots = try await instance.capturing(into: directory, window: window, longestSide: longestSide)
                guard !shots.isEmpty else { throw ApplicationFailure(subject: instance.key, reason: .noWindow) }
                for (number, shot) in shots.sorted(by: { first, second in first.key < second.key }) {
                    do throws(ApplicationFailure) {
                        let file = try shot.get()
                        run.emit(
                            instance.key,
                            [String(instance.process.pid): Tagged(instance: instance.process, value: Event.captured(Screenshot(file: file.path(percentEncoded: false), number: number)))],
                        )
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

    func run() async { await act(Self.self, .perform(element: element.id, action: action), key: element.id.description) }
}

struct SetValue: AsyncParsableCommand {
    static let configuration: CommandConfiguration = .init(commandName: "set", abstract: "Sets an element's \(kAXValueAttribute) to text. Prints its instance's next state")

    @OptionGroup var element: ElementOperand

    @Argument(help: "Text for element \(kAXValueAttribute)")
    var value: String

    func run() async { await act(Self.self, .set(element: element.id, value: value), key: element.id.description) }
}

struct Key: AsyncParsableCommand {
    static let configuration: CommandConfiguration = .init(
        abstract: "Posts one key to a process whose window blocks its main thread, leaving the frontmost app and cursor unchanged. Prints its next state"
    )

    @Argument(help: "Process ID from a line with an asking row of kind blocking")
    var pid: pid_t

    @Argument(help: "Virtual key code, \(kVK_Return) Return, \(kVK_Escape) Escape")
    var code: CGKeyCode

    func run() async { await act(Self.self, .key(process: pid, code: code), key: String(pid)) }
}

struct Quit: AsyncParsableCommand {
    static let configuration: CommandConfiguration = .init(abstract: "Sends each running instance the quit event. Prints states and exits 1 unless each instance exited")

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
    static let configuration: CommandConfiguration = .init(commandName: "processes", abstract: "Prints each name's launch agents, running processes, and newest crash reports with no launch")

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
                do throws(ApplicationFailure) { run.emit(name, try await lifecycle(of: name, reports: reports)) } catch { run.fail(name, error) }
            }
        }
    }
}
