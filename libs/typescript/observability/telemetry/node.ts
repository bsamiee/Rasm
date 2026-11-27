import { Buffer } from 'node:buffer';
import { closeSync, constants, fstatSync, mkdirSync, openSync, renameSync, rmSync, type Stats, statSync, writeSync } from 'node:fs';
import { dirname, join, parse } from 'node:path';
import { type Histogram, monitorEventLoopDelay, PerformanceObserver, constants as performanceConstants, performance } from 'node:perf_hooks';
import process, { type ProcessEventMap } from 'node:process';
import { getHeapSpaceStatistics, type HeapSpaceInfo } from 'node:v8';
import { layerGlobal, makeExternalSpan } from '@effect/opentelemetry/OtelTracer';
import { configToAttributes, layerEmpty } from '@effect/opentelemetry/Resource';
import { type DiagLogger, DiagLogLevel, diag, logs, ROOT_CONTEXT, trace } from '@opentelemetry/api';
import { diagLogLevelFromString, getStringFromEnv, W3CTraceContextPropagator } from '@opentelemetry/core';
import { EnvironmentGetter } from '@opentelemetry/propagator-env-carrier';
import {
    ATTR_NODEJS_EVENTLOOP_STATE,
    ATTR_PROCESS_PID,
    ATTR_V8JS_GC_TYPE,
    ATTR_V8JS_HEAP_SPACE_NAME,
    ATTR_V8JS_RESOURCE_TYPE,
    METRIC_NODEJS_EVENTLOOP_DELAY_MAX,
    METRIC_NODEJS_EVENTLOOP_DELAY_MEAN,
    METRIC_NODEJS_EVENTLOOP_DELAY_MIN,
    METRIC_NODEJS_EVENTLOOP_DELAY_P50,
    METRIC_NODEJS_EVENTLOOP_DELAY_P90,
    METRIC_NODEJS_EVENTLOOP_DELAY_P99,
    METRIC_NODEJS_EVENTLOOP_DELAY_STDDEV,
    METRIC_NODEJS_EVENTLOOP_TIME,
    METRIC_NODEJS_EVENTLOOP_UTILIZATION,
    METRIC_PROCESS_MEMORY_USAGE,
    METRIC_V8JS_GC_DURATION,
    METRIC_V8JS_MEMORY_HEAP_SPACE_AVAILABLE_SIZE,
    METRIC_V8JS_MEMORY_HEAP_SPACE_PHYSICAL_SIZE,
    METRIC_V8JS_MEMORY_HEAP_SPACE_SIZE,
    METRIC_V8JS_MEMORY_HEAP_USED,
    METRIC_V8JS_RESOURCE_ACTIVE,
    NODEJS_EVENTLOOP_STATE_VALUE_ACTIVE,
    NODEJS_EVENTLOOP_STATE_VALUE_IDLE,
    V8JS_GC_TYPE_VALUE_INCREMENTAL,
    V8JS_GC_TYPE_VALUE_MAJOR,
    V8JS_GC_TYPE_VALUE_MINOR,
    V8JS_GC_TYPE_VALUE_WEAKCB,
} from '@opentelemetry/semantic-conventions/incubating';
import { Array, Cause, Config, Duration, Effect, ErrorReporter, Exit, Formatter, flow, Layer, Logger, LogLevel, Match, Metric, MutableRef, Option, Order, Predicate, Queue, Record, Ref, References, Result, Runtime, Schema, SchemaGetter, Stream, Struct, UndefinedOr } from 'effect';
import envPaths from 'env-paths';
import { errorReporter, exporters, layerMinimumLogLevels, layerWithoutDefaultLogger, logRecord } from '#/otlp';

// --- [TABLES] --------------------------------------------------------------------------

const DIAGNOSTICS = { error: 'Error', warn: 'Warn', info: 'Info', debug: 'Debug', verbose: 'Trace' } as const satisfies Record<keyof DiagLogger, LogLevel.LogLevel>;
const LOG_TYPES = { Trace: 0x02, Debug: 0x02, Info: 0x01, Warn: 0x10, Error: 0x10, Fatal: 0x11 } as const satisfies Record<LogLevel.Severity, number>;
const LOG_ARGUMENT = { header: Uint8Array.of(0x02, 0x01, 0x22, 0x08), format: '\0%{public}s\0' } as const;
const DELAYS = {
    [METRIC_NODEJS_EVENTLOOP_DELAY_MIN]: (histogram: Histogram) => histogram.min,
    [METRIC_NODEJS_EVENTLOOP_DELAY_MAX]: (histogram: Histogram) => histogram.max,
    [METRIC_NODEJS_EVENTLOOP_DELAY_MEAN]: (histogram: Histogram) => histogram.mean,
    [METRIC_NODEJS_EVENTLOOP_DELAY_STDDEV]: (histogram: Histogram) => histogram.stddev,
    [METRIC_NODEJS_EVENTLOOP_DELAY_P50]: (histogram: Histogram) => histogram.percentile(50),
    [METRIC_NODEJS_EVENTLOOP_DELAY_P90]: (histogram: Histogram) => histogram.percentile(90),
    [METRIC_NODEJS_EVENTLOOP_DELAY_P99]: (histogram: Histogram) => histogram.percentile(99),
} as const;
const HEAP_SPACES = {
    [METRIC_V8JS_MEMORY_HEAP_USED]: 'space_used_size',
    [METRIC_V8JS_MEMORY_HEAP_SPACE_SIZE]: 'space_size',
    [METRIC_V8JS_MEMORY_HEAP_SPACE_AVAILABLE_SIZE]: 'space_available_size',
    [METRIC_V8JS_MEMORY_HEAP_SPACE_PHYSICAL_SIZE]: 'physical_space_size',
} as const satisfies Record<string, Exclude<keyof HeapSpaceInfo, 'space_name'>>;
const GC_TYPES = new Map([
    [performanceConstants.NODE_PERFORMANCE_GC_MAJOR, V8JS_GC_TYPE_VALUE_MAJOR],
    [performanceConstants.NODE_PERFORMANCE_GC_MINOR, V8JS_GC_TYPE_VALUE_MINOR],
    [performanceConstants.NODE_PERFORMANCE_GC_INCREMENTAL, V8JS_GC_TYPE_VALUE_INCREMENTAL],
    [performanceConstants.NODE_PERFORMANCE_GC_WEAKCB, V8JS_GC_TYPE_VALUE_WEAKCB],
]);
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
    size: Count.pipe(Schema.withConstructorDefault(Effect.succeed(8 << 20))),
    age: Count.pipe(Schema.withConstructorDefault(Effect.succeed(86_400))),
    backups: Count.pipe(Schema.withConstructorDefault(Effect.succeed(7))),
});
const Level = Schema.Literals(['trace', 'debug', 'info', 'notice', 'warning', 'error', 'critical', 'warn', 'fatal']).transform(['Trace', 'Debug', 'Info', 'Info', 'Warn', 'Error', 'Fatal', 'Warn', 'Fatal']);
const Scope = Schema.String.pipe(Schema.decodeTo(Schema.Array(Schema.String), { decode: SchemaGetter.split({ separator: '=' }), encode: SchemaGetter.transform((entry) => entry.join('=')) }), Schema.decodeTo(Schema.Tuple([Schema.Trim, Schema.Trim.pipe(Schema.decodeTo(Level))])));
const Target = Schema.Literals(['file', 'stderr']);
const GcEntry = Schema.Struct({ detail: Schema.Struct({ kind: Schema.Int }) });

// --- [ERRORS] --------------------------------------------------------------------------

class TelemetryError extends Schema.TaggedError<TelemetryError, { readonly brand: unique symbol }>()('TelemetryError', {
    reason: Schema.TaggedUnion({ Unavailable: {} }),
    subject: Schema.String,
    cause: Schema.optionalKey(Schema.Defect()),
    message: Schema.String,
}) {
    static readonly of = (reason: TelemetryError['reason']['_tag'], subject: string) => (cause?: unknown) => TelemetryError.make({ reason: { _tag: reason }, subject, ...(cause === undefined ? {} : { cause }), message: [`${reason}: ${subject}`, ...(cause === undefined ? [] : [Formatter.format(cause)])].join(': ') });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const interrupted = <A, E>(exit: Exit.Exit<A, E>) => Exit.isFailure(exit) && Cause.hasInterruptsOnly(exit.cause);
const propagator = new W3CTraceContextPropagator();
const utf8 = new TextEncoder();
const toRotatingFile = Effect.fnUntraced(function* <Message>(self: Logger.Logger<Message, string>, file: string, { size, age, backups }: typeof Rotation.Type) {
    const writer = yield* Effect.acquireRelease(
        Effect.sync(() => MutableRef.make(Option.none<{ readonly fd: number; readonly ino: number }>())),
        (held) => Effect.sync(() => MutableRef.getAndSet(held, Option.none())).pipe(Effect.map(Option.map(({ fd }) => closeSync(fd)))),
    );
    const failing = MutableRef.make(false);
    const lock = process.platform === 'darwin' ? 0x20 : 0;
    const backup = (index: number) => (index === 0 ? file : `${file}.${index}`);
    const open = (flags: number) => {
        mkdirSync(dirname(file), { recursive: true, mode: 0o700 });
        const fd = openSync(file, constants.O_WRONLY | constants.O_APPEND | constants.O_CREAT | flags, 0o600);
        return { fd, stat: fstatSync(fd) };
    };
    const due = (stat: Stats, bytes: number, expiry: number) => stat.size > 0 && (stat.size + bytes > size || stat.birthtimeMs <= expiry);
    const rotate = (stale: number, bytes: number, expiry: number) => {
        closeSync(stale);
        const locked = open(lock);
        try {
            const shifted = due(locked.stat, bytes, expiry) && statSync(file, { throwIfNoEntry: false })?.ino === locked.stat.ino ? Array.makeBy(backups + 1, (step) => backups - step) : [];
            const [kept, expired] = Array.partition(shifted, (index) => (index < backups && (statSync(backup(index), { throwIfNoEntry: false })?.mtimeMs ?? expiry) > expiry ? Result.succeed(index) : Result.fail(index)));
            expired.forEach((index) => rmSync(backup(index), { force: true }));
            kept.forEach((index) => renameSync(backup(index), backup(index + 1)));
        } finally {
            closeSync(locked.fd);
        }
        return open(0);
    };
    const append = (line: string, now: number) => {
        const [bytes, expiry, found] = [Buffer.byteLength(line), now - Duration.seconds(age).pipe(Duration.toMillis), statSync(file, { throwIfNoEntry: false })];
        const opened = MutableRef.getAndSet(writer, Option.none()).pipe(
            Option.match({
                onNone: () => open(0),
                onSome: (held) => {
                    if (found !== undefined && found.ino === held.ino) {
                        return { fd: held.fd, stat: found };
                    }
                    closeSync(held.fd);
                    return open(0);
                },
            }),
        );
        const current = due(opened.stat, bytes, expiry) ? rotate(opened.fd, bytes, expiry) : opened;
        MutableRef.set(writer, Option.some({ fd: current.fd, ino: current.stat.ino }));
        writeSync(current.fd, line);
    };
    return Logger.make((entry: Logger.Options<Message>) => {
        Result.try(() => append(`${self.log(entry)}\n`, entry.date.getTime())).pipe(
            Result.match({
                onSuccess: () => MutableRef.set(failing, false),
                onFailure: (cause) => MutableRef.getAndSet(failing, true) || process.stderr.write(`Log record append failed for ${file}: ${Formatter.format(cause)}\n`),
            }),
        );
    });
});
const toUnifiedLog = Effect.fnUntraced(function* (self: ReturnType<typeof logRecord>, subsystem: string) {
    const { dlopen, getRawPointer, toBuffer } = yield* Effect.tryPromise({ try: () => import('node:ffi'), catch: TelemetryError.of('Unavailable', 'node:ffi') });
    const {
        functions: { os_log_create: osLogCreate, os_log_type_enabled: osLogTypeEnabled, _os_log_impl: osLogImpl, _dyld_image_count: dyldImageCount, _dyld_get_image_header: dyldGetImageHeader, getsectiondata },
    } = yield* Effect.try({
        try: () =>
            dlopen(null, {
                os_log_create: { arguments: ['string', 'string'], return: 'pointer' },
                os_log_type_enabled: { arguments: ['pointer', 'u8'], return: 'bool' },
                _os_log_impl: { arguments: ['pointer', 'pointer', 'u8', 'pointer', 'buffer', 'u32'] },
                _dyld_image_count: { return: 'u32' },
                _dyld_get_image_header: { arguments: ['u32'], return: 'pointer' },
                getsectiondata: { arguments: ['pointer', 'string', 'string', 'buffer'], return: 'pointer' },
            }),
        catch: TelemetryError.of('Unavailable', 'unified log functions of the process image'),
    });
    const length = new BigUint64Array(1);
    const literal = Buffer.from(LOG_ARGUMENT.format);
    const { dso, format } = yield* Array.findFirst(Array.makeBy(dyldImageCount(), dyldGetImageHeader), (header) => {
        const section = getsectiondata(header, '__TEXT', '__oslogstring', length);
        const at = section === 0n ? -1 : toBuffer(section, Number(length[0]), false).indexOf(literal);
        return at < 0 ? Option.none() : Option.some({ dso: header, format: section + BigInt(at + 1) });
    }).pipe(Effect.fromOption(TelemetryError.of('Unavailable', 'a %{public}s format in the __oslogstring section of a loaded image')));
    const categories = new Map<string, bigint>();
    const category = (name: string) => categories.getOrInsertComputed(name, (created) => osLogCreate(subsystem, created));
    return Logger.make((entry: Logger.Options<unknown>) => {
        if (entry.logLevel === 'All' || entry.logLevel === 'None') {
            return;
        }
        const record = self.log(entry);
        const [log, type] = [category(record.scope ?? subsystem), LOG_TYPES[entry.logLevel]];
        if (osLogTypeEnabled(log, type) === 0) {
            return;
        }
        const fields = Array.sortWith(
            Object.entries(record.attributes).filter(([, value]) => value !== undefined),
            ([key]) => key,
            Order.String,
        ).map(([key, value]) => `${key}=${Predicate.isString(value) ? value : Formatter.format(value)}`);
        const text = utf8.encode(`${[record.body, ...fields].join(' ')}\0`);
        const argument = Buffer.alloc(LOG_ARGUMENT.header.length + BigUint64Array.BYTES_PER_ELEMENT);
        argument.set(LOG_ARGUMENT.header);
        argument.writeBigUInt64LE(getRawPointer(text), LOG_ARGUMENT.header.length);
        osLogImpl(dso, log, type, format, argument, argument.length);
    });
});
const stderrWarnings = Logger.make((entry: Logger.Options<unknown>) => (LogLevel.isGreaterThanOrEqualTo(entry.logLevel, 'Warn') ? Logger.defaultLogger.log(entry) : undefined));
const collections = Stream.callback<PerformanceEntry>((queue) =>
    Effect.acquireRelease(
        Effect.sync(() => {
            const observer = new PerformanceObserver((list) => {
                Queue.offerAllUnsafe(queue, list.getEntries());
            });
            observer.observe({ entryTypes: ['gc'] });
            return observer;
        }),
        (observer) => Effect.sync(() => observer.disconnect()),
    ),
);
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
    const { HostMetricsInstrumentation } = yield* Effect.promise(() => import('@opentelemetry/instrumentation-host-metrics'));
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
        instrumentations: [new HostMetricsInstrumentation({ metricGroups: ['system.cpu', 'system.memory', 'system.network', 'process.cpu'] })],
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
    Effect.map((context) => UndefinedOr.match(trace.getSpanContext(context), { onUndefined: () => Layer.empty, onDefined: flow(makeExternalSpan, Layer.parentSpan) })),
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
    const crashed = (...[error, origin]: ProcessEventMap['uncaughtExceptionMonitor']) => Cause.die(error).pipe(Effect.logFatal, Effect.annotateLogs({ origin }), Effect.runSyncWith(services)<void, never>);
    yield* Effect.acquireRelease(
        Effect.sync(() => process.on('uncaughtExceptionMonitor', crashed)),
        () => Effect.sync(() => process.removeListener('uncaughtExceptionMonitor', crashed)),
    );
}).pipe(Layer.effectDiscard);
const layerErrorReported = Effect.addFinalizer((exit) =>
    Exit.isFailure(exit) && !interrupted(exit) ? Effect.logError(exit.cause).pipe(Effect.andThen(Effect.sync(() => Option.map(Option.liftPredicate(Cause.squash(exit.cause), Predicate.isObject), (error) => Object.assign(error, { [Runtime.errorReported]: false }))))) : Effect.void,
).pipe(Layer.effectDiscard);
const layerRuntimeMetrics = Effect.gen(function* () {
    const interval = yield* Config.Int('OTEL_METRIC_EXPORT_INTERVAL').pipe(Config.withDefault(60_000));
    const delay = yield* Effect.acquireRelease(Effect.sync(() => monitorEventLoopDelay()).pipe(Effect.tap((histogram) => Effect.sync(() => histogram.enable()))), (histogram) => Effect.sync(() => histogram.disable()));
    const [loop, resources] = [MutableRef.make(performance.eventLoopUtilization()), MutableRef.make<ReadonlySet<string>>(new Set())];
    const sample = Effect.suspend(() => {
        const now = performance.eventLoopUtilization();
        const elapsed = performance.eventLoopUtilization(now, MutableRef.getAndSet(loop, now));
        const active = Record.map(
            Array.groupBy(process.getActiveResourcesInfo(), (type) => type),
            (types) => types.length,
        );
        const updates = [
            ...Record.collect(DELAYS, (name, read) => Metric.gauge(name).pipe(Metric.update(Duration.millis(read(delay) / 1e6).pipe(Duration.toSeconds)))),
            Metric.gauge(METRIC_NODEJS_EVENTLOOP_UTILIZATION).pipe(Metric.update(elapsed.utilization)),
            Metric.counter(METRIC_NODEJS_EVENTLOOP_TIME, { attributes: { [ATTR_NODEJS_EVENTLOOP_STATE]: NODEJS_EVENTLOOP_STATE_VALUE_IDLE } }).pipe(Metric.update(Duration.millis(elapsed.idle).pipe(Duration.toSeconds))),
            Metric.counter(METRIC_NODEJS_EVENTLOOP_TIME, { attributes: { [ATTR_NODEJS_EVENTLOOP_STATE]: NODEJS_EVENTLOOP_STATE_VALUE_ACTIVE } }).pipe(Metric.update(Duration.millis(elapsed.active).pipe(Duration.toSeconds))),
            ...Array.cartesianWith(getHeapSpaceStatistics(), Record.toEntries(HEAP_SPACES), (space, [name, field]) => Metric.gauge(name, { attributes: { [ATTR_V8JS_HEAP_SPACE_NAME]: space.space_name } }).pipe(Metric.update(space[field]))),
            ...MutableRef.updateAndGet(resources, (seen) => seen.union(new Set(Record.keys(active))))
                .values()
                .map((type) => Metric.gauge(METRIC_V8JS_RESOURCE_ACTIVE, { attributes: { [ATTR_V8JS_RESOURCE_TYPE]: type } }).pipe(Metric.update(active[type] ?? 0))),
            Metric.gauge(METRIC_PROCESS_MEMORY_USAGE).pipe(Metric.update(process.memoryUsage.rss())),
        ];
        delay.reset();
        return Effect.all(updates, { discard: true });
    });
    const duration = Metric.histogram(METRIC_V8JS_GC_DURATION, { boundaries: [0.01, 0.1, 1, 10] });
    yield* Effect.sleep(interval).pipe(Effect.andThen(sample), Effect.forever, Effect.forkScoped);
    yield* collections.pipe(
        Stream.runForEach((entry) =>
            Schema.decodeUnknownOption(GcEntry)(entry.toJSON()).pipe(
                Option.flatMap(({ detail }) => Option.fromUndefinedOr(GC_TYPES.get(detail.kind))),
                Option.match({ onNone: () => Effect.void, onSome: (type) => duration.pipe(Metric.withAttributes({ [ATTR_V8JS_GC_TYPE]: type }), Metric.update(Duration.millis(entry.duration).pipe(Duration.toSeconds))) }),
            ),
        ),
        Effect.forkScoped,
    );
}).pipe(Layer.effectDiscard);
const layerNode = ({ resource, level, filter, file }: { readonly resource: Parameters<typeof layerNodeSdk>[0]; readonly level: LogLevel.LogLevel; readonly filter: Parameters<typeof layerMinimumLogLevels>[1]; readonly file?: readonly [path: string, rotation: typeof Rotation.Type] }) => {
    const record = logRecord({ ...resource, attributes: { [ATTR_PROCESS_PID]: process.pid } });
    const json = Logger.map(record, Formatter.formatJson);
    const report = {
        compact: true,
        excludeEnv: true,
        reportOnFatalError: true,
        reportOnUncaughtException: true,
        ...UndefinedOr.match(file, { onUndefined: () => ({ filename: 'stderr' }), onDefined: ([path]) => ({ directory: dirname(path), filename: `${parse(path).name}.report.json` }) }),
    };
    const unified = toUnifiedLog(record, resource.serviceName).pipe(
        Effect.map((logger) => Layer.merge(Logger.layer([logger], { mergeWithExisting: true }), Layer.succeed(References.MinimumLogLevel, 'Trace'))),
        Effect.catch((error) => Effect.as(Effect.logWarning('Unified log unavailable', error), Layer.empty)),
        Layer.unwrap,
    );
    const reporting = Effect.acquireRelease(
        Effect.sync(() => {
            const before = Struct.pick(process.report, Struct.keys(report));
            Object.assign(process.report, report);
            return before;
        }),
        (before) => Effect.sync(() => Object.assign(process.report, before)),
    );
    return Layer.mergeAll(layerUncaughtExceptionMonitor, layerErrorReported, layerRuntimeMetrics, Layer.effectDiscard(reporting)).pipe(
        Layer.provideMerge(process.platform === 'darwin' ? unified : Layer.empty),
        Layer.provideMerge(layerMinimumLogLevels(level, filter)),
        Layer.provideMerge(layerNodeSdk(resource)),
        Layer.provideMerge([
            Logger.layer(file === undefined ? [Logger.withConsoleError(json)] : [toRotatingFile(json, ...file), ...(process.stderr.isTTY ? [stderrWarnings] : [])], { mergeWithExisting: true }).pipe(Layer.provide(layerWithoutDefaultLogger)),
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
        age: read('age', Config.schema(Rotation.fields.age)),
        backups: read('backups', Config.schema(Rotation.fields.backups)),
    });
    const path = file ?? join(envPaths(resource.serviceName, { suffix: '' }).log, `${resource.serviceName}.log`);
    return Ref.get(refused).pipe(
        Effect.flatMap(
            Effect.forEach(([setting, error]) => Effect.flatMap(Effect.option(Config.String(setting).pipe(Config.nested(group))), (value) => Effect.annotateLogs(Effect.logWarning('Setting value unrecognized, default stands', Cause.fail(error)), { variable: `${group}.${setting}`, ...Record.getSomes({ value }) })), {
                discard: true,
            }),
        ),
        Effect.andThen(Effect.addFinalizer((exit) => (interrupted(exit) ? Effect.logInfo('Process interrupted') : Effect.void))),
        Layer.effectDiscard,
        Layer.provideMerge(
            layerNode({
                resource,
                level: level ?? Option.getOrElse(stated.level, () => 'Info' as const),
                filter: Option.getOrElse(stated.filter, Record.empty<string, LogLevel.LogLevel>),
                ...(Option.getOrElse(stated.target, () => target) === 'stderr' ? {} : { file: [path, Rotation.make(Record.getSomes(Struct.pick(stated, ['size', 'age', 'backups'])))] as const }),
            }),
        ),
    );
}, Layer.unwrap);
const teardown: Runtime.Teardown = (exit, onExit) => (interrupted(exit) ? onExit(0) : Runtime.defaultTeardown(exit, onExit));

// --- [EXPORTS] -------------------------------------------------------------------------

export { layerErrorReported, layerFromEnvironment, layerNode, layerNodeFromConfig, layerNodeSdk, layerUncaughtExceptionMonitor, Rotation, TelemetryError, teardown, toRotatingFile };
