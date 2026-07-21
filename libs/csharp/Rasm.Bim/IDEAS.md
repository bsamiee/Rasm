# [RASM_BIM_IDEAS]

Forward pool of higher-order concepts for the host-neutral BIM-and-exchange engine. `[1]-[OPEN]` holds active ideas as cards; `[2]-[CLOSED]` records a finished or dropped idea with a one-line disposition.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — altitude only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <present only on a BLOCKED or gated card; the exact observable that flips it actionable — a catalog row landing, a member query returning evidence, a package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart card — cross-folder as `pkg` `[SLUG]` or a same-folder prerequisite `[SLUG]`, prefixed follows/precedes/mirrors when build order is load-bearing>.
Capability, Shape, Unlocks, and Anchors are required on every open card; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Ideas state higher-order concepts, never landing-grain tasks.
-->

[CONNECTION_INTERFACE_GEOMETRY_DECODE]-[QUEUED]: Land the Bim lowering and re-materialization for the seam-landed `Connect.Interface` content key.
- Capability: `IfcConnectionGeometry` and `IfcRelSpaceBoundary2ndLevel` interface surfaces ride the graph as content-keyed typed geometry instead of stranding in `Generic` attributes.
- Shape: the `Projection/relations` projector hashes the interface surface into the blob store and stamps the key on the `Connect` edge (`IfcRelConnectsElements.ConnectionGeometry` and the 2nd-level space-boundary route off `Generic`); the `Projection/egress` re-materializes it; the landed seam decodes the interface key through the one `GeometrySource.ResolveFootprint` leg — never a third port.
- Unlocks: Compute reads connection-interface geometry one-hop by content key; re-exported analysis models keep their boundary surfaces.
- Anchors: `Connect.Interface` landed seam-side (presence-delimited canonical bytes, additive `ConnectWire` field); the egress eccentricity path already reconstitutes `IfcConnectionGeometry` STEP fragments from the ctor-held profiles store — the same lane.
- Ripple: `Rasm.Element` `[CONNECTION_INTERFACE_GEOMETRY_DECODE]`.

[QUANTITY_BAG_GROUP_AXIS]-[QUEUED]: Round-trip the `IfcPhysicalComplexQuantity` `Discrimination`/`Quality`/`Usage` grouping identity once the seam `QuantityBag` carries a group axis.
- Capability: complex-quantity grouping identity survives the dot-path flatten — a formwork quantity grouped by `Discrimination` re-emits with its grouping strings instead of prefix-only reconstruction.
- Shape: the `Projection/semantic` flatten stamps the group-axis rows the seam carrier adds; the `Projection/egress` `RaiseQuantity` rebuilds nested `IfcPhysicalComplexQuantity` children from them.
- Unlocks: value-lossless AND identity-lossless complex-quantity round-trip; QTO consumers select by grouping axis.
- Anchors: `Projection/semantic` `FlattenQuantities` already recurses dot-path keys value-lossless and names the residual; the decompile-verified `Discrimination`/`Quality`/`Usage` public strings.
- Tension: the seam column ripples the counted-bag canonical-bytes injectivity law, the frozen wire, and the `Bake` merge — the seam owner lands first.
- Ripple: `Rasm.Element` `[QUANTITY_BAG_GROUP_AXIS]`.

[BCF_API_RESPONSE_ADMISSION]-[QUEUED]: Close the BCF-API round-trip with the response half of the REST projection.
- Capability: a BCF-API response admits back into the archive-domain family — status/header fold, pagination cursor, and the response-body lowering onto `BcfTopic`/`BcfComment`/`BcfViewpoint` — so a CDE round-trip reads one owner.
- Shape: `Review/issues` gains the response peer of `BcfResource` → `BcfApiRequest`: one response carrier discriminated by resource, the snake-case body admission reusing `BcfApiContext`, the paged-collection fold; execution stays on the Compute transport.
- Unlocks: live CDE topic sync onto `IssueBoard`, server-authored viewpoints landing beside `.bcfzip` imports, conflict-aware `ReviseTopic` round-trips.
- Anchors: `BcfResource`/`BcfApiRequest`/`BcfApiContext` landed; `BcfWireMapper` owns the archive↔wire correspondence the response admission reuses.
- Tension: the transport port and retry/auth policy are Compute's — the response admission consumes returned bytes and never mints a second transport owner.

[BRICK_SYSTEMS_OPERATIONS_OVERLAY]-[BLOCKED]: Compose the admitted `BrickSchema.Net` ontology as the building-systems-operations overlay on `Model/systems`, once the app-platform live-binding leg lands.
- Capability: a Brick `Point`/`Equipment`/`Location` graph overlays the static `SystemTrace` connectivity, mapping IFC `MonitoringSystem` occurrences onto Brick `Fedby`/`PointOf`/`PartOf`/`LocationOf` relations so operations consumers read one semantic systems model.
- Shape: `Model/systems` gains a Brick projector lowering the `DistributionSystem` reach set onto Brick classes; the live-point binding (`BACnetReference`/`BACnetDevice`/`ModbusDevice`) stays a reference the app-platform resolves, never a transport minted here.
- Unlocks: BMS-aware clash and coordination, live-versus-design systems reconciliation, operations-phase handover beyond COBie.
- Anchors: `BrickSchema.Net` admitted (README `[DOMAIN_VOCABULARY]`, `.api/api-brickschema-net.md` — `Aggregation.AggregateByInterval` the interval-rollup read the operations overlay exposes); `Model/systems` owns the static `SystemTrace` connectivity the overlay reads.
- Tension: which app-platform owner lands the live-point binding resolver (`BACnetReference`/`BACnetDevice`/`ModbusDevice` rows) the overlay references? Route: `libs/csharp/Rasm.AppHost/.planning/` capability pages and the AppHost growth register — Bim owns only the static ontology projection, the overlay reads `SystemTrace`, never re-minting a second connectivity store beside it.

[PROGRESS_VERIFICATION]-[QUEUED]: Close the 4D loop — scan-derived physical progress verifies the schedule and feeds earned value with observed actuals.
- Capability: a reconstructed point-cloud epoch compares against the `ConstructionState` expectation at the capture instant — observed installed elements against planned — minting a typed progress-evidence receipt per task with observed completion, variance band, and the unmatched-occurrence residue.
- Shape: `libs/csharp/Rasm.Bim/.planning/Planning/progress.md` (new page) owns the comparison fold joining `Exchange/reconstruct#RECONSTRUCTION` occurrences to `Planning/schedule#SCHEDULE` `TaskAssignment` element sets through the `Model/query#ELEMENT_SET` predicate algebra; observed completion feeds the `Planning/cost#EARNED_VALUE` fold as the actuals source beside authored `IfcTaskTime.Completion`.
- Unlocks: reality-capture progress dashboards, evidence-backed earned value, dispute-grade progress records keyed by the capture content key.
- Anchors: `ConstructionState.At` phase reads (`Completed` the finished-by read); `ConstructionTask.PercentComplete` fallback law; scan-to-BIM occurrences carrying per-occurrence fit receipts.

[ENERGY_RESULTS_ANNOTATION]-[QUEUED]: Land simulation results back on the model — Compute-read EnergyPlus outputs annotate zones and spaces as typed result quantities.
- Capability: Compute's typed energy-results receipt (keyed by the `EnergyArtifact` content key) admits onto `Model/zones` and spatial nodes as result quantities — annual and peak loads, comfort hours, EUI — re-emittable as Psets through the standing properties authority, so results survive re-export instead of dying in a run directory.
- Shape: `libs/csharp/Rasm.Bim/.planning/Energy/results.md` (new page) owns the admission fold from the Compute receipt onto zone/space quantity rows and the reverse read the AppUi report consumes; `SqlFile` decode stays Compute's per the standing simulation ruling — Bim consumes the receipt, never the SQLite.
- Unlocks: results-aware model QA (IDS facets over result thresholds), energy dashboards read from the model, results round-trip into IFC Psets.
- Anchors: `Energy/exchange#ENERGY_EXCHANGE` content-keyed artifacts; `Semantics/properties#PROPERTY_TEMPLATES` Pset authority; `Model/zones#ZONE_GRAPH` overlay; `.api/api-openstudio.md` results-seam row naming `SqlFile` as Compute's reader.

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[BIM_HOOK_RAIL]-[COMPLETE]: `Model/observability#HOOK_RAIL` landed — `BimHooks` per-composition registry record and the `BimFact` payload family over the kernel point capsule, with modality rows, id grammar, and subscriber-fault isolation arriving settled from the kernel signal capsule; progress points wired at `Exchange/import#IMPORT_RAIL` (ACadSharp `OnProgress`) and `Energy/derive#TRANSLATE_MATRIX` (OpenStudio `ProgressBar`).
[BIM_TELEMETRY_TAP]-[COMPLETE]: `Model/observability#TELEMETRY_TAP` landed — `BimTelemetry` roster-and-projection owner over kernel `InstrumentRow`/`InstrumentSet`/`Buckets`, the kernel identity mint and contributor port, the `ActivitySource` span law, and baggage-sourced tenant/model attribution, zero OTel reference.
[BIM_EVENT_FABRIC]-[COMPLETE]: `Exchange/events#EVENTS` landed — `BimEvent` closed union over the five model-mutating facts, `BimEnvelope` CloudEvents projection (`Seal`/`Encode`/`Open`, traceparent/tracestate extension rows), mint rows pinned on versioning, issues, validation, export, and energy exchange.
[BIM_BENCH_RECEIPTS]-[COMPLETE]: `Model/observability#BENCH_RECEIPTS` landed — `BimBenchClaim` per-op claim roster with corpus columns and the `BimBenchReceipt` evidence record under the AppHost corpus-gate admission row.
[TEMPLATE_AUDIT_VALIDATION_TIER]-[COMPLETE]: Ruled — `Review/validation` widened to the two-tier QA owner: `ModelHealth`/`ModelFinding` compose the `TemplateFinding` stream as the baseline tier beneath authored IDS, the case the tier discriminant, one verdict surface for `Rasm.AppUi` and the review pipeline
[UNIT_SCHEME_BIM_COUNTERPART]-[COMPLETE]: `HeaderWire.unit_scheme = 7` is landed on the seam `Graph/wire` with both Mapper legs, and the Bim ingest (`UnitsOf`) and egress (`DeclareUnits`) ends read the field — the `Rasm.Element` counterpart card closed with it.
