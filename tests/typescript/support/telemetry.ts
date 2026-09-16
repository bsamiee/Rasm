// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, Effect, type Exit, HashMap, Match, Metric, MutableRef, Option, Tracer } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type MetricDataPoint = readonly [
    series: { readonly id: string; readonly attributes: Metric.Metric.AttributeSet | undefined; readonly kind: Metric.Metric.Type; readonly occurrence: Option.Option<string> },
    value: number,
];

interface MetricChange {
    readonly series: MetricDataPoint[0];
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
    Array.flatMap((entry: Metric.Metric.Snapshot) => {
        const dataPoint = (occurrence: Option.Option<string>, value: number): MetricDataPoint => [{ id: entry.id, attributes: entry.attributes, kind: entry.type, occurrence }, value];
        return Match.value(entry).pipe(
            Match.discriminatorsExhaustive('type')({
                ['Counter']: ({ state }) => [dataPoint(Option.none(), Number(state.count))],
                ['Gauge']: ({ state }) => [dataPoint(Option.none(), Number(state.value))],
                ['Frequency']: ({ state }) => Array.map(Array.fromIterable(state.occurrences), ([occurrence, count]) => dataPoint(Option.some(occurrence), count)),
                ['Histogram']: ({ state }) => [dataPoint(Option.none(), state.count)],
                ['Summary']: ({ state }) => [dataPoint(Option.none(), state.count)],
            }),
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
                    span: (options) => {
                        const span = tracer.span(options);
                        MutableRef.update(spans, Array.append(span));
                        return span;
                    },
                }),
            ),
        );
        return {
            exit,
            metricChanges: Array.filter(
                Array.map(yield* snapshot, ([series, value]) => ({ series, before: HashMap.get(before, series), value })),
                (change) => !Option.contains(change.before, change.value),
            ),
            spans: MutableRef.get(spans),
        };
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { capture, type MetricChange, type MetricDataPoint, type Observation, snapshot };
