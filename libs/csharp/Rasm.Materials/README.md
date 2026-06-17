# [MATERIALS]

`Rasm.Materials` is the host-neutral AEC-domain owner of architectural materials: ONE polymorphic `Profile` over a `ProfileFamily` growth axis (masonry/CMU/steel/timber/glazing — masonry realized first as the brick catalogue), ONE measured appearance engine (a node `MaterialGraph`, a closed `BsdfLobe` family, a 100+-row `MaterialLibrary`, procedural texture/photometric admission), and ONE host-neutral construction data model (elements → assemblies → layout as portable scalar placements). A material is a LIBRARY ROW, an appearance variation a NODE CASE, a lobe a `[Union]` CASE, a cross-section a `ProfileFamily` ROW, a layout the ONE placement fold — never a per-material/per-family type. The package composes the `Rasm` kernel for vector/dimension value-objects, the AppUi color-space axis and the Compute unit algebra at the seam, never re-minting either. The brick concept is preserved as the FIRST `Profiles` family (the archived masonry source informs the `Profile`/`Construction` owner shape); the `.planning/` pages are decision-complete blueprints an implementation agent transcribes. Owner-state and the rails/axes registry live in `ARCHITECTURE.md`; the realized capability list in `FEATURES.md`; open work in `TASKLOG.md`.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE]                                                                      | [OWNS]                                                                                                                          |
| :-----: | :------------------------------------------------------------------------- | :---------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | [Profiles/profile](Profiles/.planning/profile.md)                          | `Profile` polymorphic owner; `ProfileFamily` growth axis (masonry realized; CMU/steel/timber/glazing queued); masonry unit/coring/bond/orientation vocabulary; OOP boundary / FP-ROP internals |
|   [2]   | [Appearance/appearance-graph](Appearance/.planning/appearance-graph.md)    | `AppearanceNode` node union + total evaluation fold; `SurfaceShade`; `MaterialParameters` canonical row; `MaterialLibrary` 100+-row table; profile-assignment generalization |
|   [3]   | [Appearance/bsdf](Appearance/.planning/bsdf.md)                            | `BsdfLobe` closed lobe family + `LayeredBsdf` weighted fold; Fresnel + GGX/Trowbridge-Reitz + Smith masking; multi-scatter compensation; RGB→SPD upsampling; ACES RRT/ODT + scene-referred tone-map |
|   [4]   | [Appearance/texture-photometric](Appearance/.planning/texture-photometric.md) | `TextureUv` procedural sampling fold; `ProceduralNoise` author-kernel; `Photometric` light-unit admission; `EmissionSpectrum` blackbody/SPD emission |
|   [5]   | [Construction/construction](Construction/.planning/construction.md)         | `Element → Assembly → Layout` host-neutral data model; `Placement` scalar tuple; `RunPath` length algebra; the station/elevation course fold |

## [2]-[ADMISSIONS_RECORD]

The admissions ledger maps each package to its consuming page, `.api` catalogue, and admission status. Versions live in `Directory.Packages.props`; this table never carries a pin. Color and spectral conversion compose `Wacton.Unicolour` through the AppUi color-space seam; the BSDF lobe math, Fresnel, GGX, procedural noise, RGB→SPD upsampling, and ACES/scene-referred tone-map are author-kernel (no managed library owns them). `Profile`/`Construction` compose the `Rasm` kernel `Dimension`/`UnitInterval` value-objects only. `[STATUS]` is `catalogue-pending` until the `.api` catalogue lands; the `[CATALOGUE]` cell holds `—` until then; author-kernel concerns carry no package row.

| [INDEX] | [PACKAGE]         | [PAGE]              | [CATALOGUE] | [STATUS]          |
| :-----: | :---------------- | :------------------ | :---------: | :---------------- |
|   [1]   | Wacton.Unicolour  | bsdf                |      —      | catalogue-pending |
|   [2]   | Rasm (kernel)     | profile             |      —      | catalogue-pending |

## [3]-[PROOF_GATES]

Proof runs at the planned phase gate, not after each edit. `[RAIL]` names the owning rail; the executable command lives with that rail owner, never restated here.

| [INDEX] | [GATE]                | [RAIL]                              | [EVIDENCE]                                                                  |
| :-----: | :-------------------- | :--------------------------------- | :------------------------------------------------------------------------- |
|  [G1]   | locked restore        | Assay restore rail                 | clean closure; unchanged `packages.lock.json`                              |
|  [G2]   | API catalogue resolve | `assay api` doctor/resolve         | Unicolour + Rasm keys resolve; catalogue current                           |
|  [G3]   | static plan + build   | Assay static rail                  | routed closure compiles, zero `': error '` lines                          |
|  [G4]   | spec law-matrix       | Assay test rail (Materials target) | BSDF energy conservation, Fresnel reciprocity, GGX normalization, gamut laws hold; `Profile` admission + `Layout` course laws hold |
|  [G5]   | page diagram render   | local mermaid-cli                  | every `MaterialLibrary` row evaluates to an in-gamut `SurfaceShade`; every masonry `Profile` row resolves a `Layout`; page diagrams render |
