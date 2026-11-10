import { SeverityNumber } from '@opentelemetry/api';
import { ATTR_CODE_FUNCTION_NAME, ATTR_CODE_STACKTRACE, ATTR_EXCEPTION_MESSAGE, ATTR_EXCEPTION_STACKTRACE, ATTR_EXCEPTION_TYPE, ATTR_SERVICE_NAME } from '@opentelemetry/semantic-conventions';
import { Array, Cause, Config, Context, Effect, ErrorReporter, type Fiber, Formatter, Iterable, Layer, Logger, LogLevel, Match, Metric, Option, Order, Predicate, Record, References, Schema, String, Struct, Tracer, UndefinedOr } from 'effect';
import { Otlp, OtlpSerialization } from 'effect/observability';

// --- [TABLES] --------------------------------------------------------------------------

const SIGNALS = { traces: 'OTEL_TRACES_EXPORTER', metrics: 'OTEL_METRICS_EXPORTER', logs: 'OTEL_LOGS_EXPORTER' } as const;
const SERIALIZATIONS = { 'http/protobuf': OtlpSerialization.layerProtobuf, 'http/json': OtlpSerialization.layerJson } as const;

// --- [MODELS] --------------------------------------------------------------------------

const exporters = Config.all({
    disabled: Config.Boolean('OTEL_SDK_DISABLED').pipe(Config.withDefault(false)),
    names: Config.all(Record.map(SIGNALS, (variable) => Config.Array(Schema.Trim, variable).pipe(Config.withDefault([])))),
}).pipe(Config.map(({ disabled, names }) => Record.map(names, (named) => (disabled || Array.contains(named, 'none') ? [] : named))));

// --- [OPERATIONS] ----------------------------------------------------------------------

const severityNumber = Match.type<LogLevel.LogLevel>().pipe(
    Match.whenOr('All', 'None', () => SeverityNumber.UNSPECIFIED),
    Match.orElse((severity) => SeverityNumber[String.toUpperCase(severity)]),
);
const scope = (fiber: Fiber.Fiber<unknown, unknown>) => Option.flatMap(Context.getOption(fiber.context, Tracer.ParentSpan), Match.valueTags({ Span: (span) => Option.some(span.name), ExternalSpan: () => Option.none<string>() }));
const formatLogRecord = (resource?: Record.ReadonlyRecord<string, unknown>) =>
    Logger.make(({ cause, date, fiber, logLevel, message }: Logger.Options<unknown>) => {
        const errors = Cause.prettyErrors(cause, { includeCauseInStack: true });
        const text = Array.map(Array.ensure(message), (part) => (Predicate.isString(part) ? part : Formatter.format(part)));
        return {
            timestamp: String.replace('Z', '000Z')(date.toISOString()),
            severity_text: String.toUpperCase(logLevel),
            severity_number: severityNumber(logLevel),
            body: (Array.isReadonlyArrayNonEmpty(text) ? text : Array.map(errors, Struct.get('message'))).join(' '),
            ...UndefinedOr.map(
                Option.getOrElse(scope(fiber), () => resource?.[ATTR_SERVICE_NAME]),
                (name) => ({ scope: name }),
            ),
            ...UndefinedOr.map(Context.getOrUndefined(fiber.context, Tracer.ParentSpan), (span) => ({ trace_id: span.traceId, span_id: span.spanId, trace_flags: span.sampled ? '01' : '00' })),
            attributes: {
                ...fiber.getRef(References.CurrentLogAnnotations),
                ...Record.fromIterableWith(fiber.getRef(References.CurrentLogSpans), ([label, started]) => [`effect.log_span.${label}`, date.getTime() - started]),
                ...(Array.isReadonlyArrayNonEmpty(errors) ? { [ATTR_EXCEPTION_TYPE]: errors[0].name, [ATTR_EXCEPTION_MESSAGE]: errors[0].message, [ATTR_EXCEPTION_STACKTRACE]: Array.map(errors, (error) => error.stack ?? error.message).join('\n') } : {}),
                ...(LogLevel.isGreaterThanOrEqualTo(logLevel, 'Warn') ? UndefinedOr.map(fiber.getRef(References.CurrentStackFrame), (frame) => ({ [ATTR_CODE_FUNCTION_NAME]: frame.name, [ATTR_CODE_STACKTRACE]: frame.stack() })) : {}),
            },
            ...(resource === undefined ? {} : { resource }),
        };
    });
const errorReporter = ErrorReporter.make(({ error, severity, attributes, fiber }) => Effect.logWithLevel(Predicate.hasProperty(error, ErrorReporter.severity) ? severity : 'Error')(Cause.fail(error)).pipe(Effect.annotateLogs(attributes), Effect.runSyncWith(fiber.context)));

// --- [COMPOSITION] ---------------------------------------------------------------------

const layerWithoutDefaultLogger = Layer.effect(
    Logger.CurrentLoggers,
    Effect.map(Logger.CurrentLoggers, (loggers) => new Set(Iterable.filter(loggers, (logger) => logger !== Logger.defaultLogger))),
);
const layerMinimumLogLevels = (minimum: LogLevel.LogLevel, scopes: Record.ReadonlyRecord<string, LogLevel.LogLevel>) => {
    const overrides = Array.sortWith(Record.toEntries(scopes), ([prefix]) => prefix.length, Order.flip(Order.Number));
    const floor = (fiber: Fiber.Fiber<unknown, unknown>) =>
        Option.match(
            Option.flatMap(scope(fiber), (name) => Array.findFirst(overrides, ([prefix]) => String.startsWith(prefix)(name))),
            { onNone: () => minimum, onSome: ([, level]) => level },
        );
    return Layer.merge(
        Layer.succeed(References.MinimumLogLevel, Array.min([minimum, ...Object.values(scopes)], LogLevel.Order)),
        Array.isReadonlyArrayEmpty(overrides)
            ? Layer.empty
            : Layer.effect(
                  Logger.CurrentLoggers,
                  Effect.map(Logger.CurrentLoggers, (loggers) => new Set(Iterable.map(loggers, (logger) => Logger.make((entry: Logger.Options<unknown>) => (LogLevel.isGreaterThanOrEqualTo(entry.logLevel, floor(entry.fiber)) ? logger.log(entry) : undefined))))),
              ),
    );
};
const layerOtlp = (resource?: NonNullable<Parameters<typeof Otlp.layerFromConfig>[0]>['resource']) =>
    Effect.all([exporters, Config.Literals(Struct.keys(SERIALIZATIONS), 'OTEL_EXPORTER_OTLP_PROTOCOL').pipe(Config.withDefault('http/protobuf'))]).pipe(
        Effect.map(([named, protocol]) => Layer.provide(Layer.merge(Otlp.layerFromConfig({ resource }), Array.contains(named.metrics, 'otlp') ? Metric.enableRuntimeMetricsLayer : Layer.empty), SERIALIZATIONS[protocol])),
        Layer.unwrap,
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { errorReporter, exporters, formatLogRecord, layerMinimumLogLevels, layerOtlp, layerWithoutDefaultLogger };
