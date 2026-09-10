import {
    Array,
    type Context,
    Effect,
    Exit,
    HashMap,
    Match,
    Metric,
    type MetricPair,
    MetricState,
    MutableRef,
    Option,
    Order,
    pipe,
    Record,
    Tracer,
} from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type MetricKind = 'counter' | 'frequency' | 'gauge' | 'histogram' | 'summary';
type SpanOutcome = 'failure' | 'open' | 'success';
type SpanArguments = Parameters<Tracer.Tracer['span']>;

interface MetricReading {
    readonly name: string;
    readonly kind: MetricKind;
    readonly tags: readonly (readonly [key: string, value: string])[];
    readonly value: number;
}

interface MetricChange extends MetricReading {
    readonly before: number;
}

interface SpanEvent {
    readonly name: string;
    readonly attributes: Readonly<Record<string, unknown>>;
}

interface SpanRecord {
    readonly name: string;
    readonly kind: Tracer.SpanKind;
    readonly parent: Option.Option<string>;
    readonly attributes: Readonly<Record<string, unknown>>;
    readonly events: readonly SpanEvent[];
    readonly outcome: SpanOutcome;
}

interface TelemetryObservation<A, E> {
    readonly exit: Exit.Exit<A, E>;
    readonly metricChanges: readonly MetricChange[];
    readonly spans: readonly SpanRecord[];
}

interface Telemetry {
    readonly snapshot: Effect.Effect<readonly MetricReading[]>;
    readonly capture: <A, E, R>(work: Effect.Effect<A, E, R>) => Effect.Effect<TelemetryObservation<A, E>, never, R>;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const _metricKey = (reading: Pick<MetricReading, 'kind' | 'name' | 'tags'>): string =>
    pipe(
        Array.sort(
            Array.map(reading.tags, ([key, value]) => `${key}=${value}`),
            Order.string,
        ),
        (sorted) => [reading.name, reading.kind, ...sorted].join(' '),
    );

const _readings = (pair: MetricPair.MetricPair.Untyped): readonly MetricReading[] => {
    const tags = Array.map(pair.metricKey.tags, (label) => [label.key, label.value] as const);
    const reading = (kind: MetricKind, value: number): MetricReading => ({ name: pair.metricKey.name, kind, tags, value });
    return Match.value(pair.metricState).pipe(
        Match.when(MetricState.isCounterState, (state) => [reading('counter', Number(state.count))]),
        Match.when(MetricState.isGaugeState, (state) => [reading('gauge', Number(state.value))]),
        Match.when(MetricState.isFrequencyState, (state) =>
            Array.map(Array.fromIterable(state.occurrences), ([word, count]) => ({
                ...reading('frequency', count),
                tags: Array.append(tags, ['occurrence', word] as const),
            })),
        ),
        Match.when(MetricState.isHistogramState, (state) => [reading('histogram', state.count)]),
        Match.when(MetricState.isSummaryState, (state) => [reading('summary', state.count)]),
        Match.orElse(() => []),
    );
};

const _keyedReadings = (pairs: readonly MetricPair.MetricPair.Untyped[]): readonly (readonly [string, MetricReading])[] =>
    Array.map(Array.flatMap(pairs, _readings), (reading) => [_metricKey(reading), reading] as const);

const _metricChanges = (
    before: readonly (readonly [string, MetricReading])[],
    after: readonly (readonly [string, MetricReading])[],
): readonly MetricChange[] => {
    const prior = HashMap.fromIterable(before);
    return Array.filterMap(after, ([key, reading]) =>
        HashMap.get(prior, key).pipe(
            Option.map((held) => held.value),
            Option.getOrElse(() => 0),
            (base) => Option.liftPredicate({ ...reading, before: base }, () => base !== reading.value),
        ),
    );
};

class _CapturedSpan implements Tracer.Span {
    readonly _tag = 'Span' as const;
    readonly attributes: Map<string, unknown> = new Map();
    readonly context: Context.Context<never>;
    readonly kind: Tracer.SpanKind;
    readonly name: string;
    readonly parent: Option.Option<Tracer.AnySpan>;
    readonly sampled = true;
    readonly spanId: string;
    readonly traceId: string;
    private _events: readonly SpanEvent[] = [];
    private _links: Tracer.SpanLink[];
    private _status: Tracer.SpanStatus;

    constructor(ordinal: number, ...[name, parent, context, links, startTime, kind]: SpanArguments) {
        this.name = name;
        this.parent = parent;
        this.context = context;
        this.kind = kind;
        this.spanId = `captured-${ordinal}`;
        this.traceId = Option.match(parent, { onNone: () => `trace-${ordinal}`, onSome: (span) => span.traceId });
        this._links = [...links];
        this._status = { _tag: 'Started', startTime };
    }

    get events(): readonly SpanEvent[] {
        return this._events;
    }

    get links(): readonly Tracer.SpanLink[] {
        return this._links;
    }

    get status(): Tracer.SpanStatus {
        return this._status;
    }

    addLinks(links: readonly Tracer.SpanLink[]): void {
        this._links = [...this._links, ...links];
    }

    attribute(key: string, value: unknown): void {
        this.attributes.set(key, value);
    }

    end(endTime: bigint, exit: Exit.Exit<unknown, unknown>): void {
        this._status = { _tag: 'Ended', startTime: this._status.startTime, endTime, exit };
    }

    event(name: string, _startTime: bigint, attributes?: Record<string, unknown>): void {
        this._events = [...this._events, { name, attributes: attributes ?? {} }];
    }
}

const _spanRecord = (span: _CapturedSpan): SpanRecord => ({
    name: span.name,
    kind: span.kind,
    parent: Option.map(span.parent, (held) =>
        Match.value(held).pipe(
            Match.tag('Span', (inner) => inner.name),
            Match.tag('ExternalSpan', (external) => external.spanId),
            Match.exhaustive,
        ),
    ),
    attributes: Record.fromEntries(span.attributes),
    events: span.events,
    outcome: Match.value(span.status).pipe(
        Match.tag('Started', (): SpanOutcome => 'open'),
        Match.tag('Ended', ({ exit }) => Exit.match(exit, { onSuccess: (): SpanOutcome => 'success', onFailure: (): SpanOutcome => 'failure' })),
        Match.exhaustive,
    ),
});

const Telemetry: Telemetry = {
    snapshot: Effect.map(Metric.snapshot, (pairs) => Array.flatMap(pairs, _readings)),
    capture: (work) =>
        Effect.gen(function* () {
            const capturedSpans = MutableRef.make<readonly _CapturedSpan[]>([]);
            const tracer = Tracer.make({
                context: (f) => f(),
                span: (...args: SpanArguments) => {
                    const span = new _CapturedSpan(MutableRef.get(capturedSpans).length + 1, ...args);
                    MutableRef.update(capturedSpans, Array.append(span));
                    return span;
                },
            });
            const before = _keyedReadings(yield* Metric.snapshot);
            const exit = yield* Effect.exit(Effect.withTracer(work, tracer));
            const after = _keyedReadings(yield* Metric.snapshot);
            return { exit, metricChanges: _metricChanges(before, after), spans: Array.map(MutableRef.get(capturedSpans), _spanRecord) };
        }),
};

// --- [EXPORTS] -------------------------------------------------------------------------

export {
    type MetricChange,
    type MetricKind,
    type MetricReading,
    type SpanEvent,
    type SpanOutcome,
    type SpanRecord,
    Telemetry,
    type TelemetryObservation,
};
