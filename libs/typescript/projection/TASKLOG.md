# [PROJECTION_TASKLOG]

Open and closed work distilled from `IDEAS.md`. Each task is a card whose leader carries a status marker — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` when open; `[COMPLETE]` or `[DROPPED]` when closed — and three to four bullets: the capability or file to build, the external packages to integrate, the integration points and boundaries or wires, and the key considerations. One idea spawns one or more tasks; each task names the exact sub-domain or file it lands in.

## [01]-[OPEN]

[BLOCKED] Wire the `convergence/law` harness onto the frozen fixture corpus.
- Re-found the `convergence/law#CONVERGENCE_LAW` `opLogEntryArb`/`conflictReceiptArb` arbitraries on the cross-libs `ONE_WIRE_FIXTURE_CORPUS` content-addressed golden bytes (the CRDT op-set, the HLC two-half stamps) read through the new branch `typescript:testing/` fixture reader rather than folder-local hand-minted bytes, so a parity drift surfaces as one corpus mismatch.
- Integrate `fast-check` (the arbitrary spine reads the frozen corpus as a `constantFrom` seed set) and `@effect/vitest`; the corpus derives from the one `@bufbuild/buf`-pinned descriptor source the C# wire authors.
- Internal to `convergence/`, reading the branch `testing/` fixture corpus (the `ONE_WIRE_FIXTURE_CORPUS` named TS consumer); the law unchanged, only the arbitrary seed source. From CONVERGENCE_PROOF_HARNESS re-open.
- The op vocabulary stays decode-admitted; the corpus is the one frozen source rather than per-folder fixtures, the cross-language parity drift caught as a single mismatch.
- Close-condition: the `de-hardcode` task landed the named `CollisionBounds` frozen table on `convergence/law#CONVERGENCE_LAW` (the `[3]-[RESEARCH]` `CORPUS_SEED_BINDING` note records the swap), so the re-founding is a one-line re-pointing of `CollisionBounds` to the fixture reader's `constantFrom` seed once the branch `typescript:testing/` fixture reader exists — confirmed absent today, and `TESTING_LIB_FOLDER` is not a committed big idea. Realizes when the fixture reader lands.

[BLOCKED] Merkle anti-entropy range reconciliation.
- Build a read-side `MerkleRangeWire` digest-comparison handshake over the `causality/vector#VERSION_VECTOR` `dominates` algebra so a reconnecting peer reconciles its `CAUSAL_STABILITY_DELIVERY` stable prefix by set-difference rather than a full changefeed re-fold.
- Integrate `effect` `HashMap`/`SortedMap`/`Order`; the `MerkleRangeWire` shape arrives from `csharp:Rasm.Persistence/Version/commits#TS_PROJECTION`.
- Internal to `causality/`; depends on the `causality/vector` wiring, the `causality/frontier` causal-stability horizon (the reconciliation prefix unit), and the `MerkleRangeWire` decode landing. From MERKLE_ANTI_ENTROPY.
- Bounds reconnect cost to the divergent range; the digest names the slice a peer pulls, never an unbounded re-scan.
- Close-condition: the forward `causality/vector#MERKLE_RECONCILE` stub cluster is named (owner, cases, boundary), but the fence body is blocked-gated on (1) `MerkleRangeWire` arriving decode-admitted on the `libs/typescript/interchange` decode rail (confirmed absent: 0 hits across `interchange/.planning`) and (2) the now-landed `causality/frontier#STABILITY_FRONTIER` stable prefix as the reconciliation unit. Realizes the fence when the C# `Version/commits#TS_PROJECTION` `MerkleRangeWire` decode lands.

## [02]-[CLOSED]

(none)
