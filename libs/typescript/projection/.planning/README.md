# [PROJECTION_PLANNING]

`projection` owns the unified key-discriminated transport-free fold algebra — every store folds a wire-vocabulary discriminant verbatim over a `Stream` or receipt sequence into an immutable `SubscriptionRef`-backed keyed map, parameterized by ONE `StreamPolicy`. Zero consumers exist; implementation is full-capability with no holding back; pages are transcribed, not re-designed. The domain collapses the three prior state pages into one fold-algebra owner because envelope folds and availability are identical in kind to the stream folds — co-located by altitude, not payload. It depends ONLY on `interchange`'s decoded `Schema` shapes, dials nothing, and the `@connectrpc/*` import ban in the monorepo's centralized config (nx module boundaries + root eslint) is the sole mechanical guard keeping the fold interior transport-free now that both neutral domains sit in one tag stratum.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE]                   | [OWNS]                                                                                     | [STATE]     |
| :-----: | :----------------------- | :----------------------------------------------------------------------------------------- | :---------- |
|   [1]   | fold-algebra.md          | the one StreamPolicy + the key-discriminated fold combinator + the five stream stores      | provisional |
|   [2]   | envelope-and-evidence.md | ReceiptStore/EvidenceFeed/AvailabilityStore + the envelope carrier + the SkewBand HLC fold | provisional |

## [2]-[WIRE_PAGES]

The domain authors no shape; it folds the decoded `interchange` vocabulary verbatim. Each page transcribes the consuming-side fold over a C# `#TS_PROJECTION` shape.

- fold-algebra.md: AppHost lifecycle-and-drain (RuntimeFeed), health-and-degradation (HealthStore), Persistence snapshot-codecs (SnapshotFeed) + sync-collaboration (ConflictPresenceStore), Compute progress-and-observation (ProgressStore).
- envelope-and-evidence.md: Compute receipts-and-benchmarks (ReceiptStore), AppUi diagnostics-evidence (EvidenceFeed + SkewBandWire), AppUi commands-availability (AvailabilityStore), AppHost runtime-ports (the envelope carrier).

## [3]-[CATALOGUE_PENDING]

None. The domain depends only on `effect` (Stream/SubscriptionRef/Schedule/Schema) and the `interchange` decoded shapes; `@effect-atom/atom` is the cell bridge consumed at the `ui` boundary, not a `projection` dependency.

## [4]-[GAP_LEDGER]

| [INDEX] | [GAP]                                                             | [CLOSED_BY (page#cluster)]                                                                          |
| :-----: | :---------------------------------------------------------------- | :-------------------------------------------------------------------------------------------------- |
|   [1]   | three parallel state pages for one fold algebra                   | fold-algebra#FOLD_ALGEBRA + envelope-and-evidence#ENVELOPE_AND_EVIDENCE (one combinator, fold rows) |
|   [2]   | an improvised reconnect loop or unbounded retry per fold          | fold-algebra#FOLD_ALGEBRA (one StreamPolicy)                                                        |
|   [3]   | the SPA recomputing the HLC skew fold to render clock-uncertainty | envelope-and-evidence#ENVELOPE_AND_EVIDENCE (SkewBand confidence-interval projection output)        |
|   [4]   | a transport dial leaking into the fold interior                   | the @connectrpc/* import ban in the centralized config (RULE_ENFORCEMENT)                            |

## [5]-[DENSITY_BAR]

| [INDEX] | [AXIS/CONCERN]               | [OWNER]                                                                            | [KIND]                 | [CASES]                                              | [STATE]   |
| :-----: | :--------------------------- | :--------------------------------------------------------------------------------- | :--------------------- | :--------------------------------------------------- | :-------- |
|   [1]   | reconnect + operator policy  | `StreamPolicy`                                                                     | one Schedule + ops     | reconnect/buffer/throttle/groupedWithin/scan         | FINALIZED |
|   [2]   | the fold combinator          | `keyedFold`                                                                        | polymorphic combinator | Stream/receipt sequence -> SubscriptionRef keyed map | FINALIZED |
|   [3]   | the five stream stores       | `RuntimeFeed`/`HealthStore`/`SnapshotFeed`/`ProgressStore`/`ConflictPresenceStore` | fold rows              | one fold per boundary concept                        | FINALIZED |
|   [3a]  | standing-query window fold   | `WindowKind`/`WindowSpec`/`Watermark`/`bucketSet`/`windowFold`                     | TaggedEnum + IVM arm   | Tumbling/Sliding(slide-stride)/Session + late retract | FINALIZED |
|   [3b]  | LWW convergent fold          | `ConflictPresenceState`/`opMerge`/`conflictMerge`/`conflictPresenceFold`           | wire-union + LWW arm   | upsert/delete/presence + ConflictOutcomeKind ledger   | FINALIZED |
|   [4]   | envelope carrier             | `ReceiptEnvelopeCarrier`                                                           | Schema factory         | binds every structured-text payload                  | FINALIZED |
|   [5]   | receipt + evidence folds     | `ReceiptStore`/`EvidenceFeed`                                                      | fold rows              | compute-receipt union, HLC-ordered evidence          | FINALIZED |
|   [6]   | availability read gate       | `AvailabilityStore`                                                                | fold row               | commands-availability rows, read by the gateway      | FINALIZED |
|   [7]   | clock-uncertainty projection | `SkewBand`                                                                         | derived fold output    | SkewBandWire -> confidence interval                  | FINALIZED |

## [6]-[BUILD_ORDER]

| [INDEX] | [FILE]                       | [TRANSCRIBES]                               | [GATE]                                    |
| :-----: | :--------------------------- | :------------------------------------------ | :---------------------------------------- |
|   [1]   | src/fold-algebra.ts          | fold-algebra#FOLD_ALGEBRA                   | tsgo --noEmit clean                       |
|   [2]   | src/envelope-and-evidence.ts | envelope-and-evidence#ENVELOPE_AND_EVIDENCE | tsgo + unit-pbt fold laws                 |
|   [3]   | src/index.ts                 | the single neutral "." export               | exports resolve; @connectrpc/* ban passes |

## [7]-[PROOF_GATES]

| [GATE]          | [COMMAND]                   | [EVIDENCE]                                       |
| :-------------- | :-------------------------- | :----------------------------------------------- |
| catalog resolve | `pnpm install`              | catalogMode strict resolves @rasm/ts             |
| typecheck       | tsgo `--noEmit`             | zero diagnostics                                 |
| import ban      | centralized config (nx + root eslint) | zero `@connectrpc/*` imports under the domain    |
| unit-pbt        | vitest project `projection` | fold associativity/idempotence/monotonicity pass |

## [8]-[PROHIBITIONS]

No transport dial and no `@connectrpc/*` import anywhere under the domain; no re-validation of a value an earlier decode admitted; no improvised reconnect loop beside `StreamPolicy`; no parallel view-state library over domain data; no second store per payload where a fold row suffices; no sixth app-service beside the five budgeted owners; no comment carrying task or process narration.

## [9]-[ADMISSIONS_RECORD]

| [INDEX] | [PACKAGE]         | [PAGE]     | [CATALOGUE] | [STATUS] |
| :-----: | :---------------- | :--------- | :---------- | :------- |
|   [1]   | effect            | both pages | api-effect  | admitted |
|   [2]   | interchange       | both pages | (intra-pkg) | admitted |

## [10]-[REFINEMENT_HORIZON]

The standing-query window vocabulary is landed as the `WindowKind` `Data.TaggedEnum` (tumbling/sliding/session) plus the `windowFold` signed-delta IVM arm folding the decoded changefeed by event-time bucket against `query-rail#STANDING_QUERY` — `bucketSet` reads `Sliding.slide` to emit the overlapping window set (stride by slide, span by size) and `windowMerge` folds the late row's signed delta into its bucket cell with the out-of-order count tracked on the watermark; the residual SPIKE is the live-changefeed confirmation of both behaviors against a running op-log replay. The convergent fold is landed as `conflictPresenceFold` (last-write-wins by HLC `(physical, logical)` with content-key idempotent-replay and tombstone guard, keyed on the imported 16-byte `ContentKey` brand verbatim, plus the `ConflictOutcomeKind` receipt ledger) over the decoded `OpLogEntryWire` upsert/delete/presence union and the `ConflictReceiptWire` outcomes against `sync-collaboration#MERGE_LAW` + `#TS_PROJECTION`, mirroring the upstream `Adjudicate` total dispatch and authoring no op vocabulary the wire does not carry; the residual SPIKE is the cross-peer strong-eventual-consistency harness proving two divergent-delivery folds reach a byte-identical state against the live adjudication. Closed by the bar: a new wire payload is a fold row, never a new store; a new window kind is one `Data.TaggedEnum` variant and a new conflict outcome is one `ConflictOutcomeKind` arm breaking its `$match`; and the algebra dials nothing.
