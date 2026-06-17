# [PROJECTION_ARCHITECTURE]

`projection` is the fold algebra as one folder: one `StreamPolicy` parameterizes every fold, one `keyedFold` combinator dispatches every store, and every cross-folder fact is a decoded `interchange` shape folded into a `SubscriptionRef`-backed keyed map. Mechanics live in the finalized `.planning/` pages; this page is the atlas — the source tree and build order, the owner registry (the one owner-state surface), dependency direction, cross-folder seams, boundaries, and prohibitions.

## [1]-[SOURCE_TREE]

The flat module layout IS the build order: the fold combinator and policy precede the envelope folds that compose them. Each leaf is one transcription unit annotated with the owners it transcribes and the owning page#cluster.

```text codemap
projection/
├── fold-algebra.ts                 # StreamPolicy, keyedFold, the five stream stores, WindowKind/windowFold, ConflictPresenceState/conflictPresenceFold — fold-algebra#FOLD_ALGEBRA
├── envelope-and-evidence.ts        # ReceiptEnvelopeCarrier, ReceiptStore, EvidenceFeed, AvailabilityStore, SkewBand — envelope-and-evidence#ENVELOPE_AND_EVIDENCE
└── index.ts                        # the single neutral "." export
```

`fold-algebra.ts` lands first: `StreamPolicy` and `keyedFold` are the vocabulary the envelope folds compose, and the five stream stores read the policy directly. `envelope-and-evidence.ts` lands after because `ReceiptStore`/`EvidenceFeed`/`AvailabilityStore` are fold rows over the same combinator and the `ReceiptEnvelopeCarrier` binds the structured-text payload the stream stores already decode. `index.ts` exports the one neutral subpath; the transport-dial ban passes because the folder imports only `effect` and the decoded shapes.

## [2]-[OWNER_REGISTRY]

The single owner-state surface for the folder. A new wire payload is a fold row, never a new store; a new window kind is one `Data.TaggedEnum` variant; a new conflict outcome is one `ConflictOutcomeKind` arm. `[STATE]` is `FINALIZED` where the owner is a transcription-complete fence with no open gate, `SPIKE` where the owner is fence-complete but its proof carries a residual probe named in the page RESEARCH cluster. This is the ONLY place owner state lives.

| [INDEX] | [AXIS/RAIL]                 | [OWNER]                                                                            | [KIND]                 | [CASES]                                              | [PAGE#CLUSTER]                          |  [STATE]  |
| :-----: | :-------------------------- | :-------------------------------------------------------------------------------- | :--------------------- | :-------------------------------------------------- | :-------------------------------------- | :-------: |
|   [1]   | reconnect + operator policy | `StreamPolicy`                                                                     | one Schedule + ops     | reconnect/buffer/throttle/groupedWithin/scan         | fold-algebra#FOLD_ALGEBRA               | FINALIZED |
|   [2]   | the fold combinator         | `keyedFold`                                                                        | polymorphic combinator | Stream/receipt sequence to SubscriptionRef keyed map | fold-algebra#FOLD_ALGEBRA               | FINALIZED |
|   [3]   | the five stream stores      | `RuntimeFeed`/`HealthStore`/`SnapshotFeed`/`ProgressStore`/`ConflictPresenceStore` | fold rows              | one fold per boundary concept                        | fold-algebra#FOLD_ALGEBRA               | FINALIZED |
|   [4]   | standing-query window fold  | `WindowKind`/`WindowSpec`/`Watermark`/`bucketSet`/`windowFold`                     | TaggedEnum + IVM arm   | Tumbling/Sliding(slide-stride)/Session + late retract | fold-algebra#FOLD_ALGEBRA               | SPIKE     |
|   [5]   | LWW convergent fold         | `ConflictPresenceState`/`opMerge`/`conflictMerge`/`conflictPresenceFold`           | wire-union + LWW arm   | upsert/delete/presence + ConflictOutcomeKind ledger   | fold-algebra#FOLD_ALGEBRA               | SPIKE     |
|   [6]   | envelope carrier            | `ReceiptEnvelopeCarrier`                                                           | Schema factory         | binds every structured-text payload                  | envelope-and-evidence#ENVELOPE_AND_EVIDENCE | FINALIZED |
|   [7]   | receipt + evidence folds    | `ReceiptStore`/`EvidenceFeed`                                                      | fold rows              | compute-receipt union, HLC-ordered evidence          | envelope-and-evidence#ENVELOPE_AND_EVIDENCE | FINALIZED |
|   [8]   | availability read gate      | `AvailabilityStore`                                                               | fold row               | commands-availability rows, read by the gateway      | envelope-and-evidence#ENVELOPE_AND_EVIDENCE | FINALIZED |
|   [9]   | clock-uncertainty projection | `SkewBand`                                                                        | derived fold output    | SkewBandWire to confidence interval                  | envelope-and-evidence#ENVELOPE_AND_EVIDENCE | FINALIZED |

The standing-query window fold is SPIKE pending the live-changefeed confirmation of tumbling/sliding/session behavior against a running op-log replay; the LWW convergent fold is SPIKE pending the cross-peer strong-eventual-consistency harness proving two divergent-delivery folds reach a byte-identical state. Both are fully shaped now, not deferred surfaces.

## [3]-[DEPENDENCY_DIRECTION]

| [INDEX] | [FOLDER]      | [MAY_REFERENCE_PROJECTION] | [PROJECTION_MAY_REFERENCE] | [BOUNDARY]                                |
| :-----: | :------------ | :------------------------: | :------------------------: | :---------------------------------------- |
|   [1]   | `interchange` |             no             |            yes             | folds the decoded `Schema` shapes verbatim |
|   [2]   | `ui`          |            yes             |             no             | render leaves subscribe through the stores |
|   [3]   | `platform`    |            yes             |             no             | the runtime feed is read through projection |
|   [4]   | `services`    |            yes             |             no             | node consumers fold the same stores         |

`projection` depends only on `interchange` and `effect`; it dials nothing and imports no transport surface. Consumers subscribe to the `SubscriptionRef`-backed stores; the fold interior stays transport-free under the centralized import ban.

## [4]-[SEAMS]

Every two-folder fact splits by altitude: mechanics live at the named `projection` cluster, consequences land at the consumer. Intra-TypeScript seams ride `pkg/page#CLUSTER`; the wire contracts the folds consume route through the Tier-0 seam ledger.

| [INDEX] | [SEAM]              | [MECHANICS_AT]                                | [CONSEQUENCE_AT]                                                       |
| :-----: | :------------------ | :-------------------------------------------- | :-------------------------------------------------------------------- |
|   [1]   | decoded vocabulary  | interchange/codec-rails#CODEC_RAILS            | fold-algebra#FOLD_ALGEBRA folds the decoded shapes verbatim            |
|   [2]   | envelope carrier    | interchange/gateway-and-quarantine#CONTRACT_INVENTORY | envelope-and-evidence#ENVELOPE_AND_EVIDENCE binds the carrier      |
|   [3]   | availability gate   | envelope-and-evidence#ENVELOPE_AND_EVIDENCE    | interchange/gateway-and-quarantine#GATEWAY_AND_QUARANTINE reads the gate |
|   [4]   | runtime feed        | fold-algebra#FOLD_ALGEBRA                       | platform/host-runtime#HOST_RUNTIME reads the `RuntimeFeed`             |
|   [5]   | evidence + receipt  | envelope-and-evidence#ENVELOPE_AND_EVIDENCE    | ui/render-surfaces#RENDER_SURFACES reads `EvidenceFeed`/`ReceiptStore` |
|   [6]   | wire contract source | the Tier-0 seam ledger                         | each fold transcribes the consuming side of the upstream fence         |

## [5]-[BOUNDARIES]

- `projection` is not a transport package, a view package, or a domain-mutation package; it owns the read-side fold algebra and nothing else.
- The domain re-validates nothing an earlier decode admitted; it folds the decoded vocabulary as settled.
- No transport dial enters the fold interior; the centralized import ban is the sole mechanical guard.
- Every boundary concept is one fold row on one combinator parameterized by one `StreamPolicy`; the algebra dials nothing.

## [6]-[PROHIBITIONS]

The closed NEVER list — the deleted patterns the owner registry forecloses.

- NEVER a transport dial or transport-dial import anywhere under the domain.
- NEVER a re-validation of a value an earlier decode admitted.
- NEVER an improvised reconnect loop beside `StreamPolicy`.
- NEVER a parallel view-state library over domain data.
- NEVER a second store per payload where a fold row suffices.
- NEVER a sixth app-service beside the budgeted owners.
- NEVER a comment carrying task or process narration.
