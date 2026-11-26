import Configuration
import Files
import Foundation
import Instrumentation
import Logging
import OTel
import OTelSemanticConventions
import Processes
public import ServiceLifecycle
import SystemMetrics
import Telemetry

// --- [TYPES] ---------------------------------------------------------------------------

enum LogTarget: String, CaseIterable {
    case file
    case stderr
}

// --- [CONSTANTS] -----------------------------------------------------------------------

private let unsupported: [String: Set<Substring>] = [
    "OTEL_PROPAGATORS": ["baggage", "b3", "b3multi", "jaeger", "xray", "otTrace"],
    "OTEL_TRACES_SAMPLER": ["parentbased_jaeger_remote", "jaeger_remote", "xray"],
]

// --- [COMPOSITION] ---------------------------------------------------------------------

extension Program {
    // --- [SETTINGS]

    private func level(_ text: String, in entry: String, of key: ConfigKey) -> Logging.Logger.Level? {
        let level = Logging.Logger.Level(rawValue: text) ?? Logging.Logger.Level.allCases.first { level in level.severity.text.lowercased() == text }
        let severities = Logging.Logger.Level.allCases.map { level in level.severity.text.lowercased() }.filter { text in Logging.Logger.Level(rawValue: text) == nil }
        if level == nil { unrecognized.add(variable: key.description, value: entry, accepted: Logging.Logger.Level.allCases.map(\.rawValue) + severities) }
        return level
    }

    private func positive(_ key: ConfigKey) -> Int? {
        switch settings.int(forKey: key) {
            case .some(let value) where value <= 0:
                unrecognized.add(variable: key.description, value: "\(value)", accepted: ["greater than 0"])
                return nil
            case let value: return value
        }
    }

    private func rules() -> LogFilter.Rules {
        let (levelKey, filterKey): (ConfigKey, ConfigKey) = ("log.level", "log.filter")
        let scopes = settings.stringArray(forKey: filterKey, default: []).compactMap { entry in
            let parts = entry.split(separator: "=", maxSplits: 1, omittingEmptySubsequences: false).map { part in part.trimmingCharacters(in: .whitespaces) }
            let (scope, text) = parts.count == 2 ? (parts[0], parts[1]) : (entry, "")
            return level(text, in: entry, of: filterKey).map { level in (scope, level) }
        }
        return LogFilter.Rules(
            level: settings.string(forKey: levelKey).flatMap { text in level(text, in: text, of: levelKey) } ?? LogFilter.Rules().level,
            scopes: Dictionary(scopes) { _, last in last },
        )
    }

    // --- [SERVICES]

    private var preferences: [String: Any] { UserDefaults.standard.persistentDomain(forName: identifier) ?? [:] }

    private func reloadSettings(_ filter: LogFilter, reporting logger: Logging.Logger) -> ClosureService {
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
            let since = settings.double(forKey: "crashReports.since").map(Date.init(timeIntervalSince1970:)) ?? .now
            store(Date.now.timeIntervalSince1970, forKey: "crashReports.since")
            let found = DiagnosticReport.reports(since: since) { report in report.kind == .crash && report.isWritten(by: .main) }
            for report in found.reports {
                let fields: [String: Logging.Logger.MetadataValue?] = [
                    OTelAttribute.process.pid: .stringConvertible(report.pid), OTelAttribute.file.path: .string(report.url.path(percentEncoded: false)),
                    OTelAttribute.exception.type: report.exception.map { type in .attributed(type, attributes: [Privacy.public]) },
                ]
                logger.critical("Crash report found", metadata: fields.compactMapValues(\.self))
            }
            for failure in found.failures { logger.warning("Crash report read failed", error: failure) }
        }
    }

    // --- [PROCESS]

    public func telemetry(scopes: [Any.Type]) -> ServiceGroup {
        let process = ProcessInfo.processInfo
        let stated = preferences.compactMapValues { value in value as? String }.filter { variable, _ in variable.hasPrefix("OTEL_") }.merging(process.environment) { _, variable in variable }
        let signals: [(enabled: WritableKeyPath<OTel.Configuration, Bool>, exporter: String)] = [
            (\.logs.enabled, "OTEL_LOGS_EXPORTER"), (\.metrics.enabled, "OTEL_METRICS_EXPORTER"), (\.traces.enabled, "OTEL_TRACES_EXPORTER"),
        ]
        var configuration = signals.reduce(into: OTel.Configuration.default) { gated, signal in gated[keyPath: signal.enabled] = stated[signal.exporter] != nil }
        let attributes: [String: String?] = [
            OTelAttribute.service.name: identifier, OTelAttribute.service.version: version, OTelAttribute.service.instance.id: UUID().uuidString,
            OTelAttribute.process.pid: String(process.processIdentifier), OTelAttribute.process.executable.name: process.processName,
            OTelAttribute.process.runtime.name: "swift", OTelAttribute.host.name: process.hostName, OTelAttribute.os.type: "darwin",
            OTelAttribute.os.description: process.operatingSystemVersionString,
        ]
        configuration.resourceAttributes = configuration.logs.enabled || configuration.metrics.enabled || configuration.traces.enabled ? attributes.compactMapValues(\.self) : [:]
        let filter = LogFilter(roots: scopes, rules: rules())
        let resource: Logging.Logger.Metadata = [
            OTelAttribute.service.name: .string(identifier), OTelAttribute.service.version: version.map(Logging.Logger.MetadataValue.string),
            OTelAttribute.process.pid: .stringConvertible(process.processIdentifier),
        ]
        .compactMapValues(\.self)
        var file = RotatingFile(directory(.logs).appending(path: "\(process.processName).log", directoryHint: .notDirectory))
        file.size = positive("log.size") ?? file.size
        file.age = positive("log.age").map { seconds in .seconds(seconds) } ?? file.age
        file.files = positive("log.files") ?? file.files
        let target = settings.string(forKey: "log.target", as: LogTarget.self, default: .file)
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
                trace: keys,
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
        let logs = configuration.logs.enabled ? Result { try OTel.makeLoggingBackend(configuration: configuration) } : nil
        let exported: (factory: @Sendable (String) -> any LogHandler, service: any Service)? = if case .success(let backend)? = logs { backend } else { nil }
        LoggingSystem.bootstrap({ [export = exported?.factory] label, _ in records(label, export?(label)) }, metadataProvider: provider)
        configuration.logs.enabled = false
        let variables = stated.filter { variable, _ in refused[variable] == nil }
        let bootstrapped = configuration.metrics.enabled || configuration.traces.enabled ? Result { try OTel.bootstrap(configuration: configuration, environment: variables) } : nil
        let started: any Service? = if case .success(let service)? = bootstrapped { service } else { nil }
        let logger = Logging.Logger(label: String(reflecting: Program.self))
        for case .failure(let error)? in [logs?.map { _ in }, bootstrapped?.map { _ in }] { logger.warning("OpenTelemetry signal start failed", error: error) }
        unrecognized.log(to: logger)
        let metrics: [any Service] = configuration.metrics.enabled && started != nil ? [SystemMetricsMonitor(logger: Logging.Logger(label: String(reflecting: SystemMetricsMonitor.self)))] : []
        let exports: [any Service] = [exported?.service, started].compactMap(\.self) + metrics + [reloadSettings(filter, reporting: logger)]
        var group = ServiceGroupConfiguration(
            services: exports.map { service in .init(service: service) } + [.init(service: crashReports(to: logger), successTerminationBehavior: .ignore, failureTerminationBehavior: .ignore)],
            logger: Logging.Logger(label: String(reflecting: ServiceGroup.self)),
        )
        group.maximumGracefulShutdownDuration = .milliseconds(500)
        return ServiceGroup(configuration: group)
    }
}
