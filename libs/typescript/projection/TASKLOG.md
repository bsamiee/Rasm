# [PROJECTION_TASKLOG]

Open and closed work distilled from `IDEAS.md`. Each task is a card whose leader carries `[ID]-[STATUS]:` plus a concise thesis, followed by `Capability`, `Shape`, `Unlocks`, `Anchors`, and optional `Tension` fields. One idea spawns one or more tasks; each task names the exact sub-domain or file it lands in.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
- Atomic: <present only on a minor-scope task; one short phrase naming the small unit so a later session does not overscope its turn>.
-->

[CONVERGENCE_FIXTURE_CORPUS]-[BLOCKED]: the convergence law reads the shared frozen corpus instead of folder-local seed bytes.
- Capability: `convergence/law#CONVERGENCE_LAW` re-founds `opLogEntryArb` and `conflictReceiptArb` on the cross-libs `ONE_WIRE_FIXTURE_CORPUS` CRDT op-set and HLC two-half stamp bytes.
- Shape: the existing `fast-check` arbitrary spine reads the frozen corpus through `constantFrom` seed sets supplied by the branch `typescript:testing/` fixture reader; the `@effect/vitest` law suite and convergence assertions stay unchanged.
- Unlocks: parity drift surfaces as one content-addressed corpus mismatch instead of separate folder-local fixture divergence, preserving the reopened `CONVERGENCE_PROOF_HARNESS` task without widening the law surface.
- Anchors: `convergence/law#CONVERGENCE_LAW`, `libs/.planning#ONE_WIRE_FIXTURE_CORPUS`, the C# wire-authored CRDT op-set, the HLC two-half stamps, `fast-check`, `@effect/vitest`, and the `@bufbuild/buf`-pinned descriptor source.
- Tension: blocked until the branch `typescript:testing/` fixture reader lands; `TESTING_LIB_FOLDER` is not yet a committed big idea, and the decode-admitted op vocabulary stays the floor until that owner exists.

[MERKLE_RECONCILE]-[BLOCKED]: Merkle range digests reconcile the causal stable prefix without a full changefeed re-fold.
- Capability: `causality/vector#MERKLE_RECONCILE` builds a read-side `MerkleRangeWire` digest-comparison handshake over `causality/vector#VERSION_VECTOR` `dominates`.
- Shape: the handshake compares stable-prefix ranges, names the divergent slice with the existing vector algebra, and folds only that slice through `effect` `HashMap`, `SortedMap`, and `Order` structures.
- Unlocks: a reconnecting browser peer reconciles its `CAUSAL_STABILITY_DELIVERY` prefix by set difference, bounding reconnect cost to the divergent range instead of replaying the whole changefeed.
- Anchors: `causality/vector`, `causality/frontier#STABILITY_FRONTIER`, `csharp:Rasm.Persistence/Version/commits#TS_PROJECTION` `MerkleRangeWire`, `libs/typescript/interchange`, and the `MERKLE_ANTI_ENTROPY` idea card.
- Tension: blocked until `MerkleRangeWire` is decode-admitted on the `libs/typescript/interchange` rail; the named vector stub realizes only after the C# wire decode and causal stability prefix are both present.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
