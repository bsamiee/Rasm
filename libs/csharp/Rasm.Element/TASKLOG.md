# [RASM_ELEMENT_TASKLOG]

Open and closed work distilled from `IDEAS.md`. `[01]-[OPEN]` carries task cards with `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` leaders; `[02]-[CLOSED]` carries `[COMPLETE]` or `[DROPPED]` cards. One idea spawns one or more tasks; each task names the exact sub-domain or file it lands in.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
- Atomic: <present only on a minor-scope task; one short phrase naming the small unit so a later session does not overscope its turn>.
-->

[QUANTITY_GROUP_COLUMN]-[QUEUED]: Land the `QuantityBag` group axis on the bag shape and the canonical bytes — the seam half of `[QUANTITY_BAG_GROUP_AXIS]`.
- Capability: grouped takeoffs cross the graph identity-lossless per the owning idea.
- Shape: one group column on `ValueBag<V>` in `libs/csharp/Rasm.Element/.planning/Properties/property.md`, presence-delimited count-prefixed write in the `quantitySet` arm of `Node.ToCanonicalBytes` in `libs/csharp/Rasm.Element/.planning/Graph/element.md`; `Merge` precedence untouched, wire column queued for the unfreeze.
- Unlocks: Bim ingest/egress ends compose the axis the moment the seam carries it.
- Anchors: counted-bag injectivity law (`Projection/address` count-prefix); `Bake` bag merge.
- Atomic: one column and one canonical-bytes arm.

[FEDERATION_HEADER_RULING]-[BLOCKED]: Resolve the federation header-reconciliation ruling that gates `[FEDERATION_AND_PARTIAL_EXCHANGE]`.
- Capability: an answered ruling turns `Federate`/`Extract` from a bet into a design landing in `libs/csharp/Rasm.Element/.planning/Graph/element.md`.
- Shape: question — caller-supplied coordination `Header` with per-source headers demoted to provenance, or a per-source `Header` roster on the federated graph? Route: user interview over the `Header.CanonicalBytes` semantic identity, mixed-`Tolerance` divergence pinned first.
- Unlocks: federated coordination review and `Members`-closed partial exchange.
- Anchors: `Header.CanonicalBytes`; tolerance-quantized measure bytes; `GeoReference` divergence cases.

[OBSERVATION_PAGE]-[BLOCKED]: Author the observation-series design for `[OBSERVATION_SERIES]` at the wire unfreeze.
- Capability: measured time-series evidence folds into the one `Bake` read beside computed assessments.
- Shape: `libs/csharp/Rasm.Element/.planning/Assessment/observation.md` (new page) carrying the series descriptor; one `Node` case and `AssignKind` row and `LegalAssign`/`Bake` arms in `libs/csharp/Rasm.Element/.planning/Graph/element.md` and `libs/csharp/Rasm.Element/.planning/Relations/relation.md`; one `NodeWire` oneof arm in `libs/csharp/Rasm.Element/.planning/Graph/wire.md`.
- Unlocks: the commissioning comparison lane and the `Rasm.Compute` computed-vs-measured routes.
- Anchors: `RasterKey`/`ResultBlob` by-reference precedent; NodaTime `Interval`; question and route stated on the owning idea — the `NodeWire` unfreeze event.

[REDACTION_IDENTITY_RULING]-[BLOCKED]: Resolve redacted-crossing identity preservation for `[REDACTION_SCOPED_EGRESS]`.
- Capability: the ruling fixes whether redacted crossings preserve or re-derive content keys, and with it the decode-side `AddressUnstable` posture.
- Shape: question and route stated on the owning idea; on resolution, pin the `WireLimits`-sibling redaction policy record and the `Encode` parameterization in `libs/csharp/Rasm.Element/.planning/Graph/wire.md` and the parity-corpus vectors for redacted nodes.
- Unlocks: partner-scoped deliverables off one stored model.
- Anchors: `IRedactorProvider`/`DataClassificationSet` in `libs/csharp/.api/api-redaction.md`; `ContentAddress.Verify` dual.

[HOOK_POINT_ROSTER]-[QUEUED]: Pin the fact-point roster and registry shape for `[ELEMENT_HOOK_RAIL]`.
- Capability: every seam lifecycle fact has one named point row with a typed payload and a declared modality.
- Shape: point rows (`rasm.element.graph.delta-applied`, `rasm.element.graph.frozen`, `rasm.element.graph.baked`, `rasm.element.projection.assembled`, `rasm.element.projection.finding`, `rasm.element.wire.decoded`) with payload types and observe/veto modality per row, registry mint and subscriber-fault isolation in `libs/csharp/Rasm.Element/.planning/Projection/observe.md`.
- Unlocks: the instrument projection subscribes; AppUi/replay consumers follow.
- Anchors: `ProjectionSuite.Of` mint shape; `ElementFault` isolation rail.

[INSTRUMENT_ROW_TABLE]-[QUEUED]: Pin the instrument rows and span names for `[GRAPH_INSTRUMENT_PROJECTION]`.
- Capability: the `rasm.element.*` meter/span surface is a closed table, never ad-hoc emit calls.
- Shape: instrument table (name, kind, UCUM unit, bounded dimensions) and `ActivitySource` span names for `Bake`/`Assemble`/`DecodeGraph`, `IMeterFactory` injection seam, in `libs/csharp/Rasm.Element/.planning/Projection/observe.md`.
- Unlocks: the AppHost `InstrumentFan` arm registers exactly this table.
- Anchors: `libs/csharp/.api/api-diagnostics-metrics.md` members; `Discipline`/`AnalysisRoute`/`ConstraintSeverity` vocabularies.
- Atomic: one table and one injection seam.

[ENVELOPE_VOCAB]-[QUEUED]: Land the envelope record and event-type tokens for `[DELTA_EVENT_ENVELOPE]`.
- Capability: every graph crossing carries typed event identity a broker lane forwards unchanged.
- Shape: envelope record (type token, source, subject `ContentAddress`, `Instant`, traceparent/tracestate slots) and the closed event-type vocabulary beside `WireLimits` in `libs/csharp/Rasm.Element/.planning/Graph/wire.md`.
- Unlocks: Persistence outbox publication and streaming dedup by subject.
- Anchors: `GraphDelta.ToCanonicalBytes`; CloudEvents attribute alignment.
- Atomic: one record and one token vocabulary.

[TABLE_ROW_SCHEMA]-[QUEUED]: Pin the row families and the `Tabulate` fold for `[ANALYTIC_TABLE_PROJECTION]`.
- Capability: the columnar schema is seam-owned, so every landing (Parquet, DuckDB, Flight) shares one shape.
- Shape: element/property/quantity/material/edge/assessment row records with the snapshot `ContentAddress` column, and the one `Tabulate(graph)` fold, in `libs/csharp/Rasm.Element/.planning/Graph/table.md` (new page).
- Unlocks: the Persistence columnar landing and Flight SQL serving counterpart.
- Anchors: `Element` flat record; `MeasureValue.Si`; `Relationship.Kind` flat edge column.

[AUDIT_FOLD]-[QUEUED]: Pin the coverage ratios, integrity sweeps, and `ModelAudit` receipt for `[MODEL_COMPLETENESS_AUDIT]`.
- Capability: model maturity is one typed receipt a gate or dashboard reads, never a per-consumer query pile.
- Shape: ratio definitions (classified/material-bound/quantified occurrence shares per discipline), integrity sweeps (dangling representation keys, unresolved `ProfileRef`s, orphan nodes, stale assessments), `Verify` drift census, and the graded `ModelAudit` receipt in `libs/csharp/Rasm.Element/.planning/Projection/audit.md` (new page).
- Unlocks: delivery gates and the model-health instrument rows.
- Anchors: `ObjectNodes`/incidence accessors; `AssessmentOutcome` columns; `ConstraintSeverity` grades.

[CORPUS_SPEC]-[QUEUED]: Pin the generator parameters, seed law, and graded corpus rows for `[SYNTHETIC_GRAPH_FORGE]`.
- Capability: benchmarks, property specs, and parity vectors all reproduce one model from seed and parameters.
- Shape: parameter record (node count, edge density, bag width, discipline mix, composition depth), deterministic seeded-Guid id law, and the graded corpus roster with expected snapshot `ContentAddress` anchors, in `libs/csharp/Rasm.Element/.planning/Graph/corpus.md` (new page).
- Unlocks: the BenchmarkDotNet corpus-gate rows over the graph hot paths and the cross-runtime parity anchors.
- Anchors: `ElementGraph.Of`/`Genesis`; `ContentAddress.OfGraph` order-independent snapshot key.

[TYPE_QUANTITY_SEAM_ROWS]-[QUEUED]: Admit the type-level quantity rows and the substance-density accessor the QTO mint reads.
- Capability: a Type-level quantity property-set row family — linear mass, surface area per length, volume per length — joins the `DetailSchema` property vocabulary as unit-carried quantities (typed value with unit identity per the kernel unit-bridge law, canonical SI encoding), and `MaterialPropertySet` gains one typed substance-density accessor returning the density quantity, never a bare double: the two seam surfaces the Materials type-quantity receipt consumes whole instead of re-deriving numeric semantics.
- Shape: canonical detail `PropertyName` rows on `libs/csharp/Rasm.Element/.planning/Properties/property.md#DETAIL_SCHEMA` and one density accessor member on `libs/csharp/Rasm.Element/.planning/Composition/material.md#MATERIAL_PROPERTY`.
- Unlocks: unblocks the Materials `[QTO_MINT_PINS]` route; type-level takeoff reads seam-owned vocabulary instead of re-deriving it.
- Anchors: the `DetailSchema` vocabulary owner, the `MaterialPropertySet.Mechanical` density column, the accumulating `Of` admission law.
- Ripple: `Rasm.Materials` `[TYPE_QUANTITY_RECEIPT]`.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
