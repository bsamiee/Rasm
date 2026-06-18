# [MATERIALS_IDEAS]

The forward pool of higher-order concepts for the host-neutral materials owner, each grounded in the folder's domain and current platform capability. Open ideas drive the tasks in `TASKLOG.md`; a finished or dropped idea moves to `[2]-[CLOSED]` with a one-line disposition so it is never re-litigated.

## [1]-[OPEN]

[OPENPBR_SLAB_LAYERING]: Re-shape the `LayeredBsdf` composition from the additive Disney-seven-lobe weighting to the OpenPBR Surface 1.1 stack-of-slabs.
- Model the surface as a formal stack — a fuzz slab, a coat slab, a thin-film Fresnel modifier, and a base substrate mixing a conductor slab against a dielectric base (opaque glossy-diffuse plus subsurface versus translucent) — composed by albedo-scaling layering operators rather than the convex-combination weight fold the `bsdf#LAYERED_COMPOSITION` runs today, with the `graph#MATERIAL_LIBRARY` `MaterialParameters` columns aligned to the OpenPBR parameter groups (base, specular, transmission, subsurface, coat, fuzz, thin_film, emission, geometry).
- Unlocks the material model the whole CG/AEC ecosystem standardizes on (Autodesk/Adobe/Arnold/V-Ray/Houdini/USD), making `MaterialParameters` a direct OpenPBR vector and the appearance wire interchangeable with MaterialX, so a `MaterialLibrary` row round-trips to any DCC tool that speaks OpenPBR.
- Draws on OpenPBR Surface 1.1 (ASWF, Aug 2025) as the canonical physically-layered material standard, whose z-up local-frame conventions match the `bsdf` `LocalVector` basis; the open question of whether the engine adopts OpenPBR wholesale or treats it as the wire-projection target is framed at `bsdf#OPENPBR_SLAB_LAYERING` without pre-deciding.

[CONSTRUCTION_LAYOUT_SOLVER]: Complete the `construction/layout` placement projection so an assignment resolves to a real `Seq<Element>` placement stream across the full course-condition matrix.
- Author the `StationStep`/`StepCourse` station-stepped placement projection (cursor/sequence/station stepping, offset-fraction course shift, run/rise per `Orientation`) the `assembly#ELEMENT_MODEL` `Placement` tuple carries, then the four queued fold stages — opening subtraction (station/elevation interruptions plus edge-cut requests), corner endpoint/turn/closure conditions, arch placement (voussoir `Profile` over a `RunPath.Arc` station-normalized rule), and pier layout/closure — each a `ConstructionFault`-railed extension of the one `ConstructionLayout.Resolve` fold, plus the `LayerSet` cumulative-thickness per-layer offset that folds a wall buildup into a stacked placement stream.
- Unlocks the actual host-neutral construction output the appearance and BIM wires consume — today `StationStep` returns an empty `Seq` and the four conditions are unbuilt, so the layout owner resolves course counts but emits no placed units; the solver turns the realized `MaterialAssignment`/`Profile`/`RunPath` vocabulary into the portable `Placement` data a host materializes.
- Draws on the immutable course-state fold over the `Profile.Unit` columns and the `masonry#PROFILE_FAMILY` `CourseTemplate` bond geometry, generalizing the masonry footprint to any `ProfileFamily` so the same fold places CMU/timber/glazing runs; the arch and corner conditions draw on standard masonry/structural detailing constraints for the scalar placement geometry, never a host curve.

[STRUCTURAL_FAMILY_VOCABULARY]: Land the four queued `ProfileFamily` vocabularies as source-backed cross-section data.
- Author each family as one `ProfileFamily` case with its own `ProfileUnit` projection — steel from the AISC Shapes Database v16.0 (depth/flange/web/fillet/Ix/Sx), timber as sawn/glulam/CLT rows, cmu as cell/face-shell rows, glazing as IGU pane/spacer/frame rows — each on its own `profiles/<family>` page the way masonry carries `Coring`/`BondName`/`Orientation`.
- Unlocks a full cross-section catalogue so `construction` extrudes steel/timber/glazing members through the same `Profile` owner and the same `ConstructionLayout.Resolve` fold, with `IfcProfileDef`-shaped profiles on the wire and a `ProfileSet` assignment per member.
- Draws on the AISC Shapes Database v16.0 (free, all standard steel shapes 1873-2016) as the steel source and the standard timber/CLT property tables; only masonry is realized today, so the four siblings are the named growth axis.

[MEASURED_SPECTRAL_LIBRARY]: Ground the conductor and dielectric library rows in measured spectral data.
- Author conductor complex-IOR per band from refractive-index tables (started for gold/copper/aluminum) and admit measured isotropic spectral BRDFs (EPFL RGL goniophotometer, 195-wavelength, brdf-loader format) through the `bsdf#SPECTRAL_UPSAMPLE` `Spd` construction, replacing the hand-authored Acescg vectors of the unspecified `graph#MATERIAL_LIBRARY` rows.
- Unlocks spectrally-grounded materials that round-trip through the Unicolour SPD-to-XYZ pipeline already in the `bsdf` page and a path to admitting acquired real-world materials, so a library row carries measured provenance rather than an authored guess.
- Draws on the EPFL RGL measured-BRDF program as the goniophotometer spectral-reflectance route; the conductor F0 rows are the seed and the remaining rows are the unspecified surface the program grounds. Wacton.Unicolour.Datasets adds named-colour, ColorChecker/Macbeth, and academic reference sets for validation only — the observer CMFs, illuminant SPDs, and reflectance stay on the main `Wacton.Unicolour` owner, which Datasets does not carry.

[MATERIALX_GRAPH_INTERCHANGE]: Align the `AppearanceNode` union and `PortValue` set with the MaterialX 1.39 node-graph schema.
- Map the six node cases onto the MaterialX node categories and the `PortValue` polarities onto the MaterialX typed ports so a `MaterialGraph` serializes to and from `.mtlx`, and admit the MaterialX standard-node library (improved Worley noise, color-ramp nodes) as the canonical node vocabulary with the Standard-Surface-to-OpenPBR translation graph riding the same fold.
- Unlocks interchange with every DCC tool that speaks MaterialX and a wire format needing no bespoke encoding, turning the node graph into a consumer of an industry standard rather than a bespoke six-case vocabulary with no serialization contract.
- Draws on MaterialX 1.39.4 (Sep 2025) as the platform-neutral node-graph content schema the ecosystem standardized on, which defines the schema, the translation graphs, and the node library the `graph`/`texture` pages align to.

[PHYSICAL_PROPERTIES]: A `physical-properties` sub-domain owning the structural, thermal, acoustic, and fire material-property model over the IFC `IfcMaterialProperties` set.
- A typed `MaterialProperty` family (mechanical/thermal/acoustic/fire-resistance) keyed by `MaterialId`, each a quantified property the BIM federation and the construction solver read.
- Unlocks the non-appearance half of the material owner — a wall assembly carries its U-value, sound-transmission class, fire rating, and structural grade as typed quantities, so a material is a full engineering object rather than a shade.
- Draws on the IFC 4.3 `IfcMaterialProperties`/`IfcMaterialConstituentSet` property-set model and the UnitsNet quantity owner; the property family is one `[Union]`/policy-row carrier keyed by `MaterialId`, never a parallel per-discipline material surface.

[WEATHERING_AGING]: An `appearance/weathering` arm modeling time-evolved material appearance — patina, oxidation, soiling, and UV fade as a parameterized aging operator over the OpenPBR slab stack.
- An aging operator driven by an age parameter the appearance graph carries, so a library row carries its weathering trajectory rather than a single frozen state.
- Unlocks physically-plausible aged materials — a copper roof greens, a facade soils, and a coating chalks as a function of the age parameter the appearance graph drives.
- Draws on the procedural-weathering literature and the MaterialX node-graph aging operators; the aging rides the existing `appearance/graph#MATERIAL_GRAPH` node fold and the OpenPBR slab operators, never a second appearance surface.

[MATERIAL_ACQUISITION]: An `appearance/acquisition` arm admitting measured real-world material capture — the SVBRDF/measured-material import path producing `MaterialParameters` rows from acquired data.
- A capture-import pipeline distinct from the spectral grounding of existing library rows: an acquired SVBRDF or measured BRDF lands as a `MaterialParameters` row with measured provenance.
- Unlocks acquired-material round-trip — the appearance engine shades real captured materials rather than authored approximations.
- Draws on the EPFL RGL measured-BRDF program and the neural-SVBRDF acquisition shift; the acquisition is a data-import concern producing `MaterialParameters` over the existing `bsdf#SPECTRAL_UPSAMPLE` construction, never a second material owner.

## [2]-[CLOSED]

[IFC_MATERIAL_ASSIGNMENT]: Landed as the `construction/assembly` design page — the `MaterialAssignment` closed-union owner over the IFC 4.3 layer-set/profile-set/constituent-set trichotomy reached the decision-complete bar with a transcription-complete fence; the remaining work is the implementation task, not a forward concept. The downstream consumption (a `LayerSet`/`ProfileSet` resolving to a placement stream) lives under [CONSTRUCTION_LAYOUT_SOLVER].
