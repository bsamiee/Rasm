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

(none)

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[OBSERVATION_PRODUCER_SEAM]-[COMPLETE]: the producer seats at `Rasm.Compute` (the S1 spine cannot reference the seam, so AppHost stays the coercing transport) — `Runtime/observation#OBSERVATION_LANE` accumulates decoded sensor runs per `(occurrence, sensor, aspect)` binding and flushes through the production `Open`/`Encode`/`From`/`Append` chain onto `Node.Observation` + `AssignKind.Observation` deltas, and `Analysis/assessment#COMMISSIONING` reads `SeriesStatistics.Representative` against the Element `AssessmentPayload.ResultMeasure` under quantity-triple agreement and the completeness floor, writing the verdict back as a commissioning assessment whose `DependsOn` names the series node; seam edges landed at both `ARCHITECTURE.md` ends (the Bim `Energy/results` `ResultMeasure` roster stays a distinct same-spelled concept, scope-qualified per the cross-package twin ruling).
[INCREMENTAL_ADDRESS]-[COMPLETE]: `Projection/address#INCREMENTAL_ADDRESS` lands `GraphMembers`/`Advance`/`ContentAddress.OfGraph(members)` — the sorted-projection cache provably byte-identical to the full-state fold, `NormalForm(Op)`-gated with the tolerance-reheader refusal arm — and the Persistence consumer folds it at `Element/codec` and the `Version/timetravel` `Scrub`/`Bisect` frames.
[FEDERATION_AND_PARTIAL_EXCHANGE]-[COMPLETE]: `Graph/element#FEDERATION` lands `Federate` (caller-supplied coordination `Header` under `[FEDERATION_HEADER_RULING]`, divergence refused, `FederationReceipt` provenance) and `Extract` (Members-closed slice); `Rasm.Bim` composes both — the coordination clash union and the scoped-export slice.
[MODEL_COMPLETENESS_AUDIT]-[COMPLETE]: `Projection/audit.md` lands the `ModelAudit` fold — coverage census, integrity sweeps, `Verify` drift census, `ConstraintSeverity`-graded findings, threshold policy — with the `observe.md` instrument rows and the `Rasm.Bim` `Review/validation` `ModelHealth` neutral-tier composition.
[READER_ROW_CUSTODY]-[COMPLETE]: the key space closes over owner provision at `Properties/property#DETAIL_SCHEMA` — `StructuralRows` holds every name two packages key on and `PropertyCategory.Row` partitions each producer's own vocabulary, so `Rasm.Bim` writes and `Rasm.Compute` reads one spelling while `Rasm.Fabrication`'s derivation rows mint under their own blessed scope; `PropertyName` stays an open key for an ingested foreign `Pset` name, which is exactly why authored rows need custody rather than a closed vocabulary.
[ANALYTIC_TABLE_PROJECTION]-[COMPLETE]: `Graph/table.md` lands `TableRow` as a `[Union]` whose cases ARE the datasets — the completeness bar is that every `Seq` a baked `Element` carries reaches a dataset — each owning its `Cells` projection so a column declaration and its payload move in one edit, with `TableFamily` rostering each `element.<source>` dataset and `GraphTable.Tabulate(graph, key, roots)` the one fold; the page composes the `Rasm.Persistence` `[WIRE]: AnalyticsSchema` custodian rather than minting a second columnar vocabulary, and every family declares its own trailing spine — `at` on `element.assessments`, `observed` elsewhere off the threaded projection frame.
[OBSERVATION_SERIES]-[COMPLETE]: `Assessment/observation.md` lands `ObservationSeries` with its sampling and grade vocabularies, by-reference `ObservationChunk` run, and the derived `SeriesStatistics` plane carrying zero authority; identity keys the STREAM and excludes the chunk run, so a live append mutates the node in place instead of re-keying it per batch, and series are occurrence-only — the one `Seq` the named type inheritance deliberately skips.
[ELEMENT_HOOK_RAIL]-[COMPLETE]: `Projection/observe#HOOK_RAIL` lands `ElementPoint`/`ElementFact`/`ElementHooks`/`ElementTap` — the roster/fact pair over the ONE kernel `HookRail`, veto fold first, each decoration bracketed in the kernel `SpanBand` under the point's own derived `TraceScope`, subscriber faults parking as `IsolatedFault` rows on the rail's `FaultCell`.
[GRAPH_INSTRUMENT_PROJECTION]-[COMPLETE]: `Projection/observe#INSTRUMENT_PROJECTION` lands `ElementInstrument` rows each carrying their kernel `InstrumentSpec` under the kernel `TelemetrySource.Element` scope over one dotted `rasm.element.<dimension>` slot block both planes spell, every write folding the kernel `TenantContext` partition onto the row-addressed `InstrumentSet.Write` rail, and `GraphInstrument.Tap` projecting each fact over the composing root's `InstrumentSet` beside the contributor-port mint.
[SYNTHETIC_GRAPH_FORGE]-[COMPLETE]: `Graph/corpus` lands `CorpusProfile`/`GraphForge` seeded deterministic generation and the graded `CorpusGrade`/`CorpusOp`/`CorpusGate` roster.
[UNIT_SCHEME_BIM_COUNTERPART]-[COMPLETE]: `Graph/wire` `HeaderWire.unit_scheme = 7` carries the `Header.Units` scheme with the Mapper transcription both ways; both Bim ends compose it.
[QUANTITY_BAG_GROUP_AXIS]-[COMPLETE]: `ValueBag<V>` carries the fifth `Groups` column (`Map<string, GroupIdentity>`, `[UnorderedEquality]`, trailing default so every 4-arg construction stands) with `GroupIdentity(Discrimination, Quality, Usage)` beside it; the `quantitySet` canonical arm writes the count-prefixed presence-delimited group run (the `propertySet` arm states its empty-by-construction law), `Merge` unions groups occurrence-wins, and `QuantitySetWire.groups = 5` landed append-only with both Mapperly legs — the `Rasm.Bim` ingest/egress ends closed in the same pass.
[CONNECTION_INTERFACE_GEOMETRY_DECODE]-[COMPLETE]: seam half whole — `Connect.Interface` rides `CanonicalBytes` and the wire and decodes through the one `GeometrySource.ResolveFootprint` leg; the Bim lowering and egress ride the `Rasm.Bim` counterpart card.
