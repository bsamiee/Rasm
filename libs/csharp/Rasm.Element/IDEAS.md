# [RASM_ELEMENT_IDEAS]

Forward pool of higher-order concepts for the lowest AEC-DOMAIN element seam.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
-->

[QUANTITY_BAG_GROUP_AXIS]-[QUEUED]: Carry the complex-quantity grouping identity on `QuantityBag` — the group axis the IFC `IfcPhysicalComplexQuantity` round-trip needs.
- Capability: a quantity row can belong to a named group with `Discrimination`/`Quality`/`Usage` identity strings, so grouped takeoffs survive the graph identity-lossless, not merely value-lossless under dot-path prefixes.
- Shape: one group-axis carrier on `ValueBag<V>` in `libs/csharp/Rasm.Element/.planning/Properties/property.md` (a group row or per-row group column), threaded through the `quantitySet` canonical-bytes arm in `libs/csharp/Rasm.Element/.planning/Graph/element.md` and the seam `Bake` merge; the `QuantityBagWire` column lands at the wire unfreeze in `libs/csharp/Rasm.Element/.planning/Graph/wire.md`.
- Unlocks: the Bim projector stamps grouping identity at ingest and the egress rebuilds nested complex quantities; QTO consumers select by group.
- Anchors: `Rasm.Bim` `Projection/semantic` `FlattenQuantities` already recurses value-lossless and names this as its one residual row; the bag's 4-column ValueBag shape admits an additive axis.
- Tension: the column ripples the counted-bag canonical-bytes injectivity law, the frozen wire, and the `Bake` merge — a seam-owner design addition, never a consumer-side patch.
- Ripple: `Rasm.Bim` `[QUANTITY_BAG_GROUP_AXIS]` (the ingest/egress ends).

[FEDERATION_AND_PARTIAL_EXCHANGE]-[BLOCKED]: `Federate(models)` disjoint-id union and `Extract(roots)` Members-closed reachable-subgraph extraction on `ElementGraph`.
- Capability: multi-model federation and partial-model exchange as first-class graph operations — a coordination model unions discipline models without id collision, and a scoped deliverable extracts a closed subgraph.
- Shape: `Federate` the disjoint-id union over per-source graphs; `Extract` the reachable closure over `Members`-completeness so no edge dangles out of the slice.
- Unlocks: federated coordination review and partial IFC deliverables over the one graph spine; both operations land in `libs/csharp/Rasm.Element/.planning/Graph/element.md`.
- Anchors: `ElementGraph` frozen snapshot + incidence index; `NodeId` regime keeps rooted ids globally unique (Guid-v7 / content-hash).
- Tension: blocked on one answerable question — does `Federate` take a caller-supplied coordination `Header` (per-source headers demoted to provenance) or carry a per-source `Header` roster on the federated graph? Route: user interview over the `Header.CanonicalBytes` semantic identity, with the mixed-`Tolerance` case pinned first (tolerance-quantized measure bytes fork content keys across sources).

[OBSERVATION_SERIES]-[BLOCKED]: A monitored/measured time-series evidence modality beside the computed assessment receipt — the operating-asset record the model keeps past handover.
- Capability: sensor telemetry (temperature, humidity, energy meters, structural-health strain, occupancy) attaches to the elements it observes as typed observation-series evidence, so measured data folds into the same `Bake` read, diffs under the same merge, crosses the same wire, and the commissioning comparison (declared U-value vs metered heat flux, predicted vs metered energy) is a graph query, never an external historian join.
- Shape: one new `Node` case (`Observation`) wrapping a series descriptor — observed `QuantityType` token, sampling cadence and observation `Interval` (NodaTime), sensor provenance, and a content-keyed series blob — attached through the existing `Assign` algebra as one `AssignKind` row, the descriptor's `CanonicalBytes` co-located on the payload per the `AssessmentPayload` discipline; descriptor page lands as `libs/csharp/Rasm.Element/.planning/Assessment/observation.md` beside case/row/arm edits in `libs/csharp/Rasm.Element/.planning/Graph/element.md`, `libs/csharp/Rasm.Element/.planning/Relations/relation.md`, and `libs/csharp/Rasm.Element/.planning/Graph/wire.md`.
- Unlocks: the digital-twin/commissioning lane over the one graph spine; `Rasm.Compute` computed-vs-measured comparison routes reading both evidence kinds off one baked element.
- Anchors: the by-reference heavy-payload pattern is proven twice (`Geospatial/coverage` `RasterKey`, `Assessment/assessment` `ResultBlob`); NodaTime is admitted substrate; `LegalAssign` and `Bake` each grow by one row/arm.
- Tension: blocked on one answerable question — which campaign event unfreezes the `rasm.element.v1` `NodeWire` oneof for new arms? Route: the `Graph/wire.md` descriptor-gate law and the wire-freeze ruling that already queues the `MaterialWire` and `ObjectType` column adds; a seam-only landing before that event strands the node at every crossing.

[REDACTION_SCOPED_EGRESS]-[BLOCKED]: Sensitivity-classed wire egress — share the model, withhold the commercial and personal columns.
- Capability: partner-scoped exchange as a first-class egress mode — one model, N lawful projections: unit costs and lifecycle rates (commercial secrets), `OwnerHistory`/`Provenance` authors (GDPR-class personal data), and supplier-confidential EPD references cross only to the peers a policy admits, the redaction typed and auditable instead of a per-deal hand-stripped copy.
- Shape: a sensitivity classification on the known columns (`CostWire`, `OwnerHistoryWire`, `ProvenanceWire`, the EPD evidence rows) with a `WireLimits`-style redaction policy record parameterizing `ElementWire.Encode` — redacted fields unset through the proto3 optional/unset forms, zero wire-schema change — composing the admitted `libs/csharp/.api/api-redaction.md` substrate (`IRedactorProvider`, `DataClassificationSet`); policy record and encode parameterization land in `libs/csharp/Rasm.Element/.planning/Graph/wire.md`.
- Unlocks: lawful federation-partner deliverables and discipline packages off one stored model; the redaction substrate catalog earns its Element consumer.
- Anchors: `Encode` is the one egress fold every crossing takes; proto3 presence semantics already model absence; the `Object` canonical bytes already exclude `OwnerHistory`, so that column redacts identity-inert.
- Tension: blocked on one answerable question — does a redacted crossing preserve source content keys (the peer's `ContentAddress.Verify` re-hash fails on redacted nodes, so the decode gate must expect `AddressUnstable`) or re-derive them over the redacted bytes (forking identity off the source model)? Route: user interview and a parity-corpus design pass over the `Projection/address` `Verify` dual and the `Graph/wire` decode gates; classified columns that FOLD into node ids (`MaterialPropertySet.CaseBytes` writes the EPD `PropertyEvidence`) are the cases the ruling must cover.

[ELEMENT_HOOK_RAIL]-[QUEUED]: Typed graph-fact tap — one `rasm.element.<domain>.<point>` hook registry every seam lifecycle fact flows through, telemetry-as-tap and app-neutral.
- Capability: domain facts (delta applied, snapshot frozen, bake computed, projection assembled, constraint finding graded, wire decoded) publish as typed fact records through one registry with observe/veto modalities and subscriber-fault isolation onto the `ElementFault` rail, so observability subscribes to facts and no emit-call scatters into graph code.
- Shape: one registry value the app composition root mints (the `ProjectionSuite.Of` minting precedent, never a process-global), point rows keyed `rasm.element.<domain>.<point>`, each fact a typed record carrying the `Op` key, the graph or delta `ContentAddress`, and the point payload; lands in `libs/csharp/Rasm.Element/.planning/Projection/observe.md` (new page beside `projection.md`).
- Unlocks: the instrument projection, replay/audit consumers, and AppUi live-model listeners all ride one tap; two apps composing the seam never fight over hook points.
- Anchors: estate hook-rail law (veto/observe/replay, fault isolation, telemetry-as-tap); `ProjectionSuite` registration shape; `ElementFault` band 2500; kernel `Op`.

[GRAPH_INSTRUMENT_PROJECTION]-[QUEUED]: Meter and ActivitySource projection off the fact tap — `rasm.element.*` instruments and spans with zero OTel reference at seam altitude.
- Capability: graph operations surface as instruments — `Bake` duration and memo-hit histograms, delta magnitude counters off `GraphDelta` `NodeCount`/`EdgeCount`, wire encode/decode size and duration, constraint findings by `ConstraintSeverity`, assessment outcomes by `Discipline`/`AnalysisRoute` — and `ActivitySource` spans for `Bake`/`Assemble`/`DecodeGraph`, dimensions bounded to the seam's closed vocabularies so cost attribution and span-profile correlation join downstream without cardinality blowups.
- Shape: one projection subscribing the `[ELEMENT_HOOK_RAIL]` tap, meters minted through an injected `IMeterFactory` (per-app and per-ALC neutral), UCUM `rasm.element.<domain>.<measure>` names per the estate wire law; lands in `libs/csharp/Rasm.Element/.planning/Projection/observe.md`.
- Unlocks: model-health dashboards, span-profile correlation at the app tier, cost attribution by discipline/route/model key, and the AppHost `InstrumentFan` Element arm.
- Anchors: `libs/csharp/.api/api-diagnostics-metrics.md` (`IMeterFactory`, `Meter.CreateHistogram`, `Histogram<T>`); estate altitude law (BCL Meter/ActivitySource at library altitude, SDK at the composition root); `AssessmentOutcome`/`ConstraintSeverity`/`Discipline` bounded vocabularies.

[DELTA_EVENT_ENVELOPE]-[QUEUED]: Transport-neutral event envelope for every graph crossing — one seam-declared attribute vocabulary any broker lane carries.
- Capability: a `GraphDeltaWire`/`ElementGraphWire` crossing carries a typed envelope — closed event-type token (`rasm.element.graphdelta.v1` / `rasm.element.graph.v1`), source identity, subject = the delta or snapshot `ContentAddress`, occurrence `Instant`, and a W3C traceparent/tracestate slot — CloudEvents-aligned, so Kafka/NATS/MQTT/CloudEvents lanes at the peer tier publish deltas without re-inventing metadata and a streaming consumer folds `WriteDelimitedTo` frames with per-event identity and content-key dedup.
- Shape: one envelope record and the closed event-type token vocabulary beside `WireLimits` in `libs/csharp/Rasm.Element/.planning/Graph/wire.md`; the traceparent slot is a data field an app-tier propagator fills — the seam references no OTel.
- Unlocks: Persistence outbox/broker publication, cross-runtime delta streaming with dedup by envelope subject, trace continuity across every graph crossing.
- Anchors: `GraphDelta.ToCanonicalBytes` order-independent content key (the dedup subject); CloudEvents distributed-tracing extension law on the estate transport axis; `NodaTime.Serialization.Protobuf` instant crossing.

[ANALYTIC_TABLE_PROJECTION]-[QUEUED]: Columnar table projection of the baked model — the SQL/analytics egress every QTO, cost, and dashboard consumer reads without re-folding the graph.
- Capability: one flatten fold projects a frozen `ElementGraph` into typed row families — element rows (id, kind, classification, predefined type, type binding), property/quantity rows (set name, property name, SI magnitude, `QuantityType` token, dimension exponents), material/edge rows, assessment rows (discipline, route, outcome, elapsed) — every row carrying the snapshot `ContentAddress` so an analytic result pins its exact model version.
- Shape: row records and the one `Tabulate(graph)` fold in `libs/csharp/Rasm.Element/.planning/Graph/table.md` (new page); columnar landing (Parquet/DuckDB) and Flight SQL serving ride the Persistence counterpart.
- Unlocks: QTO and cost dashboards over plain SQL, cross-model regression diffing in DuckDB, the lake/Flight egress axis for the AEC model.
- Anchors: `Bake` fold and the flat `Element` record (the row source); `MeasureValue.Si` base-normalized scalars; ParquetSharp.Dataset and Apache.Arrow.Flight.Sql admitted this campaign at the Persistence tier.

[MODEL_COMPLETENESS_AUDIT]-[QUEUED]: Graph-wide completeness and integrity audit — the model-maturity receipt beside the per-projection `AssemblyReceipt`.
- Capability: one audit fold grades a frozen graph — coverage ratios (occurrence share carrying classification, material, quantity, and property bags per discipline), integrity sweeps (dangling `RepresentationContentHash` keys, unresolved `ProfileRef`s, orphan non-rooted nodes, `Stale`/`Superseded` assessments), and the `ContentAddress.Verify` drift census — into a typed `ModelAudit` receipt whose findings reuse the `ConstraintSeverity` grades.
- Shape: audit fold and receipt in `libs/csharp/Rasm.Element/.planning/Projection/audit.md` (new page); every ratio a fold over the `ObjectNodes`/incidence read family, never a second graph.
- Unlocks: model-health dashboards, delivery gates (completeness thresholds per milestone), and the quality feed the instrument projection publishes.
- Anchors: `ObjectNodes`/`MaterialsOf`/`PropertiesOf` accessors; `AssessmentOutcome` behavior columns; `Verify(ElementGraph)` accumulating sweep; `ConstraintSeverity`.

[SYNTHETIC_GRAPH_FORGE]-[QUEUED]: Deterministic parameterized model forge — one seeded generator family feeding benchmarks, property specs, and the cross-runtime parity corpus.
- Capability: seeded, size-graded synthetic `ElementGraph` generation (node count, edge density, bag width, discipline mix, composition depth as parameters) with deterministic rooted ids and content keys, so a benchmark, a CsCheck property, and a parity vector all pull the same model at any scale and a regression pins its exact input by seed and parameters.
- Shape: generator parameterization and the graded corpus roster (size rows with seeds and expected snapshot `ContentAddress`) in `libs/csharp/Rasm.Element/.planning/Graph/corpus.md` (new page); BenchmarkDotNet corpus-gate rows over `Bake`/`Freeze`/`ToCanonicalBytes`/`Encode`/`DecodeGraph` and the parity vectors consume it from the tests estate.
- Unlocks: the benchmarking axis for the graph kernel hot paths, byte-for-byte cross-runtime parity anchors for the python/TS decoders, load-graded profiling scenarios.
- Anchors: `ElementGraph.Of`/`Genesis` mint entries; deterministic id minting through a seeded Guid stream in place of `NodeId.Rooted`'s random Guid-v7; estate BenchmarkReceipt family and corpus gate.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[UNIT_SCHEME_BIM_COUNTERPART]-[COMPLETE]: `Graph/wire` `HeaderWire.unit_scheme = 7` carries the `Header.Units` scheme with the Mapper transcription both ways; both Bim ends compose it.

[CONNECTION_INTERFACE_GEOMETRY_DECODE]-[COMPLETE]: seam half whole — `Connect.Interface` rides `CanonicalBytes` and the wire and decodes through the one `GeometrySource.ResolveFootprint` leg; the Bim lowering and egress ride the `Rasm.Bim` counterpart card.
