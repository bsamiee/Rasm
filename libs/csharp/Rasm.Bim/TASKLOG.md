# [RASM_BIM_TASKLOG]

Open and closed work distilled from `IDEAS.md`. `[01]-[OPEN]` carries task cards with `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` leaders; `[02]-[CLOSED]` carries `[COMPLETE]` or `[DROPPED]` cards. One idea spawns one or more tasks; each task names the exact sub-domain or file it lands in.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — concept grain only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <BLOCKED or gated cards only; the exact observable flipping it actionable — catalog row landing, member query evidence, package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart — cross-folder `pkg` `[SLUG]`, same-folder prerequisite `[SLUG]`; load-bearing build order prefixes follows/precedes/mirrors>.
- Atomic: <present only on a minor-scope task; names the small unit so a later session sizes its turn>.
Capability, Shape, Unlocks, and Anchors are required on every open card, Atomic included; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Tasks state landing-grain work decomposing an idea.
-->

[APPEARANCE_EMIT_BINDING]-[BLOCKED]: The complete `Author`/`Bind` appearance-egress pair fires on the IFC emit rail.
- Capability: an emitted IFC file carries surface styles, per-vertex UV bindings, and per-face radiometry for every appearance-bearing element — the full inverse of the landed ingest loop.
- Shape: `Projection/egress.md` `Emit` composes `AppearanceProjection.Author` per appearance-bearing `Object` and `AppearanceProjection.Bind` per authored face set; both entries exist whole at `Semantics/appearance.md` and the bound receipt is the emit-side lane evidence.
- Unlocks: styled IFC deliverables that round-trip appearance without the glTF sidecar.
- Anchors: `Semantics/appearance.md` `[02]` `Author`/`Bind`; `Projection/egress.md` Boundary names the semantic-only scope and this arming.
- Arms: a body-representation author joins the IFC emit rail — today `Emit` re-authors semantics alone by design and geometry egress rides glTF/3dm, so no face set exists to style; re-probe when an IFC-body-emit capability is chartered.

[ELEMENT_SET_VIEWPORT_SEAM]-[BLOCKED]: `ElementSet` query algebra reaches the AppUi viewport as a declared `[PROJECTION]` seam.
- Capability: model-query results (`Model/query` `ElementSet`) rendered as AppUi viewport/inspector selections.
- Shape: one `Model/query -> csharp:Rasm.AppUi/<owner> # [PROJECTION]` seam row with the consuming AppUi page fence.
- Unlocks: saved-query overlays, selection-driven dashboards, query-scoped exports.
- Anchors: `Model/query` `ElementSet`; the AppUi `[V9]` growth register carries the reciprocal record.
- Tension: no AppUi page names a consumer today — the seam row re-enters `ARCHITECTURE.md` `[02]-[SEAMS]` only when one does; deferred pressure never rides the ledger.

[SCHEDULE_CHARTS_SEAM]-[BLOCKED]: `ScheduleNetwork` CPM/4D projection reaches the AppUi Charts plane as a declared `[PROJECTION]` seam.
- Capability: 4D construction-sequencing and critical-path dashboards over the `Planning/schedule` domain.
- Shape: one `Planning/schedule -> csharp:Rasm.AppUi/Charts # [PROJECTION]` seam row with the consuming dashboards fence.
- Unlocks: 4D playback tiles, earned-value overlays beside the existing `Planning/cost` receipt row.
- Anchors: `Planning/schedule` `ScheduleNetwork`; the AppUi `[V9]` growth register carries the reciprocal record.
- Tension: no AppUi consuming fence exists today — same re-entry law as `[ELEMENT_SET_VIEWPORT_SEAM]`.

[MOISTURE_DIFFUSIVITY_BASE]-[QUEUED]: The moisture-diffusivity measure signs the base IFC declares, proven at the schema.
- Capability: `IfcMoistureDiffusivityMeasure` coerces on the base the schema declares rather than the diffusivity-convention assumption — IFC declares `m3/s` while the row signs the conventional `m2/s`, a three-order coercion difference on a mm model.
- Shape: read the schema declaration, then land the corrected `MeasureDimensions` signature or the recorded convention-divergence negative; the research row on `Projection/semantic.md` `[04]-[RESEARCH]` carries the route.
- Unlocks: the last unproven dimension signature in the measure table closes.
- Anchors: the `IfcPlanarForceMeasure` mis-signature precedent this wave fixed; the `MeasureDimensions` row column.
- Atomic: one schema read, one row verdict.

[PROGRESS_COMPARISON_FOLD]-[QUEUED]: Author the progress comparison fold and evidence receipt on the new progress page.
- Capability: capture-epoch occurrences joined to `TaskAssignment` element sets, minting per-task observed completion, variance band, and the unmatched-occurrence residue.
- Shape: `libs/csharp/Rasm.Bim/.planning/Planning/progress.md` gains the comparison fold over `Exchange/reconstruct#RECONSTRUCTION` occurrences, `ConstructionState.At` expectations, and the `Model/query#ELEMENT_SET` join, and the typed evidence receipt.
- Unlocks: `[EARNED_VALUE_ACTUALS_JOIN]` earned-value actuals and reality-capture dashboards.
- Anchors: `IDEAS.md` `[PROGRESS_VERIFICATION]`.

[EARNED_VALUE_ACTUALS_JOIN]-[QUEUED]: Join observed completion into the earned-value actuals read.
- Capability: evidence-backed actuals beside authored `IfcTaskTime.Completion` with a stated precedence law.
- Shape: `libs/csharp/Rasm.Bim/.planning/Planning/cost.md` `[EARNED_VALUE]` gains the observed-completion source row and its precedence over the authored percent and the actual-interval fraction.
- Unlocks: dispute-grade earned value.
- Anchors: `IDEAS.md` `[PROGRESS_VERIFICATION]`.
- Atomic: one source row and precedence law.

[ENERGY_RESULTS_ADMISSION_FOLD]-[QUEUED]: Author the results admission fold on the new energy results page.
- Capability: Compute results receipt admitted onto zone/space quantity rows, re-emittable as Psets, readable by the AppUi report.
- Shape: `libs/csharp/Rasm.Bim/.planning/Energy/results.md` gains the admission fold keyed by the `EnergyArtifact` content key, the zone/space quantity rows over `Model/zones#ZONE_GRAPH`, and the Pset re-emission row through `Semantics/properties#PROPERTY_TEMPLATES`.
- Unlocks: results-aware QA facets and energy dashboards from the model.
- Anchors: `IDEAS.md` `[ENERGY_RESULTS_ANNOTATION]`.

[CONNECTION_KEY_LOWERING]-[QUEUED]: Pin the connection-interface content-key lowering and re-materialization ends.
- Capability: `IfcConnectionGeometry` and 2nd-level space-boundary surfaces ride the `Connect` edge as content-keyed typed geometry.
- Shape: `libs/csharp/Rasm.Bim/.planning/Projection/relations.md` hashes the interface surface into the blob store and stamps the key on the `Connect` edge; `libs/csharp/Rasm.Bim/.planning/Projection/egress.md` re-materializes through the ctor-held profiles-store lane.
- Unlocks: one-hop Compute reads and boundary-preserving re-export.
- Anchors: `IDEAS.md` `[CONNECTION_INTERFACE_GEOMETRY_DECODE]`.

[QUANTITY_GROUP_AXIS_ENDS]-[QUEUED]: Pin the complex-quantity group-axis flatten and raise ends.
- Capability: `Discrimination`/`Quality`/`Usage` grouping identity survives the dot-path flatten and re-emits nested.
- Shape: `libs/csharp/Rasm.Bim/.planning/Projection/semantic.md` `FlattenQuantities` stamps the group-axis rows; `libs/csharp/Rasm.Bim/.planning/Projection/egress.md` `RaiseQuantity` rebuilds nested `IfcPhysicalComplexQuantity` children.
- Unlocks: identity-lossless QTO round-trip.
- Anchors: `IDEAS.md` `[QUANTITY_BAG_GROUP_AXIS]`.
- Tension: seam `QuantityBag` group-axis column lands first — the `Rasm.Element` counterpart owns the wire and `Bake` merge ripple.

[BCF_RESPONSE_CARRIER]-[QUEUED]: Author the BCF-API response carrier and paged-collection fold.
- Capability: a BCF-API response admits back into the archive-domain family — status/header fold, pagination cursor, body lowering onto `BcfTopic`/`BcfComment`/`BcfViewpoint`.
- Shape: `libs/csharp/Rasm.Bim/.planning/Review/issues.md` gains the response peer of `BcfApiRequest` discriminated by resource, the snake-case body admission over `BcfApiContext`, and the paged-collection fold; execution stays on the Compute transport.
- Unlocks: live CDE topic sync onto `IssueBoard`.
- Anchors: `IDEAS.md` `[BCF_API_RESPONSE_ADMISSION]`; `BcfWireMapper` archive-wire correspondence.

[TYPE_CANDIDATE_EXPORT]-[QUEUED]: Project reconciled type objects into Materials candidate rows — the reverse type-minting export off the IFC ingest.
- Capability: reconciled `IfcElementType` data — identity, property sets, classification — projects into a typed candidate export the Materials `ComponentRow` railed `Of` factories admit, provenance-marked as imported-library rows; the provenance-marking decision co-signs with the Materials owner.
- Shape: one projection member on `libs/csharp/Rasm.Bim/.planning/Exchange/import.md` at the type-object reconciliation end, emitting candidate rows keyed by source-library identity; admission folds stay Materials-side.
- Unlocks: an ingested manufacturer IFC library seeds the Materials catalogue instead of dying at occurrence projection; the Materials `[IFC_ADMISSION_FOLD_MAP]` route gains its ingest-side surface.
- Anchors: the import type-object reconciliation, the `Projection/semantic` property flatten, the Materials railed `Of` factory family; the durable-store leg is derived lineage — `Rasm.Persistence` `Version/provenance.md` `ProvKind.Import` attributes imported entities off the changefeed, so no store provenance column mints.
- Ripple: `Rasm.Materials` `[IFC_PRODUCT_LIBRARY_ADMISSION]`.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[READER_ROWS_RECONCILE]-[COMPLETE]: every row the connection and structural ingests stamp now resolves to a `Rasm.Element` `StructuralRows` static or a `PropertyCategory.Seam.Row` mint and no call-site `PropertyName.Create` survives either reader; the reconcile also collapsed the restraint twin the Element custody law forbids — the six DOF rows carry restraint-versus-spring on the `PropertyValue` case where a parallel `<dof>Stiffness` roster stranded the magnitude — and folded the prefix-built frame name family onto the one declared `StructuralRows.Frame` list row. The `Rasm.Compute` mirror `[STRUCTURAL_ROW_STATICS]` reads the same statics and must land the collapsed DOF and frame shapes at its end.
[BRICK_SYSTEMS_PROJECTOR]-[COMPLETE]: `Model/systems#CONNECTIVITY` `DistributionNetwork.BrickProjection` lowers the settled `DistributionSystem` view onto the Brick graph — a `BrickSystem` per system and circuit, `PartOf` membership, `PointOf` port attachment, `Fedby` flow under the one `SystemTrace.Orient` law, `LocationOf` served structures — returning the `BrickGraph` the `[WIRE]: BrickGraph` seam carries; the blocker was REFUTED on its own rail, `Rasm.AppHost` `Wire/livewire` being the named live-binding owner whose transport axis carries the BACnet/Modbus point rows. Class election and interval-rollup analytics stay that seat's through the injected `BrickBinding`, since `AddEntity<T>` binds its Brick class at compile time.
[SEAM_REGISTRY_RECONCILE]-[COMPLETE]: seam registry re-anchored to page-owned spellings — `GeoFeatureWire` holds as the seam wire at every endpoint against the interior `GeoWire` projector, `BcfTopicWire` verified page-owned at its typescript core consumer and carried bilaterally, `ModelDiff` verified page-owned as the cross-runtime wire; the `typescript:core` `[BIM_CENSUS_RECONCILE]` census now reads current mints.
[HOOK_RAIL_ROSTER]-[COMPLETE]: `Model/observability#HOOK_RAIL` owns the point roster and the per-composition `BimHooks` registry record over the kernel point capsule; modality rows and subscriber-fault isolation arrive settled from the kernel, faults parking as `IsolatedFault` rows on the composition evidence cell.
[PROGRESS_POINT_WIRING]-[COMPLETE]: progress points wired — `Exchange/import#IMPORT_RAIL` `AcadReader.Read` fires `rasm.bim.exchange.progress` off `ICadReader.OnProgress`, `Energy/derive#TRANSLATE_MATRIX` `TranslateProgress` fires `rasm.bim.energy.progress` off `onPercentageUpdated`.
[INSTRUMENT_ROSTER_MOUNT]-[COMPLETE]: `Model/observability#TELEMETRY_TAP` owns the instrument roster as kernel `InstrumentSpec` declarations carrying kind, measurement form, `Buckets` advice bounds, and dotted `rasm.bim.<dimension>` slots, materialized by an app fan through the contributor port or bound directly by a root through `InstrumentSet.Of`; every projected write rides the kernel `InstrumentSet.Write` rail out through the capsule's rail-shaped `Observe`, which parks the refusal point-attributed.
[SPAN_ATTRIBUTION_LAW]-[COMPLETE]: span and attribution law rows landed on `Model/observability#TELEMETRY_TAP` — the kernel `SpanBand` over `BimPoint`-derived planes taken on a nullable receiver, `Op`-derived span names, the kernel `TenantContext` partition on every metric write, and `rasm.bim.model` stamped on the span alone from `Traced`'s own required argument.
[EVENT_ENVELOPE_PROJECTION]-[COMPLETE]: `Exchange/events#EVENTS` owns `BimEvent`, the `BimEnvelope` projection over `JsonEventFormatter` with the traceparent/tracestate extension rows, and the `rasm.bim.<domain>.<fact>` type law.
[EVENT_MINT_ROWS]-[COMPLETE]: mint rows pinned — versioning `CommitLanded`, issues `IssueMutated`, validation `VerdictIssued`, export `ArtifactMinted`, energy exchange `EnergyMinted`.
[BENCH_RECEIPT_ROSTER]-[COMPLETE]: `Model/observability#BENCH_RECEIPTS` owns the `BimBenchClaims` kernel `BenchClaim` roster and `BimBenchReceipt` record under the AppHost corpus-gate admission row.
[SURFACE_TEXTURE_INGEST]-[COMPLETE]: `Semantics/appearance#APPEARANCE_PROJECTION` carries IFC texture styles both ways — the `TextureMode` mode-to-canonical-channel roster with its gloss/transparency polarity column, the `SurfaceTexture` `[Union]` over the URL, blob, and pixel-extent payloads with the `UvTransform` frame, and `StyledAppearance` pairing the roster with the content-keyed node so the frozen seven-value summary preimage stays sealed; egress lands every element select through ONE five-slot `IfcSurfaceStyle` constructor whose null slots carry the absent axes.
[TEXTURE_COORDINATE_BINDING]-[COMPLETE]: `Semantics/appearance#APPEARANCE_PROJECTION` `SurfaceTexture` carries `CoordinateSet` universally with `At` the re-stamp and the egress writing it back through `IfcSurfaceTexture.Parameter`; `Exchange/tessellation#EXPLICIT_TESSELLATION` resolves the ordinal off `IfcTessellatedFaceSet.HasTextures`, so `Exchange/export#EXPORT_RAIL` binds a measured set onto a mesh that now carries `TEXCOORD_0`.
[APPEARANCE_ARITY_ALIGN]-[COMPLETE]: the Bim lowering composes the seam `AppearanceSummary.Of(…, Op key) -> Fin<AppearanceSummary>` arity — `SummaryOf` returns `Fin` under the projector's own `Op`, the phantom `tolerance: 0.0` named argument is gone, and the raw-IEEE-bit writer stays inside the seam factory where no caller can fork the shared dedup key.
[GLTF_TEXTURE_CHANNEL]-[COMPLETE]: `Exchange/export#EXPORT_RAIL` binds texture maps onto the pooled glTF material through the `GltfChannel` roster — `ChannelImage.Of` admits a CANONICAL channel name and refuses one glTF has no slot for, its row names the `KnownChannel` targets one `ImageBuilder` threads (the `orm` pack reaching both the occlusion and metallic-roughness references) and the `KhrExtension` the channel obliges, and `MaterialFinish` keys the material pool on the seam `AppearanceKey` and every bound map so a textured element never inherits an untextured neighbour's material; registration unions the payload's own obliged rows with the policy roster.
[PLATE_FLOOR_HOLES_LOWER]-[COMPLETE]: `FootprintPolygon` carries `Holes` with the seam `Area` Newell fold, `Dragonfly` fills `Room2D(floorHoles:)` per interior ring through the same `Open` normalization, and the honeybee `Face3D` lower passes hole loops — a courtyard, atrium, or lightwell subtracts from conditioned floor area at both arms; the `Rasm.Element` `[FOOTPRINT_INTERIOR_RINGS]` counterpart closed with it.
[IFC_VERTEX_COLOUR_LANE]-[COMPLETE]: the blocker was REFUTED by live probe — `IfcColourRgbList.mColourList` and `IfcIndexedColourMap.mColourIndex` bind through the `Semantics/appearance#APPEARANCE_PROJECTION` `IfcInternals` `[UnsafeAccessor]` capsule, so `Exchange/tessellation#EXPLICIT_TESSELLATION` `Decode` now fills `EncodingChannel.ColorRgba` from `HasColours`; the read landed as the shared `IndexedColour` value that also folds the lane back and authors the map, and the arm carried a shape correction with it — a per-FACE colour run and a per-triangle UV index run both address CORNERS, so either present switches the walk to a per-corner unwelded gather rather than bleeding one face's colour across a shared coordinate.
[IFC_TEXTURE_MAP_BINDING]-[COMPLETE]: `Semantics/appearance#APPEARANCE_PROJECTION` `Bind` closes the UV round trip the tessellation ingest opened — `IfcIndexedTriangleTextureMap` mints through the same accessor capsule, `Maps`/`TexCoords`/`MappedTo` and the per-triangle index triples land, and the `Binders` table drives every seam attribute lane onto its representation item, so the `ColorRgba` egress rides the same entry on public surface alone; both rows guard arity first, because a lane whose corner run walks off its own end authors a file that faults in the receiving application.
[GLTF_MATERIAL_FACTORS]-[COMPLETE]: `Exchange/export#EXPORT_RAIL` `MaterialFinish` carries the seam `AppearanceSummary` whole and `Author` is the ONE material mint — scene-linear base colour and opacity into `baseColorFactor` unencoded, metalness and roughness written on every material against the unity glTF defaults that otherwise render each element as rough metal, `AlphaMode.BLEND` off sub-unit opacity alone, `KHR_materials_transmission` off the `Transmissive` bit; the display `0xRRGGBBAA` the dotbim column reads derives through the appearance projector's own sRGB OETF, and the `InterchangePolicy` presets drop every extension row no finish column can fill.
