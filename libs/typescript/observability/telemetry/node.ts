import { appendFileSync } from 'node:fs';
import { basename, dirname, join } from 'node:path';
import process, { type ProcessEventMap } from 'node:process';
import { finished } from 'node:stream/promises';
import { layerGlobal, makeExternalSpan, OtelTraceState } from '@effect/opentelemetry/OtelTracer';
import { configToAttributes, layerEmpty } from '@effect/opentelemetry/Resource';
import { type DiagLogger, DiagLogLevel, diag, logs, propagation, ROOT_CONTEXT, TraceFlags, trace } from '@opentelemetry/api';
import { CompositePropagator, diagLogLevelFromString, getStringFromEnv, W3CBaggagePropagator, W3CTraceContextPropagator } from '@opentelemetry/core';
import { EnvironmentGetter, EnvironmentSetter } from '@opentelemetry/propagator-env-carrier';
import { ATTR_PROCESS_PID } from '@opentelemetry/semantic-conventions/incubating';
import { Array, Cause, Config, Context, Duration, Effect, ErrorReporter, Exit, Formatter, flow, Layer, Logger, LogLevel, Match, Metric, MutableRef, Option, Predicate, Record, Ref, Runtime, Schema, SchemaGetter, Struct, Tracer, UndefinedOr } from 'effect';
import envPaths from 'env-paths';
import { createStream, type FileSize, type Interval, type OptionsWithoutRotate } from 'rotating-file-stream';
import { errorReporter, exporters, formatLogRecord, layerMinimumLogLevels, layerWithoutDefaultLogger } from '#/otlp';

// --- [TABLES] --------------------------------------------------------------------------

const DIAGNOSTICS = { error: 'Error', warn: 'Warn', info: 'Info', debug: 'Debug', verbose: 'Trace' } as const satisfies Record<keyof DiagLogger, LogLevel.LogLevel>;
const metricExporters = {
    grpc: Effect.promise(() => import('@opentelemetry/exporter-metrics-otlp-grpc')),
    'http/json': Effect.promise(() => import('@opentelemetry/exporter-metrics-otlp-http')),
    'http/protobuf': Effect.promise(() => import('@opentelemetry/exporter-metrics-otlp-proto')),
} as const;

// --- [MODELS] --------------------------------------------------------------------------

const metricReading = Config.all({
    protocol: Config.Literals(Struct.keys(metricExporters), 'OTEL_EXPORTER_OTLP_METRICS_PROTOCOL').pipe(
        Config.orElse(() => Config.Literals(Struct.keys(metricExporters), 'OTEL_EXPORTER_OTLP_PROTOCOL')),
        Config.withDefault('http/protobuf'),
    ),
    timing: Config.all({ exportIntervalMillis: Config.option(Config.Int('OTEL_METRIC_EXPORT_INTERVAL')), exportTimeoutMillis: Config.option(Config.Int('OTEL_METRIC_EXPORT_TIMEOUT')) }),
});

const Count = Schema.Int.check(Schema.isGreaterThan(0));
const Rotation = Schema.Struct({
    size: Count.pipe(Schema.decodeTo(Schema.TemplateLiteral([Schema.Int, 'B']), { decode: SchemaGetter.transform((bytes) => `${bytes}B` as const), encode: SchemaGetter.forbiddenEncoding })),
    interval: Count.pipe(
        Schema.decodeTo(Schema.String, { decode: SchemaGetter.transform(flow(Duration.seconds, Duration.format)), encode: SchemaGetter.forbiddenEncoding }),
        Schema.decodeTo(Schema.Union([Schema.TemplateLiteral([Schema.Int, 'd']), Schema.TemplateLiteral([Schema.Literals(['1', '2', '3', '4', '6', '8', '12', '24']), 'h']), Schema.TemplateLiteral([Schema.Literals(['1', '2', '3', '4', '5', '6', '10', '12', '15', '20', '30', '60']), Schema.Literals(['m', 's'])])])),
    ),
    maxFiles: Count,
}) satisfies Schema.Codec<{ readonly size: FileSize; readonly interval: Interval; readonly maxFiles: number }, { readonly size: number; readonly interval: number; readonly maxFiles: number }>;

const Level = Schema.Literals(['trace', 'debug', 'info', 'notice', 'warning', 'error', 'critical', 'warn', 'fatal']).transform(['Trace', 'Debug', 'Info', 'Info', 'Warn', 'Error', 'Fatal', 'Warn', 'Fatal']);
const Scope = Schema.String.pipe(Schema.decodeTo(Schema.Array(Schema.String), { decode: SchemaGetter.split({ separator: '=' }), encode: SchemaGetter.transform((entry) => entry.join('=')) }), Schema.decodeTo(Schema.Tuple([Schema.Trim, Schema.Trim.pipe(Schema.decodeTo(Level))])));
const Target = Schema.Literals(['file', 'stderr']);

// --- [ERRORS] --------------------------------------------------------------------------

class TelemetryError extends Schema.TaggedError<TelemetryError, { readonly brand: unique symbol }>()('TelemetryError', {
    reason: Schema.TaggedUnion({ Refused: {}, Unavailable: {} }),
    subject: Schema.String,
    cause: Schema.optionalKey(Schema.Defect()),
    message: Schema.String,
}) {
    static readonly of = (reason: TelemetryError['reason']['_tag'], subject: string) => (cause?: unknown) => TelemetryError.make({ reason: { _tag: reason }, subject, ...(cause === undefined ? {} : { cause }), message: [`${reason}: ${subject}`, ...(cause === undefined ? [] : [Formatter.format(cause)])].join(': ') });
}

// --- [SERVICES] ------------------------------------------------------------------------

const Baggage = Context.Reference('@rasm/telemetry/node/Baggage', { defaultValue: () => propagation.getBaggage(ROOT_CONTEXT) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const interrupted = <A, E>(exit: Exit.Exit<A, E>) => Exit.isFailure(exit) && Cause.hasInterruptsOnly(exit.cause);
const propagator = new CompositePropagator({ propagators: [new W3CTraceContextPropagator(), new W3CBaggagePropagator()] });
const lineage: (span: Tracer.AnySpan) => Context.Context<never> = Match.valueTags({ ExternalSpan: Struct.get('annotations'), Span: (local) => Option.match(local.parent, { onNone: () => local.annotations, onSome: lineage }) });
const toEnvironment = Effect.map(Effect.all([Effect.serviceOption(Tracer.ParentSpan), Baggage]), ([parent, baggage]) => {
    const environment: Record<string, string> = {};
    const carried = UndefinedOr.match(baggage, { onUndefined: () => ROOT_CONTEXT, onDefined: (entries) => propagation.setBaggage(ROOT_CONTEXT, entries) });
    const spanContext = Option.map(parent, (span) => ({ traceId: span.traceId, spanId: span.spanId, traceFlags: span.sampled ? TraceFlags.SAMPLED : TraceFlags.NONE, ...UndefinedOr.map(Context.getOrUndefined(lineage(span), OtelTraceState), (traceState) => ({ traceState })) }));
    propagator.inject(Option.match(spanContext, { onNone: () => carried, onSome: (current) => trace.setSpanContext(carried, current) }), undefined, new EnvironmentSetter(environment));
    return environment;
});
const toRotatingFile = Effect.fnUntraced(function* <Message>(self: Logger.Logger<Message, string>, filename: string, options: OptionsWithoutRotate<Interval, FileSize, FileSize>) {
    const opened = MutableRef.make(Option.none<string>());
    const stream = yield* Effect.acquireRelease(
        Effect.try({
            try: () =>
                createStream(filename, { size: '8M', interval: '1d', maxFiles: 7, ...options })
                    .on('open', (path) => {
                        MutableRef.set(opened, Option.some(path));
                    })
                    .on('error', (error) => {
                        process.stderr.write(`${Formatter.format(error)}\n`);
                    }),
            catch: TelemetryError.of('Refused', `rotation options of ${filename}`),
        }),
        (rotated) => Effect.ignore(Effect.tryPromise({ try: () => finished(rotated.end()), catch: TelemetryError.of('Unavailable', filename) })),
    );
    return Logger.make((entry: Logger.Options<Message>) => {
        const line = `${self.log(entry)}\n`;
        return Option.match(entry.logLevel === 'Fatal' ? MutableRef.get(opened) : Option.none(), { onNone: () => stream.write(line), onSome: (path) => appendFileSync(path, line) });
    });
});
const stderrWarnings = Logger.make((entry: Logger.Options<unknown>) => (LogLevel.isGreaterThanOrEqualTo(entry.logLevel, 'Warn') ? Logger.defaultLogger.log(entry) : undefined));
const metricReader = Effect.gen(function* () {
    const { protocol, timing } = yield* metricReading;
    const { OTLPMetricExporter } = yield* metricExporters[protocol];
    const { PeriodicExportingMetricReader } = yield* Effect.promise(() => import('@opentelemetry/sdk-metrics'));
    const { makeProducer } = yield* Effect.promise(() => import('@effect/opentelemetry/OtelMetrics'));
    return new PeriodicExportingMetricReader({ exporter: new OTLPMetricExporter(), metricProducers: [yield* makeProducer()], ...Record.getSomes(timing) });
}).pipe(Effect.provide(layerEmpty));
const started = Effect.fnUntraced(function* (named: Config.Success<typeof exporters>, resource: Parameters<typeof configToAttributes>[0]) {
    const local = yield* Effect.context<never>();
    const { startNodeSdk } = yield* Effect.promise(() => import('@opentelemetry/sdk-node'));
    const [{ RuntimeNodeInstrumentation }, { HostMetricsInstrumentation }] = yield* Effect.all([Effect.promise(() => import('@opentelemetry/instrumentation-runtime-node')), Effect.promise(() => import('@opentelemetry/instrumentation-host-metrics'))]);
    const diagnostics = Record.map(DIAGNOSTICS, (severity) => flow(Effect.logWithLevel(severity), Effect.runSyncWith(local)<void, never>));
    const level = diagLogLevelFromString(getStringFromEnv('OTEL_LOG_LEVEL')) ?? DiagLogLevel.WARN;
    const metrics = yield* Match.value(named.metrics).pipe(
        Match.when(Array.isReadonlyArrayEmpty, () => Effect.succeed({ metricReaders: [] })),
        Match.when(Array.contains('otlp'), () => Effect.map(metricReader, (reader) => ({ metricReaders: [reader] }))),
        Match.orElse(() => Effect.succeed({})),
    );
    const options = {
        logLevel: DiagLogLevel[Math.min(level, DiagLogLevel.WARN)],
        resourceAttributes: configToAttributes(resource),
        instrumentations: [new RuntimeNodeInstrumentation(), new HostMetricsInstrumentation()],
        ...metrics,
        ...(Array.isReadonlyArrayEmpty(named.traces) ? { spanProcessors: [] } : {}),
        ...(Array.isReadonlyArrayEmpty(named.logs) ? { logRecordProcessors: [] } : {}),
    };
    return yield* Effect.acquireRelease(Effect.sync(() => startNodeSdk(options)).pipe(Effect.tap(() => Effect.sync(() => diag.setLogger(diagnostics, { logLevel: level, suppressOverrideMessage: true })))), (sdk) =>
        Effect.tryPromise({ try: () => sdk.shutdown(), catch: TelemetryError.of('Unavailable', 'OpenTelemetry SDK shutdown') }).pipe(Effect.ignore({ log: 'Warn' }), Effect.andThen(Effect.sync(() => diag.disable()))),
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const layerFromEnvironment = Effect.sync(() => propagator.extract(ROOT_CONTEXT, undefined, new EnvironmentGetter())).pipe(
    Effect.map((context) => Layer.merge(UndefinedOr.match(trace.getSpanContext(context), { onUndefined: () => Layer.empty, onDefined: flow(makeExternalSpan, Layer.parentSpan) }), Layer.succeed(Baggage, propagation.getBaggage(context)))),
    Layer.unwrap,
);
const layerNodeSdk = Effect.fnUntraced(function* (resource: Parameters<typeof started>[1]) {
    const named = yield* exporters;
    const logged = Effect.all([Effect.promise(() => import('@opentelemetry/sdk-logs')), Effect.promise(() => import('@effect/opentelemetry/OtelLogger'))]).pipe(
        Effect.map(([{ LoggerProvider }, { layer, OtelLoggerProvider }]) => Option.match(Option.liftPredicate(logs.getLoggerProvider(), Schema.is(Schema.instanceOf(LoggerProvider))), { onNone: () => Layer.empty, onSome: (provider) => Layer.provide(layer({}), Layer.succeed(OtelLoggerProvider, provider)) })),
        Layer.unwrap,
    );
    const bridged = Layer.mergeAll(Array.isReadonlyArrayEmpty(named.traces) ? Layer.empty : layerGlobal, Array.isReadonlyArrayEmpty(named.metrics) ? Layer.empty : Metric.enableRuntimeMetricsLayer, logged);
    return Object.values(named).every(Array.isReadonlyArrayEmpty) ? Layer.empty : Layer.provide(bridged, Layer.effectDiscard(started(named, resource)));
}, Layer.unwrap);
const layerUncaughtExceptionMonitor = Effect.gen(function* () {
    const services = yield* Effect.context<never>();
    const crashed = (...[error, origin]: ProcessEventMap['uncaughtExceptionMonitor']) => Effect.runSyncWith(services)(Effect.annotateLogs(Effect.logFatal(Cause.die(error)), { origin }));
    yield* Effect.acquireRelease(
        Effect.sync(() => process.on('uncaughtExceptionMonitor', crashed)),
        () => Effect.sync(() => process.removeListener('uncaughtExceptionMonitor', crashed)),
    );
}).pipe(Layer.effectDiscard);
const layerErrorReported = Effect.addFinalizer((exit) =>
    Exit.isFailure(exit) && !interrupted(exit) ? Effect.logError(exit.cause).pipe(Effect.andThen(Effect.sync(() => Option.map(Option.liftPredicate(Cause.squash(exit.cause), Predicate.isObject), (error) => Object.assign(error, { [Runtime.errorReported]: false }))))) : Effect.void,
).pipe(Layer.effectDiscard);
const layerNode = ({ resource, file }: { readonly resource: Parameters<typeof layerNodeSdk>[0]; readonly file?: readonly [filename: string, options: OptionsWithoutRotate<Interval, FileSize, FileSize>] }) => {
    const json = formatLogRecord({ ...resource, attributes: { [ATTR_PROCESS_PID]: process.pid } });
    return Layer.merge(layerUncaughtExceptionMonitor, layerErrorReported).pipe(
        Layer.provideMerge(layerNodeSdk(resource)),
        Layer.provideMerge([
            Logger.layer(file === undefined ? [Logger.withConsoleError(json)] : [toRotatingFile(json, ...file), stderrWarnings], { mergeWithExisting: true }).pipe(Layer.provide(layerWithoutDefaultLogger)),
            ErrorReporter.layer([errorReporter], { mergeWithExisting: true }),
            Layer.succeed(Logger.LogToStderr, true),
            layerFromEnvironment,
        ]),
    );
};
const layerNodeFromConfig = Effect.fnUntraced(function* ({ resource, target, level, file }: { readonly resource: Parameters<typeof layerNode>[0]['resource']; readonly target: typeof Target.Type; readonly level?: LogLevel.LogLevel; readonly file?: string }) {
    const refused = yield* Ref.make<readonly (readonly [setting: string, error: Config.ConfigError])[]>([]);
    const group = 'log';
    const read = <A>(setting: string, config: Config.Config<A>) => Effect.catch(config.pipe(Config.option, Config.nested(setting), Config.nested(group)), (error) => Effect.as(Ref.update(refused, Array.append([setting, error] as const)), Option.none<A>()));
    const stated = yield* Effect.all({
        level: read('level', Config.schema(Level)),
        filter: read('filter', Config.Array(Scope).pipe(Config.map(Record.fromEntries))),
        target: read('target', Config.schema(Target)),
        size: read('size', Config.schema(Rotation.fields.size)),
        interval: read('age', Config.schema(Rotation.fields.interval)),
        maxFiles: read('backups', Config.schema(Rotation.fields.maxFiles)),
    });
    const rotation = {
        ...Option.getOrUndefined(Option.map(stated.size, (size) => ({ size }))),
        ...Option.getOrUndefined(Option.map(stated.interval, (interval) => ({ interval }))),
        ...Option.getOrUndefined(Option.map(stated.maxFiles, (maxFiles) => ({ maxFiles }))),
    };
    const log = file ?? join(envPaths(resource.serviceName, { suffix: '' }).log, `${resource.serviceName}.log`);
    const rotated = [basename(log), { path: dirname(log), ...rotation }] as const;
    return Ref.get(refused).pipe(
        Effect.flatMap(
            Effect.forEach(([setting, error]) => Effect.flatMap(Effect.option(Config.String(setting).pipe(Config.nested(group))), (value) => Effect.annotateLogs(Effect.logWarning('Setting refused, default stands', Cause.fail(error)), { variable: `${group}.${setting}`, ...Record.getSomes({ value }) })), {
                discard: true,
            }),
        ),
        Effect.andThen(Effect.addFinalizer((exit) => (interrupted(exit) ? Effect.logInfo('Process interrupted') : Effect.void))),
        Layer.effectDiscard,
        Layer.provideMerge(layerMinimumLogLevels(level ?? Option.getOrElse(stated.level, () => 'Info' as const), Option.getOrElse(stated.filter, Record.empty<string, LogLevel.LogLevel>))),
        Layer.provideMerge(layerNode({ resource, ...(Option.getOrElse(stated.target, () => target) === 'stderr' ? {} : { file: rotated }) })),
    );
}, Layer.unwrap);
const teardown: Runtime.Teardown = (exit, onExit) => (interrupted(exit) ? onExit(0) : Runtime.defaultTeardown(exit, onExit));

// --- [EXPORTS] -------------------------------------------------------------------------

export { Baggage, layerErrorReported, layerFromEnvironment, layerNode, layerNodeFromConfig, layerNodeSdk, layerUncaughtExceptionMonitor, Rotation, TelemetryError, teardown, toEnvironment, toRotatingFile };
