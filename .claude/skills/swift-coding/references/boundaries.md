# [BOUNDARIES]

## [01]-[DECODING]

`Line` is a `Codable` record holding `sku`, `unitPrice`, and its quantity rule in a `quantity: Bounded<1, 999>` field:

```swift
struct Bounded<let low: Int, let high: Int>: RawRepresentable, Codable, Sendable {
    let rawValue: Int

    init?(rawValue: Int) {
        guard (low...high).contains(rawValue) else { return nil }
        self.rawValue = rawValue
    }
}
```

- `RawRepresentable` over `Int` supplies `init(from:)` and `encode(to:)`, and a rejected value throws `DecodingError.dataCorrupted` at its coding path
- Key strategies (`.convertFromSnakeCase`) derive wire spellings, `CodingKeys` hold a key no strategy derives in converted spelling (`unitPrice`)

Synthesized `Codable` for an enum with associated values nests fields under a key named for the case. A wire naming its case in a field decodes through one `init(from:)` that reads the discriminator and associated values from one decoder:

```swift
@CasePathable
@dynamicMemberLookup
enum Event: Decodable, Sendable {
    case placed(Line)
    case cancelled(reason: String)
    case unknown(kind: String)

    private enum CodingKeys: CodingKey {
        case kind
        case reason
    }

    private enum Kind: String, Sendable {
        case placed
        case cancelled
    }

    init(from decoder: any Decoder) throws {
        let container: KeyedDecodingContainer<CodingKeys> = try decoder.container(keyedBy: CodingKeys.self)
        self =
            switch try container.decode(Lenient<Kind>.self, forKey: .kind) {
                case .known(.placed): .placed(try Line(from: decoder))
                case .known(.cancelled): .cancelled(reason: try container.decode(String.self, forKey: .reason))
                case .unknown(let kind): .unknown(kind: kind)
            }
    }
}
```

- `Line(from: decoder)` reads `Line` fields beside `kind`, and `Lenient<Kind>` types the discriminator with no string literal
- `unknown(kind:)` keeps a case a newer producer sends as data, and every switch over `Event` handles it

## [02]-[PATHS]

Names from outside resolve beneath a root directory's descriptor, and `stat` reads a host metadata record whole:

```swift
func attachment(_ name: FilePath, under root: FileDescriptor) throws(OrderError) -> Stat {
    do { return try name.stat(relativeTo: root, flags: .resolveBeneath) } catch { throw OrderError(subject: name.string, reason: .unreadable(error)) }
}
```

- `.resolveBeneath` makes the kernel refuse with `ENOTCAPABLE` a name resolving outside the root through `..`, a leading `/`, or a symlink
- `FileDescriptor.open(root, .readOnly, options: .directory)` opens the root once, and a rename of its path after the open moves no read
- `stat` throws `Errno` typed, and `catch` binds `Errno` with its host code
- C calls fail by returned status (`posix_spawn*`) or by `-1` or `NULL` with global `errno`, read as `Errno(rawValue:)`
- NUL-terminated C buffers read as `String(validating: buffer.prefix { unit in unit != 0 }, as: UTF8.self)`

## [03]-[PROCESSES]

`Configuration` holds one command as a value. A session with a process-group teardown makes a cancelled caller or a throwing body stop its child and every process that child started. Body forms fold standard output as it streams while standard error collects under a limit:

```swift
extension PlatformOptions {
    static func session(grace: Duration) -> Self {
        var options: Self = Self()
        options.createSession = true
        options.teardownSequence = [.gracefulShutDown(toProcessGroup: true, allowedDurationToNextStep: grace)]
        return options
    }
}

extension Configuration {
    func reduceLines<Value: Sendable>(into initial: Value, limit: Int, _ update: @Sendable (inout Value, String) -> Void) async throws(OrderError) -> Value {
        let subject: String = "\(executable) \(arguments)"
        let result: ExecutionResult<Value, SequenceOutput, StringOutput<UTF8>>
        do {
            result = try await Subprocess.run(self, input: .none, output: .sequence, error: .string(limit: limit)) { execution in
                try await execution.standardOutput.strings(separatedBy: .lineBreaks, bufferingPolicy: .maxLineLength(limit)).reduce(into: initial, update)
            }
        } catch {
            throw OrderError(subject: subject, reason: .failed(error))
        }
        guard result.terminationStatus.isSuccess else {
            throw OrderError(subject: subject, reason: .exited(result.terminationStatus, standardError: result.standardError))
        }
        return result.closureResult
    }
}

let log: Configuration = .init(executable: .name("git"), arguments: ["log", "--format=%H"], workingDirectory: directory, platformOptions: .session(grace: grace))
let revisions: [String] = try await log.reduceLines(into: [], limit: limit) { hashes, hash in hashes.append(hash) }
```

`directory` is a `FilePath`, `grace` a `Duration`, and `limit` an `Int`.

- `run` throws `SubprocessError` with a `code` when a process fails to start or its I/O fails
- Failed exits of a started process keep `terminationStatus` and standard error whole in `exited(TerminationStatus, standardError: String)`
- `limit` bounds standard error and each output line, a longer one throws `outputLimitExceeded`, and memory stays flat however long the output runs
- `.name` searches the child environment's `PATH` whatever its working directory, a missing program is `SubprocessError.Code.executableNotFound`
- Arguments reach the child as separate values with no shell, `.inherit.updating([...])` changes variables for one run and `nil` removes one
- Signalable processes report exit through `kqueue` `EVFILT_PROC` with `NOTE_EXIT | NOTE_EXITSTATUS | NOTE_EXIT_DETAIL`, `data` holding wait status
- `kAEQuitApplication` with `keyAESaveOptions` set to `kAEYes` saves every document with a location, a quit a person or agent answers leaves it unset

## [04]-[COMMAND_LINES]

Commands are `ParsableCommand` types, `AsyncParsableCommand` where `run` awaits, and the parser builds arguments through `ExpressibleByArgument`. `RawRepresentable` types over an `ExpressibleByArgument` raw value conform in an empty extension:

```swift
extension Bounded: ExpressibleByArgument {}

@main
struct Report: ParsableCommand {
    static let configuration: CommandConfiguration = .init(abstract: "Total the placed order lines of a log")

    @Argument(help: "Order log to read", transform: { URL(filePath: $0) })
    var log: URL

    @Option(help: "Smallest line quantity counted")
    var minimum: Bounded<1, 999>

    func run() throws {
        let decoder: JSONDecoder = JSONDecoder()
        decoder.keyDecodingStrategy = .convertFromSnakeCase
        let lines: [Line] = try decoder.decode([Event].self, from: Data(contentsOf: log)).compactMap(\.placed)
        print(lines.filter { line in line.quantity.rawValue >= minimum.rawValue }.reduce(Decimal.zero) { sum, line in sum + line.unitPrice * Decimal(line.quantity.rawValue) })
    }
}
```

- `nil` from `init?(rawValue:)` prints the rejected value with usage and exits with a validation status
- `validate()` holds a rule reading more than one property, a rule of one value belongs to its type
- `transform:` adapts a host type the parser lacks (`URL`)
- Errors `run` throws print as `LocalizedError.errorDescription` or `description` and exit with `EXIT_FAILURE`, a thrown `ExitCode` sets the status
- `print` writes results from `run` of the `@main` type alone, diagnostics go to `Logger`

## [05]-[LOGGING]

`Logger` derives a subsystem from the bundle and a category from an owning type, with neither spelled at a call site:

```swift
extension Logger {
    init(for owner: any Any.Type) {
        self.init(subsystem: Bundle.main.bundleIdentifier ?? ProcessInfo.processInfo.processName, category: String(reflecting: owner))
    }
}
```

Each interpolation states `privacy:` by the value it marks:

| [INDEX] | [VALUE]                               | [PRIVACY]               |
| :-----: | :------------------------------------ | :---------------------- |
|  [01]   | Count, state, or generated identifier | `.public`               |
|  [02]   | Path, name, or message body           | `.private`              |
|  [03]   | Identifier correlated across lines    | `.private(mask: .hash)` |
|  [04]   | Credential                            | `.sensitive`            |

- Errors, metatypes, and `CustomStringConvertible` values interpolate whole with a mark, and the log renders them
- Messages name the operation and its subject, a failure that paths share logs once where they join
- `OSSignposter(logger:)` marks intervals: `withIntervalSignpost` around synchronous work, `beginInterval` and `endInterval` across a suspension
