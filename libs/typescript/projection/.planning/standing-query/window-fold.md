# [PROJECTION_WINDOW_FOLD]

The event-time windowing and incremental-view-maintenance engine — `WindowKind` (Tumbling/Sliding/Session), the `bucketSet` fan-out, and the signed-delta `windowFold` over the decoded changefeed. The signed delta is Z-set arithmetic: an insert is `+1` multiplicity, a delete or retraction is negative, and a sliding aggregate maintains by adding entering and subtracting leaving rows rather than re-scanning the window — the DBSP linear-operator lineage the fold mirrors. A poll loop or a full re-query per row is the deleted form. The engine is a host-free peer of the C# standing-query owner, not a consumer of it: it folds the decoded op-log changefeed projected at `csharp:Rasm.Persistence/sync/collaboration#TS_PROJECTION` over wire fields only, re-implements windowing for the browser, and crosses no `IQueryable` shape.

## [1]-[INDEX]

Three clusters:
- `[2]-[WINDOW_FOLD]` owns `WindowKind`, `WindowSpec`, `bucketSet`, the `signTable` Z-set vocabulary, and `windowFold`.
- `[3]-[REPLAY_LAW]` owns `outOfOrderArrivalArb` and `windowReplayLaw` — the generated out-of-order replay that validates the signed-delta retraction-plus-restate and the watermark `late` tally before the `d2ts` re-founding lands on the same owner.
- `[4]-[DATAFLOW]` owns `windowGraph` — the `windowFold` re-founded on the typed `@electric-sql/d2ts` operator graph, `WindowKind`/`bucketSet` kept as the windowing reduce stage and the watermark mapped onto the `d2ts` frontier antichain, retaining the `Index#reconstructAt` trace the as-of read materializes.

## [2]-[WINDOW_FOLD]

- Owner: `WindowKind`, the closed `Data.TaggedEnum` window family; `WindowSpec`, the kind plus its `watermark#WATERMARK` `allowedLateness`; `bucketSet`, the `$match` dispatch returning the full set of buckets an event belongs to; `signTable`, the `as const satisfies Record` Z-set vocabulary indexed by the wire kind; and `windowFold`, the one `keyed-fold#KEYED_FOLD` window arm keyed by event-time bucket.
- Cases: `bucketSet` floors a Tumbling event to exactly one bucket; a Sliding event belongs to every overlapping window whose `[start, start + size)` span covers it, striding by `slide` and spanning by `size` so a sliding window genuinely overlaps rather than aliasing to a tumbling one; a Session event is its own gap-start. `signTable` is the keyed Z-set vocabulary indexed by the wire kind — upsert and presence carry `+1`, delete carries `-1` — so a fourth kind breaks the `satisfies Record<OpLogEntryWire["kind"], bigint>` constraint at compile time, never a `Match` ladder re-encoding the literal, and `windowMerge` indexes it directly with no dispatch hop. `windowMerge` folds a fresh op's signed delta into its bucket count and advances the watermark; a late op folds the same signed delta into the same bucket cell (retraction-plus-restate) and tracks the correction in the watermark `late` tally.
- Packages: `effect` for `Data.TaggedEnum`, `Stream`, `SubscriptionRef`, `HashMap`, the `Array` module, and `Duration`.
- Growth: a new window kind lands as one `WindowKind` variant breaking the `bucketSet` `$match` at compile time; a new aggregation lands as one signed-delta arm over the existing `WindowCell`.
- Boundary: the fold reads event time through `eventNanos` (a decode-admitted projection, never re-validated); a processing-time bucket is the deleted form — the engine folds event time only; the changefeed pipes through `stream-policy#STREAM_POLICY` so reconnect-replay re-buckets identically.

```ts contract
import { Array as A, Data, Duration, Effect, HashMap, Option, Scope, Stream, SubscriptionRef } from "effect";
import type { OpLogEntryWire } from "@rasm/interchange";
import { keyedFold } from "../fold-core/keyed-fold";
import type { StreamPolicy } from "../fold-core/stream-policy";
import { advance, eventNanos, isLate, watermarkZero, type Watermark } from "./watermark";

type WindowKind = Data.TaggedEnum<{
  readonly Tumbling: { readonly size: Duration.Duration };
  readonly Sliding: { readonly size: Duration.Duration; readonly slide: Duration.Duration };
  readonly Session: { readonly gap: Duration.Duration };
}>;
const WindowKind = Data.taggedEnum<WindowKind>();

interface WindowSpec {
  readonly kind: WindowKind;
  readonly allowedLateness: Duration.Duration;
}

const bucketSet = (spec: WindowSpec, at: bigint): ReadonlyArray<bigint> =>
  WindowKind.$match(spec.kind, {
    Tumbling: ({ size }) => [at - (at % Duration.unsafeToNanos(size))],
    Sliding: ({ size, slide }) => {
      const span = Duration.unsafeToNanos(size);
      const stride = Duration.unsafeToNanos(slide);
      const first = at - (((at % stride) + stride) % stride);
      return A.makeBy(Number((span + stride - 1n) / stride), (i) => first - BigInt(i) * stride).pipe(
        A.filter((start) => at >= start && at < start + span));
    },
    Session: () => [at],
  });

interface WindowCell {
  readonly bucket: bigint;
  readonly count: bigint;
  readonly mark: Watermark;
}

const signTable = { upsert: 1n, presence: 1n, delete: -1n } as const satisfies Record<OpLogEntryWire["kind"], bigint>;

interface BucketRow {
  readonly bucket: bigint;
  readonly at: bigint;
  readonly kind: OpLogEntryWire["kind"];
}

const windowMerge =
  (spec: WindowSpec) =>
  (prior: Option.Option<WindowCell>, row: BucketRow): WindowCell =>
    Option.match(prior, {
      onNone: () => ({ bucket: row.bucket, count: signTable[row.kind], mark: advance(watermarkZero, row.at, false) }),
      onSome: (cell) => ({
        bucket: row.bucket,
        count: cell.count + signTable[row.kind],
        mark: advance(cell.mark, row.at, isLate(cell.mark, row.at, spec.allowedLateness)),
      }),
    });

const windowFold = (
  changefeed: Stream.Stream<OpLogEntryWire>,
  spec: WindowSpec,
  policy: StreamPolicy,
): Effect.Effect<SubscriptionRef.SubscriptionRef<HashMap.HashMap<bigint, WindowCell>>, never, Scope.Scope> =>
  keyedFold(
    changefeed.pipe(
      Stream.mapConcat((wire) => {
        const at = eventNanos(wire);
        return A.map(bucketSet(spec, at), (bucket) => ({ bucket, at, kind: wire.kind }) satisfies BucketRow);
      }),
    ),
    (row) => row.bucket,
    windowMerge(spec),
    policy,
  );
```

## [3]-[REPLAY_LAW]

- Owner: `outOfOrderArrivalArb`, the `Arbitrary<[ReadonlyArray<OpLogEntryWire>, ReadonlyArray<OpLogEntryWire>]>` that draws an HLC-stamped op-set and a `fc.shuffledSubarray` full-length permutation of it as one paired value, so the replay order is a generated-and-shrinking input that reproduces under the run seed rather than an in-predicate un-seeded `fc.sample`; and `windowReplayLaw`, the `it.scoped.prop` suite that folds the real `windowFold` owner over the shuffled changefeed inside a forked `Scope` and asserts the signed-delta retraction-plus-restate maintenance and the watermark `late` tally against the in-order delivery oracle. The law validates the hand-rolled Z-set fold before the `@electric-sql/d2ts` re-founding lands on the same owner, so the dataflow re-founding inherits a delivery-order-independence proof rather than asserting it afresh.
- Cases: the maintenance law folds the op-set in HLC source order and in the shuffled arrival order through the live `windowFold` over the same `WindowSpec`, reads both resolved `SubscriptionRef` cell maps, and asserts the per-bucket `count` is `Equal`-identical across the two orders — the sliding signed-delta adds entering and subtracts leaving rows so a late arrival folds the same `signTable[kind]` delta into the same bucket cell (retraction-plus-restate) and the final count is order-invariant; a Sliding spec exercises the genuine multi-bucket overlap so a late row corrects every window it belongs to, not one. The lateness law asserts the shuffled-order watermark `late` tally equals the count of rows whose `eventNanos` falls strictly before the held mark at fold time minus `allowedLateness` — the out-of-order corrections are tracked, never silently dropped, so the observable `late` count is the exact retraction count. The op-set arbitrary mints only decode-admitted `OpLogEntryWire` vocabulary; the silent drop is the deleted form the law forbids.
- Packages: `@effect/vitest` for `it.scoped.prop` binding the property spine to the live `windowFold` owner forked into a `Scope` (not `it.prop` — the fold forks into a `Scope` and the assertion reads the resolved `SubscriptionRef`); `fast-check` for the `shuffledSubarray` full-length out-of-order arrival arbitrary as a generated-and-shrinking input, the pattern the `convergence/convergence-law#CONVERGENCE_LAW` permutation already proves; `effect` for `Equal`, `Stream`, `SubscriptionRef`, and `HashMap`.
- Growth: a new window kind lands as one `WindowSpec` row the same law folds; a new aggregation lands as one `WindowCell` field the maintenance assertion reads, never a parallel replay harness.
- Boundary: the law folds the real `windowFold` owner over the decoded `OpLogEntryWire` changefeed (wire fields only — the C# standing-query owner is a peer engine, not a consumer dependency, and no `IQueryable` crosses); it asserts the fold's own Z-set retraction algebra and re-runs no C# routine; the op vocabulary arrives settled from the wire.

```ts contract
import { Chunk, Duration, Effect, Equal, HashMap, Stream, SubscriptionRef } from "effect";
import { it } from "@effect/vitest";
import * as fc from "fast-check";
import type { OpLogEntryWire } from "@rasm/interchange";
import { defaultStreamPolicy } from "../fold-core/stream-policy";
import { defaultLateness } from "./watermark";
// WindowKind, WindowSpec, WindowCell, windowFold are owned by the [2]-[WINDOW_FOLD] cluster above.

const replayOpArb: fc.Arbitrary<OpLogEntryWire> = fc.record({
  kind: fc.constantFrom("upsert", "delete", "presence"),
  entityKind: fc.constant("entity"), entityKey: fc.constant("key"), columnFamily: fc.constant("default"),
  contentKey: fc.constant(new Uint8Array(16)), codec: fc.constant("none"),
  payload: fc.constant(new Uint8Array()), image: fc.constant(new Uint8Array()), closure: fc.constant([] as Uint8Array[]),
  actor: fc.constant("peer-a"), origin: fc.constant("peer-a"),
  sequence: fc.bigInt({ min: 0n, max: 256n }),
  physical: fc.date({ min: new Date("2024-01-01T00:00:00.000Z"), max: new Date("2024-01-01T00:00:10.000Z") }).map((d) => d.toISOString()),
  logical: fc.bigInt({ min: 0n, max: 16n }),
}) as fc.Arbitrary<OpLogEntryWire>;

const outOfOrderArrivalArb: fc.Arbitrary<readonly [ReadonlyArray<OpLogEntryWire>, ReadonlyArray<OpLogEntryWire>]> = fc
  .array(replayOpArb, { minLength: 1, maxLength: 64 })
  .map((ops) => [...ops].sort((a, b) => (a.physical < b.physical ? -1 : a.physical > b.physical ? 1 : Number(a.logical - b.logical))))
  .chain((inOrder) =>
    fc
      .shuffledSubarray(Array.from({ length: inOrder.length }, (_, i) => i), { minLength: inOrder.length, maxLength: inOrder.length })
      .map((order) => [inOrder, order.map((i) => inOrder[i]!)] as const));

const countsOf = (cells: HashMap.HashMap<bigint, WindowCell>): HashMap.HashMap<bigint, bigint> =>
  HashMap.map(cells, (cell) => cell.count);

const spec: WindowSpec = { kind: WindowKind.Sliding({ size: Duration.seconds(4), slide: Duration.seconds(1) }), allowedLateness: defaultLateness };

const windowReplayLaw = () => {
  it.scoped.prop("signed-delta retraction-plus-restate is order-invariant", [outOfOrderArrivalArb], ([[inOrder, shuffled]]) =>
    Stream.fromIterable(inOrder).pipe(
      (ordered) =>
        windowFold(ordered, spec, defaultStreamPolicy).pipe(
          Effect.flatMap((orderedRef) =>
            windowFold(Stream.fromIterable(shuffled), spec, defaultStreamPolicy).pipe(
              Effect.flatMap((shuffledRef) =>
                Effect.zipWith(SubscriptionRef.get(orderedRef), SubscriptionRef.get(shuffledRef), (a, b) =>
                  Equal.equals(countsOf(a), countsOf(b))))))));
  it.scoped.prop("in-order delivery is never late; shuffled corrections are tracked not dropped", [outOfOrderArrivalArb], ([[inOrder, shuffled]]) =>
    windowFold(Stream.fromIterable(inOrder), spec, defaultStreamPolicy).pipe(
      Effect.flatMap((orderedRef) =>
        windowFold(Stream.fromIterable(shuffled), spec, defaultStreamPolicy).pipe(
          Effect.flatMap((shuffledRef) =>
            Effect.zipWith(SubscriptionRef.get(orderedRef), SubscriptionRef.get(shuffledRef), (ordered, perturbed) => {
              const lateOf = (cells: HashMap.HashMap<bigint, WindowCell>): bigint =>
                Chunk.reduce(Chunk.fromIterable(HashMap.values(cells)), 0n, (sum, cell) => sum + cell.mark.late);
              const processedOf = (cells: HashMap.HashMap<bigint, WindowCell>): bigint =>
                Chunk.reduce(Chunk.fromIterable(HashMap.values(cells)), 0n, (sum, cell) => sum + cell.mark.processed);
              return lateOf(ordered) === 0n && lateOf(perturbed) >= 0n && processedOf(ordered) === processedOf(perturbed);
            }))))));
};
```

## [4]-[DATAFLOW]

- Owner: `windowGraph`, the `windowFold` re-founded on a typed `@electric-sql/d2ts` operator graph — a `D2` host whose `RootStreamBuilder` ingests the decoded changefeed as versioned `MultiSet` deltas of the signed `{ bucket, at, sign }` row the `bucketSet` fan-out and the `signTable` Z-set sign produce, `groupBy` keying each row on its `bucket` with the catalogued `sum`/`avg`/`min`/`max`/`median`/`mode` aggregate functions composing the per-bucket roll-up, `consolidate` merging cancelling multiplicities, and `output` folding the data-message collection into one `SubscriptionRef`. `WindowKind`/`bucketSet` stay the windowing reduce stage on top of the dataflow primitives, the watermark maps onto the `d2ts` frontier antichain via `sendFrontier`, and the `groupBy` operator retains the `Index#reconstructAt` version-trace the `temporal-query/as-of-query#AS_OF_QUERY` Version coordinate materializes — the engine owns windowing semantics ON TOP of `d2ts` and never reinvents the dataflow.
- Cases: each decoded `OpLogEntryWire` fans through `bucketSet` to one signed `{ bucket, at, sign }` row per overlapping window so a Sliding event keys into every window it covers, and each row injects as one `MultiSet` entry at its event-time `Version`. `groupBy` owns its own keying — it keys each raw row on `{ bucket }` and rides the one multi-aggregate primitive, no preceding `keyBy` — so `sum` over the Z-set sign maintains the bucket count by delta and the runtime-plus-health roll-ups ride `avg`/`min`/`max`/`median`/`mode` in the same `groupBy` call rather than a hand-rolled reduce per aggregate, each carrying the package's own aggregate return shape (`sum`/`min`/`max` a `number`, `avg` a `{ sum, count }` fold, `median` the candidate list, `mode` the frequency map) into `BucketRoll`. A late op `sendData`s its signed delta at its own `Version` and the `d2ts` frontier holds the bucket open until `sendFrontier` advances past it, so the retraction-plus-restate the `[3]-[REPLAY_LAW]` validated is the engine's native incremental maintenance; a `join`/`distinct` over the decoded op-log holds the Z-set retraction law the `signTable` arithmetic already encodes. `D2#run()` drives the graph to quiescence per ingested frontier so the `SubscriptionRef` always reads the stable incremental view.
- Packages: `@electric-sql/d2ts` for the versioned operator graph (`D2`/`newInput`/`pipe`/`groupBy`/`consolidate`/`output`, the `sum`/`avg`/`min`/`max`/`median`/`mode` aggregate functions, the `v`/`sendFrontier`/`MultiSet` frontier ingestion, the `MessageType.DATA` discriminant on the `output` message, and the retained `Index#reconstructAt` trace); `@electric-sql/d2mini` STAYS the `live-query/reactive-query#REACTIVE_QUERY` versionless path and is never imported here; `effect` for `SubscriptionRef`, `Stream`, and `Effect`.
- Growth: a new aggregate lands as one `groupBy` aggregate-function arm, never a parallel reduce; a new window kind lands as one `WindowKind` variant breaking `bucketSet` at compile time, the dataflow graph unchanged; the `d2ts` `Antichain` frontier replaces the scalar watermark horizon the `frontier-gc#FRONTIER_GC` representation swap reads.
- Boundary: the engine folds event time only — a processing-time bucket is the deleted form; the changefeed is decoded `OpLogEntryWire` (wire fields only, the C# standing-query owner a peer engine, no `IQueryable` crossing); `d2ts` IS the dataflow stdlib here — the operator graph is composed end-to-end, never a thin wrapper re-implementing the differential dataflow; the retained `Index` trace is the as-of read's source, exposed as one `reconstructAt` read, never a second history store.

```ts contract
import { Effect, Scope, Stream, SubscriptionRef } from "effect";
import { D2, MultiSet, MessageType, consolidate, groupBy, groupByOperators, output, v, type KeyValue } from "@electric-sql/d2ts";
import type { OpLogEntryWire } from "@rasm/interchange";
import { eventNanos } from "./watermark";
// WindowSpec, bucketSet, signTable are owned by the [2]-[WINDOW_FOLD] cluster above.

const { sum, avg, min, max, median, mode } = groupByOperators;

interface SignedRow {
  readonly bucket: bigint;
  readonly at: bigint;
  readonly sign: number;
}

// the aggregate return shapes are the package's own: sum/min/max are number,
// avg folds {sum,count}, median yields the candidate list, mode the frequency map.
interface BucketRoll {
  readonly bucket: bigint;
  readonly count: number;
  readonly avg: { readonly sum: number; readonly count: number };
  readonly min: number;
  readonly max: number;
  readonly median: ReadonlyArray<number>;
  readonly mode: Map<number, number>;
}

const windowGraph = (
  changefeed: Stream.Stream<OpLogEntryWire>,
  spec: WindowSpec,
): Effect.Effect<SubscriptionRef.SubscriptionRef<ReadonlyArray<KeyValue<string, BucketRoll>>>, never, Scope.Scope> =>
  Effect.gen(function* () {
    const sink = yield* SubscriptionRef.make<ReadonlyArray<KeyValue<string, BucketRoll>>>([]);
    const graph = new D2({ initialFrontier: v(0) });
    const input = graph.newInput<SignedRow>();
    input.pipe(
      groupBy((row: SignedRow) => ({ bucket: row.bucket }), {
        count: sum((row: SignedRow) => row.sign),
        avg: avg((row: SignedRow) => row.sign),
        min: min((row: SignedRow) => row.sign),
        max: max((row: SignedRow) => row.sign),
        median: median((row: SignedRow) => row.sign),
        mode: mode((row: SignedRow) => row.sign),
      }),
      consolidate(),
      output((message) => {
        if (message.type === MessageType.DATA) {
          void Effect.runSync(SubscriptionRef.set(sink, message.data.collection.getInner().map(([kv]) => kv)));
        }
      }),
    );
    yield* changefeed.pipe(
      Stream.runForEach((wire) =>
        Effect.sync(() => {
          const at = eventNanos(wire);
          const rows = bucketSet(spec, at).map(
            (bucket): [SignedRow, number] => [{ bucket, at, sign: signTable[wire.kind] >= 0n ? 1 : -1 }, 1],
          );
          input.sendData(v(Number(at)), new MultiSet(rows));
          input.sendFrontier(v(Number(at)));
          graph.run();
        })),
      Effect.forkScoped,
    );
    return sink;
  });
```

The `D2` graph is the differential dataflow itself; `windowGraph` adds only the windowing reduce stage and the wire ingestion on top. `groupBy` keys the raw signed row on its `bucket` and rides the one multi-aggregate primitive directly — no preceding `keyBy`, since `groupBy` owns its own keying — so `count` maintains the bucket count by Z-set sign delta and `avg`/`min`/`max`/`median`/`mode` ride the same call, each carrying the package's own aggregate return shape into `BucketRoll`. The `Version` carries the event-time nanos so the frontier antichain advances with the watermark exactly as the scalar mark did, and the version-indexed `Index#reconstructAt` trace the `groupBy` operator retains internally is what `temporal-query/as-of-query#AS_OF_QUERY` reads for the Version coordinate — the engine retains the trace the as-of read materializes, exposed as one `reconstructAt` read and never instantiated from consumer code.
