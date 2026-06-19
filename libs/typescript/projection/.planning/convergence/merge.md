# [PROJECTION_MERGE]

The strong-eventual-consistency CRDT fold over the decoded sync wire — `opMerge` last-write-wins by HLC over the upsert/delete/presence op-log union, tombstone-guarded, keyed on the imported 16-byte `ContentKey` brand, with the `ConflictOutcomeKind` adjudication-receipt ledger beside it. Every arm is commutative, associative, and idempotent over the `ContentKey` bytes under the HLC order, so two peers folding the same op-set in divergent delivery order reach a byte-identical state. The fold consumes the decoded op-log changefeed projected at `csharp:Rasm.Persistence/Sync/collaboration#TS_PROJECTION` and reaches convergence independently; its observable outcome agrees with the C# merge law because both fold the same HLC-ordered op vocabulary, not because the TypeScript fold re-runs a C# routine. The scalar fold reads the closed three-kind `OpLogEntryWire.kind` literal only and never decodes the `payload` bytes; the `crdt` column-family op carries its `CrdtOpWire` join-semilattice algebra on that same `payload` slot, decoded by `causality/vector#VERSION_VECTOR` rather than re-encoded here. The `ContentKey` is the load-bearing convergence identity, imported from `interchange/Codec/frame#FRAME_RAIL` and never re-minted.

## [01]-[INDEX]

- [01]-[LWW_MERGE]: Owns `ConflictPresenceState`, the `ConflictOutcomeKind` ledger key, `afterHlc`, `opMerge`, `conflictMerge`, and `conflictPresenceFold`.

## [02]-[LWW_MERGE]

- Owner: `ConflictPresenceState`, the convergent fold state (the live-cell map keyed by `ContentKey`, the tombstone set, the outcome-keyed conflict ledger); `opMerge`, the CmRDT fold body over the closed three-kind wire literal; `conflictMerge`, the adjudication-receipt fold; and `conflictPresenceFold`, the `ConflictPresenceStore` convergence row over the decoded sync changefeed.
- Cases: `keyHex` is the convergence currency this cluster owns and exports — the one `ContentKey`-brand-to-hex transcription string-keying every keyed map in the folder, structural byte equality over the imported brand driving the live-cell map and the idempotent-replay test, composed by `evidence/correlation#EVIDENCE_CORRELATION` rather than re-declared; `afterHlc` is the total order the C# adjudication encodes — the `(physical, logical)` lexicographic HLC pair with the `origin` store-id breaking the equal-stamp tie deterministically, so two peers folding divergent delivery order resolve the equal-stamp case identically and the content-key-equal case settles to idempotent replay; `physicalMs` floors an unparseable instant to the zero epoch so the order stays total and an `NaN` parse never silently collapses every comparison to `false` and corrupts LWW. `opMerge` installs an upsert only when strictly later than the held entry and the key is not tombstoned, tombstones a delete under the same later-than guard (a tombstoned key never re-lives under an equal-or-earlier write), and leaves the live cell untouched on a presence op. `conflictMerge` appends each receipt under its `ConflictOutcomeKind` literal so the inspector reads `LocalWin`/`RemoteWin`/`Merged`/`Rejected` evidence off the live cell.
- Packages: `effect` for `HashMap`, `HashSet`, `Match`, `Option`, `Stream`, `SubscriptionRef`, `Effect`, and `Scope`; `fast-check` and `@effect/vitest` ground the `law#CONVERGENCE_LAW` harness.
- Growth: a new conflict outcome lands as one `ConflictOutcomeKind` literal breaking the `conflictMerge` exhaustive lookup; the `crdt` column-family op carries the `CrdtOpWire` join-semilattice algebra on the `OpLogEntryWire.payload` slot — the convergence-detection read model that decodes it is `causality/vector#VERSION_VECTOR`, never a fifth scalar arm here; tombstone retention is not a per-row `expiresAt` scan here — a delete entry projects its `eventNanos` into the `retention#SOURCE_WIRING` `tombstoneReclaimable` stream, so the tombstone set is trimmed by the one unified frontier rule once its delete event time falls below the finalized horizon, never a second TTL scan in this fold.
- Boundary: the fold authors no op vocabulary the wire does not carry; the scalar fold reads the closed `kind` literal (`upsert`/`delete`/`presence`) and never decodes the `payload` bytes, so the `crdt` column-family `CrdtOpWire` rides untouched to its own owner; every cell keys on the decoded `ContentKey` bytes verbatim; the fold is order-independent so a reconnect replay through `policy#STREAM_POLICY` converges identically; the presence op rides `presence#PRESENCE` as a separate `keyedFold` row, never mutating the live geometry cell.

```ts contract
import { Effect, HashMap, HashSet, Match, Option, Scope, Stream, SubscriptionRef } from "effect";
import type { ConflictOutcomeKind, ConflictReceiptWire, ContentKey, OpLogEntryWire } from "@rasm/interchange";
import { foldStream } from "../fold/combinators";
import type { StreamPolicy } from "../fold/policy";

export const keyHex = (key: ContentKey): string =>
  Array.from(key, (b) => b.toString(16).padStart(2, "0")).join("");

const physicalMs = (entry: OpLogEntryWire): number => {
  const ms = Date.parse(entry.physical);
  return Number.isNaN(ms) ? 0 : ms;
};

const afterHlc = (a: OpLogEntryWire, b: OpLogEntryWire): boolean => {
  const ap = physicalMs(a);
  const bp = physicalMs(b);
  return ap !== bp ? ap > bp : a.logical !== b.logical ? a.logical > b.logical : a.origin > b.origin;
};

export interface ConflictPresenceState {
  readonly live: HashMap.HashMap<string, OpLogEntryWire>;
  readonly tombstones: HashSet.HashSet<string>;
  readonly conflicts: HashMap.HashMap<ConflictOutcomeKind, ReadonlyArray<ConflictReceiptWire>>;
}
export const emptyConflictPresence: ConflictPresenceState = {
  live: HashMap.empty(),
  tombstones: HashSet.empty(),
  conflicts: HashMap.empty(),
};

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

const conflictMerge = (state: ConflictPresenceState, receipt: ConflictReceiptWire): ConflictPresenceState => {
  const prior = HashMap.get(state.conflicts, receipt.outcome).pipe(Option.getOrElse(() => [] as ReadonlyArray<ConflictReceiptWire>));
  return { ...state, conflicts: HashMap.set(state.conflicts, receipt.outcome, [...prior, receipt]) };
};

export const conflictStep = (state: ConflictPresenceState, event: OpLogEntryWire | ConflictReceiptWire): ConflictPresenceState =>
  Match.value(event).pipe(
    Match.when({ outcome: Match.defined }, (receipt) => conflictMerge(state, receipt)),
    Match.orElse((entry) => opMerge(state, entry)),
  );

const conflictPresenceFold = (
  changefeed: Stream.Stream<OpLogEntryWire | ConflictReceiptWire>,
  policy: StreamPolicy,
): Effect.Effect<SubscriptionRef.SubscriptionRef<ConflictPresenceState>, never, Scope.Scope> =>
  foldStream(changefeed, emptyConflictPresence, conflictStep, policy);
```
