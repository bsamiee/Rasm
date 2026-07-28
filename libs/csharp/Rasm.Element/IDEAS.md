# [RASM_ELEMENT_IDEAS]

Forward pool of higher-order concepts for the lowest AEC-DOMAIN element seam.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — concept grain only, never a page path, row list, or member spelling>.
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
- Arms: arm when the federated-header ruling settles whether `Federate` takes one caller-supplied coordination `Header` or carries a per-source `Header` roster.
- Tension: a caller-supplied coordination `Header` demotes every source header to provenance and mints one semantic identity for the union, while a per-source roster keeps each source's own `Header.CanonicalBytes`; mixed `Tolerance` across sources is the deciding case either way, because tolerance-quantized measure bytes fork content keys the union then carries under one header.

[REDACTION_SCOPED_EGRESS]-[BLOCKED]: Sensitivity-classed wire egress — share the model, withhold the commercial and personal columns.
- Capability: partner-scoped exchange as a first-class egress mode — one model, N lawful projections: unit costs and lifecycle rates (commercial secrets), `OwnerHistory`/`Provenance` authors (GDPR-class personal data), and supplier-confidential EPD references cross only to the peers a policy admits, the redaction typed and auditable instead of a per-deal hand-stripped copy.
- Shape: a sensitivity classification on the known columns (`CostWire`, `OwnerHistoryWire`, `ProvenanceWire`, the EPD evidence rows) with a `WireLimits`-style redaction policy record parameterizing `ElementWire.Encode` — redacted fields unset through the proto3 optional/unset forms, zero wire-schema change — composing the admitted `libs/csharp/.api/api-redaction.md` substrate (`IRedactorProvider`, `DataClassificationSet`); policy record and encode parameterization land in `libs/csharp/Rasm.Element/.planning/Graph/wire.md`.
- Unlocks: lawful federation-partner deliverables and discipline packages off one stored model; the redaction substrate catalog earns its Element consumer.
- Anchors: `Encode` is the one egress fold every crossing takes; proto3 presence semantics already model absence; the `Object` canonical bytes already exclude `OwnerHistory`, so that column redacts identity-inert.
- Arms: arm when the redacted-crossing identity ruling settles whether content keys carry from the source or re-derive over the redacted bytes.
- Tension: preserving source content keys fails the peer's `Projection/address` `Verify` re-hash on every redacted node, so the `Graph/wire` decode gate must expect `AddressUnstable`; re-deriving over the redacted bytes forks identity off the source model instead. Classified columns that FOLD into node ids — `MaterialPropertySet.CaseBytes` writing the EPD `PropertyEvidence` — are the cases either arm must cover.

[MODEL_COMPLETENESS_AUDIT]-[QUEUED]: Graph-wide completeness and integrity audit — the model-maturity receipt beside the per-projection `AssemblyReceipt`.
- Capability: one audit fold grades a frozen graph — coverage ratios (occurrence share carrying classification, material, quantity, and property bags per discipline), integrity sweeps (dangling `RepresentationContentHash` keys, unresolved `ProfileRef`s, orphan non-rooted nodes, `Stale`/`Superseded` assessments), and the `ContentAddress.Verify` drift census — into a typed `ModelAudit` receipt whose findings reuse the `ConstraintSeverity` grades.
- Shape: audit fold and receipt in `libs/csharp/Rasm.Element/.planning/Projection/audit.md` (new page); every ratio a fold over the `ObjectNodes`/incidence read family, never a second graph.
- Unlocks: model-health dashboards, delivery gates (completeness thresholds per milestone), and the quality feed the instrument projection publishes.
- Anchors: `ObjectNodes`/`MaterialsOf`/`PropertiesOf` accessors; `AssessmentOutcome` behavior columns; `Verify(ElementGraph)` accumulating sweep; `ConstraintSeverity`.

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[READER_ROW_CUSTODY]-[COMPLETE]: the key space closes over owner provision at `Properties/property#DETAIL_SCHEMA` — `StructuralRows` holds every name two packages key on and `PropertyCategory.Row` partitions each producer's own vocabulary, so `Rasm.Bim` writes and `Rasm.Compute` reads one spelling while `Rasm.Fabrication`'s derivation rows mint under their own blessed scope; `PropertyName` stays an open key for an ingested foreign `Pset` name, which is exactly why authored rows need custody rather than a closed vocabulary.
[ANALYTIC_TABLE_PROJECTION]-[COMPLETE]: `Graph/table.md` lands `TableRow` as a `[Union]` whose six cases ARE the six datasets, each owning its `Cells` projection so a column declaration and its payload move in one edit, with `TableFamily` rostering each `element.<source>` dataset and `GraphTable.Tabulate(graph, key, roots)` the one fold; the page composes the `Rasm.Persistence` `[WIRE]: AnalyticsSchema` custodian rather than minting a second columnar vocabulary, and every family declares its own trailing spine — `at` on `element.assessments`, `observed` elsewhere off the threaded projection frame.
[OBSERVATION_SERIES]-[COMPLETE]: `Assessment/observation.md` lands `ObservationSeries` with its sampling and grade vocabularies, by-reference `ObservationChunk` run, and the derived `SeriesStatistics` plane carrying zero authority; identity keys the STREAM and excludes the chunk run, so a live append mutates the node in place instead of re-keying it per batch, and series are occurrence-only — the one `Seq` the named type inheritance deliberately skips.
[ELEMENT_HOOK_RAIL]-[COMPLETE]: `Projection/observe#HOOK_RAIL` lands `ElementPoint`/`ElementFact`/`ElementHookRail`/`ElementTap` — the minted point-roster composition over the kernel point capsule, veto fold first, each decoration bracketed in the kernel `SpanBand` under the point's own derived `TraceScope`, subscriber faults parking as `IsolatedFault` rows read through `TapFaults`.
[GRAPH_INSTRUMENT_PROJECTION]-[COMPLETE]: `Projection/observe#INSTRUMENT_PROJECTION` lands `ElementInstruments.Rows` as kernel `InstrumentSpec` declarations under the kernel `TelemetrySource.Element` scope over one dotted `rasm.element.<dimension>` slot block both planes spell, every write folding the kernel `TenantContext` partition onto the kernel `InstrumentSet.Write` rail, and `GraphInstrument.Tap` projecting each fact over the composing root's `InstrumentSet` beside the contributor-port mint.
[DELTA_EVENT_ENVELOPE]-[COMPLETE]: `Graph/wire#EVENT_ENVELOPE` lands `GraphEventType` and the CloudEvents-aligned `GraphEventEnvelope` with content-key subject dedup and the W3C trace slots.
[SYNTHETIC_GRAPH_FORGE]-[COMPLETE]: `Graph/corpus` lands `CorpusProfile`/`GraphForge` seeded deterministic generation and the graded `CorpusGrade`/`CorpusOp`/`CorpusGate` roster.
[UNIT_SCHEME_BIM_COUNTERPART]-[COMPLETE]: `Graph/wire` `HeaderWire.unit_scheme = 7` carries the `Header.Units` scheme with the Mapper transcription both ways; both Bim ends compose it.
[CONNECTION_INTERFACE_GEOMETRY_DECODE]-[COMPLETE]: seam half whole — `Connect.Interface` rides `CanonicalBytes` and the wire and decodes through the one `GeometrySource.ResolveFootprint` leg; the Bim lowering and egress ride the `Rasm.Bim` counterpart card.
