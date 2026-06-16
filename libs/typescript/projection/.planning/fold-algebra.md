# [PROJECTION_FOLD_ALGEBRA]

One page owns the unified key-discriminated transport-free fold algebra and the five live-cell stream stores built on it — the one `StreamPolicy` (a single `Schedule` reconnect plus a `Stream`-operator vocabulary every fold composes through `withPolicy`), the `keyedFold` combinator that collapses a `Stream` or receipt sequence into an immutable `SubscriptionRef`-backed keyed map, and the five stores `RuntimeFeed`/`HealthStore`/`SnapshotFeed`/`ProgressStore`/`ConflictPresenceStore`. The discriminant that drives every fold is the C# discriminant consumed verbatim, so the state layer is a projection of the wire vocabulary, never a parallel model. The domain is platform-neutral and transport-free: it depends only on the `interchange` decoded shapes and dials nothing.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]    | [OWNS]                                                                                                     |
| :-----: | :----------- | :-------------------------------------------------------------------------------------------------------- |
|   [1]   | FOLD_ALGEBRA | the StreamPolicy, the keyedFold combinator, the five stores, the window fold, and the LWW convergent fold |
|   [2]   | RESEARCH     | the standing-query watermark/retraction live-changefeed probe                                             |

## [2]-[FOLD_ALGEBRA]

- Owner: `StreamPolicy`, the one bounded reconnect-and-operator vocabulary applied by `withPolicy` (the single `Stream` decorator every fold pipes through); `keyedFold`, the single combinator that folds a `Stream`/receipt sequence into an immutable keyed map exposed as a `SubscriptionRef`; and the five live-cell stores — `RuntimeFeed` for the lifecycle fold (surfaced through the web domain), `HealthStore` for the health and degradation fold, `SnapshotFeed` for the snapshot fold, `ProgressStore` for the progress cells, and `ConflictPresenceStore` for the conflicts and presence fold. Every store is one `keyedFold` row over its event kinds, never a parallel store implementation.
- Cases: `RuntimeFeed` folds the `PhaseReceiptWire`/`BootMarkerWire`/`FaultRecordWire`/`DrainReceiptWire` union keyed by the discriminating `RuntimePhaseKey`/`kind` literal against `lifecycle-and-drain.md#TS_PROJECTION`; `HealthStore` folds `HealthSnapshotWire`/`DegradationWire`/`AlertReceiptWire` keyed by `HealthStatusWire`/`DegradationLevelKey`/`ruleId` and the retained-capability set gates which web surfaces are reachable against `health-and-degradation.md#TS_PROJECTION`; `SnapshotFeed` folds `SnapshotCatalogRowWire`/`SnapshotDeltaWire`/`RestoreReceiptWire` keyed by catalog id against `snapshot-codecs.md#TS_PROJECTION`; `ProgressStore` holds monotonic marks against `progress-and-observation.md#TS_PROJECTION` where a mark below the held rank never regresses the cell; `ConflictPresenceStore` folds the `OpLogEntryWire` upsert/delete/presence union, the `ConflictReceiptWire`/`ConflictOutcomeKind` adjudication outcomes, and the `PresenceRowWire` rows against `sync-collaboration.md#TS_PROJECTION` + `sync-collaboration.md#MERGE_LAW`, presence expiring on its declared `expiresAt` field and the live cell keying on the 16-byte `contentKey` verbatim with last-write-wins by HLC `(physical, logical)` exactly as the upstream `Adjudicate` total dispatch settles it; the standing-query window arm folds the decoded `OpLogEntryWire` changefeed by event-time bucket against `query-rail.md#STANDING_QUERY`. `RuntimeFeed` and `SnapshotFeed` ride the closed app-service budget; the others are fold rows beside them.
- Entry: each fold is one `keyedFold` over its event kinds into an immutable keyed map; key-discrimination is driven by the exact discriminant keys the wire shapes carry; every fold pipes its source through `withPolicy` so the bounded `Schedule` reconnect, the back-pressure buffer, the throttle, and the grouped-within batch land identically rather than per-fold improvisation; the staleness-forward retry value the envelope-and-evidence availability cluster carries grounds in this policy, so an improvised reconnect loop or an unbounded retry is the deleted form.
- Packages: `effect` for `Stream`, `SubscriptionRef`, `Schedule`, structured concurrency, and `Schema`; the `@effect-atom/atom` cell bridge is consumed at the `ui` boundary, never imported here.
- Growth: a new boundary concept lands as one `keyedFold` store row; a new event kind lands as one fold arm on its owning store's `Match.value` total dispatch; a new reconnect or back-pressure posture lands as one `StreamPolicy` field every fold inherits through `withPolicy`; a new standing-query window kind lands as one `WindowKind` `Data.TaggedEnum` variant that breaks the `bucketSet` `$match` at compile time; a new aggregation lands as one signed-delta fold arm over the existing `windowFold` cell; a new conflict outcome lands as one `ConflictOutcomeKind` arm breaking the convergent `$match`.
- Boundary: no fold re-validates a value an earlier decode gate admitted; the discriminant, the `contentKey` brand, and every wire shape are imported from `@rasm/ts` verbatim and never re-authored or intersected branch-side; ordering is borrowed verbatim from the wire — monotonic progress, last-write-wins by HLC, or monotonic merge per contract; reconnect, buffer, throttle, and batch are owned once by `StreamPolicy` and never re-derived per fold; the domain dials no transport, imports no `@connectrpc/*`, and depends only on `interchange` decoded shapes.

```ts contract
import {
  Data, Duration, Effect, HashMap, HashSet,
  Match, Option, Schedule, Scope, Stream, SubscriptionRef,
} from "effect";
import type {
  AlertReceiptWire, BootMarkerWire, ConflictReceiptWire, ContentKey,
  DegradationWire, DrainReceiptWire, FaultRecordWire, HealthSnapshotWire,
  OpLogEntryWire, PhaseReceiptWire, PresenceRowWire, ProgressMarkWire,
  RestoreReceiptWire, SnapshotCatalogRowWire, SnapshotDeltaWire,
} from "@rasm/ts"; // interchange decoded shapes — the wire contract is imported verbatim, never re-authored

// --- [SERVICES] ------------------------------------------------------------------------
// StreamPolicy is the one bounded reconnect-and-back-pressure vocabulary. Every field is a
// LIVE operator withPolicy pipes onto every fold source; no field is declared-but-dead.
interface StreamPolicy {
  readonly reconnect: Schedule.Schedule<Duration.Duration, unknown>;
  readonly buffer: { readonly capacity: number; readonly strategy: "dropping" | "sliding" | "suspend" };
  readonly throttle: { readonly cost: number; readonly units: number; readonly duration: Duration.Duration };
  readonly groupedWithin: { readonly chunkSize: number; readonly window: Duration.Duration };
}

const defaultStreamPolicy: StreamPolicy = {
  reconnect: Schedule.exponential(Duration.millis(250)).pipe(Schedule.union(Schedule.spaced(Duration.seconds(30)))),
  buffer: { capacity: 256, strategy: "sliding" },
  throttle: { cost: 1, units: 1, duration: Duration.millis(16) },
  groupedWithin: { chunkSize: 64, window: Duration.millis(100) },
};

// withPolicy applies the FULL operator set every fold composes — bounded reconnect, the
// back-pressure buffer, the rate throttle, then the grouped-within batch flatten. A fold
// that pipes a source any other way (a bare Stream.retry, an improvised loop) is the
// deleted form: this is the one place the StreamPolicy fields become behavior.
const withPolicy = <In, R>(source: Stream.Stream<In, never, R>, policy: StreamPolicy): Stream.Stream<In, never, R> =>
  source.pipe(
    Stream.retry(policy.reconnect),
    Stream.buffer({ capacity: policy.buffer.capacity, strategy: policy.buffer.strategy }),
    Stream.throttle({ cost: () => policy.throttle.cost, units: policy.throttle.units, duration: policy.throttle.duration, strategy: "shape" }),
    Stream.groupedWithin(policy.groupedWithin.chunkSize, policy.groupedWithin.window),
    Stream.flattenIterables,
  );

const keyedFold = <In, K, V>(
  source: Stream.Stream<In>,
  key: (event: In) => K,
  merge: (prior: Option.Option<V>, event: In) => V,
  policy: StreamPolicy,
): Effect.Effect<SubscriptionRef.SubscriptionRef<ReadonlyMap<K, V>>, never, Scope.Scope> =>
  Effect.gen(function* () {
    const ref = yield* SubscriptionRef.make<ReadonlyMap<K, V>>(new Map());
    yield* withPolicy(source, policy).pipe(
      Stream.runForEach((event) =>
        SubscriptionRef.update(ref, (m) => {
          const k = key(event);
          return new Map(m).set(k, merge(Option.fromNullable(m.get(k)), event));
        })),
      Effect.forkScoped,
    );
    return ref;
  });

interface ProgressStore {
  readonly marks: SubscriptionRef.SubscriptionRef<ReadonlyMap<string, ProgressMarkWire>>;
}

const progressMerge = (prior: Option.Option<ProgressMarkWire>, event: ProgressMarkWire): ProgressMarkWire =>
  Option.match(prior, { onNone: () => event, onSome: (p) => (event.rank >= p.rank ? event : p) });

// --- [LIFECYCLE_HEALTH_SNAPSHOT] -------------------------------------------------------
// The three remaining live-cell stores, each one keyedFold row over its decoded wire union.
// RuntimeFeed folds the lifecycle receipt union keyed by the RuntimePhaseKey/fault literal;
// HealthStore folds the health/degradation/alert union; SnapshotFeed folds the catalog and
// delta union keyed by catalog id. Every key and every arm reads a wire field VERBATIM —
// the discriminant is the C# discriminant the #TS_PROJECTION shape carries, never re-minted.

type RuntimeEventWire = PhaseReceiptWire | BootMarkerWire | FaultRecordWire | DrainReceiptWire;

interface RuntimeFeed {
  readonly cells: SubscriptionRef.SubscriptionRef<ReadonlyMap<string, RuntimeEventWire>>;
}

// The lifecycle event key is the phase (PhaseReceiptWire/BootMarkerWire/DrainReceiptWire all
// carry a RuntimePhaseKey) or the fault kind — total dispatch over the closed wire union by
// its present discriminant, so the latest receipt per lifecycle slot is the live cell.
const runtimeKey = (event: RuntimeEventWire): string =>
  "kind" in event ? `fault:${event.kind}`
    : "to" in event ? `phase:${event.to}`
      : "pid" in event ? `boot:${event.phase}`
        : `drain:${event.final}`;

const runtimeMerge = (_prior: Option.Option<RuntimeEventWire>, event: RuntimeEventWire): RuntimeEventWire => event;

type HealthEventWire = HealthSnapshotWire | DegradationWire | AlertReceiptWire;

interface HealthStore {
  readonly cells: SubscriptionRef.SubscriptionRef<ReadonlyMap<string, HealthEventWire>>;
}

// The health event key discriminates the snapshot (one cell), the degradation level (one
// cell), and each alert rule (one cell per ruleId) — the retained-capability set on the
// degradation cell is what gates web-surface reachability, read off the live map by the gate.
const healthKey = (event: HealthEventWire): string =>
  "entries" in event ? "health:snapshot"
    : "level" in event ? "degradation:level"
      : `alert:${event.ruleId}`;

const healthMerge = (_prior: Option.Option<HealthEventWire>, event: HealthEventWire): HealthEventWire => event;

type SnapshotEventWire = SnapshotCatalogRowWire | SnapshotDeltaWire | RestoreReceiptWire;

interface SnapshotFeed {
  readonly cells: SubscriptionRef.SubscriptionRef<ReadonlyMap<string, SnapshotEventWire>>;
}

// The snapshot event key is the catalog row id, the restore source path, or the literal
// "delta" head — a catalog row replaces its id cell, a restore receipt keys on its target,
// and the rolling delta is the single live delta cell folded over the changefeed.
const snapshotKey = (event: SnapshotEventWire): string =>
  "id" in event ? `catalog:${event.id}`
    : "source" in event ? `restore:${event.target}`
      : "delta:head";

const snapshotMerge = (_prior: Option.Option<SnapshotEventWire>, event: SnapshotEventWire): SnapshotEventWire => event;

const runtimeFeed = (events: Stream.Stream<RuntimeEventWire>, policy: StreamPolicy): Effect.Effect<RuntimeFeed, never, Scope.Scope> =>
  keyedFold(events, runtimeKey, runtimeMerge, policy).pipe(Effect.map((cells) => ({ cells })));

const healthStore = (events: Stream.Stream<HealthEventWire>, policy: StreamPolicy): Effect.Effect<HealthStore, never, Scope.Scope> =>
  keyedFold(events, healthKey, healthMerge, policy).pipe(Effect.map((cells) => ({ cells })));

const snapshotFeed = (events: Stream.Stream<SnapshotEventWire>, policy: StreamPolicy): Effect.Effect<SnapshotFeed, never, Scope.Scope> =>
  keyedFold(events, snapshotKey, snapshotMerge, policy).pipe(Effect.map((cells) => ({ cells })));

// --- [WINDOW_FOLD] ---------------------------------------------------------------------
// The standing-query window vocabulary folds the decoded changefeed by EVENT time
// (OpLogEntryWire.physical / .logical), never wall-clock arrival, so a window replays
// identically. WindowKind is the closed window family; the watermark bounds out-of-order
// tolerance and a row preceding it by less than allowedLateness emits a retraction-plus-
// restate, never a silent drop. Consumes Persistence/query-rail#STANDING_QUERY window
// vocabulary via the decoded changefeed wire fields ONLY (no IQueryable shape crosses).

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

const defaultLateness: Duration.Duration = Duration.seconds(30);

// Event time is reconstructed from the wire physical instant (ISO-8601 extended) plus the
// HLC logical half as a sub-millisecond tiebreak nanos — the decode gate already admitted
// both fields, so this is projection, not re-validation.
const eventNanos = (entry: OpLogEntryWire): bigint =>
  BigInt(new Date(entry.physical).getTime()) * 1_000_000n + entry.logical;

// Watermark is the monotonic event-time progress mark mirroring the C# Watermark(EventTime,
// Processed): eventNanos never regresses; processed is the live per-bucket folded-row count
// the UI reads off the cell to show window throughput; late is the out-of-order correction
// count so a late-arrival retraction is observable rather than silent.
interface Watermark {
  readonly eventNanos: bigint;
  readonly processed: bigint;
  readonly late: bigint;
}
const watermarkZero: Watermark = { eventNanos: 0n, processed: 0n, late: 0n };
const advance = (mark: Watermark, at: bigint, lateRow: boolean): Watermark => ({
  eventNanos: at > mark.eventNanos ? at : mark.eventNanos,
  processed: mark.processed + 1n,
  late: mark.late + (lateRow ? 1n : 0n),
});
const isLate = (mark: Watermark, at: bigint, allowed: Duration.Duration): boolean =>
  at < mark.eventNanos - BigInt(Math.trunc(Duration.toNanos(allowed)));

// bucketSet is total dispatch over the closed window family, returning the FULL set of
// buckets an event belongs to — a tumbling event floors to exactly one bucket; a sliding
// event belongs to every overlapping window whose [start, start+size) span covers it, so it
// strides by `slide` and spans by `size` (reading slide makes a sliding window genuinely
// overlap a tumbling one rather than aliasing to it); a session event is its own gap-start.
const bucketSet = (spec: WindowSpec, at: bigint): ReadonlyArray<bigint> =>
  WindowKind.$match(spec.kind, {
    Tumbling: ({ size }) => [at - (at % BigInt(Math.trunc(Duration.toNanos(size))))],
    Sliding: ({ size, slide }) => {
      const span = BigInt(Math.trunc(Duration.toNanos(size)));
      const stride = BigInt(Math.trunc(Duration.toNanos(slide)));
      const first = at - (((at % stride) + stride) % stride);
      return Array.from({ length: Number((span + stride - 1n) / stride) }, (_, i) => first - BigInt(i) * stride)
        .filter((start) => at >= start && at < start + span);
    },
    Session: () => [at],
  });

// A signed delta is the incremental-view-maintenance currency: insert positive, delete/
// retract negative, so a sliding aggregate maintains by adding entering and subtracting
// leaving rows, never re-scanning the window. The bucket count is the keyed cell.
interface WindowCell {
  readonly bucket: bigint;
  readonly count: bigint;
  readonly mark: Watermark;
}

// signOf maps the decoded wire kind to the IVM sign — upsert/presence add, delete subtracts;
// total dispatch over the closed three-kind wire literal so a fourth kind breaks at compile
// time. A processing-time bucket is the deleted form; this folds event time only.
const signOf = (kind: OpLogEntryWire["kind"]): bigint =>
  Match.value(kind).pipe(
    Match.when("delete", () => -1n),
    Match.when("upsert", () => 1n),
    Match.when("presence", () => 1n),
    Match.exhaustive,
  );

// windowMerge is the keyedFold merge arm for ONE bucket: a fresh op advances the bucket count
// by its IVM sign and advances the watermark; a late op (event time below the held watermark
// by more than allowedLateness) emits the signed retraction-plus-restate — it applies the
// sign to the SAME bucket cell and advances the processed-row count, so the windowed result
// CORRECTS by folding the late row in rather than freezing the count (the silent-drop is the
// deleted form). On-time and late paths both fold the sign; they differ only in that the late
// path is tracked as an out-of-order correction in the watermark's processed tally.
const windowMerge =
  (spec: WindowSpec) =>
  (prior: Option.Option<WindowCell>, entry: { readonly bucket: bigint; readonly wire: OpLogEntryWire }): WindowCell => {
    const at = eventNanos(entry.wire);
    const sign = signOf(entry.wire.kind);
    return Option.match(prior, {
      onNone: () => ({ bucket: entry.bucket, count: sign, mark: advance(watermarkZero, at, false) }),
      onSome: (cell) => ({
        bucket: entry.bucket,
        count: cell.count + sign,
        mark: advance(cell.mark, at, isLate(cell.mark, at, spec.allowedLateness)),
      }),
    });
  };

// windowFold is the one keyedFold window arm over the decoded changefeed — keyed by the
// event-time bucket so each tumbling/sliding/session bucket is one live cell. A poll loop or
// a full re-query per row is the deleted form: a new wire entry fans into its bucketSet (one
// bucket for tumbling/session, every overlapping window for sliding) and each fan-out element
// is one signed delta the bucket cell folds, advancing that cell's processed-row tally.
const windowFold = (
  changefeed: Stream.Stream<OpLogEntryWire>,
  spec: WindowSpec,
  policy: StreamPolicy,
): Effect.Effect<SubscriptionRef.SubscriptionRef<ReadonlyMap<bigint, WindowCell>>, never, Scope.Scope> =>
  keyedFold(
    changefeed.pipe(Stream.mapConcat((wire) => bucketSet(spec, eventNanos(wire)).map((bucket) => ({ bucket, wire })))),
    (e) => e.bucket,
    windowMerge(spec),
    policy,
  );

// --- [CONVERGENT_FOLD] -----------------------------------------------------------------
// The convergent fold over the decoded sync wire — OpLogEntryWire (the upsert/delete/presence
// op-log union) plus ConflictReceiptWire/ConflictOutcomeKind (the LWW adjudication outcomes).
// Consumes Persistence/sync-collaboration#MERGE_LAW + #TS_PROJECTION VERBATIM: there is NO
// CRDT op kind on the wire — #MERGE_LAW settles convergence as last-write-wins per column
// family adjudicated by HLC (physical, logical) with content-key equality for the idempotent-
// replay case, exactly as the upstream Adjudicate total dispatch. This projection mirrors
// that law on the consuming side; it authors no op vocabulary the wire does not carry. The
// 16-byte contentKey is the load-bearing convergence identity, imported as the interchange
// ContentKey brand and NEVER re-minted: every cell keys on the decoded bytes verbatim, so two
// peers folding the same op-set reach the identical map (strong eventual consistency).

// keyHex is the convergence currency — structural byte equality (not reference) over the
// imported ContentKey brand drives the live-cell map and the idempotent-replay test.
const keyHex = (key: ContentKey): string =>
  Array.from(key, (b) => b.toString(16).padStart(2, "0")).join("");

// The HLC event time is the (physical, logical) lexicographic order the wire carries — a
// strictly-greater pair is a later write; equal physical breaks on logical; the full tie
// (equal pair) is the idempotent-replay case the content-key equality resolves. This is the
// (incoming.Physical, incoming.Logical).CompareTo((held.Physical, held.Logical)) the upstream
// Adjudicate runs, reconstructed from the decoded wire fields without re-validation.
const afterHlc = (a: OpLogEntryWire, b: { readonly physical: string; readonly logical: bigint }): boolean => {
  const ap = Date.parse(a.physical);
  const bp = Date.parse(b.physical);
  return ap !== bp ? ap > bp : a.logical > b.logical;
};

// The convergent fold state — one live-cell map keyed by contentKey carrying the winning
// op-log entry (LWW by HLC), one tombstone set of deleted keys (a tombstoned key never
// re-lives under an equal-or-earlier write), and the conflict-receipt ledger keyed by the
// four ConflictOutcomeKind buckets so the inspector reads adjudication evidence off the cell.
interface ConflictPresenceState {
  readonly live: HashMap.HashMap<string, OpLogEntryWire>;
  readonly tombstones: HashSet.HashSet<string>;
  readonly conflicts: HashMap.HashMap<string, ReadonlyArray<ConflictReceiptWire>>;
}
const emptyConflictPresence: ConflictPresenceState = {
  live: HashMap.empty(),
  tombstones: HashSet.empty(),
  conflicts: HashMap.empty(),
};

// opMerge is the convergent CmRDT fold body — total dispatch over the closed three-kind wire
// literal via Match.value. Every arm is commutative, associative, and idempotent over the
// contentKey bytes under the HLC order, so the op-set folds to the same state regardless of
// delivery order (strong eventual consistency): an upsert installs iff strictly later than
// the held entry (or fresh), a delete tombstones under the same later-than guard, and a
// presence op never mutates the live geometry cell (it rides the presence keyedFold row).
const opMerge = (state: ConflictPresenceState, entry: OpLogEntryWire): ConflictPresenceState => {
  const ek = keyHex(entry.contentKey);
  const held = HashMap.get(state.live, ek);
  const later = Option.match(held, { onNone: () => true, onSome: (h) => afterHlc(entry, h) });
  return Match.value(entry.kind).pipe(
    Match.when("upsert", () =>
      !later || HashSet.has(state.tombstones, ek) ? state : { ...state, live: HashMap.set(state.live, ek, entry) }),
    Match.when("delete", () =>
      !later ? state : { ...state, live: HashMap.remove(state.live, ek), tombstones: HashSet.add(state.tombstones, ek) }),
    Match.when("presence", () => state),
    Match.exhaustive,
  );
};

// conflictMerge folds one adjudication outcome into the receipt ledger — total dispatch over
// the four ConflictOutcomeKind buckets, appending the receipt under its outcome key so the
// inspector surface reads LocalWin/RemoteWin/Merged/Rejected evidence off the live cell. The
// outcome bucket is the C# ConflictOutcome [Union] case name crossed as the wire literal.
const conflictMerge = (state: ConflictPresenceState, receipt: ConflictReceiptWire): ConflictPresenceState => {
  const prior = HashMap.get(state.conflicts, receipt.outcome).pipe(Option.getOrElse(() => [] as ReadonlyArray<ConflictReceiptWire>));
  return { ...state, conflicts: HashMap.set(state.conflicts, receipt.outcome, [...prior, receipt]) };
};

// conflictPresenceFold is the ConflictPresenceStore convergence row over the decoded sync
// changefeed — one Stream.scan over the convergent fold body folding both the op-log entries
// and the adjudication receipts through Match.value total dispatch, exposed as a
// SubscriptionRef so the ui cell bridge reads the live converged state. The fold is
// order-independent (LWW by HLC) so a reconnect replay through StreamPolicy converges
// identically; the source pipes through withPolicy so the bounded reconnect, buffer, throttle,
// and batch are the same operators every other fold composes.
const conflictPresenceFold = (
  changefeed: Stream.Stream<OpLogEntryWire | ConflictReceiptWire>,
  policy: StreamPolicy,
): Effect.Effect<SubscriptionRef.SubscriptionRef<ConflictPresenceState>, never, Scope.Scope> =>
  Effect.gen(function* () {
    const ref = yield* SubscriptionRef.make(emptyConflictPresence);
    yield* withPolicy(changefeed, policy).pipe(
      Stream.runForEach((event) =>
        SubscriptionRef.update(ref, (s) => ("outcome" in event ? conflictMerge(s, event) : opMerge(s, event)))),
      Effect.forkScoped,
    );
    return ref;
  });

interface ConflictPresenceStore {
  readonly state: SubscriptionRef.SubscriptionRef<ConflictPresenceState>;
  readonly presence: SubscriptionRef.SubscriptionRef<ReadonlyMap<string, PresenceRowWire>>;
}

// Presence rides the same store as one keyedFold row — a presence wire row expires on its
// declared expiresAt instant, so an expired row never gates a live editing surface.
const presenceMerge = (prior: Option.Option<PresenceRowWire>, event: PresenceRowWire): PresenceRowWire =>
  Option.match(prior, {
    onNone: () => event,
    onSome: (p) => (Date.parse(event.expiresAt) >= Date.parse(p.expiresAt) ? event : p),
  });
```

## [3]-[RESEARCH]

- [STANDING_QUERY_WATERMARK]: the landed `WindowKind` `Data.TaggedEnum` (Tumbling/Sliding/Session) plus the `windowFold` signed-delta IVM arm folds the decoded changefeed by event-time bucket against `query-rail#STANDING_QUERY`; the `bucketSet` fan-out reads `Sliding.slide` to emit the overlapping window set (stride by slide, span by size) so a sliding window genuinely overlaps rather than aliasing to a tumbling one, and `windowMerge` folds the late row's signed delta into its bucket cell (retraction-plus-restate, never a silent drop) while tracking the out-of-order correction in the watermark `late` tally — both behaviors are in the body; the residual probe is the live-changefeed confirmation that the late retraction and the sliding signed-delta maintenance hold against a running op-log replay with real out-of-order arrival.
- [LWW_CONVERGENCE]: the `conflictPresenceFold` convergent fold over the decoded `OpLogEntryWire` (upsert/delete/presence) plus the `ConflictReceiptWire`/`ConflictOutcomeKind` adjudication outcomes against `sync-collaboration#MERGE_LAW` + `#TS_PROJECTION` is landed (last-write-wins by HLC `(physical, logical)` with content-key idempotent-replay, tombstone-guarded, keyed on the imported 16-byte `ContentKey` brand verbatim — mirroring the upstream `Adjudicate` total dispatch, authoring no op vocabulary the wire does not carry); the residual probe is the strong-eventual-consistency confirmation that two peers folding the same op-set in divergent delivery order reach a byte-identical `ConflictPresenceState` against the live `#MERGE_LAW` adjudication — the fold body is commutative/associative/idempotent by construction; only the live cross-peer convergence harness remains.
