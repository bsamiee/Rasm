# [BOUNDARIES]

Covers values crossing a module's edge, from decoding external data to file metadata, child processes, command lines, and logs.

## [01]-[DECODING]

Constraints sit on a field's type, and one failable `init?(rawValue:)` serves decoding, memberwise construction, and argument parsing:

```swift
struct Bounded<let low: Int, let high: Int>: RawRepresentable, Codable, Sendable {
    let rawValue: Int

    init?(rawValue: Int) {
        guard (low...high).contains(rawValue) else { return nil }
        self.rawValue = rawValue
    }
}

struct Line: Codable, Sendable {
    let sku: String
    let quantity: Bounded<1, 999>
    let unitPrice: Decimal
}
```

- Integer generic parameters state a range in the field declaration, one type serves every bounded integer field
- `RawRepresentable` over `Int` supplies `init(from:)` and `encode(to:)`, and a `nil` raw value throws `DecodingError.dataCorrupted`
- Synthesized coders read `quantity` through `Bounded` under the property name, and each decoding error holds the field's coding path
- Key strategies (`.convertFromSnakeCase`) derive wire spellings, `CodingKeys` hold a key no strategy derives in converted spelling (`unitPrice`)

Synthesized `Codable` for an enum with payloads nests fields under a key named for the case. A wire naming its case in a field decodes through one `init(from:)` that reads the discriminator and decodes the payload from the same decoder:

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

    init(from decoder: any Decoder) throws {
        let container: KeyedDecodingContainer<CodingKeys> = try decoder.container(keyedBy: CodingKeys.self)
        let kind: String = try container.decode(String.self, forKey: .kind)
        self =
            switch kind {
                case "placed": .placed(try Line(from: decoder))
                case "cancelled": .cancelled(reason: try container.decode(String.self, forKey: .reason))
                default: .unknown(kind: kind)
            }
    }
}
```

- `Line(from: decoder)` reads payload fields beside `kind`, and the hand coder reads the discriminator alone
- `unknown(kind:)` keeps a case a newer producer sends as data, and every switch over `Event` handles it

## [02]-[PATHS]

Names from outside join a root through `lexicallyResolving(_:)`, and `stat()` reads a host metadata record whole:

```swift
func attachment(_ name: FilePath, under root: FilePath) throws(OrderError) -> Stat {
    guard let path: FilePath = root.lexicallyResolving(name) else { throw OrderError(subject: name.string, reason: .outOfRange) }
    do { return try path.stat() } catch { throw OrderError(subject: path.string, reason: .unreadable(error)) }
}
```

- `OrderError` is the module's error type, with reasons `outOfRange`, `unreadable(any Error)`, and `exited(TerminationStatus, standardError: String)`
- `lexicallyResolving(_:)` ignores a root in the name and returns `nil` where `..` escapes, and symlinks inside a root still reach outside
- `stat()` throws `Errno` typed, so `catch` binds `Errno` and the cause keeps its host code

## [03]-[PROCESSES]

`Configuration` holds one command as a value. A session with a process-group teardown makes a cancelled caller stop its child and every process that child started. Body forms fold standard output as it streams while standard error collects under a limit:

```swift
extension PlatformOptions {
    static func session(grace: Duration = .seconds(2)) -> Self {
        var options: Self = Self()
        options.createSession = true
        options.teardownSequence = [.gracefulShutDown(toProcessGroup: true, allowedDurationToNextStep: grace)]
        return options
    }
}

extension Configuration {
    func reduceLines<Value: Sendable>(into initial: Value, limit: Int = 1 << 16, _ update: (inout Value, String) -> Void) async throws(OrderError) -> Value {
        let subject: String = "\(executable) \(arguments)"
        let result: ExecutionResult<Value, SequenceOutput, StringOutput<UTF8>>
        do {
            result = try await Subprocess.run(self, input: .none, output: .sequence, error: .string(limit: limit)) { execution in
                try await execution.standardOutput.strings(separatedBy: .lineBreaks, bufferingPolicy: .maxLineLength(limit)).reduce(into: initial, update)
            }
        } catch {
            throw OrderError(subject: subject, reason: .unreadable(error))
        }
        guard result.terminationStatus.isSuccess else {
            throw OrderError(subject: subject, reason: .exited(result.terminationStatus, standardError: result.standardError))
        }
        return result.closureResult
    }
}

func revisions(in directory: FilePath) async throws(OrderError) -> [String] {
    let log: Configuration = .init(executable: .name("git"), arguments: ["log", "--format=%H"], workingDirectory: directory, platformOptions: .session())
    return try await log.reduceLines(into: []) { hashes, hash in hashes.append(hash) }
}
```

- `run` throws `SubprocessError` with a `code` when a process fails to start or its I/O fails, a started process reports through `terminationStatus`
- `isSuccess` decides the outcome, and a failure keeps `TerminationStatus` and standard error whole
- `limit` bounds standard error and each output line, a longer one throws, and memory stays flat however long the output runs
- `.name` searches the child environment's `PATH` whatever its working directory, a missing program is `SubprocessError.Code.executableNotFound`
- Arguments reach the child as separate values with no shell, `.inherit.updating([...])` changes variables for one run and `nil` removes one

## [04]-[COMMAND_LINES]

Commands declare arguments as properties, and the parser builds each value through `ExpressibleByArgument`. A `RawRepresentable` type over an `ExpressibleByArgument` raw value conforms with an empty extension, so the rule that rejects a wire value rejects an argument:

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
        print(lines.filter { line in line.quantity.rawValue >= minimum.rawValue }.map { line in line.unitPrice * Decimal(line.quantity.rawValue) }.reduce(0, +))
    }
}
```

- `nil` from `init?(rawValue:)` prints the rejected value with usage and exits with a validation status
- `validate()` holds a rule reading more than one property, a rule of one value belongs to its type
- `transform:` adapts a host type the parser lacks (`URL`), `AsyncParsableCommand` serves a `run` that awaits
- Errors `run` throws print as `LocalizedError.errorDescription` or `description` and exit with `EXIT_FAILURE`, a thrown `ExitCode` sets the status
- `print` writes results from `run` of the `@main` type alone, diagnostics go to `Logger`

## [05]-[LOGGING]

`Logger` derives a subsystem from the bundle and a category from an owning type, so no call site spells either:

```swift
extension Logger {
    init(for owner: any Any.Type) {
        self.init(subsystem: Bundle.main.bundleIdentifier ?? ProcessInfo.processInfo.processName, category: String(reflecting: owner))
    }
}
```

Each interpolation's privacy follows the value it marks:

| [INDEX] | [VALUE]                               | [PRIVACY]               |
| :-----: | :------------------------------------ | :---------------------- |
|  [01]   | Count, state, or generated identifier | `.public`               |
|  [02]   | Path, name, or message body           | `.private`              |
|  [03]   | Identifier correlated across lines    | `.private(mask: .hash)` |
|  [04]   | Credential                            | `.sensitive`            |

- Errors, metatypes, and `CustomStringConvertible` values interpolate whole with a mark, and the log renders them
- Messages name the operation and its subject, a failure that paths share logs once where they join
- `OSSignposter(logger:)` marks intervals: `withIntervalSignpost` around synchronous work, `beginInterval` and `endInterval` across a suspension
