# [MATERIALS_IDEAS]

Forward pool of higher-order concepts for the host-neutral materials owner, each grounded in the folder's domain and current platform capability. Open ideas drive the tasks in `TASKLOG.md`; a finished or dropped idea moves to `[2]-[CLOSED]` with a one-line disposition so it is never re-litigated.

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

[DESIGN_BASIS_AXIS]-[QUEUED]: One jurisdiction axis spans the capacity rail — EC3/EN 1994 steel and composite, EN 1996 masonry — beside the realized AISC/TMS verdicts.
- Capability: Capacity receipt columns folded under the governing code's resistance and interaction spellings, so the EN-seeded half of the catalogue (IPE/HE/UPN at S355) receives an EC3 verdict instead of an AISC one.
- Shape: one `DesignBasis` policy row selects per-basis kernels (EC3 §6.3.1 buckling curves with per-class imperfection α, §6.3.2 χLT, γM0/γM1 partial factors over the typed `VividOrange.Standards` `En1993`/`En1994`/`En1996` rows with `NationalAnnex` threaded); lands in `libs/csharp/Rasm.Materials/.planning/Component/capacity.md`, `libs/csharp/Rasm.Materials/.planning/Component/steel.md`, and `libs/csharp/Rasm.Materials/.planning/Component/masonry.md`.
- Unlocks: EU-deliverable member verdicts over the already-seeded two-region catalogue; the international sizing product the seed tables promise.
- Anchors: `steel#STEEL_FAMILY` `SteelDesign`/`DesignCapacity`, `capacity#SECTION_CAPACITY` `SectionCapacity`/`CapacityReceipt`, `SteelGrade.YieldMpa` already reading the Table 3.1 registered `f_y` per annex; the folder `RULINGS.md` basis-column row.
- Tension: the basis-tagged re-cut of the closed verdict vocabulary lands only with the `Rasm.Compute/Analysis/structural#DESIGN_CHECK` co-sign.

[ASSESSMENT_WIRE_INGESTION]-[QUEUED]: Materials mints a real end for the declared Assessment wire — typed in-situ assessment and declaration records lowered onto `Published<T>` evidence rows.
- Capability: assessment record vocabulary — in-situ test results, condition grades, dated declarations — admitted with provenance and expiry onto `Published<T>`/`PropertyEvidence` and folded into `MaterialPropertyCatalogue`/`SustainabilityCatalogue` rows as evidence-dated overrides beside the seed rows, so the `[WIRE]: Assessment` edge gains its owning page.
- Shape: one new page `libs/csharp/Rasm.Materials/.planning/Properties/assessment.md` — the record vocabulary, the admission fold, the assessed-over-published resolution law, and the EPD row landing surface `EPD_DATA_INGESTION` arms; earns the Properties folder its second non-eponymous sibling.
- Unlocks: assessed-condition property resolution over published seeds; a concrete landing owner for the EPD ingestion card.
- Anchors: `Properties/properties#MATERIAL_PROPERTY_CATALOGUE` `Published<T>`/`Admit`/`Lookup`, the realized `INormalDistributionUncertainty<T>` band discrimination on `Published<T>.Kind`, NodaTime `LocalDate` expiry, `ARCHITECTURE.md` `[WIRE]: Assessment` edge from `python:data`.
- Tension: wire record schema and transport are the `python:data` peer's to co-sign — the Materials-side vocabulary and fold proceed; the transport binding waits.

[PANEL_LATERAL_CAPACITY]-[BLOCKED]: Wood-structural-panel diaphragm and shear-wall unit shears as published rows feeding a lateral capacity case.
- Capability: SDPWS nominal unit shears by panel grade × thickness × nail × edge spacing — the columns `PanelRow` and `FastenPattern` already select — folded through the one `Check(demand)` rail as a lateral case, the seismic-versus-wind distinction riding the reduction the design basis applies rather than a second tabulated column.
- Shape: one published unit-shear table carrying a SINGLE nominal column per configuration sits beside `SpanRatings` with one `SectionCapacity` case and `Check` arm; the NDS `Cd` axis rides the connector-owned `DurationRow`; lands in `libs/csharp/Rasm.Materials/.planning/Component/panel.md` and `libs/csharp/Rasm.Materials/.planning/Component/capacity.md`.
- Unlocks: Sheathing selection as lateral design — the diaphragm/shear-wall dimension the member and wall rails do not cover.
- Anchors: `panel#PANEL_FAMILY` `FastenPattern`/`SpanRatings` printed-data precedent, `capacity#SECTION_CAPACITY` growth law naming the diaphragm check.
- Anchors: `SEED_ROW_LAW` licenses verbatim transcription of printed standards tables with per-column provenance — the EN 771/ASTM C216 masonry-unit rows and the frozen SDI rib geometry on this folder's own pages are the standing precedent, so custody is settled and possession alone blocks.
- Arms: the AWC SDPWS diaphragm and shear-wall tables in hand, read cell by cell from the publication.
- Tension: the current edition tabulates ONE nominal capacity per configuration where prior editions tabulated a wind value beside a seismic value, so the row shape carries a single nominal column and a re-derived pair forks the table it transcribes; a synthesized or recalled cell fails `SEED_ROW_LAW` outright.
- Ripple: `[SDPWS_TABLE_ACQUISITION]` acquires the cells.

[CONSTITUENT_SET_PRODUCER]-[BLOCKED]: Mix-design captures mint the first constituent-set producer — mix-design and multi-substance component captures.
- Capability: Fraction-tagged constituent rows derived from family data so a component's material truth stops flattening to one `SubstanceId` when the real product is a weighted composition (concrete mix, faced board, IGU frame-and-glass), and per-constituent carbon decomposes the EPD way.
- Shape: one Materials mix/constituent vocabulary supplies rows `CompositionOf` selects into the realized `CompositionAuthor.ConstituentSet`, the fourth seam case finally carrying traffic; lands in `libs/csharp/Rasm.Materials/.planning/Projection/component.md` and `libs/csharp/Rasm.Materials/.planning/Component/component.md`.
- Unlocks: `IfcMaterialConstituentSet` egress for multi-substance components; constituent-resolved carbon accounting.
- Anchors: `Projection/component#COMPOSITION_AUTHOR` `ConstituentSet` (realized, coerce-and-delegate), the seam `MaterialComposition.OfConstituentSet` fraction normalization owning the algebra — Materials mints instance rows at its own seam.
- Arms: a provenance-bound constituent-fraction data source lands — family mix-design columns or EPD-grade ingestion rows.
- Tension: no family's canonical composition is constituent-shaped today — the fraction data itself is the bet, not the vocabulary custody.

[FABRICATION_SCHEDULE_WIRE]-[BLOCKED]: Shop-deliverable schedules derived from the realization vocabularies — bar bending schedules, weld maps, stud layouts.
- Capability: Per-pour/per-member aggregation of the realized per-component scalars (BS 8666 shape codes, ACI/EN bend receipts, weld prep and stud spacing) into the contract documents a fabricator is paid against.
- Shape: Materials owns the typed scalars (realized — `RebarSchedule`/`RebarBend`/`ShapeCodes`, `WeldProfile`/`GroovePrep`/`StudRow`); `Rasm.Fabrication` owns the derivation into schedules over the projected `DetailSchema.Realization` bags and its registered `IElementProjection` row; the Materials audit lands in `libs/csharp/Rasm.Materials/.planning/Component/reinforcement.md` and `libs/csharp/Rasm.Materials/.planning/Component/joint.md`.
- Unlocks: Fabrication-deliverable pipeline — realization detail as shop documents, not graph metadata.
- Anchors: `reinforcement#REINFORCEMENT_FAMILY` host-neutral scalar law, `joint#JOINT_FAMILY` receipts, `Projection/component#COMPONENT_PROJECTOR` detail-bag authoring.
- Tension: Schedule derivation is `Rasm.Fabrication`'s process-derivation surface — a cross-package build this folder cannot land; the Materials end is realized and waiting on the Fabrication counterpart card.

[IFC_PRODUCT_LIBRARY_ADMISSION]-[BLOCKED]: Reverse type path — admitting an ingested IFC product library into `ComponentRow` candidates.
- Capability: manufacturer IFC content (curtain-wall system, proprietary deck, hanger range) minting `Component` rows through the railed `Of` factories, so authored seeds and admitted imports share one `ComponentId` space.
- Shape: one admission fold lowers reconciled `IfcElementType` data onto family-dispatched `Component.Of` construction, imported rows provenance-marked beside the seed rows; lands in `libs/csharp/Rasm.Materials/.planning/Component/component.md`.
- Unlocks: Two-way materials library — vendor BIM content as first-class catalogue rows.
- Anchors: Per-family round-trips already frozen (`SteelClass.IfcSubtype` `ProfileSet`, the `IfcMaterialLayerSet` ply round-trip, the `DetailSchema` bag round-trips), `component#CATALOGUE` fail-loud admission.
- Tension: Declared `[PORT]: IIfcTypeReconciler` reconciles ingested types AGAINST Materials-minted ids only — the reverse mint is a Bim-side ingest extension with a provenance-marking decision on `ComponentRow` the `Rasm.Bim` counterpart card must co-sign.

[EPD_DATA_INGESTION]-[BLOCKED]: Database-backed environmental vectors replacing the authored generic-EPD constants.
- Capability: EC3/Ökobaudat/EPD-Norge records per actual product — declared units, module coverage, expiry — keyed to `MaterialId`/component designations, the authored per-kg bases and fixed A4-D fractions demoted to the declared fallback.
- Shape: one peer-ingested EPD row shape lands on the Materials side with `PropertyEvidence` the per-row identity carrier (its `Option<LocalDate>` expiry axis already realized), the ingestion arriving over the declared `[WIRE]: Assessment` edge; lands in `libs/csharp/Rasm.Materials/.planning/Properties/assessment.md` and `libs/csharp/Rasm.Materials/.planning/Properties/sustainability.md`.
- Unlocks: Reportable whole-life carbon — audited submissions over evidence-dated records instead of illustrative constants.
- Anchors: `glazing#GLAZING_FAMILY` `GlazingGwp`/`GenericEpd`, the Properties sustainability rows, the `ARCHITECTURE` `[WIRE]: Assessment` edge.
- Tension: Wire record schema and transport are the `python:data` peer's to co-sign via its counterpart card, and EPD records are external provenance-bound data — no Materials-local table can seed them.

[MAGICK_BREADTH]-[BLOCKED]: Raster codec coverage widens to the container families no admitted managed codec reaches — AVIF, HEIF, JPEG XL, and the legacy DPX and Cineon plates.
- Capability: one ingest breadth tier behind the same format-keyed codec dispatch, so an asset arriving in a container the managed estate cannot decode admits without a caller-side conversion step or a silent refusal.
- Shape: one breadth engine row beside the existing managed engines, reached only where the sniffed container matches no managed row; lands in `libs/csharp/Rasm.Materials/.planning/Raster/codec.md`.
- Unlocks: asset-library ingestion over vendor and archive containers; the delivery-format half of the raster estate rather than the authoring half alone.
- Anchors: `Magick.NET-Q16-HDRI-AnyCPU` ships an osx-arm64 native and an HDRI-configured Q16 build; the folder's codec dispatch already sniffs magic bytes and carries an engine column, so a breadth engine is a row rather than a second entrypoint.
- Arms: the runtime-verified delegate roster of the shipped native proves AVIF, HEIF, and JXL decode present on osx-arm64 — a `Magick.NET` build advertises formats its bundled delegates may omit, so the format list is measured, never read off the package description.
- Tension: a native-bearing breadth tier buys containers at the cost of a P/Invoke surface and a per-RID asset the whole managed estate currently avoids, so the tier admits only where the container set it alone reaches is genuinely demanded, and every authored product still egresses through a managed engine.

[MESH_SPACE_BAKE]-[QUEUED]: Bakes address a MESH's own texture space, so occlusion, curvature, and thickness derive from real geometry instead of from a height plane's guess.
- Capability: a bake target widens from a UV rectangle to a parameterized surface with its atlas, so the fields a height-field derivation can only approximate — true ambient occlusion against the whole body, signed thickness through it, curvature off the real second fundamental form — become measured rather than inferred, and a component's baked set stops being a texture that merely resembles its geometry.
- Shape: one bake-subject case carrying the flattened chart set and its ray target, beside the existing graph, source, and slab subjects; lands in `libs/csharp/Rasm.Materials/.planning/Raster/press.md` with its derived-channel origins in `libs/csharp/Rasm.Materials/.planning/Raster/set.md`.
- Unlocks: component-accurate occlusion and curvature for the generated assemblies the projector already mints; the geometry half of the texture estate rather than the procedural half alone.
- Anchors: the kernel `Processing/flatten` `ChartAtlas` and its `ToTextureMesh` UV atlas are the exact upstream a texture-space rasterizer needs; `Parametric/surface` `UvTessellation` carries the parameterization; `Processing/sample` `SampleKind` blue-noise supplies the hemisphere draws the occlusion sweep already uses in its height-field form.
- Tension: a mesh-space bake makes the press a consumer of tessellated geometry, which the host-neutral boundary currently keeps entirely out of this folder — the subject must carry an already-flattened chart set as DATA rather than a host mesh, or the boundary moves.

[TEXTILE_LEARNED_SCORER]-[BLOCKED]: Tileability grading gains a learned second opinion beside the deterministic spectral score.
- Capability: the tile gate's verdict widens from one frequency-domain periodicity measure to a pair — the deterministic score and a learned perceptual one — so a field that is spectrally periodic yet visually repetitive, or spectrally imperfect yet visually seamless, is graded on the axis a viewer actually reads.
- Shape: one optional scorer row on the tile gate carrying its model card, its verdict column joining the existing proof; lands in `libs/csharp/Rasm.Materials/.planning/Raster/tile.md` with its registry row in `libs/csharp/Rasm.Materials/.planning/Appearance/neural.md`.
- Unlocks: a tileability proof a human reviewer agrees with; the quality gate the ingest path needs before a third-party set is admitted as tileable.
- Anchors: the model registry already carries licence class, tensor contract, provider ladder, and residual ceiling as columns, so a scorer is a `ModelCard` row rather than a new inference surface; `TileProof` already carries the score it measured, so a second column is a widening rather than a fork.
- Arms: arm when a TexTile-class tileability scorer publishes weights under a licence class the registry grants.
- Tension: a learned score cannot be the SOLE gate — the deterministic measure is reproducible across machines and the learned one is not, so the pair must rule with the deterministic half authoritative or the tile proof stops being evidence.

[TEXT_TO_MATERIAL_SEAM]-[BLOCKED]: Text-prompted material generation enters as an external service whose output crosses the ingest gate like any third-party asset.
- Capability: a prompt-to-set generator becomes reachable WITHOUT becoming a registry row — the service produces files, the ingest classifier reads them under the same alias, convention, and probe law every foreign asset crosses, and the tile gate grades the result rather than trusting it.
- Shape: one service-source arm on the ingest input union carrying its provenance and licence evidence; lands in `libs/csharp/Rasm.Materials/.planning/Raster/set.md`.
- Unlocks: generative authoring for the operator without a local diffusion runtime or a weights custody problem; the third-party half of the ingest estate exercised by a first-party caller.
- Anchors: the folder ruling already places text-to-material outside the stage registry; `SetIngest.Classify` is total and pure over probes, so a service product is an input it already accepts; the provenance and licence columns the acquisition receipt carries are the evidence shape a service response fills.
- Arms: arm when a service contract with usable output licensing exists, or when a local generator whose output survives the tile coherence gate ships weights.
- Tension: a generated set's licence follows the SERVICE terms rather than a model card, so the receipt's grant column stops being derivable from the registry and must carry the response's own declaration.

[UDIM_STREAMING]-[QUEUED]: A UDIM set resolves tile by tile, so a hundred-tile film asset loads the tiles a view needs instead of the whole grid.
- Capability: set admission and binding widen from whole-grid residency to per-tile residency, so extent, memory, and decode cost scale with what is VISIBLE rather than with what exists — the difference between a production asset that opens and one that exhausts the arena on admission.
- Shape: a residency policy column on the set and a per-tile resolution on the bind fold; lands in `libs/csharp/Rasm.Materials/.planning/Raster/set.md` and `libs/csharp/Rasm.Materials/.planning/Raster/plane.md`.
- Unlocks: film and vfx asset scale for the ingest path; the tiled and chunked-window growth leg the plane arena already declares.
- Anchors: `UdimTile` already carries the Mari grammar and derives its grid coordinate; the plane arena declares its tiled window growth leg; the set content key already folds channel-ordered digests, so a per-tile digest is a preimage widening rather than a key redesign.
- Tension: per-tile residency makes the set key a function of what is RESIDENT unless the key stays over the full declared grid — the key must remain whole-set or two views of one asset address different blobs.

[ATLAS_PACKER]-[QUEUED]: The estate packs its own atlases, so N materials sharing one sheet is a produced artefact rather than an ingested convention.
- Capability: an atlas becomes a first-class product — a packing over N sets producing one plane per channel plus the per-set UV transforms — while remaining a PLANE-level sharing fact, so each participating set keeps its own key and its own appearance identity and a texture edit re-keys exactly the sets that read it.
- Shape: a packing fold producing the shared planes and the per-set transform rows, beside the set owner; lands in `libs/csharp/Rasm.Materials/.planning/Raster/set.md`.
- Unlocks: draw-call reduction for the generated assemblies; the atlas half of the sharing law the set owner already states as a boundary.
- Anchors: the atlas boundary is already ruled — N sets referencing one blob by content address, never a set-level merge behind one appearance key — so the packer produces exactly that shape; the kernel `Processing/flatten` chart packing is the same bin-packing problem already solved once in the estate.
- Ripple: precedes `[MESH_SPACE_BAKE]` — a mesh-space bake over an atlased target needs the transform rows this card produces.

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[TYPE_QUANTITY_RECEIPT]-[COMPLETE]: the seam co-sign was never a foreign ask — `Rasm.Element` gained three owner-declared `DetailSchema` takeoff statics under a `Takeoff` bag at `InheritanceMode.TypeDrivenOverride`, driving every occurrence off one Type-bound bag with no `Bake` edit, and a `Density` accessor total over both stiffness carriers; `TypeTakeoff` mints the set at `Projection/component#COMPONENT_PROJECTOR`, and `LinearDensity`/`VolumePerLength` being real UnitsNet registry quantities is what clears the dimension check a consumer mint answers `None` on.
[CATALOGUE_ANALYTICS_EGRESS]-[COMPLETE]: `Projection/analytics.md` rebuilt onto the custodian seam — the folder-local `ColumnType`/`ColumnRow`/`AnalyticsSchema` twin died, `ColumnToken`/`DatasetColumn`/`DatasetWire` declare the producer half of `[WIRE]: AnalyticsSchema` with `Admission` the crossing, and each of `MaterialsDatasets`' five rows declares `observed` as its spine beside the `gwp` and `elapsed_s` measures, so Series, Fleet, and Lake provision one declaration; the folds thread `ProjectionContext` and `PropertyColumn` carries its unit column with four dimensioned UnitsNet selectors.
[KERNEL_BENCH_PROFILE_CORPUS]-[COMPLETE]: landed as `Projection/benchmarks.md` `[03]-[GATE_COMPOSITION]` — `MaterialsBench.Fresh` projects each content-bound workload through `BenchmarkReceipt.Of` and `MaterialsBench.Gate` traverses the corpus through `BenchmarkGate.Gate`, with harness and claim residence injected; the `BenchWorkload` row joined `Rasm.AppHost/Observability/benchmarks#CLAIM_FIELD_MAP`.
[CMU_SUBTYPE_CARRIER]-[COMPLETE]: Ruled a realization-bag row — `CmuSeed.Rows` seeds the `DetailSchema.ProfileSubtype` token off `CmuPhysics.IfcSubtypeOf` (the family widened to `DetailLane.Realization`), the `Rasm.Bim` egress profile lane resolves the subtype from the carried row, never a `CmuRow` seed column and never a cross-package call.
[MATERIALS_SIGNAL_TAP]-[COMPLETE]: landed as kernel composition on `Projection/observability.md` — `MaterialsFact` family, the seven-point `MaterialsHooks` rail over the kernel capsule, the `MaterialsInstruments` `InstrumentSpec` roster with the contributor port, `MaterialsLog` and `MaterialsLatency`, and the `MaterialsDescriptors` pack over the kernel SLO algebra; the descriptor iac decode row stays open on `TASKLOG.md` `[SIGNAL_DESCRIPTOR_ROWS]`.
