# [MATERIALS_PLANNING]

Rasm.Materials is the suite APPEARANCE ENGINE: a world-class physically-based material system in which 100+ materials — metal, glass, plastic, skin, fabric, car paint, wax, ceramic, liquid — are PARAMETER ROWS over ONE node-based appearance graph, ONE closed BSDF lobe family, and ONE `MATERIAL_LIBRARY` data table, never per-material types. These pages are decision-complete blueprints an implementation agent transcribes — never re-designed downstream. The package owns measured appearance: a `MaterialGraph` evaluating typed appearance nodes to a `SurfaceShade`, the closed multi-lobe `BsdfModel` (Fresnel · GGX/Trowbridge-Reitz · multi-scatter compensation · transmission · sheen · clearcoat), the `MaterialLibrary` parameterization table, procedural `TextureUv` sampling, and `Photometric` light-unit admission — composing the AppUi display-color owner and the Compute unit algebra at the seam, never re-minting either.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE]                                              | [OWNS]                                                                                                                                                                                                       |
| :-----: | :------------------------------------------------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | [appearance-graph](appearance-graph.md)            | `MaterialGraph` node union + total evaluation fold; `AppearanceNode` typed node family (input/texture/math/mix/bsdf-assembly); `SurfaceShade` evaluation result; `MaterialLibrary` 100+ row parameter table; `MaterialParameters` canonical row shape; brick-assignment consumer generalization |
|   [2]   | [bsdf](bsdf.md)                                     | `BsdfModel` closed lobe family ([Union]: diffuse · dielectric-specular · conductor-specular · transmission · sheen · clearcoat · subsurface); Fresnel (Schlick + exact dielectric/conductor) author-kernel; GGX/Trowbridge-Reitz NDF + Smith masking-shadowing; multi-scatter energy compensation; RGB→SPD upsampling; ACES RRT/ODT + scene-referred tone-map author-kernels |
|   [3]   | [texture-photometric](texture-photometric.md)      | `TextureUv` procedural sampling fold (noise · checker · gradient · image · triplanar); `ProceduralNoise` author-kernel (Perlin/Simplex/Worley/fBm); `Photometric` light-unit admission composing the Compute `QuantityFamily.Illuminance` row and author-kernel raw-double radiometry for luminance/radiance/luminous-flux; `EmissionSpectrum` blackbody/SPD emission |

## [2]-[POLYMORPHIC_MANDATE]

THE LOAD-BEARING ACCEPTANCE. 100+ materials are PARAMETER ROWS in ONE `MaterialLibrary` data table over ONE `MaterialGraph` + ONE closed `BsdfModel` lobe family — never a per-material type, class, or factory. ANY conceivable material is a PARAMETERIZATION: a row of `MaterialParameters` values (base color, metalness, roughness, IOR, transmission, sheen, clearcoat, subsurface radius, anisotropy, emission) routed through the same graph evaluation and the same lobe set.

- A material is a ROW: `MaterialLibrary` is `[KIND]=table` with 100+ `MaterialParameters` rows keyed by `MaterialId` (`metal.gold`, `glass.crown`, `plastic.abs`, `skin.caucasian`, `fabric.velvet`, `paint.metallic-flake`, `wax.beeswax`, `ceramic.glazed`, `liquid.water`). Adding a material is adding a row.
- A material is NEVER a TYPE: there is exactly ONE material concept (`MaterialParameters`) and ONE evaluation surface (`MaterialGraph`). A second material type, a `GoldMaterial`/`GlassMaterial` class, a `MetalFactory`/`PlasticFactory`, or a per-family graph variant is THE NAMED DEFECT.
- THE ACCEPTANCE TEST: "any conceivable material — metal, glass, plastic, skin, fabric, car paint, wax — is a ROW, not a CLASS." A reviewer who cannot express a new material as a `MaterialLibrary` row without authoring a new type has found a defect, not a missing feature. The pressure to add a second material surface is the signal to add a parameter column to `MaterialParameters` and a row to `MaterialLibrary`.
- The lobe family is CLOSED: every appearance is a weighted combination of the fixed `BsdfModel` lobe set. A material differs only by lobe weights and parameters, never by a new lobe type added per material. A new lobe is admitted only when no parameterization of the existing closed set reproduces the measured physics — and then as one [Union] case serving ALL materials, never one material.

## [3]-[DENSITY_BAR]

Implementation collapses to one owner per axis and one entrypoint family per rail; density means no parallel rails, no near-duplicate shapes, no re-derived logic. A material is a ROW, an appearance variation is a NODE CASE, a lobe is a [Union] CASE — never a new surface. Dispatch runs over row data through generated total Switches and frozen tables. `[STATE]` carries `FINALIZED` where the owner is a transcription-complete fence with no open gate, `SPIKE` where the owner is fence-complete but its proof carries a residual numeric or spectral probe named in the page's RESEARCH cluster.

| [INDEX] | [AXIS/CONCERN]            | [OWNER]               | [KIND]                                             |              [CASES]              |  [STATE]  |
| :-----: | :------------------------ | :-------------------- | :------------------------------------------------- | :-------------------------------: | :-------: |
|   [1]   | Appearance graph + fold   | `MaterialGraph`       | record + total `Evaluate` fold over `AppearanceNode` |     `Evaluate`/`Compile`/`Shade`     |  PLANNED  |
|   [2]   | Appearance node family    | `AppearanceNode`      | [Union]                                            |  input/texture/math/mix/bsdf-assembly  |  PLANNED  |
|   [3]   | Material library          | `MaterialLibrary`     | table (`FrozenDictionary<MaterialId, MaterialParameters>`) |             100+ rows             |  PLANNED  |
|   [4]   | Canonical material row     | `MaterialParameters`  | record + value-object columns                       |   base/metal/rough/IOR/transmit/… |  PLANNED  |
|   [5]   | BSDF lobe union           | `BsdfModel`           | [Union] closed lobe family                          | diffuse/dielectric/conductor/transmit/sheen/clearcoat/subsurface | PLANNED |
|   [6]   | Texture sampling fold     | `TextureUv`           | static surface + `TextureSource` [Union]            |  noise/checker/gradient/image/triplanar  |  PLANNED  |
|   [7]   | Photometric admission     | `Photometric`         | static fold over `PhotometricQuantity` (illuminance via Compute `QuantityFamily.Illuminance`; luminance/radiance/luminous-flux as author-kernel raw doubles) | `Admit`/`Emission` — `Fin` |  PLANNED  |
|   [8]   | Material fault family      | `MaterialFault`       | [Union] fault, band 2400                            |       gamut/parameter/graph        |  PLANNED  |
|   [9]   | Material key policy        | `MaterialKeyPolicy`   | comparer accessor                                   |              ordinal              |  PLANNED  |

## [4]-[ADMISSIONS_RECORD]

Versions live in `Directory.Packages.props`; this table never carries a pin. `[STATUS]` in {admitted, author-kernel}.

| [INDEX] | [CONCERN]                                  | [OWNER]               | [SOURCE]                                            | [STATUS]      |
| :-----: | :----------------------------------------- | :-------------------- | :-------------------------------------------------- | :------------ |
|   [1]   | Color / spectral display conversion        | composed at seam      | `Wacton.Unicolour` 7.0.0 (AppUi/.api/api-unicolour.md) | admitted — COMPOSED |
|   [2]   | RGB→SPD spectral upsampling                | `BsdfModel`           | author-kernel (Smits 1999 / Jakob-Hanika 2019)      | author-kernel |
|   [3]   | ACES RRT / ODT tone-map operators          | `BsdfModel`           | author-kernel (RRT/ODT CTL-equivalent math)         | author-kernel |
|   [4]   | Scene-referred HDR tone mapping            | `BsdfModel`           | author-kernel (filmic/Reinhard/exposure)            | author-kernel |
|   [5]   | Fresnel + GGX/Trowbridge-Reitz + multi-lobe | `BsdfModel`           | author-kernel (no managed lib covers BSDF lobe math) | author-kernel |
|   [6]   | Procedural noise                           | `TextureUv`           | author-kernel (Perlin/Simplex/Worley/fBm)           | author-kernel |

Wacton.Unicolour 7.0.0 (MIT, decompiled at `Rasm.AppUi/.api/api-unicolour.md`) is COMPOSED, never re-minted: 40-member `ColourSpace` for construction/conversion, `RgbConfiguration.Acescg`/`.Aces20651`/`.Rec2100Pq` scene-linear presets feeding the linear-light shading pipeline, `Spd`→XYZ for measured illuminants, `DeltaE.Ciede2000`/`.Itp`/`.Z` for appearance match, and `.IsInRgbGamut`/`GamutMap.OklchChromaReduction` for gamut. The library's own NOT_COVERED list bounds the author-kernel scope exactly: RGB→SPD upsampling, ACES RRT/ODT, and scene-referred tone-mapping curves are absent from Unicolour and are authored from the published math; the BSDF lobe family, Fresnel, GGX, and procedural noise have no managed owner and are author-kernel.

## [5]-[COUPLES_TO]

Compose at the seam; never re-mint a coupled owner.

| [SEAM]                                              | [DIRECTION] | [LAW]                                                                                                                  |
| :------------------------------------------------- | :---------- | :------------------------------------------------------------------------------------------------------------------- |
| `Rasm.AppUi` custom-visuals#COLOR_SPACE_AXIS        | consumes    | The suite display-color axis (`ColorSpaceAxis`, SkiaSharp-`SKColorSpace`-backed for encode/working-space), with Wacton.Unicolour composed for construction/conversion/spectral through the SAME `COLOR_SPACE_AXIS` seam tag per `api-unicolour.md`. Materials NEVER re-mints a color axis or a second `ColourSpace` wrapper. |
| `Rasm.AppUi` viewport-pipeline#PATH_TRACE           | provides — PLANNED SEAM | The renderer SHADES FROM this `BsdfModel`. Materials owns the lobe evaluation contract; the viewport path tracer SHALL call `BsdfModel.Evaluate`/`Sample` and never re-derive lobe math. AppUi `PathTracePass` today owns an independent CPU reference path tracer as its correctness oracle; this is the planned seam AppUi adopts — ratify the `BsdfModel.Evaluate`/`Sample` consumption split in the suite ledger before AppUi's shading path routes through it. |
| `Rasm.Compute` units-boundary#QUANTITY_TABLE        | consumes    | `Photometric` composes the Compute `QuantityFamily` algebra for illuminance ONLY (`QuantityFamily.Illuminance` is the sole photometric/radiometric row that exists today); conversion runs once at admission and NEVER re-mints a unit owner. Luminance/radiance/luminous-flux route through the author-kernel as raw doubles. NOTED cross-package dependency: admitting those three as composed `QuantityFamily` rows requires landing `Luminance`/`LuminousFlux`/`LuminousIntensity` on the Compute `QuantityFamily` owner first (one row each, per its Growth rule). |
| `Rasm.Materials/Bricks` (appearance-ASSIGNMENT)     | generalizes | The brick `BrickDesignation`/`Coring`/`JointProfile` operator source is an appearance-ASSIGNMENT consumer the engine generalizes: a brick maps to a `MaterialId` row, never to a brick-specific material type. Bricks/ is NEVER modified by the engine. |

## [6]-[BUILD_ORDER]

Vocabulary owners first, then shapes, the graph fold, the lobe family, texture/photometric, dispatch, composition. The color seam resolves through the AppUi `COLOR_SPACE_AXIS` owner (consumed, never re-declared); the unit seam resolves through the Compute `QuantityFamily` owner. `MaterialParameters` and the lobe vocabulary land before `MaterialGraph` (the graph evaluates against settled parameter columns and the closed lobe set).

| [INDEX] | [FILE]                          | [CLUSTERS]                                                                                                      | [PROOF] |
| :-----: | :------------------------------ | :------------------------------------------------------------------------------------------------------------- | :------ |
|   [1]   | `Faults.cs`                     | material fault family, key policy                                                                              | static  |
|   [2]   | `Bsdf/Lobes.cs`                 | `BsdfModel` lobe union, Fresnel, GGX/Trowbridge-Reitz NDF, Smith masking, multi-scatter compensation           | specs   |
|   [3]   | `Bsdf/Spectral.cs`              | RGB→SPD upsampling, ACES RRT/ODT, scene-referred tone-map, Unicolour scene-linear composition                  | specs   |
|   [4]   | `Texture/Noise.cs`              | procedural noise (Perlin/Simplex/Worley/fBm)                                                                   | specs   |
|   [5]   | `Texture/Sampling.cs`           | `TextureUv` sampling fold, `TextureSource` union, triplanar                                                     | specs   |
|   [6]   | `Photometric.cs`                | photometric quantity admission (Compute `QuantityFamily.Illuminance` compose; luminance/radiance/luminous-flux author-kernel raw doubles), emission spectrum | specs   |
|   [7]   | `Parameters.cs`                 | `MaterialParameters` canonical row, value-object columns                                                       | specs   |
|   [8]   | `Graph/Nodes.cs`                | `AppearanceNode` union, node evaluation arms                                                                   | specs   |
|   [9]   | `Graph/MaterialGraph.cs`        | graph record, total `Evaluate` fold, `SurfaceShade`, compile/shade rails                                       | specs   |
|  [10]   | `MaterialLibrary.cs`            | 100+ row parameter table, `MaterialId` keys, brick-assignment generalization                                  | specs   |

## [7]-[PROOF_GATES]

Assay rows use `uv run python -m tools.assay`; proof runs at the planned phase gate, not after each edit.

| [GATE] | [RAIL]                         | [EVIDENCE]                                                          |
| :----: | :----------------------------- | :----------------------------------------------------------------- |
|  [G1]  | `dotnet restore` Materials     | lockfile unchanged; zero NU1004                                    |
|  [G2]  | `api doctor` + `api resolve`   | Unicolour key resolves; catalogue current                         |
|  [G3]  | `static plan` + `static build` | routed closure compiles                                            |
|  [G4]  | `test run` Materials target    | BSDF energy conservation, Fresnel reciprocity, GGX normalization, gamut laws hold |
|  [G5]  | G4 spec rail                   | every `MaterialLibrary` row evaluates to an in-gamut `SurfaceShade` |

## [8]-[PROHIBITIONS]

- [NEVER] mint a per-material TYPE, class, or factory — no `GoldMaterial`, `GlassMaterial`, `MetalFactory`, `PlasticFactory`, or per-family graph variant. A material is a `MaterialLibrary` row; a second material surface is the named defect.
- [NEVER] add a lobe per material; the `BsdfModel` lobe family is closed and serves all materials. A new lobe is one [Union] case admitted only when no parameterization reproduces the physics.
- [NEVER] re-mint the color axis; compose the AppUi `ColorSpaceAxis` owner through the `COLOR_SPACE_AXIS` seam (Wacton.Unicolour for construction/conversion/spectral) — no second `ColourSpace` wrapper, no package-local color space enum.
- [NEVER] re-mint a unit owner; `Photometric` composes the Compute `QuantityFamily.Illuminance` row and coerces once at admission (luminance/radiance/luminous-flux are author-kernel raw doubles until those rows land on `QuantityFamily`). A quantity type never crosses an interior signature.
- [NEVER] hand-roll color/spectral conversion Unicolour already owns (40 `ColourSpace`, `RgbConfiguration` presets, `DeltaE`, `Spd`→XYZ, gamut checks) — only the documented NOT_COVERED set (RGB→SPD, RRT/ODT, scene-referred tone-map) is author-kernel.
- [NEVER] modify `Rasm.Materials/Bricks/` or the `.csproj`/`packages.lock.json` — operator source; the engine generalizes the brick assignment consumer without touching it.
- [NEVER] add a public surface beside the budgeted owners; a new appearance capability is a node case, a lobe case, a texture source case, or a parameter column.
- [NEVER] introduce a generic `IMaterial`/`IAppearance` abstraction; `MaterialParameters` is the single material shape and `BsdfModel` cases stay typed.
- [NEVER] propagate NaN/sentinel shading outward; out-of-gamut and non-finite results project to `Option`/`Fin` at the boundary.
