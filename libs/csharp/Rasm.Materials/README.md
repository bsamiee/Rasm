# [MATERIALS]

`Rasm.Materials` is the host-neutral AEC-domain owner of architectural materials across five sub-domains. `Profiles/` owns one polymorphic `Profile` over a closed `ProfileFamily` axis — masonry, steel, CMU, timber, and glazing realized. `Connection/` owns one polymorphic `ConnectionItem` over the closed `ConnectionFamily` axis — reinforcement, fastener, hanger — with anchor folded as a `FastenerKind` arm; the catalogue serializes over the BIM wire as the IFC `IfcReinforcingBar`/`IfcMechanicalFastener` element. `Appearance/` owns one measured appearance engine: a node `MaterialGraph`, a closed seven-lobe `BsdfLobe` family lowered from the OpenPBR Surface 1.1 `SlabStack`, a `MaterialLibrary` row table grounded by the measured conductor complex-IOR table with the Pointer real-surface gamut and CVD-preview seam, procedural texture and photometric admission, the Kubelka-Munk pigment/coat-stack finish engine, the weathering aging operator, measured-material acquisition import, and the OpenPBR/MaterialX wire vocabulary host-free peers decode. `Construction/` owns the host-neutral element-to-assembly-to-layout data model, with materials assigned by the IFC 4.3 layer-set/profile-set/constituent-set trichotomy and resolved to portable scalar placements. `Properties/` owns the typed `MaterialProperty` engineering-property family — mechanical, thermal, acoustic over per-octave-band spectra, and fire over the IFC `IfcMaterialProperties` set — plus the `AssemblyProperty` series-resistance/rule-of-mixtures/layered-STC aggregation folds. A material is a LIBRARY ROW, an appearance variation a NODE CASE, a lobe a `[Union]` CASE, a cross-section a `ProfileFamily` ROW, a connection a `ConnectionFamily` ROW, a layout the ONE placement fold, and an assembly property the ONE aggregation fold — never a per-material or per-family type. The package composes the `Rasm` kernel for vector/dimension value-objects, consumes `Wacton.Unicolour` as its scene-linear/spectral color owner, and composes the Compute unit algebra at the seam, never re-minting a vector, a color space, or a unit owner. The sub-domain map and domain law live in `ARCHITECTURE.md`, the forward concept pool in `IDEAS.md`, and the work log in `TASKLOG.md`.

## [01]-[ROUTER]

- [01]-[PROFILE](.planning/Profiles/profile.md)
- [02]-[MASONRY](.planning/Profiles/masonry.md)
- [03]-[STEEL](.planning/Profiles/steel.md)
- [04]-[CMU](.planning/Profiles/cmu.md)
- [05]-[TIMBER](.planning/Profiles/timber.md)
- [06]-[GLAZING](.planning/Profiles/glazing.md)
- [07]-[CONNECTION](.planning/Connection/connection.md): `ConnectionItem` `[Union]` reinforcement/fastener/hanger/anchor catalogue over the closed `ConnectionFamily` axis (`anchor` folded as a `FastenerKind` arm), band 2360
- [08]-[REINFORCEMENT](.planning/Connection/reinforcement.md)
- [09]-[FASTENER](.planning/Connection/fastener.md)
- [10]-[HANGER](.planning/Connection/hanger.md)
- [11]-[BSDF](.planning/Appearance/bsdf.md)
- [12]-[GRAPH](.planning/Appearance/graph.md)
- [13]-[TEXTURE](.planning/Appearance/texture.md)
- [14]-[PHOTOMETRIC](.planning/Appearance/photometric.md)
- [15]-[WEATHERING](.planning/Appearance/weathering.md)
- [16]-[ACQUISITION](.planning/Appearance/acquisition.md)
- [17]-[FINISH](.planning/Appearance/finish.md)
- [18]-[INTERCHANGE](.planning/Appearance/interchange.md)
- [19]-[ASSEMBLY](.planning/Construction/assembly.md)
- [20]-[LAYOUT](.planning/Construction/layout.md)
- [21]-[PROPERTIES](.planning/Properties/properties.md)

## [02]-[DOMAIN_PACKAGES]

Domain packages admitted by this folder; versions are centralized in the one C# manifest and corroborated by this folder's `.api/`.

[COLOR_SPECTRAL]:
- `Wacton.Unicolour`
- `Wacton.Unicolour.Datasets`

[UNITS]:
- `UnitsNet`

[PROJECTS]:
- `Rasm`
- `Rasm.Compute`

## [03]-[SUBSTRATE_PACKAGES]

Substrate packages from the C# registry consumed by this folder; full registry and substrate contracts live in [`libs/csharp/.planning/README.md`](../csharp/.planning/README.md) and this folder's `.api/`.

[FUNCTIONAL_CORE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `JetBrains.Annotations`
