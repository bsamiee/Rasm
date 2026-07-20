# [MATERIALS]

`Rasm.Materials` owns architectural substance, appearance, and buildable component type — what a building is made of, how it performs, and how it renders. One polymorphic `Component` carries every standardized family as policy rows over one section-profile algebra and one capacity rail, so a new family, section shape, or unit is a data row and every standards value traces to its published source or vendor factory. Its appearance plane is physically based end to end — BSDF lobes, OpenPBR lowering, spectral and perceptual color science, Kubelka-Munk finishes — held at render grade.

Component rows feed generated assemblies and fabrication physics, capacity receipts feed structural assessment, and appearance feeds path-traced viewports and MaterialX interchange. It projects property catalogues, the component-family axis, section profiles, and capacity receipts onto the `Rasm.Element` seam through the one `ComponentProjector`, reminting no vector, color, unit, or seam type; it references no AEC peer — alignment travels through seam contracts and the content-keyed wire.

## [01]-[ROUTER]

[COMPONENT]:
- [01]-[COMPONENT](.planning/Component/component.md): Polymorphic component owner and the one section solver over the profile algebra.
- [02]-[MASONRY](.planning/Component/masonry.md): Masonry component family.
- [03]-[STEEL](.planning/Component/steel.md): Steel family over the catalogued AISC and EN sections.
- [04]-[CMU](.planning/Component/cmu.md): Concrete-masonry-unit family.
- [05]-[TIMBER](.planning/Component/timber.md): Timber family over sawn, glulam, and CLT lamellae.
- [06]-[GLAZING](.planning/Component/glazing.md): Glazing family over insulated-glass pane, spacer, and cavity records.
- [07]-[REINFORCEMENT](.planning/Component/reinforcement.md): Reinforcement family over the rebar arrangement and prestressing-strand line.
- [08]-[FASTENER](.planning/Component/fastener.md): Fastener family over the threaded bolt, nut, and washer assembly.
- [09]-[CONNECTOR](.planning/Component/connector.md): Connector component family.
- [10]-[JOINT](.planning/Component/joint.md): Joint family over the weld, adhesive, and stud connection record.
- [11]-[PANEL](.planning/Component/panel.md): Panel family over sheet-goods built elements.
- [12]-[CAPACITY](.planning/Component/capacity.md): One section-capacity resolution and check rail.

[APPEARANCE]:
- [13]-[BSDF](.planning/Appearance/bsdf.md): Closed BSDF lobe family and the microfacet kernel.
- [14]-[GRAPH](.planning/Appearance/graph.md): Material node-DAG program and the material-library table.
- [15]-[SURFACE](.planning/Appearance/surface.md): OpenPBR color-science lowering and the layered slab stack.
- [16]-[TEXTURE](.planning/Appearance/texture.md): Texture-sampling fold over the closed texture-source union.
- [17]-[PHOTOMETRIC](.planning/Appearance/photometric.md): Light-unit admission fold and the in-folder UnitsNet boundary.
- [18]-[WEATHERING](.planning/Appearance/weathering.md): Aging fold over the closed weathering-effect union.
- [19]-[ACQUISITION](.planning/Appearance/acquisition.md): Capture-import fold over the closed capture-source union.
- [20]-[FINISH](.planning/Appearance/finish.md): Kubelka-Munk pigment-reflectance finish engine.
- [21]-[INTERCHANGE](.planning/Appearance/interchange.md): Material wire and MaterialX .mtlx interchange projection.

[PROPERTIES]:
- [22]-[PROPERTIES](.planning/Properties/properties.md): Intrinsic mechanical, thermal, acoustic, and fire measurements.
- [23]-[SUSTAINABILITY](.planning/Properties/sustainability.md): Lifecycle impact, unit-cost basis, and classification rows.

[PROJECTION]:
- [24]-[PROJECTION](.planning/Projection/component.md): One `ComponentProjector` minting Type Objects and material subgraphs.

## [02]-[DOMAIN_PACKAGES]

Domain packages admitted by this folder; versions centralize in the one C# manifest and corroborate against this folder's `.api/`.

[SECTION_CAPACITY]:
- `VividOrange.Profiles.Catalogue` — AISC and EN typed profiles grounding the section seed in published data.
- `VividOrange.Sections.SectionProperties` — polygon section solver over every `ComponentFamily`.
- `VividOrange.Sections` — concrete-section and rebar-layout engines the `RcSection` assembler composes.
- `VividOrange.InteractionDiagram` — biaxial N-M-M capacity surface over strain sweep and fibre integration.

[MATERIAL_STANDARDS]:
- `VividOrange.Materials` — EN/Eurocode grade-to-property factories and the constitutive-model family.
- `VividOrange.Standards` — cited Eurocode standard rows over inline literals.

[PROPERTY_UNCERTAINTY]:
- `VividOrange.Uncertainties` — scalar uncertainty arithmetic riding the published measurement rows.
- `VividOrange.Uncertainties.Quantities` — UnitsNet quantity uncertainty over the published measurement surfaces.

[APPEARANCE]:
- `Wacton.Unicolour.Datasets` — reference observers, illuminants, and named datasets over the `Wacton.Unicolour` owner.
- `MessagePack` — binary appearance-interchange wire with source-generated resolver and untrusted-data hardening.
- `MessagePackAnalyzer` — build-only proof of `[Key]` coverage on every wire record.

[PROJECTS]:
- `Rasm` — geometry kernel: value-objects, the admission floor, and the seed-zero content-hash entry.
- `Rasm.Element` — projected element seam: graph algebra, material vocabulary, and `IElementProjection`.

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the C# registry; the registry and its charters own the full contracts, and `libs/csharp/.api/` holds the shared API evidence.

[FUNCTIONAL_CORE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `Thinktecture.Runtime.Extensions.Json`
- `Thinktecture.Runtime.Extensions.MessagePack`
- `JetBrains.Annotations`

[NUMERIC]:
- `UnitsNet`
- `MathNet.Numerics` — overdetermined least-squares rail for the measured-BRDF GGX fit.
- `CommunityToolkit.HighPerformance` — dense appearance planes read as spans, never offsets.

[MAPPING_GRAPH]:
- `Riok.Mapperly` — source-generated boundary transcription under the completeness gate.
- `QuikGraph` — appearance-DAG topological sort.

[COLOR_SCIENCE]:
- `Wacton.Unicolour` — color-space conversion and perceptual difference for the appearance engine.
