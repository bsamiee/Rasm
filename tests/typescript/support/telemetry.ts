import { Effect, Metric, Tracer } from 'effect';

// --- [OPERATIONS] ----------------------------------------------------------------------

const observe = Effect.fnUntraced(
    function* <A, E, R>(effect: Effect.Effect<A, E, R>) {
        const tracer = yield* Effect.tracer;
        const spans: Tracer.Span[] = [];
        const recording = Tracer.make({
            ...tracer,
            span: (options) => {
                const span = tracer.span(options);
                spans.push(span);
                return span;
            },
        });
        const exit = yield* Effect.exit(Effect.withTracer(effect, recording));
        return { exit, metrics: yield* Metric.snapshot, spans };
    },
    Effect.provideServiceEffect(
        Metric.MetricRegistry,
        Effect.sync(() => new Map()),
    ),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { observe };
