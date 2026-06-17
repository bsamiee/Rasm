# [MATERIALS_FEATURES]

The realized AEC-domain capability list across the three sub-domains. Every feature is a row or case on a budgeted owner, never a new surface; mechanics live at the `.planning/` page#cluster anchor named on each row, and the owner's realization state is read from `ARCHITECTURE.md` `[OWNER_REGISTRY]`. The masonry `Profile` catalogue is the realized cross-section vocabulary (the brick concept generalized as the first family); the appearance engine and the construction layout are the designed deepening.

## [1]-[PROFILES]

The polymorphic cross-section owner and its family growth axis — a cross-section is a `ProfileFamily` row, never a per-material type.

| [INDEX] | [FEATURE]                                                                       | [PAGE#CLUSTER]          |
| :-----: | :------------------------------------------------------------------------------ | :---------------------- |
|   [1]   | One `Profile` polymorphic owner over a closed `ProfileFamily` family-kind axis   | profile#PROFILE_OWNER   |
|   [2]   | Masonry realized as the first family (brick catalogue generalized); CMU/steel/timber/glazing the named growth axis | profile#PROFILE_OWNER |
|   [3]   | `ProfileUnit` dimensional cross-section composing the kernel `Dimension` value-object | profile#PROFILE_OWNER |
|   [4]   | Masonry void-class / bond / orientation / cut / special-shape vocabulary as data rows | profile#PROFILE_FAMILY |
|   [5]   | `BondName` template/generated bond catalogue with `Course`/`Fits` (OOP boundary, FP-ROP internals) | profile#PROFILE_FAMILY |

## [2]-[APPEARANCE_GRAPH]

The node-graph appearance engine and the polymorphic material library — a material is a row, an appearance variation a node case.

| [INDEX] | [FEATURE]                                                                       | [PAGE#CLUSTER]                  |
| :-----: | :------------------------------------------------------------------------------ | :------------------------------ |
|   [6]   | One `AppearanceNode` union over a typed `PortValue` channel set                   | appearance-graph#MATERIAL_GRAPH |
|   [7]   | Topological evaluation fold terminating at one `BsdfOutput` sink to a `SurfaceShade` | appearance-graph#MATERIAL_GRAPH |
|   [8]   | Compile-once Kahn ordering, per-sample re-entry over the frozen order              | appearance-graph#MATERIAL_GRAPH |
|   [9]   | Canonical Disney-principled default wiring every library row drives                | appearance-graph#MATERIAL_GRAPH |
|  [10]   | One `MaterialParameters` canonical row — base/metal/rough/IOR/transmit/sheen/clearcoat/subsurface/aniso/emission | appearance-graph#MATERIAL_LIBRARY |
|  [11]   | 100+-material library as data rows keyed by `MaterialId`; profile assignment generalized | appearance-graph#MATERIAL_LIBRARY |

## [3]-[BSDF_LOBE_FAMILY]

The closed multi-lobe BSDF and its scene-linear spectral edge.

| [INDEX] | [FEATURE]                                                                       | [PAGE#CLUSTER]            |
| :-----: | :------------------------------------------------------------------------------ | :----------------------- |
|  [12]   | Seven-lobe closed `BsdfLobe` union under one `Evaluate`/`Sample`/`Pdf` contract   | bsdf#LOBE_FAMILY         |
|  [13]   | Fresnel (Schlick + exact dielectric/conductor), GGX/Trowbridge-Reitz NDF, Smith masking | bsdf#MICROFACET_KERNEL |
|  [14]   | Kulla-Conty multi-scatter energy compensation with the exact hemispherical-albedo integral | bsdf#LOBE_FAMILY      |
|  [15]   | `LayeredBsdf` weighted-lobe fold with MIS-balanced sample/pdf — a material is a row of weights | bsdf#LAYERED_COMPOSITION |
|  [16]   | RGB→SPD spectral upsampling feeding Unicolour `Spd`→XYZ scene-linear admission     | bsdf#SPECTRAL_UPSAMPLE   |
|  [17]   | ACES RRT/ODT + scene-referred filmic/Reinhard/exposure tone-map operator table     | bsdf#TONE_MAP            |

## [4]-[TEXTURE_AND_PHOTOMETRIC]

The procedural texture sampling fold and the light-unit admission.

| [INDEX] | [FEATURE]                                                                       | [PAGE#CLUSTER]                   |
| :-----: | :------------------------------------------------------------------------------ | :------------------------------- |
|  [18]   | One `TextureUv` sampling fold over a closed `TextureSource` union (noise/checker/gradient/image/triplanar) | texture-photometric#TEXTURE_UV |
|  [19]   | Author-kernel procedural noise — Perlin/OpenSimplex2/Worley/fBm over one basis band | texture-photometric#TEXTURE_UV |
|  [20]   | Closed address/filter bands — repeat/clamp/mirror, nearest/bilinear/bicubic/trilinear | texture-photometric#TEXTURE_UV |
|  [21]   | `Photometric` light-unit admission composing Compute illuminance, author-kernel radiometry | texture-photometric#PHOTOMETRIC |
|  [22]   | `EmissionSpectrum` blackbody/SPD emission color through the Unicolour seam          | texture-photometric#PHOTOMETRIC  |

## [5]-[CONSTRUCTION]

The host-neutral element → assembly → layout data model — a layout is the one placement fold over any profile family.

| [INDEX] | [FEATURE]                                                                       | [PAGE#CLUSTER]              |
| :-----: | :------------------------------------------------------------------------------ | :------------------------- |
|  [23]   | `Element → Assembly → Layout` three-layer host-neutral data model               | construction#ELEMENT_MODEL |
|  [24]   | `Placement` scalar station/elevation/run/rise/path-angle tuple (no host geometry) | construction#ELEMENT_MODEL |
|  [25]   | `RunPath` closed line/arc path with the one arc-length algebra                    | construction#ELEMENT_MODEL |
|  [26]   | `ConstructionLayout.Resolve` station/elevation course fold over any `Profile` family | construction#ASSEMBLY_FOLD |
|  [27]   | `JointPolicy` head/bed joint resolution off the `Profile.Standard` coordinating thickness | construction#ASSEMBLY_FOLD |
