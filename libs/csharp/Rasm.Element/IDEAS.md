# [RASM_ELEMENT_IDEAS]

Forward pool of higher-order concepts for the lowest AEC-DOMAIN element seam.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — altitude only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <BLOCKED or gated cards only; the exact observable flipping it actionable — catalog row landing, member query evidence, package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart — cross-folder `pkg` `[SLUG]`, same-folder prerequisite `[SLUG]`; load-bearing build order prefixes follows/precedes/mirrors>.
Capability, Shape, Unlocks, and Anchors are required on every open card; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Ideas state higher-order concepts, never landing-grain tasks.
-->

[QUANTITY_BAG_GROUP_AXIS]-[QUEUED]: Carry the complex-quantity grouping identity on `QuantityBag` — the group axis the IFC `IfcPhysicalComplexQuantity` round-trip needs.
- Capability: a quantity row can belong to a named group with `Discrimination`/`Quality`/`Usage` identity strings, so grouped takeoffs survive the graph identity-lossless, not merely value-lossless under dot-path prefixes.
- Shape: one group-axis carrier on `ValueBag<V>` in `libs/csharp/Rasm.Element/.planning/Properties/property.md` (a group row or per-row group column), threaded through the `quantitySet` canonical-bytes arm in `libs/csharp/Rasm.Element/.planning/Graph/element.md` and the seam `Bake` merge; the `QuantityBagWire` column lands as one append-only numbered field under the `libs/csharp/Rasm.Element/.planning/Graph/wire.md` contract-evolution law.
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

[OBSERVATION_SERIES]-[QUEUED]: A monitored/measured time-series evidence modality beside the computed assessment receipt — the operating-asset record the model keeps past handover.
- Capability: sensor telemetry (temperature, humidity, energy meters, structural-health strain, occupancy) attaches to the elements it observes as typed observation-series evidence, so measured data folds into the same `Bake` read, diffs under the same merge, crosses the same wire, and the commissioning comparison (declared U-value vs metered heat flux, predicted vs metered energy) is a graph query, never an external historian join.
- Shape: one new `Node` case (`Observation`) wrapping a series descriptor — observed `QuantityType` token, sampling cadence and observation `Interval` (NodaTime), sensor provenance, and a content-keyed series blob — attached through the existing `Assign` algebra as one `AssignKind` row, the descriptor's `CanonicalBytes` co-located on the payload per the `AssessmentPayload` discipline; descriptor page lands as `libs/csharp/Rasm.Element/.planning/Assessment/observation.md` beside case/row/arm edits in `libs/csharp/Rasm.Element/.planning/Graph/element.md`, `libs/csharp/Rasm.Element/.planning/Relations/relation.md`, and `libs/csharp/Rasm.Element/.planning/Graph/wire.md`.
- Unlocks: the digital-twin/commissioning lane over the one graph spine; `Rasm.Compute` computed-vs-measured comparison routes reading both evidence kinds off one baked element.
- Anchors: the by-reference heavy-payload pattern is proven twice (`Geospatial/coverage` `RasterKey`, `Assessment/assessment` `ResultBlob`); NodaTime is admitted substrate; `LegalAssign` and `Bake` each grow by one row/arm; a new `NodeWire` oneof arm is additive under the `Graph/wire.md` contract-evolution law, so the wire crossing lands with the node case.

[REDACTION_SCOPED_EGRESS]-[BLOCKED]: Sensitivity-classed wire egress — share the model, withhold the commercial and personal columns.
- Capability: partner-scoped exchange as a first-class egress mode — one model, N lawful projections: unit costs and lifecycle rates (commercial secrets), `OwnerHistory`/`Provenance` authors (GDPR-class personal data), and supplier-confidential EPD references cross only to the peers a policy admits, the redaction typed and auditable instead of a per-deal hand-stripped copy.
- Shape: a sensitivity classification on the known columns (`CostWire`, `OwnerHistoryWire`, `ProvenanceWire`, the EPD evidence rows) with a `WireLimits`-style redaction policy record parameterizing `ElementWire.Encode` — redacted fields unset through the proto3 optional/unset forms, zero wire-schema change — composing the admitted `libs/csharp/.api/api-redaction.md` substrate (`IRedactorProvider`, `DataClassificationSet`); policy record and encode parameterization land in `libs/csharp/Rasm.Element/.planning/Graph/wire.md`.
- Unlocks: lawful federation-partner deliverables and discipline packages off one stored model; the redaction substrate catalog earns its Element consumer.
- Anchors: `Encode` is the one egress fold every crossing takes; proto3 presence semantics already model absence; the `Object` canonical bytes already exclude `OwnerHistory`, so that column redacts identity-inert.
- Tension: blocked on one answerable question — does a redacted crossing preserve source content keys (the peer's `ContentAddress.Verify` re-hash fails on redacted nodes, so the decode gate must expect `AddressUnstable`) or re-derive them over the redacted bytes (forking identity off the source model)? Route: user interview and a parity-corpus design pass over the `Projection/address` `Verify` dual and the `Graph/wire` decode gates; classified columns that FOLD into node ids (`MaterialPropertySet.CaseBytes` writes the EPD `PropertyEvidence`) are the cases the ruling must cover.

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

[READER_ROW_CUSTODY]-[QUEUED]: Single-owner key space for the seam property bags — reader rows enter by owner provision, never call-site mints.
- Capability: every bag row name a writer stamps or a reader keys resolves to an owner-declared static or an owner-blessed reader-local category, so a spelling fork between non-referencing packages becomes uncompilable.
- Shape: a reader-provision band on `libs/csharp/Rasm.Element/.planning/Properties/property.md` — the structural wire-name statics declared, the reader-local category blessed, and the associated-material-grade boundary restated to distinguish the element-own EXPRESS token it admits.
- Unlocks: `Rasm.Bim` ingest rows and `Rasm.Compute` analysis reads key one vocabulary; the `SteelGrade` writer/owner contradiction dissolves at its root.
- Anchors: `libs/csharp/.planning/RULINGS.md` seam-bag custody row; the `DetailSchema` accumulating `Of` admission law; `PropertyName` owner statics.
- Ripple: `[DETAIL_SCHEMA_READER_PROVISION]` decomposes this.

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[ELEMENT_HOOK_RAIL]-[COMPLETE]: `Projection/observe#HOOK_RAIL` lands `HookPoint`/`ElementFact`/`ElementHookRail`/`ElementTap` — the minted point-roster composition over the kernel point capsule, veto fold first, subscriber faults parking as `IsolatedFault` rows read through `TapFaults`.
[GRAPH_INSTRUMENT_PROJECTION]-[COMPLETE]: `Projection/observe#INSTRUMENT_PROJECTION` lands `ElementInstruments.Rows` as kernel `InstrumentRow` declarations and the `GraphInstrument` `Mount`/`AsTap`/`Traced` capsule minted through the kernel identity entry off the injected `IMeterFactory`, beside the contributor-port mint.
[DELTA_EVENT_ENVELOPE]-[COMPLETE]: `Graph/wire#EVENT_ENVELOPE` lands `GraphEventType` and the CloudEvents-aligned `GraphEventEnvelope` with content-key subject dedup and the W3C trace slots.
[SYNTHETIC_GRAPH_FORGE]-[COMPLETE]: `Graph/corpus` lands `CorpusProfile`/`GraphForge` seeded deterministic generation and the graded `CorpusGrade`/`CorpusOp`/`CorpusGate` roster.
[UNIT_SCHEME_BIM_COUNTERPART]-[COMPLETE]: `Graph/wire` `HeaderWire.unit_scheme = 7` carries the `Header.Units` scheme with the Mapper transcription both ways; both Bim ends compose it.
[CONNECTION_INTERFACE_GEOMETRY_DECODE]-[COMPLETE]: seam half whole — `Connect.Interface` rides `CanonicalBytes` and the wire and decodes through the one `GeometrySource.ResolveFootprint` leg; the Bim lowering and egress ride the `Rasm.Bim` counterpart card.
