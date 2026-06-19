# [PROJECTION_REACTIVE_QUERY]

The versionless incremental-query surface over the decoded changefeed — `LiveQuery` wraps one `@electric-sql/d2mini` `D2` graph, drives a typed `PipedOperator` pipeline of `map`/`filter`/`keyBy`/`join`/`reduce`/`distinct`/`consolidate` over `MultiSet` signed-multiplicity deltas, and folds the `output` sink back into one `SubscriptionRef` so a composite reactive view (a health-gated runtime view, an evidence-correlated-by-`ContentKey` timeline, a join of two base stores) is one declared derivation owned at the `projection` altitude rather than recombined manually in `ui`. This is the non-windowed reactive-query path: d2mini drops the version axis and the frontier, so it serves a fully-arrived or order-insensitive query and never the event-time engine — the windowed event-time IVM stays on `standing-query/window-fold#WINDOW_FOLD` over `@electric-sql/d2ts`, and the two engines are disjoint by frontier requirement. The pipeline consumes the decode-admitted wire rows verbatim; the query mints no identity and re-validates no field.

## [1]-[INDEX]

One cluster: `[2]-[REACTIVE_QUERY]` owns `LiveQuery`, the `queryStore` graph-fold constructor, and the `consolidatedSink` `MultiSet`-to-map collapse.

## [2]-[REACTIVE_QUERY]

- Owner: `LiveQuery<In, Out>`, the declared derivation carrying the input `Stream` and the `PipedOperator<In, Out>` pipeline; `queryStore`, the constructor that builds one `D2` graph, wires the pipeline through `pipe`, terminates it in an `output` sink folded into a `SubscriptionRef<HashMap<string, Out>>`, drives `sendData`/`run` per arrived event through `stream-policy#STREAM_POLICY` `withPolicy` and `Stream.runForEach`, and forks into the enclosing `Scope`; `consolidatedSink`, the `MultiSet.getInner` collapse that nets the signed multiplicities into the keyed map so a positive net is a present row and a zero net is an evicted row.
- Cases: a `map`/`filter`/`keyBy` pipeline is a stateless projection the graph runs per chunk; a `join` over two base streams emits a `KeyValue<K, [V1, V2]>` keyed delta the sink nets; a `reduce`/`count`/`distinct` keyed aggregate maintains by delta because d2mini owns the operator state; `consolidate` precedes the `output` sink so multiplicity cancellations within a chunk net before the fold reads them, and the sink reads `MultiSet.getInner` `[value, multiplicity][]` pairs folding a positive net to a set and a non-positive net to a remove. The graph runs to quiescence per arrived chunk because d2mini carries no frontier — every input is implicitly complete at `run`.
- Packages: `@electric-sql/d2mini` for `D2`, `MultiSet`, the `map`/`filter`/`keyBy`/`join`/`reduce`/`distinct`/`consolidate`/`output` operators, and `PipedOperator`; `effect` for `Stream`, `SubscriptionRef`, `Ref`, `HashMap`, `Effect`, and `Scope`.
- Growth: a new composite view lands as one `LiveQuery` value carrying its pipeline, never a new constructor; a new operator lands as one stage in the `PipedOperator` chain the existing `queryStore` runs; the windowed event-time path is never re-founded here — that engine rides `@electric-sql/d2ts` on `window-fold#WINDOW_FOLD`.
- Boundary: d2mini is the version-free engine — a `sendFrontier`, a `Version`, or an `Antichain` argument is the rejected form, reserved for the `d2ts` windowed engine; the `output` sink is a synchronous host callback so the `Ref.update` discharges through `Effect.runSync` at that one interop seam, the only synchronous discharge in the fold and never a domain-logic escape; the pipeline reads decode-admitted wire rows and re-validates none; the input pipes through `stream-policy#STREAM_POLICY` `withPolicy` so reconnect-replay re-runs the graph identically; consumers read the `SubscriptionRef` and the `@effect-atom/atom` bridge binds each derived view at the `ui` boundary, never imported here; the recombination logic stays at the `projection` altitude so `ui` carries no view-state library.

```ts contract
import { Effect, HashMap, Ref, Scope, Stream, SubscriptionRef } from "effect";
import { consolidate, D2, MultiSet, output, type PipedOperator } from "@electric-sql/d2mini";
import { withPolicy, type StreamPolicy } from "../fold-core/stream-policy";

interface LiveQuery<In, Out> {
  readonly source: Stream.Stream<In>;
  readonly pipeline: PipedOperator<In, readonly [string, Out]>;
}

const consolidatedSink = <Out>(
  ref: SubscriptionRef.SubscriptionRef<HashMap.HashMap<string, Out>>,
) => (delta: MultiSet<readonly [string, Out]>): void => {
  for (const [[key, value], multiplicity] of delta.getInner()) {
    Effect.runSync(Ref.update(ref, (m) => (multiplicity > 0 ? HashMap.set(m, key, value) : HashMap.remove(m, key))));
  }
};

const queryStore = <In, Out>(
  query: LiveQuery<In, Out>,
  policy: StreamPolicy,
): Effect.Effect<SubscriptionRef.SubscriptionRef<HashMap.HashMap<string, Out>>, never, Scope.Scope> =>
  Effect.gen(function* () {
    const ref = yield* SubscriptionRef.make(HashMap.empty<string, Out>());
    const graph = new D2();
    const input = graph.newInput<In>();
    input.pipe(query.pipeline, consolidate(), output(consolidatedSink(ref)));
    graph.finalize();
    yield* withPolicy(query.source, policy).pipe(
      Stream.runForEach((event) =>
        Effect.sync(() => {
          input.sendData(new MultiSet([[event, 1]]));
          graph.run();
        })),
      Effect.forkScoped,
    );
    return ref;
  });
```

## [3]-[RESEARCH]

- [SINK_VALUE_KEYING]: the `consolidatedSink` reads the pipeline's terminal `readonly [string, Out]` keyed shape, so every `LiveQuery` pipeline terminates in a `keyBy`-projected key the net collapses on; a pipeline whose terminal stage is `unkey`-stripped carries no map key and binds a scalar fold instead. The terminal-keyed contract is the one shape `queryStore` reads — a pipeline producing a bare `Out` with no key is the deleted form that recombines into a scalar `foldStream` rather than the keyed map.
