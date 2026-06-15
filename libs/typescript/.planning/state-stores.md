# [TYPESCRIPT_STATE_STORES]

One page owns the conversion of the boundary's per-frame outputs into durable, immutable, keyed application state. It holds the key-discriminated folds, each collapsing a server-stream or a receipt sequence into a keyed immutable map with the ordering the wire declares and the staleness markers decode tolerance surfaces. The discriminant that drives every fold is the C# discriminant consumed verbatim, so the state layer is a projection of the wire vocabulary, never a parallel model. The page consumes clusters 1, 2, 4, 5, 6, 8, 9, 10, and 11 as folds; it owns no rendering and no transport.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]                  | [OWNS]                                                      |
| :-----: | :------------------------- | :---------------------------------------------------------- |
|   [1]   | STREAM_FOLDS               | the live-cell folds over server-streams                     |
|   [2]   | RECEIPT_AND_ENVELOPE_FOLDS | the receipt fold, the evidence fold, the envelope carrier   |
|   [3]   | STALENESS_AND_AVAILABILITY | staleness-forward posture and the command-availability gate |

## [2]-[STREAM_FOLDS]

- Owner: the live-cell folds over server-streams — `RuntimeFeed` for the lifecycle fold (surfaced through the host), `HealthStore` for the health and degradation fold, `SnapshotFeed` for the snapshot fold, `ProgressStore` for the progress cells, and `ConflictPresenceStore` for the conflicts and presence fold.
- Cases: `RuntimeFeed` folds phases, drain receipts, boot markers, and fault records keyed by phase against `lifecycle-and-drain.md#TS_PROJECTION`; `HealthStore` folds the health snapshot and degradation level and gates which view surfaces are reachable against `health-and-degradation.md#TS_PROJECTION`; `SnapshotFeed` folds decoded binary deltas and catalog rows against `snapshot-codecs.md#TS_PROJECTION`; `ProgressStore` holds monotonic marks against `progress-and-observation.md#TS_PROJECTION` where a mark below the held rank never regresses the cell; `ConflictPresenceStore` folds the sync-segment outcomes and presence rows against `sync-collaboration.md#TS_PROJECTION`, presence expiring on its declared expiry field.
- Entry: each fold is one owner over its event kinds into an immutable keyed map; key-discrimination is driven by the exact discriminant keys the wire shapes carry; the runtime and snapshot folds ride the `RuntimeFeed` and `SnapshotFeed` app-services, never a parallel store beside the service.
- Packages: the effect core for streams, subscription references, and structured concurrency; the atom layer as the bridge exposing a fold as a subscribable cell.
- Growth: a new boundary concept lands as one fold owner; a new event kind lands as one fold arm on its owning fold.
- Boundary: no fold re-validates a value an earlier decode gate admitted; ordering is borrowed verbatim from the wire — monotonic progress, last-write-wins or monotonic merge per contract.

## [3]-[RECEIPT_AND_ENVELOPE_FOLDS]

- Owner: `ReceiptStore` for the compute-receipt fold, `EvidenceFeed` for the evidence fold, and the envelope carrier that binds the payload type for every structured-text receipt.
- Cases: `ReceiptStore` folds the compute-receipt union against `receipts-and-benchmarks.md#TS_PROJECTION`; `EvidenceFeed` orders rows by the HLC stamp with skew bands against `diagnostics-evidence.md#TS_PROJECTION`, decoding embedded geometry through the geometry rail the wire-contracts page owns; the envelope carrier resolves the package marker, the HLC-stamp shape, and the receipt-envelope carrier against `runtime-ports.md#TS_PROJECTION` and feeds both the runtime and evidence folds.
- Entry: every structured-text receipt payload binds as the envelope payload type with the envelope discriminant mirroring the payload discriminant; the evidence timeline carries envelopes whole; each payload decodes against its owning package contract; cluster-11 geometry rides inside the envelope payload rather than on the evidence row shape.
- Packages: the effect core for the fold and ordering primitives; the atom layer as the cell bridge.
- Growth: a new receipt payload lands as one payload row bound through the envelope, zero new carrier; a new evidence kind lands as one fold arm.
- Boundary: the envelope shape is never re-aggregated branch-side; the `runtime-ports.md#WIRE_LAW` anchor is read for the HLC-stamp ordering discipline only, never for token grounding; the HLC logical counter resets on every physical advance, so it never approaches the JSON number envelope.

## [4]-[STALENESS_AND_AVAILABILITY]

- Owner: the staleness-forward posture across every fold plus `AvailabilityStore`, the command-availability fold.
- Cases: a disconnected fold renders its last value with a staleness marker and resumes on redial; stream interruption folds to a typed retry policy value; `AvailabilityStore` folds availability rows against `commands-availability.md#TS_PROJECTION` and gates the write edge so a disabled command never fires; the degradation level key is an input to availability, never re-derived.
- Entry: each fold carries staleness markers forward rather than dropping them; the retry policy is a domain value, not an improvised loop.
- Packages: the effect core for the retry-policy primitive and the subscription reference.
- Growth: a new gating input lands as one availability-row field; a new retry posture lands as one policy value.
- Boundary: no fold holds a value the boundary did not produce; availability gating is the only write-edge precondition and is owned here, consumed by the control-edge page.
