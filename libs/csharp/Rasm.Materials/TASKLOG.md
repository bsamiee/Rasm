# [MATERIALS_TASKLOG]

The open and closed work for the host-neutral materials owner, distilled from `IDEAS.md`. Each task card leads with a status marker — `[QUEUED]`/`[ACTIVE]`/`[BLOCKED]` open, `[COMPLETE]`/`[DROPPED]` closed — and names the exact sub-domain and file it lands in. One idea spawns one or more tasks; a task is scoped guidance, not a full spec.

## [1]-[OPEN]

[QUEUED] OpenPBR slab-tree reshape of `appearance/bsdf`.
- Re-shape the `bsdf#LAYERED_COMPOSITION` `LayeredBsdf` from the additive `Seq<LobeWeight>` convex-combination fold to the OpenPBR Surface 1.1 stack-of-slabs (fuzz slab, coat slab, thin-film modifier, conductor-vs-dielectric base substrate) with albedo-scaling layering operators, and align the `graph#MATERIAL_LIBRARY` `MaterialParameters` columns to the OpenPBR parameter groups.
- Integrate the OpenPBR slab operators against the existing `bsdf#LOBE_FAMILY` lobe kernels (the fuzz slab is a new lobe case, the coat-affects-base is a layering operator); compose Wacton.Unicolour for the scene-linear color and the `bsdf#SPECTRAL_UPSAMPLE` RGB→SPD edge.
- Boundary: internal to `appearance/bsdf`; the renderer shading contract crosses to `csharp:AppUi/viewport-pipeline#PATH_TRACE` and the `graph#MATERIAL_GRAPH` sink, never coupled by re-deriving lobe math at the consumer.
- Considerations: a fuzz slab is the one new closed-set lobe case the seven-lobe set lacks; the open question of wholesale OpenPBR adoption versus wire-projection target is framed at `bsdf#OPENPBR_SLAB_LAYERING` and resolves before the `LocalVector`-basis local-frame convention is ratified against the Adobe reference BSDF.

[QUEUED] Layout placement solver in `construction/layout`.
- Build the `construction/layout#ASSEMBLY_FOLD` placement projection: the `StationStep`/`StepCourse` station-stepped course fold (currently an empty `Seq` stub) producing the `Seq<Element>` of scalar `Placement` tuples, then the four queued `ConstructionFault`-railed fold stages (opening subtraction, corner/closure, arch placement, pier solving) and the `LayerSet` cumulative-thickness per-layer offset, each an extension of the one `ConstructionLayout.Resolve` fold over any `ProfileFamily`.
- No new external package; composes `masonry#PROFILE_FAMILY` `BondName.Course`/`CourseTemplate` for the course geometry, `assembly#ELEMENT_MODEL` `RunPathAlgebra` for path length/angle, and `assembly#MATERIAL_ASSIGNMENT` `LayerSet`/`ProfileSet` for the resolved buildup; the `Profile.Unit` columns drive the footprint run/rise.
- Boundary: internal to `construction/layout`, a sibling of `assembly`; the resolved `Layout` is portable scalar data the host boundary materializes at the future app root (never a `Rhino.Geometry` type here) and the appearance engine shades through `appearance/graph#MATERIAL_LIBRARY` by `MaterialId`.
- Considerations: the fold is immutable course-state, never a mutable placement-list accumulation; a CMU/timber/glazing run is the SAME `Resolve` over a different `Profile`, never a per-family layout; arch placement carries source-backed voussoir detailing constraints, and until a stage lands its run rails `ConstructionFault.Course` rather than emitting a degenerate course; a `BondKind.Generated` bond rails `ProfileFault.Bond` through `BondName.Course`, gated on the `masonry#GENERATED_BOND_INTERPRETER` probe.

[QUEUED] Steel `ProfileFamily` vocabulary in `profiles/steel`.
- Author the `profiles/steel` page as one `ProfileFamily.Steel` case with its `ProfileUnit` section-property projection (depth/flange/web/fillet/Ix/Sx) and the `ProfileCatalogue` steel rows.
- Integrate the AISC Shapes Database v16.0 (free, all standard steel shapes 1873-2016) as the row data source; compose the `Rasm` kernel `Dimension` for every length column and `profile#PROFILE_OWNER` for the one `Profile` shape.
- Boundary: internal to `profiles/`, a sibling of `masonry`; the member extrusion crosses to `construction/layout#ASSEMBLY_FOLD` through the same `Resolve` fold over the steel `Profile`, never a per-family layout.
- Considerations: align the section-property columns to the `IfcProfileDef` I-shape/U-shape/L-shape subtypes so a steel member round-trips as an `IfcMaterialProfileSet`; cmu/timber/glazing land as their own sibling pages by the same shape.

[QUEUED] cmu, timber, and glazing `ProfileFamily` vocabularies.
- Author `profiles/cmu` (cell/face-shell columns), `profiles/timber` (sawn/glulam/CLT lamella/grade columns), and `profiles/glazing` (IGU pane/spacer/frame columns), each one `ProfileFamily` case with its own `ProfileUnit` projection.
- Integrate the standard cmu cell/face-shell tables, the standard timber/CLT property tables, and the IGU pane/spacer/frame tables as row data; compose `profile#PROFILE_OWNER` and the `Rasm` kernel `Dimension`.
- Boundary: internal to `profiles/`, three sibling pages of `masonry`/`steel`; each member resolves through the same `construction/layout#ASSEMBLY_FOLD`.
- Considerations: each family is data, not a type — a new family is one `ProfileFamily` case carrying its unit vocabulary; the `IfcProfileDef` alignment per family is the wire shape.

[QUEUED] Measured-spectral grounding of `appearance/graph` library rows.
- Ground the `graph#MATERIAL_LIBRARY` conductor and dielectric rows in measured spectral data — conductor complex-IOR per band from refractive-index tables, and a measured-BRDF admission path through `bsdf#SPECTRAL_UPSAMPLE`.
- Integrate the EPFL RGL goniophotometer measured BRDFs (195-wavelength, brdf-loader format) and the refractive-index tables; compose Wacton.Unicolour `Spd`→XYZ for the round-trip and the existing `bsdf#SPECTRAL_UPSAMPLE` Smits upsampling.
- Boundary: internal to `appearance/`; the measured-BRDF loader is a data-import concern that produces `MaterialParameters` rows, never a second material surface.
- Considerations: the conductor F0 rows are the measured seed; the brdf-loader format admits through one `Spd` construction per band, and a saturated measured primary must round-trip in-gamut through the white-furnace + gamut sweep at `bsdf#WHITE_FURNACE_HARNESS`.

[QUEUED] MaterialX node-graph interchange for `appearance/graph` and `appearance/texture`.
- Map the `graph#MATERIAL_GRAPH` `AppearanceNode` cases and `PortValue` polarities onto the MaterialX 1.39 node categories and typed ports, add a `.mtlx` serialization contract, and admit the MaterialX standard-node library (improved Worley noise, color-ramp) as the canonical node vocabulary.
- No new managed package owns MaterialX content; the schema mapping is author-data over the existing node union; compose Wacton.Unicolour for the color ports and the Standard-Surface-to-OpenPBR translation graph (paired with the OpenPBR reshape task).
- Boundary: internal to `appearance/`; the `.mtlx` wire crosses to any DCC consumer, never coupling the node fold to a tool-specific encoding.
- Considerations: the node serialization pairs with the OpenPBR slab reshape (the translation graph targets OpenPBR); the `texture` `TextureSource` vocabulary aligns to the MaterialX texture node categories at `texture#MATERIALX_NODE_PARITY`.

[QUEUED] Catalogue the photometric UnitsNet unit enums in `.api/api-unitsnet.md`.
- The `appearance/photometric.md` fences assert `IlluminanceUnit.Lux`, `LuminanceUnit.CandelaPerSquareMeter`, `LuminousFluxUnit.Lumen`, `LuminousIntensityUnit.Candela`, and `IrradianceUnit.WattPerSquareMeter` as settled fence code, but `.api/api-unitsnet.md` catalogues only the 15 mechanical-quantity unit enums (`LengthUnit`/`PowerUnit`/… ) and omits the photometric/radiometric family, so those five enum members ride as unverified fence code against the planning-standard `.api`-truthfulness rule.
- Integrate the UnitsNet photometric/radiometric unit-enum surface into `api-unitsnet.md` (`IlluminanceUnit`, `LuminanceUnit`, `LuminousFluxUnit`, `LuminousIntensityUnit`, `IrradianceUnit`, and the `Illuminance`/`Luminance`/`LuminousFlux`/`LuminousIntensity`/`Irradiance` quantity structs) with their SI-base canonical members.
- Boundary: catalogue-only on the Materials `.api`; the `photometric` fences already name the members, so the catalogue ratifies them rather than changing the page.
- Considerations: until the catalogue lands, the photometric unit-enum members stay an `.api` verification gap on a finalized-looking page; the `PowerUnit.Watt` member the page also uses is already catalogued.

## [2]-[CLOSED]

None.
