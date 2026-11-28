import { Buffer } from 'node:buffer';
import { subscribe, unsubscribe } from 'node:diagnostics_channel';
import { closeSync, constants, fstatSync, mkdirSync, openSync, renameSync, rmSync, type Stats, statSync, writeSync } from 'node:fs';
import { availableParallelism, type CpuInfo, cpus, freemem, totalmem } from 'node:os';
import { dirname, join, parse } from 'node:path';
import { type EventLoopUtilization, type Histogram, monitorEventLoopDelay, PerformanceObserver, constants as performanceConstants, performance } from 'node:perf_hooks';
import process, { type ProcessEventMap } from 'node:process';
import { getHeapSpaceStatistics, type HeapSpaceInfo } from 'node:v8';
import { layerGlobal, makeExternalSpan } from '@effect/opentelemetry/OtelTracer';
import { configToAttributes, layerEmpty } from '@effect/opentelemetry/Resource';
import { type DiagLogger, DiagLogLevel, diag, logs, ROOT_CONTEXT, trace } from '@opentelemetry/api';
import { diagLogLevelFromString, getStringFromEnv, W3CTraceContextPropagator } from '@opentelemetry/core';
import { EnvironmentGetter } from '@opentelemetry/propagator-env-carrier';
import {
    ATTR_CPU_LOGICAL_NUMBER,
    ATTR_CPU_MODE,
    ATTR_NETWORK_INTERFACE_NAME,
    ATTR_NETWORK_IO_DIRECTION,
    ATTR_NODEJS_EVENTLOOP_STATE,
    ATTR_PROCESS_CONTEXT_SWITCH_TYPE,
    ATTR_PROCESS_PID,
    ATTR_SYSTEM_MEMORY_STATE,
    ATTR_SYSTEM_PAGING_DIRECTION,
    ATTR_SYSTEM_PAGING_FAULT_TYPE,
    ATTR_V8JS_GC_TYPE,
    ATTR_V8JS_HEAP_SPACE_NAME,
    ATTR_V8JS_RESOURCE_TYPE,
    CPU_MODE_VALUE_IDLE,
    CPU_MODE_VALUE_INTERRUPT,
    CPU_MODE_VALUE_NICE,
    CPU_MODE_VALUE_SYSTEM,
    CPU_MODE_VALUE_USER,
    METRIC_NODEJS_EVENTLOOP_DELAY_MAX,
    METRIC_NODEJS_EVENTLOOP_DELAY_MEAN,
    METRIC_NODEJS_EVENTLOOP_DELAY_MIN,
    METRIC_NODEJS_EVENTLOOP_DELAY_P50,
    METRIC_NODEJS_EVENTLOOP_DELAY_P90,
    METRIC_NODEJS_EVENTLOOP_DELAY_P99,
    METRIC_NODEJS_EVENTLOOP_DELAY_STDDEV,
    METRIC_NODEJS_EVENTLOOP_TIME,
    METRIC_NODEJS_EVENTLOOP_UTILIZATION,
    METRIC_PROCESS_CONTEXT_SWITCHES,
    METRIC_PROCESS_CPU_TIME,
    METRIC_PROCESS_CPU_UTILIZATION,
    METRIC_PROCESS_MEMORY_USAGE,
    METRIC_PROCESS_MEMORY_VIRTUAL,
    METRIC_PROCESS_PAGING_FAULTS,
    METRIC_PROCESS_THREAD_COUNT,
    METRIC_PROCESS_UNIX_FILE_DESCRIPTOR_COUNT,
    METRIC_SYSTEM_CPU_TIME,
    METRIC_SYSTEM_CPU_UTILIZATION,
    METRIC_SYSTEM_MEMORY_LIMIT,
    METRIC_SYSTEM_MEMORY_USAGE,
    METRIC_SYSTEM_MEMORY_UTILIZATION,
    METRIC_SYSTEM_NETWORK_ERRORS,
    METRIC_SYSTEM_NETWORK_IO,
    METRIC_SYSTEM_NETWORK_PACKET_COUNT,
    METRIC_SYSTEM_NETWORK_PACKET_DROPPED,
    METRIC_SYSTEM_PAGING_FAULTS,
    METRIC_SYSTEM_PAGING_OPERATIONS,
    METRIC_V8JS_GC_DURATION,
    METRIC_V8JS_MEMORY_HEAP_SPACE_AVAILABLE_SIZE,
    METRIC_V8JS_MEMORY_HEAP_SPACE_PHYSICAL_SIZE,
    METRIC_V8JS_MEMORY_HEAP_SPACE_SIZE,
    METRIC_V8JS_MEMORY_HEAP_USED,
    METRIC_V8JS_RESOURCE_ACTIVE,
    NETWORK_IO_DIRECTION_VALUE_RECEIVE,
    NETWORK_IO_DIRECTION_VALUE_TRANSMIT,
    NODEJS_EVENTLOOP_STATE_VALUE_ACTIVE,
    NODEJS_EVENTLOOP_STATE_VALUE_IDLE,
    PROCESS_CONTEXT_SWITCH_TYPE_VALUE_INVOLUNTARY,
    PROCESS_CONTEXT_SWITCH_TYPE_VALUE_VOLUNTARY,
    SYSTEM_MEMORY_STATE_VALUE_AVAILABLE,
    SYSTEM_MEMORY_STATE_VALUE_USED,
    SYSTEM_PAGING_DIRECTION_VALUE_IN,
    SYSTEM_PAGING_DIRECTION_VALUE_OUT,
    SYSTEM_PAGING_FAULT_TYPE_VALUE_MAJOR,
    SYSTEM_PAGING_FAULT_TYPE_VALUE_MINOR,
    V8JS_GC_TYPE_VALUE_INCREMENTAL,
    V8JS_GC_TYPE_VALUE_MAJOR,
    V8JS_GC_TYPE_VALUE_MINOR,
    V8JS_GC_TYPE_VALUE_WEAKCB,
} from '@opentelemetry/semantic-conventions/incubating';
import { Darwin } from '@rasm/darwin';
import { Array, Cause, Config, Duration, Effect, ErrorReporter, Exit, FileSystem, Formatter, Function, flow, Layer, Logger, LogLevel, Match, Metric, MutableRef, Option, Order, Predicate, Queue, Record, Ref, References, Result, Runtime, Schema, SchemaGetter, Stream, String, Struct, UndefinedOr } from 'effect';
import envPaths from 'env-paths';
import { errorReporter, exporters, layerMinimumLogLevels, layerWithoutDefaultLogger, logRecord } from '#/otlp';

// --- [TABLES] --------------------------------------------------------------------------

const DIAGNOSTICS = { error: 'Error', warn: 'Warn', info: 'Info', debug: 'Debug', verbose: 'Trace' } as const satisfies Record<keyof DiagLogger, LogLevel.LogLevel>;
const LOG_TYPES = { Trace: 'debug', Debug: 'debug', Info: 'info', Warn: 'error', Error: 'error', Fatal: 'fault' } as const satisfies Record<LogLevel.Severity, string>;
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
const DURATIONS = Metric.exponentialBoundaries({ start: 1e-4, factor: 10, count: 7 });
const CPU_MODES = { user: CPU_MODE_VALUE_USER, nice: CPU_MODE_VALUE_NICE, sys: CPU_MODE_VALUE_SYSTEM, idle: CPU_MODE_VALUE_IDLE, irq: CPU_MODE_VALUE_INTERRUPT } as const satisfies Record<keyof CpuInfo['times'], string>;
const PROCESS_CPU_MODES = { user: CPU_MODE_VALUE_USER, system: CPU_MODE_VALUE_SYSTEM } as const satisfies Record<keyof NodeJS.CpuUsage, string>;
const DIRECTIONS = { receive: NETWORK_IO_DIRECTION_VALUE_RECEIVE, transmit: NETWORK_IO_DIRECTION_VALUE_TRANSMIT } as const;
const TRAFFIC = { bytes: METRIC_SYSTEM_NETWORK_IO, packets: METRIC_SYSTEM_NETWORK_PACKET_COUNT, errors: METRIC_SYSTEM_NETWORK_ERRORS, dropped: METRIC_SYSTEM_NETWORK_PACKET_DROPPED } as const;
const PAGING = {
    pageins: { [ATTR_SYSTEM_PAGING_DIRECTION]: SYSTEM_PAGING_DIRECTION_VALUE_IN, [ATTR_SYSTEM_PAGING_FAULT_TYPE]: SYSTEM_PAGING_FAULT_TYPE_VALUE_MINOR },
    pageouts: { [ATTR_SYSTEM_PAGING_DIRECTION]: SYSTEM_PAGING_DIRECTION_VALUE_OUT, [ATTR_SYSTEM_PAGING_FAULT_TYPE]: SYSTEM_PAGING_FAULT_TYPE_VALUE_MINOR },
    swapins: { [ATTR_SYSTEM_PAGING_DIRECTION]: SYSTEM_PAGING_DIRECTION_VALUE_IN, [ATTR_SYSTEM_PAGING_FAULT_TYPE]: SYSTEM_PAGING_FAULT_TYPE_VALUE_MAJOR },
    swapouts: { [ATTR_SYSTEM_PAGING_DIRECTION]: SYSTEM_PAGING_DIRECTION_VALUE_OUT, [ATTR_SYSTEM_PAGING_FAULT_TYPE]: SYSTEM_PAGING_FAULT_TYPE_VALUE_MAJOR },
} as const;
const FAULTS = { minorPageFault: SYSTEM_PAGING_FAULT_TYPE_VALUE_MINOR, majorPageFault: SYSTEM_PAGING_FAULT_TYPE_VALUE_MAJOR } as const satisfies Partial<Record<keyof NodeJS.ResourceUsage, string>>;
const SWITCHES = { voluntaryContextSwitches: PROCESS_CONTEXT_SWITCH_TYPE_VALUE_VOLUNTARY, involuntaryContextSwitches: PROCESS_CONTEXT_SWITCH_TYPE_VALUE_INVOLUNTARY } as const satisfies Partial<Record<keyof NodeJS.ResourceUsage, string>>;
const THREADPOOL = {
    attribute: 'nodejs.threadpool.work.type',
    channels: new Map<string | symbol, string>(Array.map(['crypto', 'fs.cp', 'fs.readfile', 'fs.writefile', 'glob', 'histogram.qrde', 'node_api', 'readdir_recursive', 'node_sqlite3.BackupJob', 'zlib'], (type) => [`uv.threadpool.work.${type}`, type])),
} as const;
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
const Work = Schema.Struct({ enqueued: Schema.Finite, started: Schema.Finite, ended: Schema.Finite });

// --- [ERRORS] --------------------------------------------------------------------------

class TelemetryError extends Schema.TaggedError<TelemetryError, { readonly brand: unique symbol }>()('TelemetryError', {
    reason: Schema.TaggedUnion({ Unavailable: {} }),
    subject: Schema.String,
    cause: Schema.optionalKey(Schema.Defect()),
    message: Schema.String,
}) {
    static readonly of = (reason: TelemetryError['reason'], subject: string) => (cause?: unknown) => TelemetryError.make({ reason, subject, ...(cause === undefined ? {} : { cause }), message: [subject, String.toLowerCase(reason._tag), ...Record.values(Struct.omit(reason, ['_tag']))].join(' ') });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const interrupted = <A, E>(exit: Exit.Exit<A, E>) => Exit.isFailure(exit) && Cause.hasInterruptsOnly(exit.cause);
const propagator = new W3CTraceContextPropagator();
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
        {
            using held = new DisposableStack();
            const locked = held.adopt(open(lock), ({ fd }) => closeSync(fd));
            const shifted = due(locked.stat, bytes, expiry) && statSync(file, { throwIfNoEntry: false })?.ino === locked.stat.ino ? Array.makeBy(backups + 1, (step) => backups - step) : [];
            const [kept, expired] = Array.partition(shifted, (index) => (index < backups && (statSync(backup(index), { throwIfNoEntry: false })?.mtimeMs ?? expiry) > expiry ? Result.succeed(index) : Result.fail(index)));
            expired.forEach((index) => rmSync(backup(index), { force: true }));
            kept.forEach((index) => renameSync(backup(index), backup(index + 1)));
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
const toUnifiedLog = Effect.fnUntraced(function* (self: ReturnType<typeof logRecord>, subsystem: string, darwin: Darwin['Service']) {
    const write = yield* darwin.unifiedLog(subsystem);
    const text = ({ body, attributes }: ReturnType<typeof self.log>) =>
        [
            body,
            ...Array.sortWith(
                Object.entries(attributes).filter(([, value]) => value !== undefined),
                ([key]) => key,
                Order.String,
            ).map(([key, value]) => `${key}=${Predicate.isString(value) ? value : Formatter.format(value)}`),
        ].join(' ');
    return Logger.make((entry: Logger.Options<unknown>) => {
        if (entry.logLevel === 'All' || entry.logLevel === 'None') {
            return;
        }
        const record = self.log(entry);
        write(record.scope ?? subsystem, LOG_TYPES[entry.logLevel], () => text(record));
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
const works = Stream.callback<readonly [name: string | symbol, message: unknown]>((queue) => {
    const offer = (message: unknown, name: string | symbol) => {
        Queue.offerUnsafe(queue, [name, message]);
    };
    return Effect.acquireRelease(
        Effect.sync(() => THREADPOOL.channels.keys().forEach((name) => subscribe(name, offer))),
        Function.constant(
            Effect.sync(() =>
                THREADPOOL.channels.keys().forEach((name) => {
                    unsubscribe(name, offer);
                }),
            ),
        ),
    );
});
const devices = (table: string): Effect.Success<Darwin['Service']['interfaces']> =>
    table.split('\n').flatMap((line) => {
        const [name = '', counters = ''] = line.split(':');
        const columns = counters.trim().split(/\s+/u).map(Number);
        return Predicate.isTupleOf(columns, 16) ? [{ name: name.trim(), traffic: { receive: { bytes: columns[0], packets: columns[1], errors: columns[2], dropped: columns[3] }, transmit: { bytes: columns[8], packets: columns[9], errors: columns[10], dropped: columns[11] } } }] : [];
    });
const sampling = (darwin: Option.Option<Darwin['Service']>, files: FileSystem.FileSystem) => {
    const interfaces = Option.match(darwin, {
        onNone: () =>
            files.readFileString('/proc/net/dev').pipe(
                Effect.map(devices),
                Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed(Array.empty<ReturnType<typeof devices>[number]>())),
            ),
        onSome: (image) => image.interfaces,
    });
    return Effect.all({
        interfaces,
        memory: darwin.pipe(
            Option.map((image) => image.memory),
            Effect.transposeOption,
        ),
        task: darwin.pipe(
            Option.map((image) => image.task),
            Effect.transposeOption,
        ),
        runtime: Effect.sync(() => ({ processors: cpus(), cpu: process.cpuUsage(), thread: process.threadCpuUsage(), usage: process.resourceUsage(), limit: totalmem(), free: freemem(), parallelism: availableParallelism() })),
    });
};
const counter =
    (totals: Map<string, number>) =>
    <const A extends Readonly<Record<string, string>>>(name: string, attributes: A, total: number) => {
        const key = [name, ...Record.values(attributes)].join(' ');
        const before = totals.get(key) ?? 0;
        const spent = total >= before ? total - before : total;
        totals.set(key, total);
        return { attributes, spent, update: Metric.counter(name, { attributes, incremental: true }).pipe(Metric.update(spent)) };
    };
const hostMetrics = (counted: ReturnType<typeof counter>, wall: number, { interfaces, memory, task, runtime: { processors, cpu, thread, usage, limit, free, parallelism } }: Effect.Success<ReturnType<typeof sampling>>) => {
    const used = Option.match(memory, { onNone: () => limit - free, onSome: (statistics) => statistics.used });
    return [
        ...processors.flatMap(({ times }, index) => {
            const modes = Record.collect(CPU_MODES, (field, mode) => counted(METRIC_SYSTEM_CPU_TIME, { [ATTR_CPU_MODE]: mode, [ATTR_CPU_LOGICAL_NUMBER]: `${index}` }, Duration.millis(times[field]).pipe(Duration.toSeconds)));
            const busy = Math.sumPrecise(modes.map(({ spent }) => spent));
            return modes.flatMap(({ attributes, spent, update }) => [update, Metric.gauge(METRIC_SYSTEM_CPU_UTILIZATION, { attributes }).pipe(Metric.update(busy === 0 ? 0 : spent / busy))]);
        }),
        ...Record.collect(PROCESS_CPU_MODES, (field, mode) => {
            const { attributes, spent, update } = counted(METRIC_PROCESS_CPU_TIME, { [ATTR_CPU_MODE]: mode }, Duration.micros(BigInt(cpu[field])).pipe(Duration.toSeconds));
            return [update, Metric.gauge(METRIC_PROCESS_CPU_UTILIZATION, { attributes }).pipe(Metric.update(spent / wall / parallelism)), counted('nodejs.eventloop.cpu.time', attributes, Duration.micros(BigInt(thread[field])).pipe(Duration.toSeconds)).update];
        }).flat(),
        Metric.gauge(METRIC_SYSTEM_MEMORY_LIMIT).pipe(Metric.update(limit)),
        ...Record.collect({ [SYSTEM_MEMORY_STATE_VALUE_USED]: used, [SYSTEM_MEMORY_STATE_VALUE_AVAILABLE]: limit - used }, (state, bytes) => [
            Metric.gauge(METRIC_SYSTEM_MEMORY_USAGE, { attributes: { [ATTR_SYSTEM_MEMORY_STATE]: state } }).pipe(Metric.update(bytes)),
            Metric.gauge(METRIC_SYSTEM_MEMORY_UTILIZATION, { attributes: { [ATTR_SYSTEM_MEMORY_STATE]: state } }).pipe(Metric.update(bytes / limit)),
        ]).flat(),
        ...Option.toArray(memory).flatMap((statistics) => [counted(METRIC_SYSTEM_PAGING_FAULTS, {}, statistics.faults).update, ...Record.collect(PAGING, (field, attributes) => counted(METRIC_SYSTEM_PAGING_OPERATIONS, attributes, statistics[field]).update)]),
        ...interfaces.flatMap(({ name, traffic }) => Record.collect(traffic, (direction, counters) => Record.collect(counters, (field, total) => counted(TRAFFIC[field], { [ATTR_NETWORK_INTERFACE_NAME]: name, [ATTR_NETWORK_IO_DIRECTION]: DIRECTIONS[direction] }, total).update)).flat()),
        ...Record.collect(FAULTS, (field, type) => counted(METRIC_PROCESS_PAGING_FAULTS, { [ATTR_SYSTEM_PAGING_FAULT_TYPE]: type }, usage[field]).update),
        ...Option.match(task, {
            onNone: () => Record.collect(SWITCHES, (field, type) => counted(METRIC_PROCESS_CONTEXT_SWITCHES, { [ATTR_PROCESS_CONTEXT_SWITCH_TYPE]: type }, usage[field]).update),
            onSome: ({ threads, virtual, descriptors, switches }) => [
                Metric.gauge(METRIC_PROCESS_THREAD_COUNT).pipe(Metric.update(threads)),
                Metric.gauge(METRIC_PROCESS_MEMORY_VIRTUAL).pipe(Metric.update(virtual)),
                Metric.gauge(METRIC_PROCESS_UNIX_FILE_DESCRIPTOR_COUNT).pipe(Metric.update(descriptors)),
                counted(METRIC_PROCESS_CONTEXT_SWITCHES, {}, switches).update,
            ],
        }),
    ];
};
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
    const diagnostics = Record.map(DIAGNOSTICS, (severity) => flow(Effect.logWithLevel(severity), Effect.runSyncWith(local)<void, never>));
    const level = diagLogLevelFromString(getStringFromEnv('OTEL_LOG_LEVEL')) ?? DiagLogLevel.WARN;
    const metrics = yield* Match.value(named.metrics).pipe(
        Match.when(Array.isReadonlyArrayEmpty, () => Effect.succeed({ metricReaders: [] })),
        Match.when(Array.contains('otlp'), () => Effect.map(metricReader, (reader) => ({ metricReaders: [reader] }))),
        Match.orElse(() => Effect.succeed({})),
    );
    const options = {
        ...UndefinedOr.match(DiagLogLevel[Math.min(level, DiagLogLevel.WARN)], { onUndefined: () => ({}), onDefined: (logLevel) => ({ logLevel }) }),
        resourceAttributes: configToAttributes(resource),
        ...metrics,
        ...(Array.isReadonlyArrayEmpty(named.traces) ? { spanProcessors: [] } : {}),
        ...(Array.isReadonlyArrayEmpty(named.logs) ? { logRecordProcessors: [] } : {}),
    };
    return yield* Effect.acquireRelease(Effect.sync(() => startNodeSdk(options)).pipe(Effect.tap(() => Effect.sync(() => diag.setLogger(diagnostics, { logLevel: level, suppressOverrideMessage: true })))), (sdk) =>
        Effect.tryPromise({ try: () => sdk.shutdown(), catch: TelemetryError.of({ _tag: 'Unavailable' }, 'OpenTelemetry SDK shutdown') }).pipe(Effect.ignore({ log: 'Warn' }), Effect.andThen(Effect.sync(() => diag.disable()))),
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
    const [reading, delay] = yield* Effect.all([
        Effect.map(Effect.all([Effect.serviceOption(Darwin), FileSystem.FileSystem]), ([darwin, files]) => sampling(darwin, files)),
        Effect.sync(monitorEventLoopDelay).pipe(
            Effect.tap((histogram) => Effect.sync(() => histogram.enable())),
            Effect.acquireDisposable,
        ),
    ]);
    const [loop, resources, counted] = [MutableRef.make<EventLoopUtilization>({ idle: 0, active: 0, utilization: 0 }), MutableRef.make<ReadonlySet<string>>(new Set()), counter(new Map())];
    const sample = Effect.gen(function* () {
        const host = yield* reading;
        const now = performance.eventLoopUtilization();
        const elapsed = performance.eventLoopUtilization(now, MutableRef.getAndSet(loop, now));
        const active = Record.map(
            Array.groupBy(process.getActiveResourcesInfo(), (type) => type),
            (types) => types.length,
        );
        const updates = [
            ...Record.collect(DELAYS, (name, read) => Metric.gauge(name).pipe(Metric.update(Duration.millis(read(delay) / 1e6).pipe(Duration.toSeconds)))),
            Metric.gauge(METRIC_NODEJS_EVENTLOOP_UTILIZATION).pipe(Metric.update(elapsed.utilization)),
            Metric.counter(METRIC_NODEJS_EVENTLOOP_TIME, { attributes: { [ATTR_NODEJS_EVENTLOOP_STATE]: NODEJS_EVENTLOOP_STATE_VALUE_IDLE }, incremental: true }).pipe(Metric.update(Duration.millis(elapsed.idle).pipe(Duration.toSeconds))),
            Metric.counter(METRIC_NODEJS_EVENTLOOP_TIME, { attributes: { [ATTR_NODEJS_EVENTLOOP_STATE]: NODEJS_EVENTLOOP_STATE_VALUE_ACTIVE }, incremental: true }).pipe(Metric.update(Duration.millis(elapsed.active).pipe(Duration.toSeconds))),
            ...Array.cartesianWith(getHeapSpaceStatistics(), Record.toEntries(HEAP_SPACES), (space, [name, field]) => Metric.gauge(name, { attributes: { [ATTR_V8JS_HEAP_SPACE_NAME]: space.space_name } }).pipe(Metric.update(space[field]))),
            ...MutableRef.updateAndGet(resources, (seen) => seen.union(new Set(Record.keys(active))))
                .values()
                .map((type) => Metric.gauge(METRIC_V8JS_RESOURCE_ACTIVE, { attributes: { [ATTR_V8JS_RESOURCE_TYPE]: type } }).pipe(Metric.update(active[type] ?? 0))),
            Metric.gauge(METRIC_PROCESS_MEMORY_USAGE).pipe(Metric.update(process.memoryUsage.rss())),
            ...hostMetrics(counted, Duration.millis(elapsed.idle + elapsed.active).pipe(Duration.toSeconds), host),
        ];
        delay.reset();
        return yield* Effect.all(updates, { discard: true });
    });
    const [collected, queued, ran] = [Metric.histogram(METRIC_V8JS_GC_DURATION, { boundaries: DURATIONS }), Metric.histogram('nodejs.threadpool.work.queue.duration', { boundaries: DURATIONS }), Metric.histogram('nodejs.threadpool.work.run.duration', { boundaries: DURATIONS })];
    yield* Effect.sleep(interval).pipe(
        Effect.andThen(sample),
        Effect.catch((error) => Effect.logWarning('Runtime metrics sample failed', error)),
        Effect.forever,
        Effect.forkScoped,
    );
    yield* collections.pipe(
        Stream.runForEach((entry) =>
            Schema.decodeUnknownOption(GcEntry)(entry.toJSON()).pipe(
                Option.flatMap(({ detail }) => Option.fromUndefinedOr(GC_TYPES.get(detail.kind))),
                Option.match({ onNone: () => Effect.void, onSome: (type) => collected.pipe(Metric.withAttributes({ [ATTR_V8JS_GC_TYPE]: type }), Metric.update(Duration.millis(entry.duration).pipe(Duration.toSeconds))) }),
            ),
        ),
        Effect.forkScoped,
    );
    yield* works.pipe(
        Stream.runForEach(([name, message]) =>
            Option.all({ type: Option.fromUndefinedOr(THREADPOOL.channels.get(name)), work: Schema.decodeUnknownOption(Work)(message) }).pipe(
                Option.match({
                    onNone: () => Effect.void,
                    onSome: ({ type, work }) =>
                        Effect.all(
                            [
                                queued.pipe(Metric.withAttributes({ [THREADPOOL.attribute]: type }), Metric.update(Duration.millis(work.started - work.enqueued).pipe(Duration.toSeconds))),
                                ran.pipe(Metric.withAttributes({ [THREADPOOL.attribute]: type }), Metric.update(Duration.millis(work.ended - work.started).pipe(Duration.toSeconds))),
                            ],
                            { discard: true },
                        ),
                }),
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
    const unified = Effect.serviceOption(Darwin).pipe(
        Effect.flatMap(
            Option.match({
                onNone: () => Effect.succeed(Layer.empty),
                onSome: (darwin) =>
                    toUnifiedLog(record, resource.serviceName, darwin).pipe(
                        Effect.map((logger) => Layer.merge(Logger.layer([logger], { mergeWithExisting: true }), Layer.succeed(References.MinimumLogLevel, 'Trace'))),
                        Effect.catch((error) => Effect.as(Effect.logWarning('Unified log unavailable', error), Layer.empty)),
                    ),
            }),
        ),
        Layer.unwrap,
        Layer.provideMerge(Darwin.layer.pipe(Layer.catchTag('DarwinError', (error) => Layer.effectDiscard(Effect.logWarning('Darwin interfaces unavailable', error))))),
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
