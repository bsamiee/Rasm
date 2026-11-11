import Applications
import Configuration
import Files
import Foundation
import Instrumentation
import Logging
public import OTel
public import ServiceLifecycle
import SystemMetrics
import Telemetry

// --- [TYPES] ---------------------------------------------------------------------------

enum Target: String, CaseIterable {
    case file
    case stderr
}

// --- [CONSTANTS] -----------------------------------------------------------------------

private let unsupported: [String: Set<Substring>] = [
    "OTEL_PROPAGATORS": ["baggage", "b3", "b3multi", "jaeger", "xray", "otTrace"],
    "OTEL_TRACES_SAMPLER": ["parentbased_jaeger_remote", "jaeger_remote", "xray"],
]
private let levelNames: [String] =
    Logging.Logger.Level.allCases.map(\.rawValue) + Logging.Logger.Level.allCases.map { level in level.severity.text.lowercased() }.filter { text in Logging.Logger.Level(rawValue: text) == nil }

// --- [OPERATIONS] ----------------------------------------------------------------------

private func pair(_ entry: String) -> (String, String)? {
    let parts = entry.split(separator: "=", maxSplits: 1, omittingEmptySubsequences: false)
    return parts.count == 2 ? (parts[0].trimmingCharacters(in: .whitespaces), parts[1].trimmingCharacters(in: .whitespaces)) : nil
}

// --- [COMPOSITION] ---------------------------------------------------------------------

extension Program {
    // --- [LEVELS]

    private func level(_ text: String, in entry: String, of key: ConfigKey) -> Logging.Logger.Level? {
        let level = Logging.Logger.Level(rawValue: text) ?? Logging.Logger.Level.allCases.first { level in level.severity.text.lowercased() == text }
        if level == nil { unrecognized.add(variable: key.description, value: entry, accepted: levelNames) }
        return level
    }

    private func rules() -> LogFilter.Rules {
        let (levelKey, filterKey): (ConfigKey, ConfigKey) = ("log.level", "log.filter")
        let defaults = LogFilter.Rules()
        let scopes = settings.stringArray(forKey: filterKey, default: []).compactMap { entry in
            let (scope, text) = pair(entry) ?? (entry, "")
            return level(text, in: entry, of: filterKey).map { level in (scope, level) }
        }
        return LogFilter.Rules(
            level: settings.string(forKey: levelKey).flatMap { text in level(text, in: text, of: levelKey) } ?? defaults.level,
            others: defaults.others,
            scopes: Dictionary(scopes) { _, last in last },
        )
    }

    // --- [SERVICES]

    private var preferences: [String: Any] { UserDefaults.standard.persistentDomain(forName: identifier) ?? [:] }

    private func reloading(_ filter: LogFilter, reporting logger: Logging.Logger) -> ClosureService {
        let file = URL.libraryDirectory.appending(components: "Preferences", "\(identifier).plist", directoryHint: .notDirectory)
        return ClosureService {
            await cancelWhenGracefulShutdown {
                var known = Set(Self.values(preferences).keys)
                for await change in FileEvent.events(of: file) {
                    if case .failure(let failure) = change { logger.warning("Settings watch failed", error: failure) }
                    let next = Self.values(preferences)
                    stored.setValues(known.subtracting(next.keys).reduce(into: next.mapValues(Optional.some)) { values, removed in values[removed] = .some(nil) })
                    known = Set(next.keys)
                    filter.rules = rules()
                    unrecognized.log(to: logger)
                }
            }
            try await gracefulShutdown()
        }
    }

    private func crashReports(to logger: Logging.Logger) -> ClosureService {
        ClosureService {
            let since = UserDefaults.standard.object(forKey: "crashReports.since") as? Date ?? .now
            UserDefaults.standard.set(Date.now, forKey: "crashReports.since")
            let found = CrashReport.reports(since: since) { report in report.bundleIdentifier == identifier }
            for report in found.reports {
                let fields: [String: Logging.Logger.MetadataValue?] = [
                    "process.pid": .stringConvertible(report.pid), "file": .string(report.url.path(percentEncoded: false)),
                    "exception.type": report.exception.map { type in .attributed(type, attributes: [Privacy.public]) },
                ]
                logger.critical("Crash report found", metadata: fields.compactMapValues(\.self))
            }
            for failure in found.failures { logger.warning("Crash report read failed", error: failure) }
        }
    }

    // --- [PROCESS]

    public func telemetry(scopes: [Any.Type], configuration: OTel.Configuration = .default, shutdownWithin limit: Duration = .milliseconds(500)) -> ServiceGroup {
        let stated = preferences.compactMapValues { value in value as? String }.filter { variable, _ in variable.hasPrefix("OTEL_") }.merging(environment) { _, variable in variable }
        let signals: [(enabled: WritableKeyPath<OTel.Configuration, Bool>, exporter: String)] = [
            (\.logs.enabled, "OTEL_LOGS_EXPORTER"), (\.metrics.enabled, "OTEL_METRICS_EXPORTER"), (\.traces.enabled, "OTEL_TRACES_EXPORTER"),
        ]
        var configuration = signals.reduce(into: configuration) { gated, signal in gated[keyPath: signal.enabled] = gated[keyPath: signal.enabled] && stated[signal.exporter] != nil }
        let process = ProcessInfo.processInfo
        let detected: [String: String] =
            configuration.logs.enabled || configuration.metrics.enabled || configuration.traces.enabled
            ? [
                "service.instance.id": UUID().uuidString, "process.executable.name": process.processName, "host.name": process.hostName, "os.type": "darwin",
                "os.description": process.operatingSystemVersionString, "process.runtime.name": "swift",
            ] : [:]
        configuration.resourceAttributes =
            detected
            .merging(["service.name": identifier, "service.version": version, "process.pid": String(process.processIdentifier)].compactMapValues(\.self)) { _, own in own }
            .merging(configuration.resourceAttributes) { _, caller in caller }
        let filter = LogFilter(roots: scopes, rules: rules())
        let resource: Logging.Logger.Metadata = [
            "service.name": configuration.resourceAttributes["service.name"].map(Logging.Logger.MetadataValue.string),
            "service.version": configuration.resourceAttributes["service.version"].map(Logging.Logger.MetadataValue.string), "process.pid": .stringConvertible(process.processIdentifier),
        ]
        .compactMapValues(\.self)
        var file = RotatingFile(directory(.log).appending(path: "\(name).log", directoryHint: .notDirectory))
        file.size = settings.int(forKey: "log.size", default: file.size)
        file.age = settings.int(forKey: "log.age", as: Duration.self, default: file.age)
        file.files = settings.int(forKey: "log.files", default: file.files)
        let target = settings.string(forKey: "log.target", as: Target.self, default: .file)
        let interactive = target == .file && isatty(STDERR_FILENO) == 1
        let keys = OTel.Configuration.LoggingMetadataProviderConfiguration.default
        let span = OTel.makeLoggingMetadataProvider(configuration: keys)
        let provider = Logging.Logger.MetadataProvider {
            let context = span.get()
            guard !context.isEmpty else { return [:] }
            return context.merging([keys.traceFlagsKey: InstrumentationContext.current?.otelIsSampled == true ? "01" : "00"]) { _, flags in flags }
                .mapValues { value in .attributed(value.description, attributes: [Privacy.public]) }
        }
        let records: @Sendable (String, any LogHandler?) -> JSONLogHandler = { [file] label, export in
            var terminal = StreamLogHandler.standardError(label: label)
            terminal.logLevel = .warning
            return JSONLogHandler(
                label: label,
                to: (target == .file ? file : nil, export, interactive ? terminal : nil),
                filter: filter,
                resource: resource,
                trace: (keys.traceIDKey, keys.spanIDKey, keys.traceFlagsKey),
                metadataProvider: provider,
            )
        }
        let diagnostics = Logging.Logger(label: String(reflecting: OTel.self)) { label in OnceLogHandler(records(label, nil)) }
        configuration.diagnosticLogger = .custom(diagnostics)
        configuration.diagnosticLogLevel = .warning
        let refused: [String: String] = stated.filter { variable, value in
            guard let values = unsupported[variable] else { return false }
            let reached: ArraySlice<Substring> = value.split(separator: "," as Character).prefix { element in element != "none" }
            return !values.isDisjoint(with: reached)
        }
        withLogger(diagnostics) { _ in
            for (variable, value) in refused {
                withLogger(mergingMetadata: ["environment_key": "\(variable, privacy: .public)", "environment_value": "\(value, privacy: .private)"]) { refusal in
                    refusal.warning("Failed to update configuration from environment")
                }
            }
        }
        let logs =
            configuration.logs.enabled
            ? Result { () throws -> (factory: @Sendable (String) -> any LogHandler, service: any Service) in
                let backend = try OTel.makeLoggingBackend(configuration: configuration)
                return (backend.factory, backend.service)
            } : nil
        let exported: (factory: @Sendable (String) -> any LogHandler, service: any Service)? = if case .success(let backend)? = logs { backend } else { nil }
        LoggingSystem.bootstrap(
            { [export = exported?.factory] label, _ in records(label, export?(label)) },
            metadataProvider: provider,
        )
        configuration.logs.enabled = false
        let variables = stated.filter { variable, _ in refused[variable] == nil }
        let bootstrapped =
            configuration.metrics.enabled || configuration.traces.enabled ? Result { () throws -> any Service in try OTel.bootstrap(configuration: configuration, environment: variables) } : nil
        let logger = Logging.Logger(label: String(reflecting: Program.self))
        for case .failure(let error)? in [logs.map { backend in backend.map(\.service) }, bootstrapped] { logger.warning("OpenTelemetry signal start failed", error: error) }
        unrecognized.log(to: logger)
        let started: any Service? = if case .success(let service)? = bootstrapped { service } else { nil }
        let metrics: [any Service] = configuration.metrics.enabled && started != nil ? [SystemMetricsMonitor(logger: Logging.Logger(label: String(reflecting: SystemMetricsMonitor.self)))] : []
        let exports: [any Service] = [exported?.service, started].compactMap(\.self) + metrics + [reloading(filter, reporting: logger)]
        var group = ServiceGroupConfiguration(
            services: exports.map { service in .init(service: service) } + [.init(service: crashReports(to: logger), successTerminationBehavior: .ignore, failureTerminationBehavior: .ignore)],
            logger: Logging.Logger(label: String(reflecting: ServiceGroup.self)),
        )
        group.maximumGracefulShutdownDuration = limit
        return ServiceGroup(configuration: group)
    }
}
