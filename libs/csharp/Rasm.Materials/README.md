# [MATERIALS]

`Rasm.Materials` is the host-neutral AEC-domain owner of architectural materials across three domains: `profiles/` (one polymorphic `Profile` over a closed `ProfileFamily` axis — masonry realized, cmu/steel/timber/glazing planned), `appearance/` (one measured appearance engine — a node `MaterialGraph`, a closed seven-lobe `BsdfLobe` family, a `MaterialLibrary` row table, procedural texture and photometric admission), and `construction/` (the host-neutral element-to-assembly-to-layout data model, materials assigned by the IFC 4.3 layer-set/profile-set/constituent-set trichotomy, resolved to portable scalar placements). A material is a LIBRARY ROW, an appearance variation a NODE CASE, a lobe a `[Union]` CASE, a cross-section a `ProfileFamily` ROW, a layout the ONE placement fold — never a per-material or per-family type. The package composes the `Rasm` kernel for vector/dimension value-objects, consumes Wacton.Unicolour directly as its scene-linear/spectral color owner, and composes the Compute unit algebra at the seam, never re-minting a vector, a color space, or a unit owner. This README routes the `.planning/` design pages and lists every external package the folder uses; the sub-domain map and domain law live in `ARCHITECTURE.md`, the forward concept pool in `IDEAS.md`, and the work in `TASKLOG.md`.

## [1]-[ROUTER]

The design pages under `.planning/`, grouped by sub-domain.

- profiles: [profile](.planning/profiles/profile.md) · [masonry](.planning/profiles/masonry.md)
- appearance: [bsdf](.planning/appearance/bsdf.md) · [graph](.planning/appearance/graph.md) · [texture](.planning/appearance/texture.md) · [photometric](.planning/appearance/photometric.md)
- construction: [assembly](.planning/construction/assembly.md) · [layout](.planning/construction/layout.md)

## [2]-[PACKAGES]

Every external library the folder uses, planned or implemented, as a flat list. Versions are centralized in the one C# manifest and never pinned here; admissions land here from the folder's ideas and tasks.

[FUNCTIONAL_CORE]:
- LanguageExt.Core
- Thinktecture.Runtime.Extensions

[COLOR_SPECTRAL]:
- Wacton.Unicolour

[UNITS]:
- UnitsNet (the photometric quantity/unit enums — `IlluminanceUnit`/`LuminanceUnit`/`LuminousFluxUnit`/`LuminousIntensityUnit`/`IrradianceUnit` — the `photometric` author-kernel rescales to SI base)

[PROJECTS]:
- Rasm (kernel — `Vectors` value-objects: `VectorFrame`/`Direction`/`Dimension`/`UnitInterval`/`PositiveMagnitude`; `Domain` `Context`)
- Rasm.Compute (`QuantityFamily`/`UnitAlgebra`/`UnitPolicy`/`UnitEvidence` unit seam, composing the admitted `UnitsNet` quantity owner)
