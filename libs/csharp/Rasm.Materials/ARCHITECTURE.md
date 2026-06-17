# [MATERIALS_ARCHITECTURE]

`Rasm.Materials` is the host-neutral AEC-domain owner of architectural materials across three sub-domains: `Profiles/` (one polymorphic `Profile` over a `ProfileFamily` axis), `Appearance/` (one measured appearance engine), `Construction/` (one host-neutral element→assembly→layout data model). Every cross-section is a `ProfileFamily` row, every appearance a node case, every material a library row, every lobe a closed-union case, every layout the one placement fold — never a per-material/per-family type. Mechanics live in the finalized sub-domain `.planning/` pages; this page is the atlas — the source tree (the build order), the owner registry (the one owner-state surface), dependency direction, cross-package seams, the boundaries, and the prohibitions.

## [1]-[SOURCE_TREE]

The package mirrors the three sub-domains, each an eventual source sub-tree. The build order is upward: the `Profiles/` cross-section vocabulary and the `Appearance/` lobe/graph engine land before `Construction/` (a construction assembly composes a `Profile` and the appearance engine shades a placed element). Each leaf is annotated with the owning `<sub-domain>/<page>#CLUSTER`; sub-folders group the flat file set by concern axis. The archived `Rasm.Materials.Bricks` masonry source is at the repo `.archive/` (tabula-rasa); its catalogue/layout logic informs the `Profiles`/`Construction` owner shape and is transcribed as masonry-family rows, never re-imported.

```text codemap
Rasm.Materials/
├── Profiles/
│   ├── Faults.cs                    # ProfileFault union (band 2300), ProfileKeyPolicy ordinal accessor — profile#PROFILE_OWNER
│   ├── ProfileFamily.cs             # ProfileFamily SmartEnum axis (masonry realized; cmu/steel/timber/glazing growth) — profile#PROFILE_OWNER
│   ├── Profile.cs                   # Profile polymorphic owner, ProfileId, ProfileUnit/ProfileStandard, Of/Lookup — profile#PROFILE_OWNER
│   ├── Masonry/
│   │   ├── Vocabulary.cs            # Coring/CoringClass/BondKind/Orientation/Cut/ClosureRule/SpecialShape masonry algebra — profile#PROFILE_FAMILY
│   │   ├── BondName.cs              # BondName template/generated catalogue, Course/Fits — profile#PROFILE_FAMILY
│   │   └── Catalogue.cs             # ProfileCatalogue masonry Profile rows (us/uk/din/au/is) — profile#PROFILE_FAMILY
├── Appearance/
│   ├── Faults.cs                    # MaterialFault union (band 2400), MaterialKeyPolicy ordinal accessor — bsdf#SHADING_FRAME
│   ├── Bsdf/
│   │   ├── Lobes.cs                 # BsdfLobe union, ShadingFrame, Fresnel, GGX/Trowbridge-Reitz NDF, Smith masking, multi-scatter, LayeredBsdf — bsdf#SHADING_FRAME, #MICROFACET_KERNEL, #LOBE_FAMILY, #LAYERED_COMPOSITION
│   │   └── Spectral.cs              # SpectralUpsample RGB→SPD, ToneMap ACES RRT/ODT + scene-referred, Unicolour scene-linear compose — bsdf#SPECTRAL_UPSAMPLE, #TONE_MAP
│   ├── Texture/
│   │   ├── Noise.cs                 # ProceduralNoise author-kernel (Perlin/Simplex/Worley/fBm) — texture-photometric#TEXTURE_UV
│   │   └── Sampling.cs             # TextureUv sampling fold, TextureSource union, AddressMode/FilterMode bands, triplanar — texture-photometric#TEXTURE_UV
│   ├── Photometric.cs               # Photometric admission fold (Compute QuantityFamily.Illuminance compose; luminance/radiance/luminous-flux author-kernel), EmissionSpectrum — texture-photometric#PHOTOMETRIC
│   ├── Parameters.cs                # MaterialParameters canonical row, value-object columns — appearance-graph#MATERIAL_LIBRARY
│   ├── Graph/
│   │   ├── Nodes.cs                 # AppearanceNode union, PortValue channel set, node evaluation arms — appearance-graph#MATERIAL_GRAPH
│   │   └── MaterialGraph.cs        # MaterialGraph record, total Evaluate fold, SurfaceShade, Compile/Shade rails, MaterialGraph.Default — appearance-graph#MATERIAL_GRAPH
│   └── MaterialLibrary.cs           # 100+-row parameter table, MaterialId keys, profile-assignment generalization — appearance-graph#MATERIAL_LIBRARY
└── Construction/
    ├── Faults.cs                    # ConstructionFault union (band 2350), ConstructionKeyPolicy ordinal accessor — construction#ELEMENT_MODEL
    ├── Element.cs                   # RunPath union, Placement scalar tuple, Element placed-unit shape, RunPathAlgebra length/angle — construction#ELEMENT_MODEL
    └── Layout.cs                    # Assembly run, Layout placement stream, JointPolicy, ConstructionLayout.Resolve course fold — construction#ASSEMBLY_FOLD
```

`Profiles/Faults.cs` (band 2300) and `Appearance/Faults.cs` (band 2400) land first in their sub-domains — every owner reads them. `Profiles/` lands its vocabulary owners (`ProfileFamily`, `Coring`, `Orientation`) before `Profile.cs` (the owner composes the settled vocabulary). `Appearance/Bsdf/Lobes.cs` precedes `Spectral.cs`; `Texture/Noise.cs` precedes `Sampling.cs`; `Parameters.cs` and the lobe vocabulary precede `Graph/`; `MaterialLibrary.cs` lands last in `Appearance/`. `Construction/Element.cs` precedes `Layout.cs` (the fold reads the placement/path algebra), and `Construction/` lands after `Profiles/` and `Appearance/` (a layout composes a `Profile` and shades through the appearance library).

## [2]-[OWNER_REGISTRY]

The single owner-state surface for the package across all three sub-domains. Implementation collapses to one owner per axis and one entrypoint family per rail; a cross-section is a `ProfileFamily` ROW, a material a `MaterialLibrary` ROW, an appearance variation a NODE CASE, a lobe a `[Union]` CASE, a layout the one `Resolve` FOLD — never a new surface; a public type outside these owner regions is the named defect. `[STATE]` is `FINALIZED` where the owner is a transcription-complete fence with no open gate, `SPIKE` where the owner is fence-complete but its proof carries a residual cross-page seam, cross-package admission, or queued depth-fill named in the page RESEARCH cluster; a SPIKE owner is fully shaped now, never a deferred surface. This is the ONLY place owner state lives — FEATURES, TASKLOG, and README route here.

| [INDEX] | [AXIS/RAIL]              | [OWNER]              | [KIND]                                                              | [CASES]                          | [PAGE#CLUSTER]                  |  [STATE]  |
| :-----: | :---------------------- | :------------------- | :---------------------------------------------------------------- | :------------------------------- | :----------------------------- | :-------: |
|   [1]   | profile family axis     | `ProfileFamily`      | `[SmartEnum<string>]` family-kind axis                            | masonry realized; cmu/steel/timber/glazing growth | profile#PROFILE_OWNER | SPIKE |
|   [2]   | profile owner           | `Profile`            | record + `ProfileUnit`/`ProfileStandard` columns, `Of`/`Lookup`   | one shape, all families          | profile#PROFILE_OWNER          | SPIKE     |
|   [3]   | profile fault family    | `ProfileFault`       | `[Union]` fault, band 2300                                        | dimension/coring/family/bond     | profile#PROFILE_OWNER          | FINALIZED |
|   [4]   | profile key policy      | `ProfileKeyPolicy`   | ordinal comparer accessor                                         | ordinal                          | profile#PROFILE_OWNER          | FINALIZED |
|   [5]   | masonry bond catalogue  | `BondName`           | `[SmartEnum<string>]` template/generated bond + `Course`/`Fits`   | template rows; generated probe   | profile#PROFILE_FAMILY         | SPIKE     |
|   [6]   | masonry unit vocabulary | `Coring`/`Orientation` | `[SmartEnum]`/`[Union]` void-class + course-orientation algebra | void classes; 6 orientations     | profile#PROFILE_FAMILY         | FINALIZED |
|   [7]   | shading frame           | `ShadingFrame`       | boundary capsule over `Vectors.VectorFrame`                       | local↔world transforms           | bsdf#SHADING_FRAME             | FINALIZED |
|   [8]   | material fault family   | `MaterialFault`      | `[Union]` fault, band 2400                                        | gamut/parameter/graph            | bsdf#SHADING_FRAME             | FINALIZED |
|   [9]   | material key policy     | `MaterialKeyPolicy`  | ordinal comparer accessor                                         | ordinal                          | bsdf#SHADING_FRAME             | FINALIZED |
|  [10]   | microfacet kernel       | `FresnelMode`        | Schlick + exact dielectric/conductor; GGX NDF + Smith masking    | 2 fresnel modes                  | bsdf#MICROFACET_KERNEL         | FINALIZED |
|  [11]   | lobe family             | `BsdfLobe`           | `[Union]` closed lobe family + per-lobe `Evaluate`/`Sample`/`Pdf` | 7 lobes                          | bsdf#LOBE_FAMILY               | FINALIZED |
|  [12]   | layered composition     | `LayeredBsdf`        | weighted-lobe fold + MIS-balanced sample/pdf                     | 1 fold, lobe weights per row     | bsdf#LAYERED_COMPOSITION       | SPIKE     |
|  [13]   | spectral upsample       | `SpectralUpsample`   | RGB→SPD coefficient kernel + Unicolour `Spd`→XYZ compose          | scene-linear admission           | bsdf#SPECTRAL_UPSAMPLE         | FINALIZED |
|  [14]   | tone map                | `ToneMap`            | static surface + `ToneOperator` table (ACES RRT/ODT + scene-referred) | 4 tone operators            | bsdf#TONE_MAP                  | FINALIZED |
|  [15]   | texture sampling fold   | `TextureUv`          | static fold over `TextureSource` `[Union]` + address/filter/noise bands | noise/checker/gradient/image/triplanar | texture-photometric#TEXTURE_UV | FINALIZED |
|  [16]   | procedural noise        | `ProceduralNoise`    | author-kernel over `NoiseKind` band                              | perlin/simplex/worley/fbm        | texture-photometric#TEXTURE_UV | FINALIZED |
|  [17]   | photometric admission   | `Photometric`        | static fold over `PhotometricQuantity` + `EmissionSpectrum`       | illuminance via Compute; luminance/radiance/luminous-flux author-kernel | texture-photometric#PHOTOMETRIC | SPIKE |
|  [18]   | appearance node family  | `AppearanceNode`     | `[Union]` over typed `PortValue` channel set                     | input/texture/math/mix/normal/bsdf-output | appearance-graph#MATERIAL_GRAPH | FINALIZED |
|  [19]   | appearance graph + fold | `MaterialGraph`      | record + total `Evaluate`/`Compile`/`Shade` fold + `SurfaceShade` | 1 graph fold, 1 Disney default   | appearance-graph#MATERIAL_GRAPH | FINALIZED |
|  [20]   | canonical material row   | `MaterialParameters` | record + value-object columns                                  | base/metal/rough/IOR/transmit/sheen/clearcoat/subsurface/aniso/emission | appearance-graph#MATERIAL_LIBRARY | FINALIZED |
|  [21]   | material library        | `MaterialLibrary`    | `FrozenDictionary<MaterialId, MaterialParameters>` table        | 31 seed rows of 100+             | appearance-graph#MATERIAL_LIBRARY | FINALIZED |
|  [22]   | element model           | `Element`            | record + `Placement`/`RunPath` host-neutral shape                | line/arc path, scalar placement  | construction#ELEMENT_MODEL     | SPIKE     |
|  [23]   | construction fault      | `ConstructionFault`  | `[Union]` fault, band 2350                                        | path/joint/course/opening        | construction#ELEMENT_MODEL     | FINALIZED |
|  [24]   | assembly layout fold    | `Layout`             | `Assembly` run + `ConstructionLayout.Resolve` course fold        | 1 fold, all families             | construction#ASSEMBLY_FOLD     | SPIKE     |

One rail per entrypoint, named in the return type: a `SurfaceShade`/`Unicolour` carrier where the result is total, `Fin<T>` where a band-2300 `ProfileFault`, band-2350 `ConstructionFault`, or band-2400 `MaterialFault` can route. `ProfileFamily`/`Profile`/`BondName` hold at SPIKE until the generated-bond interpreter and the cmu/steel/timber/glazing family depth-fill land (queued, not fence defects). `LayeredBsdf` holds at SPIKE until the `viewport-pipeline#PATH_TRACE` consumption split and owner-name reconciliation are ratified in the suite ledger. `Photometric` holds at SPIKE until `Luminance`/`LuminousFlux`/`LuminousIntensity` land as Compute `QuantityFamily` rows. `Element`/`Layout` hold at SPIKE until the opening/corner/arch/pier fold stages land (queued).

## [3]-[DEPENDENCY_DIRECTION]

| [INDEX] | [PROJECT]          | [MAY_REFERENCE_MATERIALS] | [MATERIALS_MAY_REFERENCE] | [BOUNDARY]                                          |
| :-----: | :----------------- | :-----------------------: | :-----------------------: | :------------------------------------------------- |
|   [1]   | `Rasm`             |            no             |        yes (compose)      | composes `Vectors.VectorFrame`/`Direction`/`Dimension`/`UnitInterval` value-objects |
|   [2]   | `Rasm.AppUi`       |            yes            |        yes (seam)         | viewport path tracer shades FROM `LayeredBsdf`; color-space axis composed |
|   [3]   | `Rasm.Compute`     |            no             |        yes (seam)         | `Photometric` composes the `QuantityFamily` unit algebra |
|   [4]   | host packages      |       library compose     |            no             | `Wacton.Unicolour` color/spectral composed at the seam |
|   [5]   | future APP root    |            yes            |            no             | materializes the host-neutral `Layout` `Placement` stream into host geometry |

`Materials` is AEC-DOMAIN: it references the kernel `Rasm` and, minimally, composes the AppUi color-space axis and the Compute unit algebra at the seam; it never re-mints a color axis, a unit owner, a vector primitive, or a dimension value-object, and never references a host-boundary package. The construction `Layout` is host-neutral portable data — the host materialization is the future APP root's concern, never an interior `Rhino.Geometry` type here. The brick concept is preserved as the first masonry `Profile` family generalized off the archived source; the appearance assignment of a `Profile` is an `Appearance/appearance-graph#MATERIAL_LIBRARY` `MaterialId` row.

## [4]-[SEAMS]

Cross-folder facts split by altitude: mechanics live at the named `Materials` cluster, consequences land at the consumer. Intra-package seams (between the three sub-domains) and cross-package seams (to AppUi/Compute) both record here; only genuinely cross-language facts route to the Tier-0 region-map.

| [INDEX] | [SEAM]                       | [MECHANICS_AT]                   | [CONSEQUENCE_AT]                                                              |
| :-----: | :--------------------------- | :------------------------------- | :--------------------------------------------------------------------------- |
|   [1]   | profile appearance assignment | profile#PROFILE_OWNER           | `Appearance/appearance-graph#MATERIAL_LIBRARY` resolves a `Profile.AppearanceId` `MaterialId` row; the appearance engine generalizes the assignment, no profile-specific appearance type |
|   [2]   | profile layout composition   | profile#PROFILE_FAMILY           | `Construction/construction#ASSEMBLY_FOLD` composes `BondName.Course`/`Orientation`; the layout fold reads the bond vocabulary, never re-minting it |
|   [3]   | construction host materialization | construction#ELEMENT_MODEL  | the future APP root turns each `Placement` scalar tuple into host geometry; the construction model holds no `Rhino.Geometry` type |
|   [4]   | color-space axis             | bsdf#SPECTRAL_UPSAMPLE           | `csharp:AppUi/custom-visuals#COLOR_SPACE_AXIS` (Wacton.Unicolour composed); Materials never re-mints a `ColourSpace` |
|   [5]   | path-trace shading           | bsdf#LAYERED_COMPOSITION         | `csharp:AppUi/viewport-pipeline#PATH_TRACE` shades from `LayeredBsdf.Evaluate`/`Sample`; never re-derives lobe math |
|   [6]   | photometric unit algebra     | texture-photometric#PHOTOMETRIC  | `csharp:Compute/units-boundary#QUANTITY_TABLE` `QuantityFamily.Illuminance`; admitting luminance/radiance/luminous-flux requires landing those rows first |

## [5]-[BOUNDARIES]

- `Rasm.Materials` is a DEEPENING target across three sub-domains: the masonry `Profile` catalogue is the realized capability (the brick concept generalized), and the appearance engine and construction layout fill out through the `.planning/` pages. It is NOT a merge candidate: `Materials` stays a package; `Profiles`, `Appearance`, and `Construction` are source-mirroring sub-domains, each with its own `.planning/`.
- A cross-section is a `ProfileFamily` row, never a type: there is exactly one `Profile` concept and one `ProfileFamily` growth axis. A `BrickProfile`/`SteelSection` class, a per-family `Profile` variant, or a per-bond layout method is the named defect; a new family is one `ProfileFamily` case carrying its unit vocabulary.
- A material is a `MaterialLibrary` row, never a type: exactly one material concept (`MaterialParameters`) and one evaluation surface (`MaterialGraph`). A `GoldMaterial`/`GlassMaterial` class, a `MetalFactory`/`PlasticFactory`, or a per-family graph variant is the named defect.
- The lobe family is CLOSED: every appearance is a weighted combination of the fixed `BsdfLobe` set. A new lobe is one `[Union]` case admitted only when no parameterization of the existing set reproduces the measured physics — and then serving ALL materials, never one.
- The construction model is HOST-NEUTRAL: a `Placement` is a scalar tuple, a `Layout` a `Seq<Element>` of placements, never a `Rhino.Geometry` curve/transform. The host materialization is the future APP root's concern; an interior host geometry type is the named defect. The layout is the ONE `Resolve` fold over any `ProfileFamily`, never a per-family layout owner.
- The color axis and unit owner are COMPOSED, never re-minted: the AppUi color-space axis (Wacton.Unicolour for construction/conversion/spectral) and the Compute `QuantityFamily` algebra cross the seam; no second `ColourSpace` wrapper, no package-local color enum, no interior quantity type past admission.

## [6]-[PROHIBITIONS]

The closed NEVER list — the deleted patterns the owner registry forecloses.

- NEVER a per-material or per-family type, class, or factory; a material is a `MaterialLibrary` row, a cross-section a `ProfileFamily` row, and a second material/profile surface is the named defect.
- NEVER a lobe per material; the `BsdfLobe` family is closed and serves all materials.
- NEVER a per-family or per-bond layout method; the `ConstructionLayout.Resolve` fold is the one layout owner over any `Profile`.
- NEVER a host geometry type inside the construction model; a `Placement` is a scalar tuple and the host materializes it at the future APP root.
- NEVER a re-minted color axis or a second `ColourSpace` wrapper; compose the AppUi color-space axis through the `COLOR_SPACE_AXIS` seam.
- NEVER a re-minted unit owner; `Photometric` composes the Compute `QuantityFamily.Illuminance` row and coerces once at admission; a quantity type never crosses an interior signature.
- NEVER a re-minted vector/dimension primitive; `ProfileUnit`/`Placement`/`ShadePoint` compose the `Rasm` kernel `Dimension`/`VectorFrame`/`UnitInterval` value-objects.
- NEVER hand-rolled color/spectral conversion Unicolour already owns; only the documented NOT_COVERED set (RGB→SPD, RRT/ODT, scene-referred tone-map) and the BSDF/Fresnel/GGX/noise math are author-kernel.
- NEVER a public surface beside the budgeted owners; a new appearance capability is a node case, a lobe case, a texture-source case, or a parameter column; a new cross-section is a `ProfileFamily` row; a new layout stage is a fold extension.
- NEVER a generic `IMaterial`/`IProfile`/`IAppearance` abstraction; `Profile`/`MaterialParameters` are the single shapes and `BsdfLobe` cases stay typed.
- NEVER a propagated NaN/sentinel shade or placement; out-of-gamut, non-finite, and degenerate results project to `Option`/`Fin` at the boundary through the banded fault unions.
