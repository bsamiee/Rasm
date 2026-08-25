# [MATERIALS]

`Rasm.Materials` owns architectural substance, appearance, and buildable component type: what a building is made of, how it performs, and how it renders. One polymorphic `Component` carries every standardized family as policy rows over one section-profile algebra and one capacity rail, so a new family, section shape, or unit is a data row and every standards value traces to its published source or vendor factory. Its appearance plane is physically based end to end, from BSDF lobes and OpenPBR lowering through spectral color science to Kubelka-Munk finishes, held at render grade.

Component rows feed generated assemblies and fabrication physics, capacity verdicts feed structural assessment, and appearance feeds path-traced viewports and MaterialX interchange. It composes the `Rasm` admission floor, reaches the signal plane through the kernel capsule, and references `Rasm.AppHost` for the benchmark gate and the branch's neutral generated-message admission. It projects property catalogues, the component-family axis, section profiles, and section capacities onto the `Rasm.Element` seam through the one `ComponentProjector`, reminting no vector, color, unit, or seam type.

## [01]-[ROUTER]

[COMPONENT]:
- [01]-[COMPONENT](.planning/Component/component.md): Polymorphic component owner and the one section solver over the profile algebra.
- [02]-[MASONRY](.planning/Component/masonry.md): Masonry family and the generative bond algebra.
- [03]-[STEEL](.planning/Component/steel.md): Steel family over the catalogued AISC and EN sections.
- [04]-[CMU](.planning/Component/cmu.md): Concrete-masonry-unit family grounded in the ASTM/TMS published rows.
- [05]-[TIMBER](.planning/Component/timber.md): Timber family over sawn, glulam, and CLT lamellae.
- [06]-[GLAZING](.planning/Component/glazing.md): Glazing family over insulated-glass pane, spacer, and cavity records.
- [07]-[REINFORCEMENT](.planning/Component/reinforcement.md): Reinforcement family — rebar and tendon rosters with the reinforced-section assembler.
- [08]-[FASTENER](.planning/Component/fastener.md): Fastener family over the threaded and plain stock arms with published design values.
- [09]-[CONNECTOR](.planning/Component/connector.md): Framing-connector family over evaluation-report rows and directional allowable algebra.
- [10]-[JOINT](.planning/Component/joint.md): Joint family over the weld, adhesive, and stud connection record.
- [11]-[PANEL](.planning/Component/panel.md): Panel family — the `PanelSpecification` payload, `FastenPattern`, and frozen standards rows.
- [12]-[CONCRETE](.planning/Component/concrete.md): Cast-in-place concrete family over the grade and role axes with the exposure-driven cover regime.
- [13]-[PRECAST](.planning/Component/precast.md): Precast product family over the two-sourced hollowcore and double-tee ladders.
- [14]-[ALUMINUM](.planning/Component/aluminum.md): Aluminum family over the EN 1999 alloy bands and the authored die roster.
- [15]-[INSULATION](.planning/Component/insulation.md): Insulation family over the non-board batt, roll, loose-fill, and spray forms.
- [16]-[FINISHES](.planning/Component/finishes.md): Finish and fireproofing families split by their `DetailLane` row.
- [17]-[PIPEWORK](.planning/Component/pipework.md): Pipework family over the published pressure-pipe system rosters.
- [18]-[DUCTWORK](.planning/Component/ductwork.md): Ductwork family over the SMACNA pressure-class and gauge schedules.
- [19]-[ELECTRICAL](.planning/Component/electrical.md): Electrical family over the conductor rosters and ampacity rating rows.
- [20]-[CAPACITY](.planning/Component/capacity.md): One section-capacity resolution and check rail.

[APPEARANCE]:
- [21]-[BSDF](.planning/Appearance/bsdf.md): Closed BSDF lobe family and the microfacet kernel.
- [22]-[GRAPH](.planning/Appearance/graph.md): Material node-DAG program, its batched plane evaluator, and the material-library table.
- [23]-[SURFACE](.planning/Appearance/surface.md): OpenPBR color-science lowering and the layered slab stack.
- [24]-[TEXTURE](.planning/Appearance/texture.md): Texture-sampling fold over the closed texture-source union and its wrap-exact lattice period.
- [25]-[PHOTOMETRIC](.planning/Appearance/photometric.md): Light-unit admission fold and the in-folder UnitsNet boundary.
- [26]-[WEATHERING](.planning/Appearance/weathering.md): Aging fold over the closed weathering-effect union.
- [27]-[ACQUISITION](.planning/Appearance/acquisition.md): Capture-import fold over the closed capture-source union and the acquired plane set.
- [28]-[FINISH](.planning/Appearance/finish.md): Kubelka-Munk pigment-reflectance finish engine.
- [29]-[INTERCHANGE](.planning/Appearance/interchange.md): Appearance wire family, texture egress, and the MaterialX .mtlx projection.
- [30]-[ENVIRONMENT](.planning/Appearance/environment.md): Sky synthesis, environment-map admission, IBL prefilter, and the environment-light row.
- [31]-[NEURAL](.planning/Appearance/neural.md): Photo-to-PBR model registry and the inference stage plan.

[RASTER]:
- [32]-[PLANE](.planning/Raster/plane.md): Typed-texel plane arena, the decoded row rails, and the mip chain with its sampler bridge.
- [33]-[CODEC](.planning/Raster/codec.md): Container roster, the band-2460 raster fault, and the KTX gate over its CLI floor.
- [34]-[FILTER](.planning/Raster/filter.md): Plane-transform algebra, the stage scheduler, and the height-field correspondence.
- [35]-[TILE](.planning/Raster/tile.md): Set-coherent tiling synthesizer and the deterministic tileability gate.
- [36]-[SET](.planning/Raster/set.md): Channel roster, the content-keyed baked set, ingest classification, and the appearance rebind.
- [37]-[PRESS](.planning/Raster/press.md): Bake engine over the batched plane evaluator and its content-identity veto.
- [38]-[GPU](.planning/Raster/gpu.md): Surfaceless bake device and the closed WGSL module table with its golden vectors.

[PROPERTIES]:
- [39]-[PROPERTIES](.planning/Properties/properties.md): Published engineering data per material, the fib durability table, and the mix-design fold.
- [40]-[SUSTAINABILITY](.planning/Properties/sustainability.md): Lifecycle impact, unit-cost basis, and classification rows.
- [41]-[ASSESSMENT](.planning/Properties/assessment.md): Dated declaration records and the assessed-over-published resolution law.

[PROJECTION]:
- [42]-[COMPONENT](.planning/Projection/component.md): `ComponentProjector` fold minting Type Objects and material subgraphs.
- [43]-[OBSERVABILITY](.planning/Projection/observability.md): Closed fact family over the folder hook rail with its instrument projection.
- [44]-[BENCHMARKS](.planning/Projection/benchmarks.md): Content-bound `BenchKernel` workload corpus with benchmark gating.
- [45]-[ANALYTICS](.planning/Projection/analytics.md): Analytics datasets declared as wire onto flat row streams for the columnar custodian.

## [02]-[DOMAIN_PACKAGES]

Domain-specific libraries admitted by this folder; versions centralize in `Directory.Packages.props` and corroborate against this folder's `.api/`.

[SECTION_CAPACITY]:
- `VividOrange.Profiles.Catalogue` — AISC and EN typed profiles grounding the section seed in published data.
- `VividOrange.Sections.SectionProperties` — Polygon section solver over every `ComponentFamily`.
- `VividOrange.Sections` — Concrete-section and rebar-layout engines the `RcSection` assembler composes.
- `VividOrange.InteractionDiagram` — Biaxial N-M-M capacity surface over strain sweep and fibre integration.
- `VividOrange.ForceMomentInteraction` — Welded capacity-hull engine the interaction diagram builds and caches; transitive floor, no manifest row.
- `VividOrange.IForceMomentInteraction` — Hull interface the capacity ray-cast reads instead of the concrete mesh; transitive floor, no manifest row.
- `Triangle` — Engine-internal mesher reached through `VividOrange.InteractionDiagram`; catalogued for encapsulation, never called, no manifest row.

[MATERIAL_STANDARDS]:
- `VividOrange.Materials` — EN/Eurocode grade-to-property factories and the constitutive-model family.
- `VividOrange.Standards` — Cited Eurocode standard rows over inline literals; the typed governing-code column every capacity verdict names.
- `VividOrange.Serialization` — Taxonomy round-trip behind the capacity-hull artifact, producer-to-consumer only; transitive floor, no manifest row.

[PROPERTY_UNCERTAINTY]:
- `VividOrange.Uncertainties` — Scalar uncertainty arithmetic riding the published measurement rows.
- `VividOrange.Uncertainties.Quantities` — UnitsNet quantity uncertainty over the published measurement surfaces.

[APPEARANCE_MEDIA]:
- `Magick.NET-Q16-HDRI-AnyCPU` — Ingest-only breadth engine where no managed decoder reaches; never an egress.
- `SixLabors.ImageSharp` — Managed production containers across the `L8`-to-`RgbaVector` depth ladder, carrying ICC.
- `TinyEXR.NET` — Owns OpenEXR past flat-scanline reach: block-level part, level, and deep access beside the spectral and colour folds.
- `TextureCompressor` — Pure-managed GPU texture payloads over a format-keyed coder registry.
- `TextureCompressor.FileFormats.Ktx` — KTX container pair with the supercompression the deep-store rows select.
- `TextureCompressor.FileFormats.Hdr` — Radiance RGBE ingest decoding straight to a float plane.
- `Wacton.Unicolour.Datasets` — Reference observers, illuminants, and named datasets over the `Wacton.Unicolour` owner.

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the C# registry, whose charters own the full contracts; `libs/dotnet/.api/` holds the shared API evidence.

[CORE_SUBSTRATE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `Thinktecture.Runtime.Extensions.Json`
- `JetBrains.Annotations`
- `NodaTime` — `Duration` fact columns, `Instant`/`Interval` run stamps, and the `LocalDate` evidence expiry.
- `QuikGraph` — Appearance-DAG topological sort and the Edmonds-Karp max-flow cut behind the tileability synthesizer.
- `Riok.Mapperly` — Source-generated boundary transcription under the completeness gate.
- `Wacton.Unicolour` — Color-space conversion and perceptual difference for the appearance engine.

[NUMERIC_SUBSTRATE]:
- `UnitsNet`
- `MathNet.Numerics` — Numeric folds under the appearance and capacity engines, from quadrature to measured-BRDF least squares.
- `CommunityToolkit.HighPerformance` — Appearance planes as spans, the `ParallelHelper` partition fan-out, and `MemoryOwner`/`SpanOwner` pooling.

[GPU_DEVICE]:
- `Silk.NET.WebGPU` — Surfaceless bake device, WGSL compute dispatch, and texture-to-buffer readback.
- `Silk.NET.WebGPU.Extensions.WGPU` — `Wgpu` extension view: instance extras, `DevicePoll` map advance, submit-index waits, native log callback.
- `Silk.NET.WebGPU.Native.WGPU` — `wgpu_native` runtime binaries the binding P/Invokes; binaries only, no managed surface to catalogue.

[OBSERVABILITY]:
- `Microsoft.Extensions.Logging.Abstractions` — `ILogger` and generated-log contracts for the fixed-severity fault projection.
- `Microsoft.Extensions.Telemetry.Abstractions` — `ILatencyContext` checkpoint ledger over the eager constructions.

[WIRE_CODEGEN]:
- `Google.Protobuf` — Bounded protobuf-binary appearance and declaration decoding over generated messages.
- `NodaTime.Serialization.Protobuf` — Generated `google.type.Date` projection onto the assessment domain's `LocalDate` values.
