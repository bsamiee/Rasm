# [MATERIALS]

`Rasm.Materials` is the host-neutral AEC-domain owner of architectural materials across four domains: `profiles/` (one polymorphic `Profile` over a closed `ProfileFamily` axis — masonry, steel, cmu, timber, and glazing realized), `appearance/` (one measured appearance engine — a node `MaterialGraph`, a closed seven-lobe `BsdfLobe` family lowered from the OpenPBR Surface 1.1 `SlabStack`, a `MaterialLibrary` row table grounded by the measured conductor complex-IOR table, procedural texture and photometric admission, the weathering aging operator, the measured-material acquisition import, and the OpenPBR/MaterialX material wire vocabulary the host-free peers decode), `construction/` (the host-neutral element-to-assembly-to-layout data model, materials assigned by the IFC 4.3 layer-set/profile-set/constituent-set trichotomy, resolved to portable scalar placements), and `physical-properties/` (the typed `MaterialProperty` engineering-property family — mechanical/thermal/acoustic/fire over the IFC `IfcMaterialProperties` set). A material is a LIBRARY ROW, an appearance variation a NODE CASE, a lobe a `[Union]` CASE, a cross-section a `ProfileFamily` ROW, a layout the ONE placement fold — never a per-material or per-family type. The package composes the `Rasm` kernel for vector/dimension value-objects, consumes Wacton.Unicolour directly as its scene-linear/spectral color owner, and composes the Compute unit algebra at the seam, never re-minting a vector, a color space, or a unit owner. This README routes the `.planning/` design pages and lists every external package the folder uses; the sub-domain map and domain law live in `ARCHITECTURE.md`, the forward concept pool in `IDEAS.md`, and the work in `TASKLOG.md`.

## [1]-[ROUTER]

The design pages under `.planning/`, grouped by sub-domain.

- profiles: [profile](.planning/profiles/profile.md) · [masonry](.planning/profiles/masonry.md) · [steel](.planning/profiles/steel.md) · [cmu](.planning/profiles/cmu.md) · [timber](.planning/profiles/timber.md) · [glazing](.planning/profiles/glazing.md)
- connection (planned): reinforcement · fastener · hanger — the `ConnectionItem` reinforcement/fastener/hanger/anchor catalogue, queued in `TASKLOG.md`
- appearance: [bsdf](.planning/appearance/bsdf.md) · [graph](.planning/appearance/graph.md) · [texture](.planning/appearance/texture.md) · [photometric](.planning/appearance/photometric.md) · [weathering](.planning/appearance/weathering.md) · [acquisition](.planning/appearance/acquisition.md) · [interchange](.planning/appearance/interchange.md) · finish (planned, the Kubelka-Munk pigment/coat-stack finish engine, queued in `TASKLOG.md`)
- construction: [assembly](.planning/construction/assembly.md) · [layout](.planning/construction/layout.md)
- physical-properties: [properties](.planning/physical-properties/properties.md)

## [2]-[PACKAGES]

Every external library the folder uses, planned or implemented, as a flat list. Versions are centralized in the one C# manifest and never pinned here; admissions land here from the folder's ideas and tasks.

[FUNCTIONAL_CORE]:
- LanguageExt.Core
- Thinktecture.Runtime.Extensions

[COLOR_SPECTRAL]:
- Wacton.Unicolour (scene-linear/spectral color owner; `Pigment`/`new Unicolour(Pigment[], double[])` Kubelka-Munk reflectance, `MapToPointerGamut`/`IsInPointerGamut` real-surface gamut, and `Simulate(Cvd)` drive the planned `appearance/finish` pigment engine and the accessibility-preview seam)
- Wacton.Unicolour.Datasets (named-colour lists, ColorChecker/Macbeth reference sets, perceptual colourmaps, the `ArtistPaint` Golden 19-pigment Kubelka-Munk reflectance set the planned `appearance/finish` engine mixes, and academic reference datasets only; observer CMFs, illuminant SPDs, and generic reflectance stay on the main `Wacton.Unicolour` owner)

[UNITS]:
- UnitsNet (the photometric quantity/unit enums — `IlluminanceUnit`/`LuminanceUnit`/`LuminousFluxUnit`/`LuminousIntensityUnit`/`IrradianceUnit` — the `photometric` author-kernel rescales to SI base)

[PROJECTS]:
- Rasm (kernel — `Vectors` value-objects: `VectorFrame`/`Direction`/`Dimension`/`UnitInterval`/`PositiveMagnitude`; `Domain` `Context`)
- Rasm.Compute (`QuantityFamily`/`UnitAlgebra`/`UnitPolicy`/`UnitEvidence` unit seam, composing the admitted `UnitsNet` quantity owner)
