# [PROJECTION_LWW_MERGE]

The strong-eventual-consistency CRDT fold over the decoded sync wire — `opMerge` last-write-wins by HLC over the upsert/delete/presence op-log union, tombstone-guarded, keyed on the imported 16-byte `ContentKey` brand, with the `ConflictOutcomeKind` adjudication-receipt ledger beside it. Every arm is commutative, associative, and idempotent over the `ContentKey` bytes under the HLC order, so two peers folding the same op-set in divergent delivery order reach a byte-identical state. The fold consumes the decoded op-log changefeed projected at `csharp:Rasm.Persistence/sync/collaboration#TS_PROJECTION` and reaches convergence independently; its observable outcome agrees with the C# merge law because both fold the same HLC-ordered op vocabulary, not because the TypeScript fold re-runs a C# routine. The wire carries no CRDT op kind, so the fold authors none. The `ContentKey` is the load-bearing convergence identity, imported from `interchange/artifacts/frame-reassembly#FRAME_RAIL` and never re-minted.

## [1]-[INDEX]

Two clusters:
- `[2]-[LWW_MERGE]` owns `ConflictPresenceState`, `opMerge`, `conflictMerge`, and `conflictPresenceFold`.
- `[3]-[RESEARCH]` carries the open `ConflictOutcomeKind` outcome-bucket member spelling.

## [2]-[LWW_MERGE]

- Owner: `ConflictPresenceState`, the convergent fold state (the live-cell map keyed by `ContentKey`, the tombstone set, the outcome-keyed conflict ledger); `opMerge`, the CmRDT fold body over the closed three-kind wire literal; `conflictMerge`, the adjudication-receipt fold; and `conflictPresenceFold`, the `ConflictPresenceStore` convergence row over the decoded sync changefeed.
- Cases: `keyHex` is the convergence currency this cluster owns and exports — the one `ContentKey`-brand-to-hex transcription string-keying every keyed map in the folder, structural byte equality over the imported brand driving the live-cell map and the idempotent-replay test, composed by `evidence/evidence-correlation#EVIDENCE_CORRELATION` rather than re-declared; `afterHlc` is the `(physical, logical)` lexicographic order the wire carries, the equal pair resolving to the idempotent-replay case content-key equality settles. `opMerge` installs an upsert only when strictly later than the held entry and the key is not tombstoned, tombstones a delete under the same later-than guard (a tombstoned key never re-lives under an equal-or-earlier write), and leaves the live cell untouched on a presence op. `conflictMerge` appends each receipt under its `ConflictOutcomeKind` bucket so the inspector reads `LocalWin`/`RemoteWin`/`Merged`/`Rejected` evidence off the live cell.
- Packages: `effect` for `HashMap`, `HashSet`, `Match`, `Option`, `Stream`, `SubscriptionRef`, `Effect`, and `Scope`; `fast-check` and `@effect/vitest` ground the convergence law harness.
- Growth: a new conflict outcome lands as one `ConflictOutcomeKind` arm breaking the `conflictMerge` lookup; the GraphFork CRDT op vocabulary lands only after the upstream op-log merge-law amendment carries it on the wire.
- Boundary: the fold authors no op vocabulary the wire does not carry; every cell keys on the decoded `ContentKey` bytes verbatim; the fold is order-independent so a reconnect replay through `stream-policy#STREAM_POLICY` converges identically; the presence op rides `presence#PRESENCE` as a separate `keyedFold` row, never mutating the live geometry cell.

```ts contract
import { Effect, HashMap, HashSet, Match, Option, Scope, Stream, SubscriptionRef } from "effect";
import type { ConflictReceiptWire, ContentKey, OpLogEntryWire } from "@rasm/ts";
import { foldStream } from "../fold-core/keyed-fold";
import type { StreamPolicy } from "../fold-core/stream-policy";

export const keyHex = (key: ContentKey): string =>
  Array.from(key, (b) => b.toString(16).padStart(2, "0")).join("");

const afterHlc = (a: OpLogEntryWire, b: { readonly physical: string; readonly logical: bigint }): boolean => {
  const ap = Date.parse(a.physical);
  const bp = Date.parse(b.physical);
  return ap !== bp ? ap > bp : a.logical > b.logical;
};

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

const conflictStep = (state: ConflictPresenceState, event: OpLogEntryWire | ConflictReceiptWire): ConflictPresenceState =>
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

## [3]-[RESEARCH]

- [CONFLICT_OUTCOME_KIND]: the `conflictMerge` outcome bucket is the C# `ConflictOutcome` `[Union]` case name crossed as the wire literal (`LocalWin`/`RemoteWin`/`Merged`/`Rejected`); the exact `ConflictReceiptWire.outcome` member spelling resolves against the `csharp:Rasm.Persistence/sync/collaboration#TS_PROJECTION` decoded shape before the arm is transcribed.
