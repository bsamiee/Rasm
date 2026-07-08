# [MATERIALS]

`Rasm.Materials` separates substance, placement-free buildable component type, and sited element. Material property catalogues and component type data project as seam `Material` nodes and deterministic-rooted Type `Object`s through one `ComponentProjector`.

A component carries complete generative parametric data for the geometry a generator builds. Family variation lands as `ComponentFamily` rows and `SectionProfile` arms; standards data flows through `SEED_ROW_LAW` provenance columns and one `ComponentFamily.Rows -> Component.Of -> SectionSolver.Solve` generator.

- `Component.SubstanceId` is the canonical material identity — engineering, lifecycle, classification, cost; `AppearanceId` independent visual/finish identity.
- `Panel` is the sheet/deck/membrane product-form family. Roof, wall, and floor are layout/use roles outside `ComponentFamily`.
- `Properties/` has two owners: intrinsic engineering measurements and lifecycle/cost-basis/classification; assembly performance is computed above Materials.
- Standards data carries `VENDOR`/`DEFINED`/`PUBLISHED` provenance and enters through the single `ComponentFamily.Rows -> Component.Of -> SectionSolver.Solve` generator.

`Rasm.Materials` owns architectural material properties, appearance, component families, section profiles, capacity receipts, and projection onto the element seam. It composes the kernel, `Rasm.Element`, `Wacton.Unicolour`, `UnitsNet`, and VividOrange section/material packages without reminting vector, color, unit, or seam types.

## [01]-[ROUTER]

- [01]-[COMPONENT](.planning/Component/component.md)
- [02]-[MASONRY](.planning/Component/masonry.md)
- [03]-[STEEL](.planning/Component/steel.md)
- [04]-[CMU](.planning/Component/cmu.md)
- [05]-[TIMBER](.planning/Component/timber.md)
- [06]-[GLAZING](.planning/Component/glazing.md)
- [07]-[REINFORCEMENT](.planning/Component/reinforcement.md)
- [08]-[FASTENER](.planning/Component/fastener.md)
- [09]-[CONNECTOR](.planning/Component/connector.md)
- [10]-[JOINT](.planning/Component/joint.md)
- [11]-[PANEL](.planning/Component/panel.md)
- [12]-[CAPACITY](.planning/Component/capacity.md)
- [13]-[BSDF](.planning/Appearance/bsdf.md)
- [14]-[GRAPH](.planning/Appearance/graph.md)
- [15]-[SURFACE](.planning/Appearance/surface.md)
- [16]-[TEXTURE](.planning/Appearance/texture.md)
- [17]-[PHOTOMETRIC](.planning/Appearance/photometric.md)
- [18]-[WEATHERING](.planning/Appearance/weathering.md)
- [19]-[ACQUISITION](.planning/Appearance/acquisition.md)
- [20]-[FINISH](.planning/Appearance/finish.md)
- [21]-[INTERCHANGE](.planning/Appearance/interchange.md)
- [22]-[PROPERTIES](.planning/Properties/properties.md)
- [23]-[SUSTAINABILITY](.planning/Properties/sustainability.md)
- [24]-[PROJECTION](.planning/Projection/component.md)

## [02]-[DOMAIN_PACKAGES]

Domain packages admitted by this folder; versions are centralized in the one C# manifest and corroborated by this folder's `.api/`.

[COLOR_SPECTRAL]:
- `Wacton.Unicolour`
- `Wacton.Unicolour.Datasets`

[NUMERICS]:
- `MathNet.Numerics`

[SECTION_CATALOGUE]:
- `VividOrange.Profiles.Catalogue` — AISC and EN typed profiles with `UnitsNet.Length` geometry; grounds the section seed in published data.
- `VividOrange.Sections.SectionProperties` — polygon section solver: `IPerimeter` → the 20-field `ComputedSection`; one solver over every `ComponentFamily`.
- `VividOrange.Sections` — `ConcreteSection`/`Section` + rebar layout engines over the pinned `ISections` floor; composed by the `RcSection` assembler.

[STRUCTURAL_MATERIAL_DATA]:
- `VividOrange.Materials` — EN/Eurocode grade data over the `IMaterials` floor: grade→property factories, partial factors, and the constitutive-model family.
- `VividOrange.Standards` — Eurocode data over the `IStandards` floor: `En1990`..`En1999` typed rows + `NationalAnnexUtility`; cited, never inline literals.

[SECTION_CAPACITY_ENGINE]:
- `VividOrange.InteractionDiagram` — biaxial N-M-M capacity surface: strain sweep + fibre integration to the `IForceMomentMesh` hull; closure floors pinned.

[PROPERTY_UNCERTAINTY]:
- `VividOrange.Uncertainties` — scalar uncertainty arithmetic (absolute/relative/interval/normal); propagation rides the property rows, never tolerance fields.
- `VividOrange.Uncertainties.Quantities` — UnitsNet quantity uncertainty; value/unit/uncertainty travel the property surfaces; `IUncertainties` the floor.

[WIRE_SERIALIZERS]:
- `MessagePack` — binary appearance-interchange wire: source-generated resolver, `Lz4BlockArray`, `UntrustedData` hardening; analyzer proves `[Key]` coverage.

[PROJECTS]:
- `Rasm` — the kernel: `Rasm.Numerics` value-objects, the `IObjectFactory` admission floor, and the seed-zero `XxHash128` hash entry `ContentAddress` composes.
- `Rasm.Element` — the projected element seam: graph algebra, material vocabulary, section handle, `DetailSchema` bag, `IElementProjection`; peers never couple.

## [03]-[SUBSTRATE_PACKAGES]

Substrate packages from the C# registry consumed by this folder; full registry and substrate contracts live in [`libs/csharp/.planning/README.md`](../.planning/README.md), with shared API evidence in `libs/csharp/.api/`.

[FUNCTIONAL_CORE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `Thinktecture.Runtime.Extensions.Json`
- `Thinktecture.Runtime.Extensions.MessagePack`
- `Riok.Mapperly` — boundary transcription: `WireMap` `OpenPbrSurface`→`OpenPbrGroupsWire` + `Provenance`→`WireProvenance` under the RMG completeness gate.
- `JetBrains.Annotations`

[NUMERIC_SUBSTRATE]:
- `UnitsNet`

[MEMORY_PLANES]:
- `CommunityToolkit.HighPerformance` — dense planes: mips via `AsMemory2D` into a `ReadOnlyMemory2D<ShadeVec4>` pyramid; samplers read spans, never offsets.

[GRAPH_ALGORITHM]:
- `QuikGraph` — `Appearance/graph` folds the DAG into `AdjacencyGraph<PortId, SEdge<PortId>>` + `SourceFirstTopologicalSort`, replacing the Kahn walk.
