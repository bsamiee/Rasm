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
- Anchors: the kernel `Processing/flatten` `ChartAtlas` and its `ToTextureMesh` UV atlas are the exact upstream a texture-space rasterizer needs; `Parametric/surface` `UvTessellation` carries the parameterization; `Processing/sample` `SampleKind` blue-noise supplies the hemisphere draws the occlusion sweep already uses in its height-field form; `filter#PLANE_OP` `Dilate` closes the chart-gutter bleed a texture-space bake's own mip chain otherwise carries.
- Tension: a mesh-space bake makes the press a consumer of tessellated geometry, which the host-neutral boundary currently keeps entirely out of this folder — the subject must carry an already-flattened chart set as DATA rather than a host mesh, or the boundary moves.
- Ripple: follows `[ATLAS_PACKER]` — an atlased bake target reads the transform rows that card produces.

[TEXTILE_LEARNED_SCORER]-[BLOCKED]: Tileability grading gains a learned second opinion beside the deterministic spectral score.
- Capability: the tile gate's verdict widens from one frequency-domain periodicity measure to a pair — the deterministic score and a learned perceptual one — so a field that is spectrally periodic yet visually repetitive, or spectrally imperfect yet visually continuous, is graded on the axis a viewer reads.
- Shape: one optional scorer row on the tile gate carrying its model card, its verdict column joining the existing proof; lands in `libs/csharp/Rasm.Materials/.planning/Raster/tile.md` with its registry row in `libs/csharp/Rasm.Materials/.planning/Appearance/neural.md`.
- Unlocks: a tileability proof a human reviewer agrees with; the quality gate the ingest path needs before a third-party set is admitted as tileable.
- Anchors: the model registry already carries licence class, tensor contract, provider ladder, and residual ceiling as columns, so a scorer is a `ModelCard` row rather than a new inference surface; `TileProof` already carries the score it measured, so a second column is a widening rather than a fork.
- Arms: arm when a TexTile-class tileability scorer publishes weights whose OWN card declares a granting licence class and a fixed-shape tile contract — every surveyed scorer's weight card is silent, so admission is blocked on the card, not on the export.
- Tension: a learned score cannot be the SOLE gate — the deterministic measure is reproducible across machines and the learned one is not, so the pair must rule with the deterministic half authoritative or the tile proof stops being evidence.

[TEXT_TO_MATERIAL_SEAM]-[BLOCKED]: Text-prompted material generation enters as an external service whose output crosses the ingest gate like any third-party asset.
- Capability: a prompt-to-set generator becomes reachable WITHOUT becoming a registry row — the service produces files, the ingest classifier reads them under the same alias, convention, and probe law every foreign asset crosses, and the tile gate grades the result rather than trusting it.
- Shape: one service-source arm on the ingest input union carrying its provenance and licence evidence; lands in `libs/csharp/Rasm.Materials/.planning/Raster/set.md`.
- Unlocks: generative authoring for the operator without a local diffusion runtime or a weights custody problem; the third-party half of the ingest estate exercised by a first-party caller.
- Anchors: the folder ruling already places text-to-material outside the stage registry; `SetIngest.Classify` is total and pure over probes, so a service product is an input it already accepts; the provenance and licence columns the acquisition receipt carries are the evidence shape a service response fills.
- Arms: arm when a service contract with usable output licensing exists, or when a local generator whose output survives the tile coherence gate ships weights.
- Tension: a generated set's licence follows the SERVICE terms rather than a model card, so the receipt's grant column stops being derivable from the registry and must carry the response's own declaration.

[UDIM_STREAMING]-[QUEUED]: UDIM sets resolve tile by tile, so a hundred-tile film asset loads the tiles a view needs instead of the whole grid.
- Capability: set admission and binding widen from whole-grid residency to per-tile residency, so extent, memory, and decode cost scale with what is VISIBLE rather than with what exists — the difference between a production asset that opens and one that exhausts the arena on admission.
- Shape: a residency policy column on the set and a per-tile resolution on the bind fold; lands in `libs/csharp/Rasm.Materials/.planning/Raster/set.md` and `libs/csharp/Rasm.Materials/.planning/Raster/plane.md`.
- Unlocks: film and vfx asset scale for the ingest path; the tiled and chunked-window growth leg the plane arena already declares.
- Anchors: `UdimTile` already carries the Mari grammar and derives its grid coordinate; the plane arena declares its tiled window growth leg; the set content key already folds channel-ordered digests, so a per-tile digest is a preimage widening rather than a key redesign.
- Tension: per-tile residency makes the set key a function of what is RESIDENT unless the key stays over the full declared grid — the key must remain whole-set or two views of one asset address different blobs.

[MICROFACET_GENERIC_MATH]-[QUEUED]: The microfacet kernel generalizes over the numeric scalar, so exact dual-number Jacobians flow through the one forward model.
- Capability: `bsdf#MICROFACET_KERNEL` computes over any `T : INumber<T>, IRootFunctions<T>, IPowerFunctions<T>, IExponentialFunctions<T>, ILogarithmicFunctions<T>, ITrigonometricFunctions<T>` — the exact `Dual<T>` constraint set — with `double` the instantiation every existing lobe keeps, so the BRDF acquisition fit takes the kernel `Lm.Minimize`/`DualModel` exact-Jacobian lane instead of its hand-rolled central-difference Gauss-Newton loop.
- Shape: `LocalVector` becomes `LocalVector<T>` and `Ndf`/`Lambda`/`MaskingShadowing`/`Masking`/`FresnelSchlick`/`FresnelDielectric`/`FresnelConductor` become generic bodies at `Appearance/bsdf.md`; then `acquisition#ACQUISITION` lands `BrdfResidual : IDualResidual` over `Lm.Minimize(new DualModel(residual), SolvePolicy.Canonical, key)`, the `(Lo, Hi)` bounds riding a differentiable box reparameterization.
- Unlocks: the 12-iteration hand loop, its `0.5 * delta` damping, the QR/SVD fallback step, and the explicit central-difference `Jacobian` all delete; the `acquisition.md` `[EXPRESSION_SPINE]` exemption retires with the loop.
- Anchors: the kernel dual floor at `Rasm` `Solving/solver` (`IDualResidual.Row`, the exact-Jacobian-only boundary law); measured blast radius — 80 `LocalVector` sites in `bsdf.md`, 14 in `environment.md`, 7 in `acquisition.md`, 2 in `surface.md`, 28 `Microfacet.` call sites — so the rebuild lands whole, never half.
- Tension: the half-landed-vocabulary law — a partial generic rebuild leaves two scalar dialects in one kernel, so the pass is all-or-nothing across the four pages.

[ATLAS_PACKER]-[QUEUED]: `Raster/set` packs its own atlases, so N materials sharing one sheet is a produced artefact rather than an ingested convention.
- Capability: an atlas becomes a first-class product — a packing over N sets producing one plane per channel and the per-set UV transforms — while remaining a PLANE-level sharing fact, so each participating set keeps its own key and its own appearance identity and a texture edit re-keys exactly the sets that read it.
- Shape: a packing fold producing the shared planes and the per-set transform rows, beside the set owner; lands in `libs/csharp/Rasm.Materials/.planning/Raster/set.md`.
- Unlocks: draw-call reduction for the generated assemblies; the atlas half of the sharing law the set owner already states as a boundary.
- Anchors: the atlas boundary is already ruled — N sets referencing one blob by content address, never a set-level merge behind one appearance key — so the packer produces exactly that shape; the kernel `Processing/flatten` rules packing downstream of the chart atlas, so the packer is this folder's own fold over the kernel `ChartAtlas` operand; `filter#PLANE_OP` `Dilate` closes the inter-chart gutter bleed a packed sheet's mip chain otherwise carries.
- Ripple: precedes `[MESH_SPACE_BAKE]` — a mesh-space bake over an atlased target needs the transform rows this card produces.

[COAT_ANISOTROPY]-[QUEUED]: Coat layers carry their own grain, so a brushed-metal topcoat and an anisotropic clear lacquer shade their real highlight.
- Capability: coat-layer anisotropy joins the base layer's realized directional roughness, so a two-layer surface stops forcing its coat isotropic while its base shades a stretched lobe.
- Shape: one `coat_roughness_anisotropy` input threading `Slab.Coat` into the coat lobe's alpha pair; lands in `libs/csharp/Rasm.Materials/.planning/Appearance/surface.md` `[05]-[OPENPBR_SLAB]` and `libs/csharp/Rasm.Materials/.planning/Appearance/bsdf.md` `[04]-[LOBE_FAMILY]`.
- Unlocks: OpenPBR 1.1 coat-grain parity; the `geometry_coat_tangent` channel gains a shading consumer.
- Anchors: `Slab.Coat` and the `geometry_coat_tangent` channel are carriers already present; the `SlabStack.LowerBase` Disney aspect remap and the landed `BsdfLobe` `Rotation` radian column are the algebra to reuse; the collapsed one-`Alpha` `ThinFilm` states the honest isotropic floor the widening replaces.

[SKY_AS_PRESS_SUBJECT]-[QUEUED]: Synthesized domes are bake subjects, so a sky plane inherits the press engine's partitioning, cancellation, receipt, and accelerator lane instead of carrying its own sweep.
- Capability: one bake engine over every synthesized field, so a sky render is content-keyed by a `PressPlan` the same way a material bake is and its cost lands on the same receipt.
- Shape: one `PressSubject.Sky` case with its `PressProgram` arm at `libs/csharp/Rasm.Materials/.planning/Raster/press.md` `[03]-[TEXTURE_PRESS]`, `SkyRender.Render` collapsing to the per-texel radiance evaluation the band arm calls, at `libs/csharp/Rasm.Materials/.planning/Appearance/environment.md` `[02]-[SKY_MODEL]`.
- Unlocks: `IDEAS.md [MESH_SPACE_BAKE]` — one subject union covering every field the estate bakes.
- Anchors: `press#PRESS_PLAN` `PressSubject`/`LayerLaw.CubeFaces` (whose `Face(int face, double u, double v)` fold is already the frozen equirect correspondence the environment shares), `press#PRESS_RECEIPT` `PressProduct.Preview` for the accelerator lane, `gpu#WGSL_KERNEL` `equirectToCube`.
- Tension: the S1/S2 strata split — `Raster` is S1 and the `Appearance` frontier is S2, so the subject case may not name a `SkyModel` type directly; the case carries a `Func<WorldDirection, RgbSpectrum>` radiance closure the frontier supplies, exactly how `PressSubject.Source` carries a `TextureSource` without the press knowing a noise basis.

[CURVATURE_DRIVEN_WEAR]-[QUEUED]: Aging reads the surface's own shape, so an arris abrades and a gutter accumulates from the same trajectory.
- Capability: edge-wear, rain-streak, and deposit-shedding become rows of the existing effect table rather than authored masks.
- Shape: widen `CavityResponse.Scale` to `Scale(double age, SurfaceExposure exposure)` over a `SurfaceExposure(double Occlusion, double Curvature)` carrier, adding `Convex`/`Concave` rows beside `Crevice`/`Exposed`/`Uniform`; lands in `libs/csharp/Rasm.Materials/.planning/Appearance/weathering.md` `[02]-[WEATHERING]` with a second `Option<TextureSource> CurvatureField` beside the landed `CavityField` on `PressSubject.Slab` and one `Curvature` column on the landed `LadderRungs` carrier at `libs/csharp/Rasm.Materials/.planning/Raster/press.md` `[02]-[PRESS_PLAN]`.
- Unlocks: shape-aware weathering over the landed cavity chain; the signed `curvature` channel gains its aging consumer.
- Anchors: `set#TEXTURE_CHANNEL` `Curvature` already derives from `height` through `filter#PLANE_OP` `HeightDerivative.Curvature(CurvatureMeasure.Mean)`; the `WeatheringEffect` rows already carry their exposure law as a delegate column, so a fourth and fifth row cost zero dispatch edits; on a single occlusion scalar `Convex` is byte-identical to `Exposed`, which is why the pair waits for the second field this card owns.
- Ripple: the `PressSubject.Slab` field set and `LadderRungs` each widen once for the curvature axis, so this card shares the landed cavity carriers rather than adding parallel ones.
- Tension: `Convex` and `Crevice` are independent axes, so the honest cost is a three-dimensional ladder — the card must price the `LadderRungs` cell product against a per-texel `Apply` before landing.

[IBL_PREVIEW_PRODUCT]-[QUEUED]: IBL prefiltering gains its accelerator product shape, so a GPU dome preview exists without ever reaching a content address.
- Capability: the prefilter's accelerator lane produces a preview-class product structurally unable to reach `EnvironmentBlobs`, so the transcription-proven kernels gain a dispatching consumer under the CPU-mint veto.
- Shape: the preview split at the prefilter owner's grain in `libs/csharp/Rasm.Materials/.planning/Appearance/environment.md` `[04]-[IBL_PREFILTER]`, mirroring the `press#PRESS_RECEIPT` `PressProduct.Minted`/`Preview` structural split.
- Unlocks: the `IblPolicy.Backend` + `Prefilter(device)` seam already landed stops refusing its accelerator arm; an operator watches a dome resolve while the CPU mint runs.
- Anchors: `environment#IBL_PREFILTER` `Prefilter`'s total `(ContentAuthoritative, device)` switch names this product shape as its one unsettled decision; `gpu#WGSL_KERNEL` `prefilterSpecular`/`irradianceSh`/`equirectToCube` are fixture-proven and equirect-correct; the content-identity veto rules the GPU lane preview-only.

[MTLX_FLAKE_NODES]-[QUEUED]: MaterialX 1.39 flake and hex-tile node families gain corpus rows, so metallic-flake and anti-repetition texturing stop being unrepresentable sources.
- Capability: `flake2d`/`flake3d` (multi-output id/rand/presence/flake-normal — a real metallic-flake appearance capability) and `hextiledimage` (hex-lattice anti-repetition image tiling) become texture-source vocabulary instead of silent MaterialX-only nodes.
- Shape: candidate `TextureSource` cases or declared lossy-edge rows in `libs/csharp/Rasm.Materials/.planning/Appearance/texture.md` `[02]-[TEXTURE_UV]`, each answering the `MtlxNode` binding question its ingress direction poses.
- Unlocks: automotive-paint and large-surface material authoring the current source union cannot spell; the `.mtlx` ingress half of the 1.39 node set.
- Anchors: the `MtlxParameters` projection already routes per-node ports; the `[MTLX_UNIFIED_NOISE_INGRESS]` research row on the same page asks the ingress-direction question these nodes inherit.
- Tension: a multi-output node breaks the one-sample-one-`ShadeVec4` shape of the source union — the flake family may demand an output-selector column rather than a case per output, and that decision precedes any row.

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[OPENPBR_GROUPS_MAP_COLUMNS]-[DROPPED]: plane storage remains on `TextureSetWire` and sampling policy on the per-bind `UvFrame`; adding either to scalar `OpenPbrGroupsWire` would duplicate owner truth and lose pack, level, layer, UDIM, transfer, and coordinate-set semantics.
[TYPE_QUANTITY_RECEIPT]-[COMPLETE]: the seam co-sign was never a foreign ask — `Rasm.Element` gained three owner-declared `DetailSchema` takeoff statics under a `Takeoff` bag at `InheritanceMode.TypeDrivenOverride`, driving every occurrence off one Type-bound bag with no `Bake` edit, and a `Density` accessor total over both stiffness carriers; `TypeTakeoff` mints the set at `Projection/component#COMPONENT_PROJECTOR`, and `LinearDensity`/`VolumePerLength` being real UnitsNet registry quantities is what clears the dimension check a consumer mint answers `None` on.
[CATALOGUE_ANALYTICS_EGRESS]-[COMPLETE]: `Projection/analytics.md` rebuilt onto the custodian seam — the folder-local `ColumnType`/`ColumnRow`/`AnalyticsSchema` twin died, `ColumnToken`/`DatasetColumn`/`DatasetWire` declare the producer half of `[WIRE]: AnalyticsSchema` with `Admission` the crossing, and each of `MaterialsDatasets`' five rows declares `observed` as its spine beside the `gwp` and `elapsed_s` measures, so Series, Fleet, and Lake provision one declaration; the folds thread `ProjectionContext` and `PropertyColumn` carries its unit column with four dimensioned UnitsNet selectors.
[KERNEL_BENCH_PROFILE_CORPUS]-[COMPLETE]: landed as `Projection/benchmarks.md` `[03]-[GATE_COMPOSITION]` — `MaterialsBench.Fresh` projects each content-bound workload through `BenchmarkReceipt.Of` and `MaterialsBench.Gate` traverses the corpus through `BenchmarkGate.Gate`, with harness and claim residence injected; the `BenchWorkload` row joined `Rasm.AppHost/Observability/benchmarks#CLAIM_FIELD_MAP`.
[CMU_SUBTYPE_CARRIER]-[COMPLETE]: Ruled a realization-bag row — `CmuSeed.Rows` seeds the `DetailSchema.ProfileSubtype` token off `CmuPhysics.IfcSubtypeOf` (the family widened to `DetailLane.Realization`), the `Rasm.Bim` egress profile lane resolves the subtype from the carried row, never a `CmuRow` seed column and never a cross-package call.
[MATERIALS_SIGNAL_TAP]-[COMPLETE]: landed as kernel composition on `Projection/observability.md` — `MaterialsFact` family, the seven-point `MaterialsHooks` rail over the kernel capsule, the `MaterialsInstruments` `InstrumentSpec` roster with the contributor port, `MaterialsLog` and `MaterialsLatency`, and the `MaterialsDescriptors` pack over the kernel SLO algebra; the descriptor iac decode row stays open on `TASKLOG.md` `[SIGNAL_DESCRIPTOR_ROWS]`.
