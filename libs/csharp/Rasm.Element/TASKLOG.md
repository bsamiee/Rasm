# [RASM_ELEMENT_TASKLOG]

Open and closed work distilled from `IDEAS.md`. `[01]-[OPEN]` carries task cards with `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` leaders; `[02]-[CLOSED]` carries `[COMPLETE]` or `[DROPPED]` cards. One idea spawns one or more tasks; each task names the exact sub-domain or file it lands in.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — altitude only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <present only on a BLOCKED or gated card; the exact observable that flips it actionable — a catalog row landing, a member query returning evidence, a package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart card — cross-folder as `pkg` `[SLUG]` or a same-folder prerequisite `[SLUG]`, prefixed follows/precedes/mirrors when build order is load-bearing>.
- Atomic: <present only on a minor-scope task; names the small unit so a later session sizes its turn>.
Capability, Shape, Unlocks, and Anchors are required on every open card, Atomic included; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Tasks state landing-grain work decomposing an idea.
-->

[QUANTITY_GROUP_COLUMN]-[QUEUED]: Land the `QuantityBag` group axis on the bag shape and the canonical bytes — the seam half of `[QUANTITY_BAG_GROUP_AXIS]`.
- Capability: grouped takeoffs cross the graph identity-lossless per the owning idea.
- Shape: one group column on `ValueBag<V>` in `libs/csharp/Rasm.Element/.planning/Properties/property.md`, presence-delimited count-prefixed write in the `quantitySet` arm of `Node.ToCanonicalBytes` in `libs/csharp/Rasm.Element/.planning/Graph/element.md`; `Merge` precedence untouched, the wire column one append-only numbered field under the `Graph/wire.md` contract-evolution law.
- Unlocks: Bim ingest/egress ends compose the axis the moment the seam carries it.
- Anchors: counted-bag injectivity law (`Projection/address` count-prefix); `Bake` bag merge.
- Atomic: one column and one canonical-bytes arm.

[FEDERATION_HEADER_RULING]-[BLOCKED]: Resolve the federation header-reconciliation ruling that gates `[FEDERATION_AND_PARTIAL_EXCHANGE]`.
- Capability: an answered ruling turns `Federate`/`Extract` from a bet into a design landing in `libs/csharp/Rasm.Element/.planning/Graph/element.md`.
- Shape: question — caller-supplied coordination `Header` with per-source headers demoted to provenance, or a per-source `Header` roster on the federated graph? Route: user interview over the `Header.CanonicalBytes` semantic identity, mixed-`Tolerance` divergence pinned first.
- Unlocks: federated coordination review and `Members`-closed partial exchange.
- Anchors: `Header.CanonicalBytes`; tolerance-quantized measure bytes; `GeoReference` divergence cases.

[OBSERVATION_PAGE]-[QUEUED]: Author the observation-series design for `[OBSERVATION_SERIES]`.
- Capability: measured time-series evidence folds into the one `Bake` read beside computed assessments.
- Shape: `libs/csharp/Rasm.Element/.planning/Assessment/observation.md` (new page) carrying the series descriptor; one `Node` case and `AssignKind` row and `LegalAssign`/`Bake` arms in `libs/csharp/Rasm.Element/.planning/Graph/element.md` and `libs/csharp/Rasm.Element/.planning/Relations/relation.md`; one `NodeWire` oneof arm in `libs/csharp/Rasm.Element/.planning/Graph/wire.md`.
- Unlocks: the commissioning comparison lane and the `Rasm.Compute` computed-vs-measured routes.
- Anchors: `RasterKey`/`ResultBlob` by-reference precedent; NodaTime `Interval`; a new `NodeWire` oneof arm is additive under the `Graph/wire.md` contract-evolution law.

[REDACTION_IDENTITY_RULING]-[BLOCKED]: Resolve redacted-crossing identity preservation for `[REDACTION_SCOPED_EGRESS]`.
- Capability: the ruling fixes whether redacted crossings preserve or re-derive content keys, and with it the decode-side `AddressUnstable` posture.
- Shape: question and route stated on the owning idea; on resolution, pin the `WireLimits`-sibling redaction policy record and the `Encode` parameterization in `libs/csharp/Rasm.Element/.planning/Graph/wire.md` and the parity-corpus vectors for redacted nodes.
- Unlocks: partner-scoped deliverables off one stored model.
- Anchors: `IRedactorProvider`/`DataClassificationSet` in `libs/csharp/.api/api-redaction.md`; `ContentAddress.Verify` dual.

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

[CORPUS_SPEC]-[BLOCKED]: Seal the graded whole-graph parity pins after the current forge is executable from the tests estate.
- Capability: exact `S`/`M`/`L`/`XL` snapshot addresses turn deterministic witnesses into cross-runtime regression gates.
- Shape: execute every current `GraphForge.Mint` grade from `tests/csharp`, commit the four literal `ContentAddress` values in `Graph/corpus#CORPUS_ROSTER`, and mirror the roster in `libs/python` and `libs/typescript/core`.
- Unlocks: build-breaking whole-graph parity across C#, Python, and TypeScript.
- Anchors: `CorpusGate.Stable`; `CorpusWitness.Snapshot`; `ContentAddress.OfGraph`.
- Tension: arming trigger — the tests-estate corpus harness can execute the settled forge against current source owners; no unset `Option` or self-derived runtime value qualifies as an expected pin.

[TYPE_QUANTITY_SEAM_ROWS]-[QUEUED]: Admit the type-level quantity rows and the substance-density accessor the QTO mint reads.
- Capability: a Type-level quantity property-set row family — linear mass, surface area per length, volume per length — joins the `DetailSchema` property vocabulary as unit-carried quantities (typed value with unit identity per the kernel unit-bridge law, canonical SI encoding), and `MaterialPropertySet` gains one typed substance-density accessor returning the density quantity, never a bare double: the two seam surfaces the Materials type-quantity receipt consumes whole instead of re-deriving numeric semantics.
- Shape: canonical detail `PropertyName` rows on `libs/csharp/Rasm.Element/.planning/Properties/property.md#DETAIL_SCHEMA` and one density accessor member on `libs/csharp/Rasm.Element/.planning/Composition/material.md#MATERIAL_PROPERTY`.
- Unlocks: unblocks the Materials `[QTO_MINT_PINS]` route; type-level takeoff reads seam-owned vocabulary instead of re-deriving it.
- Anchors: the `DetailSchema` vocabulary owner, the `MaterialPropertySet.Mechanical` density column, the accumulating `Of` admission law.
- Ripple: `Rasm.Materials` `[TYPE_QUANTITY_RECEIPT]`.

[DETAIL_SCHEMA_READER_PROVISION]-[QUEUED]: Land the owner-declared structural row statics and the blessed reader-local category.
- Capability: the seam bag's key space closes over owner provision — structural wire names become owner statics, reader-only enrichment rows gain their blessed category, and the associated-material-grade prohibition names the element-own EXPRESS token it does not cover.
- Shape: `libs/csharp/Rasm.Element/.planning/Properties/property.md` — the structural `PropertyName` statics (`AtStart` and the force/moment rows), the reader-local category clause, and the sharpened grade boundary sentence.
- Unlocks: IDEAS.md `[READER_ROW_CUSTODY]` — writer and reader adoption becomes a retarget, not a redesign.
- Anchors: `libs/csharp/.planning/RULINGS.md` seam-bag custody row; the `DetailSchema` vocabulary owner.
- Ripple: precedes `Rasm.Bim` `[READER_ROWS_RECONCILE]` and `Rasm.Compute` `[STRUCTURAL_ROW_STATICS]`.

[WIRE_EVOLUTION_PROSE_ALIGN]-[QUEUED]: Growth prose on the graph and material pages aligns to the additive wire law.
- Capability: column-add growth reads as ordinary additive evolution; no page narrates a campaign freeze the wire owner's law forecloses.
- Shape: `libs/csharp/Rasm.Element/.planning/Graph/element.md` and `libs/csharp/Rasm.Element/.planning/Composition/material.md` growth lines — the "frozen this campaign, lands with the wire unfreeze" narration restates as additive column adds under the `Graph/wire.md` contract-evolution law, column content unchanged.
- Unlocks: `RULINGS.md` `[02]` unfreeze-retirement row holds with zero contradicting prose.
- Anchors: `Graph/wire.md` contract-evolution law; the folder `RULINGS.md`.
- Atomic: two growth-line rewrites.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[HOOK_POINT_ROSTER]-[COMPLETE]: `Projection/observe#HOOK_RAIL` pins the `HookPoint` roster with the kernel `HookModality` column, the `ElementFact` payloads, and the `ElementHookRail.Of` mint over the kernel point capsule — subscriber faults park as point-attributed `IsolatedFault` rows read through `TapFaults`.
[INSTRUMENT_ROW_TABLE]-[COMPLETE]: `Projection/observe#INSTRUMENT_PROJECTION` pins `ElementInstruments.Rows` as kernel `InstrumentRow` declarations with `Buckets` advice, the span-name constants, the contributor-port mint, and the `IMeterFactory` injection seam through the kernel identity entry.
[ENVELOPE_VOCAB]-[COMPLETE]: `Graph/wire#EVENT_ENVELOPE` pins the `GraphEventType` token rows and the `GraphEventEnvelope` record with `Attributes`/`Admit` and the W3C trace slots.
