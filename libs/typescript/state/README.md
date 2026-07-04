# [STATE]

`state` is the W1 host-free fold algebra of the TypeScript branch — the one owner of keyed folds, CRDT merge, causality, evidence vocabulary, and live queries, importing `kernel` only and running identically in browser, node, and bun. The authoritative thing is ONE algebra at two altitudes: browser apps fold wire-decoded events in memory; node apps fold journal events durably through the `store/project` binding of the SAME algebra — never a second fold implementation per runtime. The algebra is generic over the op vocabulary: the C#-minted wire op-log family and app-authored journal event families are instances, never forks. `state` never imports `wire`: wire decodes INTO state vocabulary, so the algebra stays transport-free and browser-safe by construction. It owns the keyed fold and replay law with the d2ts incremental-dataflow lane (`fold/`), the CRDT merge semantics and convergence laws (`crdt/`), the version-vector/commit/branch shapes, Merkle comparison, happened-before folds over the kernel honest-uncertainty windows, the causal delivery buffer, the stability frontier (GLB meet), causal finalize, and the retention-frontier handoff to `store/journal` (`causal/`), the typed evidence vocabulary — the `ReceiptEnvelope`-decoded receipt family never erased into one shape, the `DegradationLevel`/`CommandAvailability` vocabulary the wire gateway gate types against, the progress-mark folds, and the evidence feed/timeline folds (`evidence/`), and the `Subscribable` live queries with the presence semantics `edge/live` serves plus the windowed folds carrying the REPLAY_LAW spine, AsOf three-coordinate time-travel reads, `asOfDiff`, and HLC event-time watermarks (`query/`). The domain map and seam record live in `ARCHITECTURE.md`, the forward concept pool in `IDEAS.md`, and the work log in `TASKLOG.md`.

## [1]-[ROUTER]

- [01]-[ALGEBRA](.planning/fold/algebra.md): the keyed fold owner — one algebra, two altitudes (browser in-memory, store durable), generic over the op vocabulary.
- [02]-[REPLAY](.planning/fold/replay.md): the replay law and the d2ts incremental-dataflow lane — a fold rebuilt from any prefix converges to the live fold; an array re-sort fallback is the named defect.
- [03]-[MERGE](.planning/crdt/merge.md): the CRDT op merge semantics — one merge algebra generic over the op vocabulary; the C#-minted wire family and app-authored journal families are instances.
- [04]-[CONVERGE](.planning/crdt/converge.md): the convergence laws — commutativity, associativity, idempotence — with fixture hooks into the `tests/contracts` corpus (TS readers in `tests/typescript/_testkit`).
- [05]-[VECTOR](.planning/causal/vector.md): version vectors, commit/branch shapes, and Merkle comparison.
- [06]-[ORDER](.planning/causal/order.md): happened-before folds over the kernel honest-uncertainty windows, the causal delivery buffer, the stability frontier (GLB meet), causal finalize, and the retention-frontier handoff to `store/journal`.
- [07]-[RECEIPT](.planning/evidence/receipt.md): the `ReceiptEnvelope`-decoded evidence vocabulary — the typed receipt family, never erased.
- [08]-[AVAILABILITY](.planning/evidence/availability.md): the `DegradationLevel`/`CommandAvailability` vocabulary the wire gateway gate types against.
- [09]-[PROGRESS](.planning/evidence/progress.md): the progress-mark evidence folds.
- [10]-[TIMELINE](.planning/evidence/timeline.md): the evidence feed/timeline folds plus the content-keyed, producer-opaque `Document` result-artifact reference with its column band.
- [11]-[LIVE](.planning/query/live.md): the `Subscribable` live queries and the presence semantics `edge/live` serves.
- [12]-[WINDOW](.planning/query/window.md): the windowed query folds and the REPLAY_LAW spine — AsOf three-coordinate time-travel reads, `asOfDiff`, and HLC event-time watermarks.

## [2]-[DOMAIN_PACKAGES]

The folder-local packages, catalogued at `.api/`; versions live only in the `pnpm-workspace.yaml` catalog.

[INCREMENTAL_DATAFLOW]:
- `@electric-sql/d2ts` — the differential-dataflow runtime behind the `fold/replay` incremental lane: versioned frontiers, signed multisets, and the operator library that maintains a fold incrementally instead of re-folding the prefix.
- `@electric-sql/d2mini` — the minimal differential core; the same incremental lane at its smallest surface for the in-memory fold paths.

[MERGE_LAW]:
- `@effect/typeclass` — the Semigroup/semilattice instance vocabulary the `crdt/merge` algebra and the `@rasm/ts-testkit` law combinators (`tests/typescript/_testkit`) share; a merge is a lawful semilattice instance, never an ad hoc function.

## [3]-[SUBSTRATE_PACKAGES]

The branch substrate this folder consumes; the registry is `libs/typescript/.planning/README.md` `[02]-[SUBSTRATE_PACKAGES]`, the catalogue `libs/typescript/.api/`.

- `effect` — rails, `Schema`, `Layer`, `Match`, `Stream`, `Subscribable`; the whole algebra is expressed in core `effect` with zero platform binding.
