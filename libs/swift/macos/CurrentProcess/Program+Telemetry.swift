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

private enum Signal: String, CaseIterable {
    case logs = "OTEL_LOGS_EXPORTER"
    case metrics = "OTEL_METRICS_EXPORTER"
    case traces = "OTEL_TRACES_EXPORTER"
}

private enum Exporter: String, CaseIterable {
    case otlp
    case off = "none"
}

// --- [CONSTANTS] -----------------------------------------------------------------------

private let exporters: [String] = Exporter.allCases.map(\.rawValue)
private let supported: [(variable: String, values: [String])] = [
    (Signal.logs.rawValue, exporters), (Signal.metrics.rawValue, exporters), (Signal.traces.rawValue, exporters), ("OTEL_PROPAGATORS", ["tracecontext", "none"]),
    ("OTEL_TRACES_SAMPLER", ["always_on", "always_off", "traceidratio", "parentbased_always_on", "parentbased_always_off", "parentbased_traceidratio"]),
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
    // --- [EXPORT]

    private var preferences: [String: Any] { UserDefaults.standard.persistentDomain(forName: identifier) ?? [:] }

    private func exportVariables() -> [String: String] {
        let stated = preferences.compactMapValues { value in value as? String }.filter { variable, _ in variable.hasPrefix("OTEL_") }
        return supported.reduce(into: stated.merging(environment) { _, variable in variable }) { variables, row in
            guard let stated = variables[row.variable] else { return }
            let values = stated.split(separator: ",").map { value in value.trimmingCharacters(in: .whitespaces) }
            let kept = values.filter(row.values.contains)
            if kept.count != values.count { unrecognized.add(variable: row.variable, value: stated, accepted: row.values) }
            variables[row.variable] = kept.isEmpty ? nil : kept.joined(separator: ",")
        }
    }

    private func exporting(_ configuration: OTel.Configuration, from variables: [String: String]) -> OTel.Configuration {
        let otel = ConfigReader(provider: EnvironmentVariablesProvider(environmentVariables: variables), accessReporter: unrecognized)
        let enabled = !otel.bool(forKey: "otel.sdk.disabled", default: false)
        let exported: [Signal] = enabled ? Signal.allCases.filter { signal in variables[signal.rawValue].flatMap(Exporter.init(rawValue:)) == .otlp } : []
        var configuration = configuration
        configuration.logs.enabled = configuration.logs.enabled && exported.contains(.logs)
        configuration.metrics.enabled = configuration.metrics.enabled && exported.contains(.metrics)
        configuration.traces.enabled = configuration.traces.enabled && exported.contains(.traces)
        configuration.logs.exporter = .otlp
        let process = ProcessInfo.processInfo
        let detected: [String: String] =
            configuration.logs.enabled || configuration.metrics.enabled || configuration.traces.enabled
            ? [
                "service.instance.id": UUID().uuidString, "process.executable.name": process.processName, "host.name": process.hostName, "os.type": "darwin",
                "os.description": process.operatingSystemVersionString, "process.runtime.name": "swift",
            ] : [:]
        let stated = otel.stringArray(forKey: "otel.resource.attributes", default: []).compactMap(pair)
        configuration.resourceAttributes = detected.merging(["service.version": version, "process.pid": String(process.processIdentifier)].compactMapValues(\.self)) { _, own in own }
            .merging(configuration.resourceAttributes) { _, caller in caller }
            .merging(stated) { _, variable in variable }
        configuration.serviceName = otel.string(forKey: "otel.service.name", default: configuration.resourceAttributes["service.name"] ?? identifier)
        return configuration
    }

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
            let found = CrashReport.reports(for: identifier, since: since)
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
        let variables = exportVariables()
        var configuration = exporting(configuration, from: variables)
        let filter = LogFilter(roots: scopes, rules: rules())
        let resource: Logging.Logger.Metadata = [
            "service.name": .string(configuration.serviceName), "service.version": configuration.resourceAttributes["service.version"].map(Logging.Logger.MetadataValue.string),
            "process.pid": .stringConvertible(ProcessInfo.processInfo.processIdentifier),
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
        configuration.diagnosticLogger = .custom(Logging.Logger(label: String(reflecting: OTel.self)) { label in OnceLogHandler(records(label, nil)) })
        configuration.diagnosticLogLevel = .warning
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
        let signals =
            configuration.metrics.enabled || configuration.traces.enabled ? Result { () throws -> any Service in try OTel.bootstrap(configuration: configuration, environment: variables) } : nil
        let logger = Logging.Logger(label: String(reflecting: Program.self))
        for case .failure(let error)? in [logs.map { backend in backend.map(\.service) }, signals] { logger.warning("OpenTelemetry signal start failed", error: error) }
        unrecognized.log(to: logger)
        let started: any Service? = if case .success(let service)? = signals { service } else { nil }
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
