import { appendFileSync } from 'node:fs';
import process from 'node:process';
import { finished } from 'node:stream/promises';
import { layer, OtelLoggerProvider } from '@effect/opentelemetry/OtelLogger';
import { makeProducer } from '@effect/opentelemetry/OtelMetrics';
import { layerGlobal, makeExternalSpan, OtelTraceState } from '@effect/opentelemetry/OtelTracer';
import { configToAttributes, layerEmpty } from '@effect/opentelemetry/Resource';
import { type DiagLogger, DiagLogLevel, diag, logs, propagation, ROOT_CONTEXT, TraceFlags, trace } from '@opentelemetry/api';
import { CompositePropagator, diagLogLevelFromString, getStringFromEnv, W3CBaggagePropagator, W3CTraceContextPropagator } from '@opentelemetry/core';
import type { Instrumentation } from '@opentelemetry/instrumentation';
import { HostMetricsInstrumentation } from '@opentelemetry/instrumentation-host-metrics';
import { RuntimeNodeInstrumentation } from '@opentelemetry/instrumentation-runtime-node';
import { EnvironmentGetter, EnvironmentSetter } from '@opentelemetry/propagator-env-carrier';
import { LoggerProvider } from '@opentelemetry/sdk-logs';
import { PeriodicExportingMetricReader } from '@opentelemetry/sdk-metrics';
import { startNodeSdk } from '@opentelemetry/sdk-node';
import { ATTR_SERVICE_NAME, ATTR_SERVICE_VERSION } from '@opentelemetry/semantic-conventions';
import { ATTR_PROCESS_PID } from '@opentelemetry/semantic-conventions/incubating';
import { Array, Cause, Config, Context, Effect, ErrorReporter, Exit, Formatter, Function, flow, Layer, Logger, LogLevel, Match, Metric, MutableRef, Option, Predicate, Record, Runtime, Schema, Struct, Tracer, UndefinedOr } from 'effect';
import { createStream, type FileSize, type Interval, type Options } from 'rotating-file-stream';
import { errorReporter, exporters, formatLogRecord, layerWithoutDefaultLogger } from '#/index';

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

const Rotation = Schema.Struct({
    size: Schema.TemplateLiteral([Schema.Int, Schema.Literals(['B', 'K', 'M', 'G'])]),
    interval: Schema.Union([
        Schema.TemplateLiteral([Schema.Int, Schema.Literals(['M', 'd'])]),
        Schema.TemplateLiteral([Schema.Literals(['1', '2', '3', '4', '6', '8', '12', '24']), 'h']),
        Schema.TemplateLiteral([Schema.Literals(['1', '2', '3', '4', '5', '6', '10', '12', '15', '20', '30', '60']), Schema.Literals(['m', 's'])]),
    ]),
    maxFiles: Schema.Int.check(Schema.isGreaterThan(0)),
}) satisfies Schema.Schema<{ readonly size: FileSize; readonly interval: Interval; readonly maxFiles: number }>;
const ROTATION = { size: '8M', interval: '1d', maxFiles: 7 } as const satisfies typeof Rotation.Type;

// --- [SERVICES] ------------------------------------------------------------------------

const Baggage = Context.Reference<ReturnType<typeof propagation.getBaggage>>('@rasm/telemetry/node/Baggage', { defaultValue: Function.constUndefined });

// --- [OPERATIONS] ----------------------------------------------------------------------

const propagator = new CompositePropagator({ propagators: [new W3CTraceContextPropagator(), new W3CBaggagePropagator()] });
const lineage: (span: Tracer.AnySpan) => Context.Context<never> = Match.valueTags({ ExternalSpan: Struct.get('annotations'), Span: (local) => Option.match(local.parent, { onNone: () => local.annotations, onSome: lineage }) });
const toEnvironment = Effect.map(Effect.all([Effect.serviceOption(Tracer.ParentSpan), Baggage]), ([parent, baggage]) => {
    const environment: Record<string, string> = {};
    const carried = UndefinedOr.match(baggage, { onUndefined: () => ROOT_CONTEXT, onDefined: (entries) => propagation.setBaggage(ROOT_CONTEXT, entries) });
    const spanContext = Option.map(parent, (span) => ({ traceId: span.traceId, spanId: span.spanId, traceFlags: span.sampled ? TraceFlags.SAMPLED : TraceFlags.NONE, ...UndefinedOr.map(Context.getOrUndefined(lineage(span), OtelTraceState), (traceState) => ({ traceState })) }));
    propagator.inject(Option.match(spanContext, { onNone: () => carried, onSome: (current) => trace.setSpanContext(carried, current) }), undefined, new EnvironmentSetter(environment));
    return environment;
});
const toRotatingFile = Effect.fnUntraced(function* <Message>(self: Logger.Logger<Message, string>, filename: string, options: Options<Interval, FileSize, FileSize>) {
    const opened = MutableRef.make(Option.none<string>());
    const stream = yield* Effect.acquireRelease(
        Effect.try({
            try: () =>
                createStream(filename, { ...ROTATION, ...options })
                    .on('open', (path) => MutableRef.set(opened, Option.some(path)))
                    .on('error', (error) => process.stderr.write(`${Formatter.format(error)}\n`)),
            catch: (cause) => new Cause.UnknownError(cause, `Rotation options of ${filename} refused`),
        }),
        (rotated) => Effect.ignore(Effect.tryPromise(() => finished(rotated.end()))),
    );
    return Logger.make((entry: Logger.Options<Message>) => {
        const line = `${self.log(entry)}\n`;
        return Option.match(
            Option.filter(MutableRef.get(opened), () => entry.logLevel === 'Fatal'),
            { onNone: () => stream.write(line), onSome: (path) => appendFileSync(path, line) },
        );
    });
});
const stderrWarnings = Logger.make((entry: Logger.Options<unknown>) => (LogLevel.isGreaterThanOrEqualTo(entry.logLevel, 'Warn') ? Logger.defaultLogger.log(entry) : undefined));
const metricReader = Effect.gen(function* () {
    const { protocol, timing } = yield* metricReading;
    const [{ OTLPMetricExporter }, producer] = yield* Effect.all([metricExporters[protocol], makeProducer()]);
    return new PeriodicExportingMetricReader({ exporter: new OTLPMetricExporter(), metricProducers: [producer], ...Record.getSomes(timing) });
}).pipe(Effect.provide(layerEmpty));
const started = Effect.fnUntraced(function* (named: Config.Success<typeof exporters>, resource: Parameters<typeof configToAttributes>[0], instrumentations: Function.LazyArg<Instrumentation[]>) {
    const local = yield* Effect.context<never>();
    const diagnostics = Record.map(
        DIAGNOSTICS,
        (severity) =>
            (message: string, ...details: unknown[]) =>
                Effect.runSyncWith(local)(Effect.logWithLevel(severity)(message, ...details)),
    );
    const level = diagLogLevelFromString(getStringFromEnv('OTEL_LOG_LEVEL')) ?? DiagLogLevel.WARN;
    const metrics = yield* Match.value(named.metrics).pipe(
        Match.when(Array.isReadonlyArrayEmpty, () => Effect.succeed({ metricReaders: [] })),
        Match.when(Array.contains('otlp'), () => Effect.map(metricReader, (reader) => ({ metricReaders: [reader] }))),
        Match.orElse(() => Effect.succeed({})),
    );
    return yield* Effect.acquireRelease(
        Effect.sync(() =>
            startNodeSdk({
                logLevel: DiagLogLevel[Math.min(level, DiagLogLevel.WARN)],
                resourceAttributes: configToAttributes(resource),
                instrumentations: instrumentations(),
                ...metrics,
                ...(Array.isReadonlyArrayEmpty(named.traces) ? { spanProcessors: [] } : {}),
                ...(Array.isReadonlyArrayEmpty(named.logs) ? { logRecordProcessors: [] } : {}),
            }),
        ).pipe(Effect.tap(() => Effect.sync(() => diag.setLogger(diagnostics, { logLevel: level, suppressOverrideMessage: true })))),
        (sdk) => Effect.tryPromise(() => sdk.shutdown()).pipe(Effect.ignore({ log: 'Warn', message: 'OpenTelemetry SDK shutdown' }), Effect.andThen(Effect.sync(() => diag.disable()))),
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const layerFromEnvironment = Layer.unwrap(
    Effect.sync(() => {
        const context = propagator.extract(ROOT_CONTEXT, undefined, new EnvironmentGetter());
        return Layer.merge(UndefinedOr.match(trace.getSpanContext(context), { onUndefined: () => Layer.empty, onDefined: flow(makeExternalSpan, Layer.parentSpan) }), Layer.succeed(Baggage, propagation.getBaggage(context)));
    }),
);
const layerNodeSdk = Effect.fnUntraced(function* (resource: Parameters<typeof started>[1], instrumentations: Parameters<typeof started>[2] = () => [new RuntimeNodeInstrumentation(), new HostMetricsInstrumentation()]) {
    const named = yield* exporters;
    return Object.values(named).every(Array.isReadonlyArrayEmpty)
        ? Layer.empty
        : Layer.mergeAll(
              Array.isReadonlyArrayEmpty(named.traces) ? Layer.empty : layerGlobal,
              Array.isReadonlyArrayEmpty(named.metrics) ? Layer.empty : Metric.enableRuntimeMetricsLayer,
              Layer.unwrap(Effect.sync(() => Option.match(Option.liftPredicate(logs.getLoggerProvider(), Schema.is(Schema.instanceOf(LoggerProvider))), { onNone: () => Layer.empty, onSome: (provider) => Layer.provide(layer({}), Layer.succeed(OtelLoggerProvider, provider)) }))),
          ).pipe(Layer.provide(Layer.effectDiscard(started(named, resource, instrumentations))));
}, Layer.unwrap);
const layerUncaughtExceptionMonitor = Layer.effectDiscard(
    Effect.gen(function* () {
        const services = yield* Effect.context<never>();
        const crashed: NodeJS.UncaughtExceptionListener = (error, origin) => Effect.logFatal(Cause.die(error)).pipe(Effect.annotateLogs({ origin }), Effect.runSyncWith(services));
        yield* Effect.acquireRelease(
            Effect.sync(() => process.on('uncaughtExceptionMonitor', crashed)),
            () => Effect.sync(() => process.removeListener('uncaughtExceptionMonitor', crashed)),
        );
    }),
);
const layerErrorReported = Layer.effectDiscard(
    Effect.addFinalizer((exit) =>
        Exit.isFailure(exit) && !Cause.hasInterruptsOnly(exit.cause)
            ? Effect.andThen(
                  Effect.logError(exit.cause),
                  Effect.sync(() => Option.map(Option.liftPredicate(Cause.squash(exit.cause), Predicate.isObject), (error) => Object.assign(error, { [Runtime.errorReported]: false }))),
              )
            : Effect.void,
    ),
);
const layerNode = ({ resource, file, instrumentations }: { readonly resource: Parameters<typeof layerNodeSdk>[0]; readonly file?: readonly [filename: string, options: Parameters<typeof toRotatingFile>[2]]; readonly instrumentations?: Parameters<typeof layerNodeSdk>[1] }) => {
    const json = Logger.map(formatLogRecord({ [ATTR_SERVICE_NAME]: resource.serviceName, [ATTR_SERVICE_VERSION]: resource.serviceVersion, [ATTR_PROCESS_PID]: process.pid }), Formatter.formatJson);
    return Layer.merge(layerUncaughtExceptionMonitor, layerErrorReported).pipe(
        Layer.provideMerge(layerNodeSdk(resource, instrumentations)),
        Layer.provideMerge(
            Layer.mergeAll(
                Logger.layer(file === undefined ? [Logger.withConsoleError(json)] : [toRotatingFile(json, ...file), stderrWarnings], { mergeWithExisting: true }).pipe(Layer.provide(layerWithoutDefaultLogger)),
                ErrorReporter.layer([errorReporter], { mergeWithExisting: true }),
                Layer.succeed(Logger.LogToStderr, true),
                layerFromEnvironment,
            ),
        ),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Baggage, layerErrorReported, layerFromEnvironment, layerNode, layerNodeSdk, layerUncaughtExceptionMonitor, Rotation, toEnvironment, toRotatingFile };
