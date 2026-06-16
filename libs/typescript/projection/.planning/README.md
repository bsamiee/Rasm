# [PROJECTION_PLANNING]

`@rasm/projection` owns the unified key-discriminated transport-free fold algebra — every store folds a wire-vocabulary discriminant verbatim over a `Stream` or receipt sequence into an immutable `SubscriptionRef`-backed keyed map, parameterized by ONE `StreamPolicy`. Zero consumers exist; implementation is full-capability with no holding back; pages are transcribed, not re-designed. The domain collapses the three prior state pages into one fold-algebra owner because envelope folds and availability are identical in kind to the stream folds — co-located by altitude, not payload. It depends ONLY on `@rasm/interchange`'s decoded `Schema` shapes, dials nothing, and the `no-restricted-imports` ban on `@connectrpc/*` is the sole mechanical guard keeping the fold interior transport-free now that both neutral domains sit in one tag stratum.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE]                     | [OWNS]                                                                       | [STATE]     |
| :-----: | :------------------------- | :-------------------------------------------------------------------------- | :---------- |
|   [1]   | fold-algebra.md            | the one StreamPolicy + the key-discriminated fold combinator + the five stream stores | provisional |
|   [2]   | envelope-and-evidence.md   | ReceiptStore/EvidenceFeed/AvailabilityStore + the envelope carrier + the SkewBand HLC fold | provisional |

## [2]-[WIRE_PAGES]

The domain authors no shape; it folds the decoded `@rasm/interchange` vocabulary verbatim. Each page transcribes the consuming-side fold over a C# `#TS_PROJECTION` shape.

- fold-algebra.md: AppHost lifecycle-and-drain (RuntimeFeed), health-and-degradation (HealthStore), Persistence snapshot-codecs (SnapshotFeed) + sync-collaboration (ConflictPresenceStore), Compute progress-and-observation (ProgressStore).
- envelope-and-evidence.md: Compute receipts-and-benchmarks (ReceiptStore), AppUi diagnostics-evidence (EvidenceFeed + SkewBandWire), AppUi commands-availability (AvailabilityStore), AppHost runtime-ports (the envelope carrier).

## [3]-[CATALOGUE_PENDING]

None. The domain depends only on `effect` (Stream/SubscriptionRef/Schedule/Schema) and the `@rasm/interchange` decoded shapes; `@effect-atom/atom` is the cell bridge consumed at the `@rasm/web` boundary, not a `@rasm/projection` dependency.

## [4]-[GAP_LEDGER]

| [INDEX] | [GAP]                                                              | [CLOSED_BY (page#cluster)]                       |
| :-----: | :---------------------------------------------------------------- | :----------------------------------------------- |
|   [1]   | three parallel state pages for one fold algebra                   | fold-algebra#FOLD_ALGEBRA + envelope-and-evidence#ENVELOPE_AND_EVIDENCE (one combinator, fold rows) |
|   [2]   | an improvised reconnect loop or unbounded retry per fold          | fold-algebra#FOLD_ALGEBRA (one StreamPolicy)     |
|   [3]   | the SPA recomputing the HLC skew fold to render clock-uncertainty | envelope-and-evidence#ENVELOPE_AND_EVIDENCE (SkewBand confidence-interval projection output) |
|   [4]   | a transport dial leaking into the fold interior                   | the @connectrpc/* no-restricted-imports ban (RULE_ENFORCEMENT) |

## [5]-[DENSITY_BAR]

| [INDEX] | [AXIS/CONCERN]              | [OWNER]                  | [KIND]                | [CASES]                                              | [STATE]    |
| :-----: | :-------------------------- | :----------------------- | :-------------------- | :--------------------------------------------------- | :--------- |
|   [1]   | reconnect + operator policy | `StreamPolicy`           | one Schedule + ops    | reconnect/buffer/throttle/groupedWithin/scan          | FINALIZED  |
|   [2]   | the fold combinator         | `keyedFold`              | polymorphic combinator | Stream/receipt sequence -> SubscriptionRef keyed map  | FINALIZED  |
|   [3]   | the five stream stores      | `RuntimeFeed`/`HealthStore`/`SnapshotFeed`/`ProgressStore`/`ConflictPresenceStore` | fold rows | one fold per boundary concept | FINALIZED  |
|   [4]   | envelope carrier            | `ReceiptEnvelopeCarrier` | Schema factory         | binds every structured-text payload                  | FINALIZED  |
|   [5]   | receipt + evidence folds    | `ReceiptStore`/`EvidenceFeed` | fold rows         | compute-receipt union, HLC-ordered evidence          | FINALIZED  |
|   [6]   | availability read gate      | `AvailabilityStore`      | fold row               | commands-availability rows, read by the gateway       | FINALIZED  |
|   [7]   | clock-uncertainty projection | `SkewBand`              | derived fold output    | SkewBandWire -> confidence interval                   | FINALIZED  |

## [6]-[BUILD_ORDER]

| [INDEX] | [FILE]                    | [TRANSCRIBES]                                  | [GATE]                       |
| :-----: | :------------------------ | :--------------------------------------------- | :--------------------------- |
|   [1]   | src/fold-algebra.ts       | fold-algebra#FOLD_ALGEBRA                      | tsgo --noEmit clean          |
|   [2]   | src/envelope-and-evidence.ts | envelope-and-evidence#ENVELOPE_AND_EVIDENCE | tsgo + unit-pbt fold laws    |
|   [3]   | src/index.ts              | the single neutral "." export                  | exports resolve; @connectrpc/* ban passes |

## [7]-[PROOF_GATES]

| [GATE]          | [COMMAND]                  | [EVIDENCE]                                        |
| :-------------- | :------------------------- | :------------------------------------------------ |
| catalog resolve | `pnpm install`             | catalogMode strict resolves @rasm/projection      |
| typecheck       | tsgo `--noEmit`            | zero diagnostics                                  |
| import ban      | eslint flat-config         | zero `@connectrpc/*` imports under src            |
| unit-pbt        | vitest project `projection` | fold associativity/idempotence/monotonicity pass |

## [8]-[PROHIBITIONS]

No transport dial and no `@connectrpc/*` import anywhere under the domain; no re-validation of a value an earlier decode admitted; no improvised reconnect loop beside `StreamPolicy`; no parallel view-state library over domain data; no second store per payload where a fold row suffices; no sixth app-service beside the five budgeted owners; no comment carrying task or process narration.

## [9]-[ADMISSIONS_RECORD]

| [INDEX] | [PACKAGE]            | [PAGE]            | [CATALOGUE] | [STATUS]   |
| :-----: | :------------------- | :---------------- | :---------- | :--------- |
|   [1]   | effect               | both pages        | api-effect  | admitted   |
|   [2]   | @rasm/interchange    | both pages        | (workspace) | admitted   |

## [10]-[REFINEMENT_HORIZON]

The next deepening drives the standing-query window vocabulary — tumbling, sliding, and session windows plus watermarks over the fold algebra — landed as `StreamPolicy` operator rows and one `keyedFold` window arm, gated on the C# `query-rail#STANDING_QUERY` window vocabulary and recorded as a TS-downstream DAG seam. The GraphFork CRDT fold row waits on the C# `sync-collaboration#MERGE_LAW` op-log amendment before `ConflictPresenceStore` folds a CRDT op vocabulary. Closed by the bar: a new wire payload is a fold row, never a new store, and the algebra dials nothing.
