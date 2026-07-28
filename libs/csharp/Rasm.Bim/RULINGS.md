# [RASM_BIM_RULINGS]

`Rasm.Bim` rulings settle package-scoped decisions.

## [01]-[PACKAGES]

- `GeometryGymIFC_Core` sole-IFC-model-surface KEEP — do NOT consolidate the IFC model surface onto the admitted xBIM leaves: none carries the full IFC4.3 entity vocabulary with schema-versioned STEP/ifcXML/ifcJSON write the egress re-author rides, so consolidation trades the one `DatabaseIfc` authority for a capability hole the leaf packages cannot fill; reopens only on an xBIM release owning that full write surface.

## [02]-[SHAPE]

- Ingested `IfcClass`/`PredefinedType` tokens admit BARE at `SemanticProjector` ingress, validity deferring to the `Emit` egress gate [PREDEFINED_TOKEN_RULING] — ingress validation aborts a whole import on one unknown entity and forks the token vocabulary between ingress and egress.
- Every `IfcRel*` name, directionality, and inverse-attribute pair lives on `IfcRelKind` rows lowering onto the neutral `Relationship` edge, the typed case carrying only `SubKind` and `Generic` alone carrying wire-name and attribute bag [NEUTRAL_EDGE_RULING] — a typed `IfcRel*` seam case leaks GeometryGym below the seam and forks the neutral edge algebra.
- Content keys ride ONE kernel seed-zero hasher across every federation, solver, cache, and diff edge — a per-page hash, a second scheme, or a `Guid`-keyed join forks the content space Compute's content-addressing lane shares, and a downward `InterchangeIdentity` reference from Bim inverts the strata.
- Texture bindings ride BESIDE the neutral appearance summary and never inside it — `StyledAppearance` pairs the content-keyed `Node.Appearance` with its `SurfaceTexture` roster, so an IFC texture style survives ingest whole while the frozen seven-value `AppearanceSummary` preimage stays sealed; folding a map's mean into a scalar channel is the averaged-map defect and widening the summary forks the very dedup key this projector mints against, so a FIELD-valued presentation read grows a roster beside the summary and a SCALAR read grows an element-select arm onto it. Re-litigation opens only if the seam retires the frozen preimage.
- This branch CLASSIFIES and CARRIES texture payloads and decodes none — `TextureMode` resolves an IFC mode token to its canonical channel and its gloss/transparency polarity, `SurfaceTexture` carries the URL, blob, or pixel-extent payload, and the glTF rail binds bytes a texture owner already sealed through `TextureBuilder.PrimaryImage`; an image codec, a texel resample, or the polarity inversion executed here mints a second raster owner inside a host-neutral exchange leaf and hides the divergence behind a channel that still renders. Re-litigation opens only if a raster owner seats inside this package.
- Every glTF extension a payload obliges registers from the PAYLOAD, unioned with the policy roster — a bound KTX2, WebP, DDS, or transform-bearing map names its own `KhrExtension` row, so a caller who omitted the row from `InterchangePolicy.Extensions` still emits a registered extension rather than an unresolvable block; trusting the policy roster alone is the recurring move, and it fails silently because the writer emits the block either way. Re-litigation opens only if SharpGLTF registers every in-box extension unconditionally.
- The CANONICAL channel name is the only vocabulary crossing this package's texture surfaces, and every foreign channel space reaches it through one owning roster — `TextureMode` resolves an IFC mode token onto the canonical name and `GltfChannel` resolves that name onto its glTF `KnownChannel` targets, so a call site choosing a `KnownChannel` is the unowned correspondence both rosters exist to delete and a canonical name no row claims REFUSES at admission rather than lighting a nearest slot that still renders. One canonical name may target SEVERAL glTF channels — the `orm` pack is one image glTF reads through both an occlusion and a metallic-roughness reference — so the row carries a target list and a per-channel copy of identical bytes is the deleted form. Re-litigation opens only if the canonical channel roster stops being closed.
- The seam `AppearanceSummary` is the WHOLE factor source of an exported glTF material and each of its channels stays in its declared domain — the scene-linear base colour enters `baseColorFactor` unencoded while the display-referred `0xRRGGBBAA` column is its egress projection through the one sRGB OETF this package owns, metalness and roughness are written on EVERY material because the glTF factor defaults are both unity and an unwritten material renders as rough metal, sub-unit opacity alone selects `AlphaMode.BLEND`, and the `Transmissive` bit writes `KHR_materials_transmission` and never alpha mode. The recurring moves are packing the summary into a byte tint and feeding that tint to the linear factor, and reading transmission off a sub-unit alpha — each silently re-renders every exported element. Re-litigation opens only if the seam retires the neutral summary.
- A rostered glTF extension a producer cannot FILL is the deleted phantom — a policy preset declares write capability, the payload's own obliged rows are the truth, and the two union at registration, so a row naming a factor the seam summary drops or a scene object this rail never authors governs nothing while reading as support. A row lands WITH its arm and returns the moment a finish column or a scene arm carries its value. Re-litigation opens only if a preset stops being a capability declaration.
- Model identity is SPAN-grade, never a metric dimension [MODEL_SLOT_RULING] — models mint unbounded, so `rasm.bim.model` multiplies every instrument by the live model count while a sampler-thinned span carries it free; the slot carries the package namespace a sibling also needs, `BimTelemetry.Traced` stamps it from its OWN required argument — a slot left to caller discipline is the slot no caller stamps — and "just the active model" re-mints that cardinality behind a bounded-sounding qualifier.

## [03]-[COLLAPSE]

- (none)

## [04]-[STRUCTURE]

- (none)

## [05]-[PROCESS]

- (none)
