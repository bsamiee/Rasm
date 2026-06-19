# [PROJECTION_CONVERGENCE_LAW]

The algebraic-law harness that elevates the strong-eventual-consistency claim from prose to mutation-killable evidence — `convergenceLaw` generates one op-set and a divergent delivery permutation, folds both orders through the `lww-merge#LWW_MERGE` `conflictStep` reduction, and asserts a byte-identical `ConflictPresenceState` under the `Equal` trait, encoding the commutativity, associativity, and idempotence of `opMerge` as the external oracle the whole sync surface rests on. The harness is the gate any future CRDT op kind passes before admission: a `crdt` column-family arm or a GraphFork op kind that breaks delivery-order independence fails the permutation law before it reaches a peer. The op-set arbitrary mints only the decode-admitted `OpLogEntryWire`/`ConflictReceiptWire` vocabulary the wire carries; the harness asserts the fold's own algebra and re-runs no C# routine.

## [1]-[INDEX]

One cluster: `[2]-[CONVERGENCE_LAW]` owns the `opLogEntryArb`/`conflictReceiptArb` arbitraries, the `permutedPairArb` delivery-order arbitrary, and the `convergenceLaw` commutativity/idempotence law suite.

## [2]-[CONVERGENCE_LAW]

- Owner: `opLogEntryArb`, the `Arbitrary<OpLogEntryWire>` minting the closed three-kind op with a bounded `contentKey` byte set, an HLC `(physical, logical)` pair, and an `origin` from a bounded peer set so the key space collides and the LWW order is exercised; `conflictReceiptArb`, the `Arbitrary<ConflictReceiptWire>` over the four `ConflictOutcomeKind` literals; `permutedPairArb`, the `opSetArb.chain` arbitrary that draws the op-set and a `fc.shuffledSubarray` full-length permutation of it as one paired value so the permutation is a generated input that shrinks and replays under the run seed, never an in-predicate `fc.sample` whose un-seeded draw makes a counterexample irreproducible; and `convergenceLaw`, the `it.prop` suite folding both orders through `conflictStep` and asserting `Equal.equals` plus the idempotent-replay and tombstone-monotonicity corollaries.
- Cases: the commutativity-plus-associativity law folds the op-set in source order and in `permute` order and asserts byte-identical `ConflictPresenceState` — the live map, the tombstone set, and the outcome ledger all `Equal`; the idempotence law folds the op-set, then folds the same set appended to itself, and asserts the doubled delivery equals the single delivery (a replayed op never moves the cell); the tombstone-monotonicity corollary asserts a key tombstoned by any delivery order stays tombstoned under every order so a late upsert never resurrects it. The bounded `contentKey` and `origin` sets force key and stamp collisions so the equal-stamp `origin` tiebreak and the tombstone guard are exercised rather than trivially distinct.
- Packages: `fast-check` for `Arbitrary`, `record`, `constantFrom`, `bigInt`, `date`, `array`, `oneof`, `chain`, and `shuffledSubarray`; `@effect/vitest` for `it.prop` binding the property spine to the pure `conflictStep` reduction; `@stryker-mutator/core` gating the kill ratio over the `conflictStep` arms so a mutated merge guard fails the law.
- Growth: a new CRDT op kind lands as one arbitrary arm and passes the same permutation law before admission; a new convergence corollary lands as one assertion in the existing suite, never a parallel harness.
- Boundary: the arbitrary mints only decode-admitted wire vocabulary — a generated op carrying a field the wire does not is the deleted form; the law asserts the `conflictStep` algebra directly so it is plumbing-free and isolates the merge law from the `Stream`/`SubscriptionRef` scaffold; the `Equal.equals` assertion reads the `HashMap`/`HashSet` `Equal` trait so byte-identity is structural, never a hand-rolled deep compare; the harness is internal to `convergence/` and the op vocabulary arrives settled from the wire.

```ts contract
import { Equal, HashSet } from "effect";
import { it } from "@effect/vitest";
import * as fc from "fast-check";
import type { ConflictReceiptWire, OpLogEntryWire } from "@rasm/interchange";
import { conflictStep, emptyConflictPresence, type ConflictPresenceState } from "./lww-merge";

const ORIGINS = ["peer-a", "peer-b", "peer-c"] as const;
const KEY_BYTES = [0, 1, 2, 3] as const;

const FIXED_OP = {
  entityKind: "entity", entityKey: "key", columnFamily: "default", codec: "none",
  payload: new Uint8Array(), image: new Uint8Array(), actor: "peer-a", closure: [] as Uint8Array[],
} as const;

const opLogEntryArb: fc.Arbitrary<OpLogEntryWire> = fc.record({
  kind: fc.constantFrom("upsert", "delete", "presence"),
  contentKey: fc.constantFrom(...KEY_BYTES).map((b) => new Uint8Array(16).fill(b)),
  sequence: fc.bigInt({ min: 0n, max: 64n }),
  physical: fc.date({ min: new Date("2024-01-01T00:00:00.000Z"), max: new Date("2024-01-01T00:00:01.000Z") }).map((d) => d.toISOString()),
  logical: fc.bigInt({ min: 0n, max: 8n }),
  origin: fc.constantFrom(...ORIGINS),
}).map((stamp) => ({ ...FIXED_OP, ...stamp }));

const conflictReceiptArb: fc.Arbitrary<ConflictReceiptWire> = fc.constantFrom("LocalWin", "RemoteWin", "Merged", "Rejected").map((outcome) => ({
  outcome,
  entityKind: "entity",
  entityKey: "key",
  columnFamily: "default",
  heldPhysical: "2024-01-01T00:00:00.000Z",
  heldLogical: 0n,
  heldActor: "peer-a",
  incomingPhysical: "2024-01-01T00:00:00.000Z",
  incomingLogical: 0n,
  incomingActor: "peer-b",
  correlation: "corr",
  at: "2024-01-01T00:00:00.000Z",
}));

const opSetArb = fc.array(fc.oneof(opLogEntryArb, conflictReceiptArb), { minLength: 1, maxLength: 32 });
const permutedPairArb = opSetArb.chain((ops) =>
  fc
    .shuffledSubarray(Array.from({ length: ops.length }, (_, i) => i), { minLength: ops.length, maxLength: ops.length })
    .map((order) => [ops, order.map((i) => ops[i])] as const),
);

const foldOrder = (ops: ReadonlyArray<OpLogEntryWire | ConflictReceiptWire>): ConflictPresenceState =>
  ops.reduce(conflictStep, emptyConflictPresence);

const stateEqual = (a: ConflictPresenceState, b: ConflictPresenceState): boolean =>
  Equal.equals(a.live, b.live) && Equal.equals(a.tombstones, b.tombstones) && Equal.equals(a.conflicts, b.conflicts);

const convergenceLaw = () => {
  it.prop("delivery-order independence", [permutedPairArb], ([[ops, permuted]]) =>
    stateEqual(foldOrder(ops), foldOrder(permuted)),
  );
  it.prop("idempotent replay", [opSetArb], ([ops]) => stateEqual(foldOrder(ops), foldOrder([...ops, ...ops])));
  it.prop("tombstone monotonicity", [opSetArb], ([ops]) => {
    const tombstoned = HashSet.size(foldOrder(ops).tombstones);
    return HashSet.size(foldOrder([...ops, ...ops]).tombstones) >= tombstoned;
  });
};
```

## [3]-[RESEARCH]

- [CRDT_LAW_ADMISSION]: [RESOLVED] — the `crdt` column-family op carries the `CrdtOpWire` join-semilattice algebra on the `OpLogEntryWire.payload` slot, decoded by `causality-graph/version-vector#CRDT_SEMILATTICE` off the `interchange/codecs/decode-rail#CRDT_OP_DECODE` ten-arm union; the convergence law that gates a `CrdtOpWire` arm asserts the same delivery-order independence over the version-vector `vectorJoin` least-upper-bound rather than the scalar LWW state.
