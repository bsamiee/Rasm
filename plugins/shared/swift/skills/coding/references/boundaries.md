# [BOUNDARIES]

## [01]-[CODABLE]

Wire records are synthesized `Codable`, and a field's type holds its wire rule. Enums a producer extends are `Lenient<Known>` fields from `Encoding`, and a tuple switch reads the known cases beside the unknown one:

```swift
struct Limit: Decodable, Sendable {
    enum Meter: String, Sendable {
        case weeklyAll = "weekly_all"
        case weeklyScoped = "weekly_scoped"
    }

    let kind: Lenient<Meter>
    let percent: Double
    let resetsAt: Date?
    let model: String?

    var rateLimitKind: RateLimitKind? {
        switch (kind, model) {
            case (.known(.weeklyAll), _): .weekly
            case (.known(.weeklyScoped), let model?): .model(model)
            case (.known(.weeklyScoped), nil), (.unknown, _): nil
        }
    }
}
```

`RateLimitKind` is the module's enum of `weekly` and `model(String)`:
- `Lenient` decodes `Known.RawValue` through a single-value container, and a meter a newer producer sends reads as no window, never a failed report
- Host enums take the same field (`Lenient<AXError>` keeps a code the SDK leaves unnamed), as `Lenient` constrains the raw value alone
- Standard library `RawRepresentable` coders exist per concrete raw type, and a type generic over its raw value writes both coders through a single-value container
- Tokens deciding whether a message is accepted are a plain enum refused at decode (`Version` with `case two = "2.0"`), and `Lenient` holds tokens a reader skips
- Validated scalars are `RawRepresentable` structs over a standard raw type, `init?(rawValue:)` holds the rule, and `nil` throws `DecodingError.dataCorrupted` at the field's coding path
- `Date` fields stay `Date`, and one coder per wire states the spelling as a strategy (`JSONEncoder.jsonLines` writes ISO 8601 with fractional seconds) in place of a coder per model
- Key strategies (`.convertFromSnakeCase`) derive wire spellings, `CodingKeys` raw values hold a spelling no strategy derives
- Bytes decode once into their target type, and a `JSONValue` encoded again to decode parses twice and rounds integers above 2^53 through `Double`

Synthesized `Codable` nests an enum's associated values under a key named for the case. A wire naming its case in a field decodes through one `init(from:)` reading the discriminator and the case's fields from one container:

```swift
extension AppleEventValue: Decodable {
    private enum Tag: String, Decodable {
        case type
        case file
        case specifier
    }

    private enum Key: String, CodingKey {
        case tag = "_tag"
        case name, url, want, seld, from
    }

    init(from decoder: any Decoder) throws {
        let fields = try decoder.container(keyedBy: Key.self)
        self =
            switch try fields.decode(Tag.self, forKey: .tag) {
                case .type: try .type(fields.decode(String.self, forKey: .name))
                case .file: try .file(fields.decode(URL.self, forKey: .url))
                case .specifier:
                    try .specifier(
                        want: fields.decode(String.self, forKey: .want),
                        seld: fields.decode(Self.self, forKey: .seld),
                        from: fields.decodeIfPresent(Self.self, forKey: .from) ?? .null,
                    )
            }
    }
}
```

`AppleEventValue` is a recursive enum holding `null`, `type(String)`, `file(URL)`, and `specifier(want:seld:from:)`:
- Nested values decode through `decode(Self.self, forKey:)` on the same decoder, one parse at every depth
- Hand `init(from:)` also states a fact synthesis loses: `contains(.result)` tells `"result": null` from a missing key, and a cross-field rule throws `dataCorrupted` in the same initializer
- `encodeIfPresent(usage?[case: \.success], forKey: .usage)` writes an optional `Result` field's success and omits absence and failure
- `CaseKeyed` and `CaseLabeled` from `Encoding` write a single-payload enum as `{"<case>": payload}` and a case as its label, where synthesis nests the payload under `_0`

## [02]-[FILES]

File work runs through System (`FilePath`, `FileDescriptor`, `Stat`, `Errno`) and `URL`. Reads go to the file and match its missing-file error as absence, as a `fileExists` check before the read races a writer:

```swift
func ifPresent<Value, Failure: Error>(_ read: () throws(Failure) -> Value) throws(Failure) -> Value? {
    do {
        return try read()
    } catch {
        return switch error as any Error {
            case CocoaError.fileNoSuchFile, CocoaError.fileReadNoSuchFile, .noSuchFileOrDirectory as Errno: nil
            default: throw error
        }
    }
}

let inode: Inode? = try ifPresent { () throws(Errno) in try path.stat().inode }
```

`Files` declares `ifPresent`, and `path` is a `FilePath`:
- `FilePath.stat` and `Stat.init` throw `Errno` typed, and `FileDescriptor.open`, `read`, and `write` throw untyped holding an `Errno`
- `Stat.inode` names a file across renames, and a descriptor whose inode differs from its path's lost the path to another writer
- `.exclusiveLock` (`O_EXLOCK`) on `open` serializes writers across processes
- Names from outside stat beneath a root descriptor (`name.stat(relativeTo: root, flags: .resolveBeneath)`), and the kernel refuses a name outside the root with `ENOTCAPABLE`
- Kernel paths (`proc_pidpath`) compare after `realpath` (`URL.canonical()` from `Files`), as `resolvingSymlinksInPath()` drops `/private`
- Per-user folders read `URL` members (`applicationSupportDirectory`, `cachesDirectory`, `temporaryDirectory`, `libraryDirectory` for `Logs`) under the bundle identifier
- `Program.current.directory(_:)` from `CurrentProcess` names a process's state, log, and media folders
- Bytes read as text through `String(validating: bytes, as: UTF8.self)`, `nil` for invalid UTF-8

## [03]-[SETTINGS]

Settings read through a swift-configuration `ConfigReader`, and `Program.current.settings` from `CurrentProcess` reads key `<stem>` from `<NAME>_<STEM>` in the environment, then from the bundle identifier's defaults domain:
- Values with a rule read as their type (`int(forKey:as:)` over `ExpressibleByConfigInt`, `string(forKey:as:)` over `RawRepresentable<String>`), and the type's `init?` holds the rule
- Conversion failures reach the reader's `AccessReporter` as `conversionError`, a non-throwing read returns its default, and a `required` read throws
- Checks beside the reader (a `positive` helper logging its own refusal) open a second rejection path, and the value's type replaces them

## [04]-[PROCESSES]

`Subprocess.run` launches a child with no shell. Body forms fold standard output as it streams while standard error collects under a limit:

```swift
func history(_ arguments: [String], limit: Int) async throws(ProcessFailure) -> Deque<LogEntry> {
    let subject = "log \(arguments.joined(separator: " "))"
    let shown: ExecutionResult<Deque<LogEntry>, SequenceOutput, StringOutput<UTF8>>
    do {
        shown = try await Subprocess.run(.name("log"), arguments: Arguments(arguments), input: .none, output: .sequence, error: .string(limit: 1 << 16)) { execution in
            try await execution.standardOutput.strings(separatedBy: .unicodeScalarSequence(["\n"])).reduce(into: Deque<LogEntry>()) { held, line in
                held.append(try decoder.decode(LogEntry.self, from: Data(line.utf8)))
                if held.count > limit { held.removeFirst() }
            }
        }
    } catch {
        throw ProcessFailure(subject: subject, error)
    }
    guard shown.terminationStatus.isSuccess else { throw ProcessFailure(subject: subject, reason: .exit(shown.terminationStatus, standardError: shown.standardError)) }
    return shown.closureResult
}
```

`decoder` is the `log show` decoder with its date strategy, and `ProcessFailure(subject:_:)` classifies a thrown error into `cancelled`, `subprocess`, or `io`:
- `run` throws `SubprocessError` when launch or I/O fails, and a nonzero exit is a result whose status and standard error enter the exit reason whole
- `.string(limit:)` bounds collected standard error, `.maxLineLength` bounds each streamed line and throws `outputLimitExceeded` past it, and a fold holds memory flat
- `strings()` splits on `.lineBreaks`, U+2028 and U+2029 included, and JSON lines split on `.unicodeScalarSequence(["\n"])`, as a JSON string holds both unescaped
- `.name` searches the child environment's `PATH` and never a working directory, a missing program throws `executableNotFound`, and `.path` takes a `FilePath`
- Arguments reach the child as separate values, and `.inherit.updating([...])` changes variables for one run, `nil` removing one
- Children starting processes of their own run under `createSession` with a `.gracefulShutDown(toProcessGroup: true, …)` teardown step (`PlatformOptions.session()` from `Processes`)
- Teardown runs when the caller is cancelled or the body throws, sends `SIGTERM` to the group, then `SIGKILL`, and without a session the group holds the caller
- Subprocess sets no deadline, and a run with a budget races it through `withTimeout` from `Results` into a `timedOut` reason

Processes the caller did not start are named by pid and kernel start time (`proc_pid_rusage` `ri_proc_start_abstime`), as a later process reuses a pid:
- `ProcessInstance` from `Processes` holds both, and exit watches, signals, and crash report matches take the instance
- `kqueue` `EVFILT_PROC` with `NOTE_EXIT | NOTE_EXITSTATUS | NOTE_EXIT_DETAIL` reports a process's exit with its wait status in `data`, which `DispatchSource` omits
- Kernel `filt_procattach` accepts exit status registration from a parent, a tracer, or a caller allowed to `SIGKILL` the target and refuses others with `EACCES`

## [05]-[COMMAND_LINES]

Command-line products are ArgumentParser `ParsableCommand` types, `AsyncParsableCommand` where `run` awaits, and a parent command lists its `subcommands` in its `CommandConfiguration`:

```swift
struct Analyze: ParsableCommand {
    static let configuration: CommandConfiguration = .init(abstract: "Reports the containers of the given project.xcproj and contents.xcworkspacedata files")

    @Argument(help: "project.xcproj and contents.xcworkspacedata files", transform: { file in FilePath(file).removingLastComponent() })
    var containers: [FilePath]

    func run() throws {
        try FileHandle.standardOutput.write(contentsOf: encoded(report(containers)))
    }
}
```

`report` builds the `Encodable` result, and `encoded` writes it through one `JSONEncoder`:
- `transform:` adapts a type the parser lacks (`FilePath`), and a `RawRepresentable` over an `ExpressibleByArgument` raw value conforms in an empty extension
- `nil` from a conversion prints the rejected value with usage and exits with `ExitCode.validationFailure`, and `validate()` holds a rule reading more than one property
- Errors `run` throws print as `LocalizedError.errorDescription`, else `String(describing:)`, and exit with `ExitCode.failure`, and a thrown `ExitCode` sets the status alone
- Results go to standard output as encoded bytes, and diagnostics go to the logger

## [06]-[TELEMETRY]

Code logs through swift-log: a `Logger` labeled by its owning type, a literal message naming the operation, values in `metadata:`, and a failure as `error:`:

```swift
let logger = Logger(label: String(reflecting: AccountStore.self))
logger.error("Accounts load failed", error: error, metadata: ["file": "\(file.lastPathComponent, privacy: .public)"])
logger.notice("Loaded accounts", metadata: ["file": "\(file.lastPathComponent, privacy: .public)", "count": .stringConvertible(accounts.count)])
```

Each metadata interpolation states `privacy:`, an attribute `Privacy` from `Telemetry` declares over swift-log `MetadataValueAttributes`:

| [INDEX] | [VALUE]                                 | [PRIVACY]  |
| :-----: | :-------------------------------------- | :--------- |
|  [01]   | Count, state, or generated identifier   | `.public`  |
|  [02]   | Path, name, message body, or credential | `.private` |

- `.stringConvertible(<value>)` keeps a number or flag typed in the log file and public in every sink, and a private number goes through the interpolation
- Messages name the operation and its subject, and a failure that paths share logs once where they join
- Libraries log through `Logger.current` (bound by `withLogger`) or a `Logger` parameter at `info` and below, and return a failure typed without logging it
- Process entries alone call `Program.current.telemetry(scopes:)` from `CurrentProcess`, which bootstraps logging, metrics, and tracing and returns the `ServiceGroup` the entry runs
- Attribute names come from `OTelAttribute` of swift-otel-semantic-conventions (`OTelAttribute.process.pid`), and a deprecated name carries `renamed:` to its successor
- Typed span setters (`span.attributes.rpc.method`) compile under the package's `Experimental` trait alone, which pins the package to a minor version
- `withSpan(_:context:ofKind:)` from `Tracing` wraps a boundary operation, and its untyped `rethrows` widens a typed failure to `any Error`, which the caller converts once
- Trace context enters through `InstrumentationSystem.instrument.extract` from message fields, and reaches a child's environment as `TRACEPARENT` through `EnvironmentCarrier`
- `OSSignposter` marks Instruments intervals: `withIntervalSignpost` around synchronous work, `beginInterval` and `endInterval` across a suspension
