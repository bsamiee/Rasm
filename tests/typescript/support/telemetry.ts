// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, Data, Effect, type Exit, HashMap, Match, Metric, type MetricKey, type MetricPair, MetricState, MutableRef, Option, Tracer } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type MetricKind = 'counter' | 'frequency' | 'gauge' | 'histogram' | 'summary';

interface MetricSeries {
    readonly key: MetricKey.MetricKey.Untyped;
    readonly kind: MetricKind;
    readonly occurrence: Option.Option<string>;
}

type MetricDataPoint = readonly [series: MetricSeries, value: number];

interface MetricChange {
    readonly series: MetricSeries;
    readonly before: Option.Option<number>;
    readonly value: number;
}

interface Observation<A, E> {
    readonly exit: Exit.Exit<A, E>;
    readonly metricChanges: readonly MetricChange[];
    readonly spans: readonly Tracer.Span[];
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const snapshot: Effect.Effect<readonly MetricDataPoint[]> = Effect.map(
    Metric.snapshot,
    Array.flatMap(({ metricKey: key, metricState: state }: MetricPair.MetricPair.Untyped) => {
        const dataPoint = (kind: MetricKind, occurrence: Option.Option<string>, value: number): MetricDataPoint => [Data.struct({ key, kind, occurrence }), value];
        return Match.value(state).pipe(
            Match.when(MetricState.isCounterState, ({ count }) => [dataPoint('counter', Option.none(), Number(count))]),
            Match.when(MetricState.isGaugeState, ({ value }) => [dataPoint('gauge', Option.none(), Number(value))]),
            Match.when(MetricState.isFrequencyState, ({ occurrences }) => Array.map(Array.fromIterable(occurrences), ([occurrence, count]) => dataPoint('frequency', Option.some(occurrence), count))),
            Match.when(MetricState.isHistogramState, ({ count }) => [dataPoint('histogram', Option.none(), count)]),
            Match.when(MetricState.isSummaryState, ({ count }) => [dataPoint('summary', Option.none(), count)]),
            Match.orElseAbsurd,
        );
    }),
);

const capture = <A, E, R>(effect: Effect.Effect<A, E, R>): Effect.Effect<Observation<A, E>, never, R> =>
    Effect.gen(function* () {
        const tracer = yield* Effect.tracer;
        const spans = MutableRef.make<readonly Tracer.Span[]>([]);
        const before = HashMap.fromIterable(yield* snapshot);
        const exit = yield* Effect.exit(
            Effect.withTracer(
                effect,
                Tracer.make({
                    ...tracer,
                    span: (...args) => {
                        const span = tracer.span(...args);
                        MutableRef.update(spans, Array.append(span));
                        return span;
                    },
                }),
            ),
        );
        return {
            exit,
            metricChanges: Array.filterMap(yield* snapshot, ([series, value]) =>
                Option.liftPredicate({ series, before: HashMap.get(before, series), value }, (change) => !Option.contains(change.before, change.value)),
            ),
            spans: MutableRef.get(spans),
        };
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { capture, type MetricChange, type MetricDataPoint, type MetricKind, type MetricSeries, type Observation, snapshot };
