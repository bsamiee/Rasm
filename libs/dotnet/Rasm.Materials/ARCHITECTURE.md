# [MATERIALS_ARCHITECTURE]

`Rasm.Materials` is the host-neutral AEC-domain projector onto the `Rasm.Element` seam. `Component`, `Appearance`, `Properties`, and `Projection` collapse to one owner per axis; the one `ComponentProjector : IElementProjection` lowers every owner into the shared `ElementGraph`. Its `Project` fold splits the `Substance` and Type-minting `Type` arms, mints the deterministic-rooted Type `Object` from canonical content, and authors the content-keyed `Material`/`Appearance` subgraph the seam `Assemble` fold merges. AEC peers depend up on `{Rasm, Rasm.Element}` and align by seam contract.

`Rasm.Materials` also references `Rasm.AppHost` by name under the cycle-safe branch ruling. `Projection/benchmarks#GATE_COMPOSITION` reads the benchmark gate; appearance and declaration boundaries compose its neutral `WireAdmission` after bounded protobuf-binary parsing. It references `Rasm.Contracts` for both generated families and carries no validator or generated-message mirror of its own.

## [01]-[DOMAIN_MAP]

```text codemap
Rasm.Materials/            # AEC-DOMAIN materials projector; refs {Rasm, Rasm.Element, Rasm.AppHost, Rasm.Contracts}; no host geometry
├── Component/             # One polymorphic Component over the closed component-family axis, class-discriminated
│   ├── Component.cs       # Component record, closed SectionProfile algebra, MaterialGrade rows, and the ComponentSeed traverse and gates
│   ├── Masonry.cs         # ComponentFamily.Masonry policy row and the bond algebra; a unit is a Component row, never a Brick type
│   ├── Steel.cs           # SteelSeed.Roster spans the registered domains beside the generated cold-formed lattice; SteelSeed.Law binds it
│   ├── Cmu.cs             # CmuSeed.Roster/Law under the block policy row; ASTM and TMS cells ride as published data
│   ├── Timber.cs          # TimberSeed.Roster/Law over EN strength-class tables; members and cross-laminated panels are each one row
│   ├── Glazing.cs         # IGU rows as SectionProfile.Layered with PlyRole panes, interlayers, and cavities; performance derives from physics
│   ├── Reinforcement.cs   # ReinforcementRow roster and the host-neutral RcSection assembler; bars and tendons are each one row
│   ├── Fastener.cs        # StockRow.Threaded pairs ThreadRow with a grade; StockRow.Plain carries published nail, dowel, and rivet data
│   ├── Connector.cs       # Evaluation-report cells with directional allowables; every cell carries its issuing report and safety basis
│   ├── Joint.cs           # Continuous weld/adhesive/stud vocabulary; no thread or bar section, so nothing folds into the fastener family
│   ├── Panel.cs           # Board geometry as SectionProfile.Layered over the shared bounded PlyRole; deck geometry rides its own profile
│   ├── Concrete.cs        # ConcreteSeed.Roster/Law over the CIP policy row; exposure classes drive the cover regime
│   ├── Precast.cs         # PrecastSeed.Roster/Law; a plank, tee, or panel is one catalogued product row
│   ├── Aluminum.cs        # EN 1999 characteristic bands beside the authored die roster; section truth is die-owned, the inverse of steel's
│   ├── Insulation.cs      # Non-board thermal forms under the covering token; the board split law routes rigid boards to the panel family
│   ├── Finishes.cs        # TWO family rows over ONE algebra; each shape states once and the family column carries the split
│   ├── Pipework.cs        # Pressure-pipe product rows across the material systems; shared dimension rules state once
│   ├── Ductwork.cs        # SMACNA product rows: pressure-class ladder, gauges, seal and liner classes, geometry
│   ├── Electrical.cs      # Conductor product rows with NEC/IEC ampacity cells as RATING rows beside the containment vocabularies
│   └── Capacity.cs        # SectionCapacity [Union] and the Check fold; one Demand against one capacity is the typed Utilisation
├── Appearance/            # Measured appearance engine — node graph, BSDF lobe family, and the material wire
│   ├── Bsdf.cs            # BsdfLobe [Union] under one Evaluate/Sample/Pdf contract; Microfacet<T> generic GGX/Smith/Fresnel kernel
│   ├── Graph.cs           # AppearanceNode [Union] over typed PortValue channels; Compile orders the DAG once on the QuikGraph substrate
│   ├── Surface.cs         # SpectralUpsample, the ToneMap operator table, and the OpenPBR construction half the wire and library drive
│   ├── Texture.cs         # TextureSource sampling under AddressMode/FilterMode bands; ProceduralNoise seeds over the NoiseBasis band
│   ├── Photometric.cs     # PhotometricQuantity band rows each carrying one closed Coercion discriminant; the 683 lm/W efficacy divide
│   ├── Weathering.cs      # WeatheringEffect policy rows drive a library row along AgeParameter, so a row carries its trajectory
│   ├── Acquisition.cs     # Acquisition.Import produces AcquiredMaterial with its CaptureProvenance receipt and admitted plane set
│   ├── Finish.cs          # Finish.Resolve over a pigment-weight vector and coat stack; spectrally-grounded BaseColor, measured provenance
│   ├── Interchange.cs     # Generated appearance egress, descriptor-admitted Set boundary, MaterialX, and the interior stage crossing
│   ├── Environment.cs     # SkyModel [Union], EnvironmentMap admission, and the IBL prefilter; scene-linear radiance end to end
│   └── Neural.cs          # ModelCard frozen registry keyed by ModelCardId; stage, licence, weights, tensor contract, provider ladder as DATA
├── Raster/                # Texture-map generation — the plane substrate, the bake engine, and its container estate
│   ├── Plane.cs           # TexturePlane typed-texel pooled arena over the kernel lattice seat; storage, transfer, primaries, alpha, range
│   ├── Codec.cs           # RasterFormat [SmartEnum<string>] rows carry extension, magic claim, alpha association, capability, engine case
│   ├── Filter.cs          # PlaneOp [Union] under one Apply that PLANS shapes, SCHEDULES stages by dependency class, then rents outputs
│   ├── Tile.cs            # TileStrategy [SmartEnum<string>] closes the tiling algebra; TileProof.Grade is the one tileability mint
│   ├── Set.cs             # TextureChannel [SmartEnum<string>] rows carry group, components, transfer, neutral; SetBind re-binds the library
│   ├── Press.cs           # TexturePress.Press drives a PressSubject across a PressPlan; the content-identity veto guards every mint
│   └── Gpu.cs             # PressDevice headless adapter with per-kernel compiled pipelines behind one cache; the only Silk.NET.WebGPU speller
├── Properties/            # Typed engineering-property source lowered onto the seam property sets
│   ├── Properties.cs      # MaterialPropertyCatalogue rows, the MechanicalSource vendor-delegation axis, and Published<T> the shared ingress carrier
│   ├── Sustainability.cs  # SustainabilityCatalogue in exact roster parity with its engineering sibling; Lower mints the seam cases
│   └── Assessment.cs      # AssessmentSet.Of resolves dated declarations over the curated catalogues; DeclarationWire.Decode admits the wire
└── Projection/            # One IElementProjection onto the Rasm.Element seam + the observability, benchmark, and analytics projections
    ├── Component.cs       # Project folds payload-complete Substance and Type cases onto Fin<GraphDelta> behind the ProjectionGate veto
    ├── Observability.cs   # MaterialsPoint roster over the kernel IHookRoster floor; MaterialsHooks.Live mints the one folder rail
    ├── Benchmarks.cs      # BenchKernel rows pin BenchInput and resolved content keys, so content changes fork lineage, never row spellings
    └── Analytics.cs       # Dataset rows and projection folds; column types, residences, dialects, and DDL home at Rasm.Persistence
```

VividOrange grounds the structural section, capacity, and rebar data in-folder, never a hand-keyed literal; the per-page consumption law lives on the owning pages. Return type names the rail: a `SurfaceShade`/`Unicolour` carrier where the result is total, `Fin<T>` where a banded fault routes, the seam `Fin<GraphDelta>` from the projector.

C# is the sole producer of the generated appearance family: `Appearance/interchange.md` mints `Material` and each completed `Set`, with the Set crossing the shared `WireAdmission` descriptor gate once. TypeScript and Python consume generated bindings rather than mirrors. Python-minted `Set` values land at `Raster/set.md` `SetIngest.Peer` as classification input through the same gate; the generated declaration family independently lands at `Properties/assessment.md` `DeclarationWire.Decode` and reaches `AssessmentSet.Of` unchanged.

## [02]-[STRATA]

Strata rank the five sub-domains; `Appearance` spans ranks, its core seated at the floor while its egress composes `Raster` products, so seating follows the folder's own dependency truth rather than a folder boundary.

- S0 law — `Component` and the `Appearance` core consume no sibling, and every sibling above reads at least one of the two floors.
- S1 seat — `Properties` lowers engineering dimensional mints through the S0 `QuantityRow` and basis-relative scalars to the seam factories.
- S1→S0 — `Raster` reads the core graph, sampler, and vector and writes back through `SetBind` alone on a `MaterialGraph` VALUE counter-edge.
- S2 seat — the `Appearance` egress composes `Raster` planes and sets; `Raster` names no egress type, so the plane estate stands alone.
- S2 law — a flat `Appearance` stratum turns every egress read of a plane product into an upward edge; the split rank is the dependency truth.
- S3 law — nothing composes `Projection`; the projector folds the lower owners into `Fin<GraphDelta>` and the projections read down alone.

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart TB
    accTitle: Rasm.Materials interior strata
    accDescr: How the projector composes the egress, plane, and floor ranks; the counter-edge returns a MaterialGraph value.
    subgraph S3["S3 PROJECTION"]
        Projector[ComponentProjector]
    end
    subgraph S2["S2 APPEARANCE EGRESS"]
        Wire[Generated Material + Set]
        Environment[EnvironmentLight]
    end
    subgraph S1["S1 PROPERTIES + RASTER"]
        Catalogue[MaterialPropertyCatalogue]
        Sustainability[SustainabilityCatalogue]
        Raster[TextureSet]
    end
    subgraph S0["S0 COMPONENT + APPEARANCE CORE"]
        Component[Component]
        QuantityRow[QuantityRow]
        Library[MaterialLibrary]
    end
    Projector e1@-->|"[IMPORT]: MaterialPropertyCatalogue"| Catalogue
    Projector e2@-->|"[IMPORT]: SustainabilityCatalogue"| Sustainability
    Projector e3@-->|"[IMPORT]: Component"| Component
    Projector e4@-->|"[IMPORT]: AppearanceSummary"| Wire
    Catalogue e5@-->|"[IMPORT]: QuantityRow"| QuantityRow
    Raster e6@-->|"[IMPORT]: MaterialGraph"| Library
    Raster e7@-.->|"[COUNTER]: MaterialGraph"| Library
    Wire e8@-->|"[IMPORT]: TextureSet"| Raster
    Wire e9@-->|"[IMPORT]: MaterialParameters"| Library
    Environment e10@-->|"[IMPORT]: TexturePlane"| Raster
    Component f1@-->|"forbidden: owner upward"| S3
```

## [03]-[SEAMS]

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Materials AEC-domain projection seams
    accDescr: Which projection, section, property, and detail contracts cross between the Materials owners and the AEC peers.
    subgraph materials[RASM.MATERIALS]
        Projection[Projection contracts]
        Component[Component families]
        Properties[Property source]
        Appearance[Appearance engine]
    end
    Element{{Rasm.Element}}
    Bim{{Rasm.Bim}}
    Element e1@-->|"[SHAPE]: IElementProjection"| Projection
    Projection e2@-->|"[PROJECTION]: GraphDelta"| Element
    Projection e3@-->|"[SHAPE]: DetailSchema"| Bim
    Component e4@<-->|"[SHAPE]: ProfileRef"| Element
    Component e5@-->|"[PORT]: IIfcTypeReconciler"| Bim
    Bim e6@-->|"[SHAPE]: TypeCandidate"| Component
    Bim e7@-->|"[SHAPE]: TextureRoster"| Appearance
    Properties e8@<-->|"[SHAPE]: MaterialPropertySet"| Element
    Appearance e9@-->|"[CONTENT_KEY]: AppearanceSummary"| Element
    Component e10@<-->|"[SHAPE]: DetailSchema"| Element
```

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Materials platform, compute, and cross-runtime seams
    accDescr: Which capacity, appearance, telemetry, and analytics contracts cross between Materials and the platform, host, and peer runtimes.
    subgraph materials[RASM.MATERIALS]
        Component[Component families]
        Properties[Property source]
        Appearance[Appearance engine]
        Raster[Raster plane estate]
        Projection[Projection contracts]
    end
    Compute{{Rasm.Compute}}
    AppHost([Rasm.AppHost])
    AppUi([Rasm.AppUi])
    Persistence([Rasm.Persistence])
    PyArtifacts([python:artifacts])
    PyData([python:data])
    PyRuntime([python:runtime])
    Core([typescript:core])
    Ui([typescript:ui])
    Host([Host boundary])
    Rasm([Rasm])
    Rasm e1@-->|"[SHAPE]: SunPosition"| Appearance
    Rasm e2@-->|"[SHAPE]: SpectralArena"| Raster
    Rasm e3@-->|"[SHAPE]: CellLattice"| Raster
    Rasm e4@-->|"[SHAPE]: ChannelDtype"| Raster
    Rasm e5@-->|"[PROJECTION]: ChartAtlas"| Raster
    Rasm e6@-->|"[WIRE]: DualModel"| Appearance
    Rasm e7@-->|"[WIRE]: PatternPlan + InstanceStream"| Component
    Rasm e8@-->|"[SHAPE]: MaterialSymmetry"| Component
    Rasm e9@-->|"[SHAPE]: RgbProfile"| Appearance
    Rasm e10@-->|"[SHAPE]: TapSeries"| Appearance
    Rasm e11@-->|"[SHAPE]: SparseMatrix"| Raster
    Rasm e12@-->|"[PORT]: ReceiptSinkPort"| Projection
    Rasm e13@-->|"[SHAPE]: BenchClaim"| Projection
    Component e14@-->|"[WIRE]: SectionCapacity"| Compute
    Properties e15@-->|"[WIRE]: MaterialPropertySet"| Compute
    Appearance e16@-->|"[WIRE]: StageRequest"| Compute
    Compute e17@-->|"[WIRE]: StageResult"| Appearance
    Appearance e18@-->|"[BOUNDARY]: LayeredBsdf + SurfaceShade + EnvironmentLight + TextureSet"| AppUi
    Appearance e19@-->|"[WIRE]: appearance.Material"| Core
    Appearance e20@-->|"[WIRE]: appearance.Material"| PyRuntime
    Appearance e21@-->|"[WIRE]: appearance.Set"| Core
    Appearance e22@-->|"[WIRE]: appearance.Set"| PyRuntime
    Appearance e23@-->|"[WIRE]: appearance.Material"| Ui
    PyArtifacts e24@-->|"[WIRE]: appearance.Set"| Raster
    PyData e25@-->|"[WIRE]: DeclarationRecord"| Properties
    Host e26@-->|"[WIRE]: CaptureSource"| Appearance
    Projection e27@-->|"[PORT]: TelemetryContributorPort"| AppHost
    Projection e28@-->|"[WIRE]: BenchmarkReceipt"| AppHost
    Projection e29@-->|"[WIRE]: MaterialsDataset"| Persistence
    Raster e30@-->|"[CONTENT_KEY]: TextureSet"| Persistence
```

## [04]-[INTERNAL]

`Component` mints from published seed rosters once and every capacity read composes the minted row; `Appearance` compiles the one `MaterialGraph` program and every baked plane, set, and wire derives from that compile, CPU-minted wherever a content key follows. Per-stage wiring lives on the owning implementation pages.

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Rasm.Materials component spine
    accDescr: How a published seed roster becomes a typed component whose capacity verdict a demand check reads.
    Roster(["Seed roster rows"]) e1@--> Mint[[ComponentSeed generator]]
    Mint e2@--> Row[[Component row]]
    Row e3@--> Section[[Section solver]]
    Section e4@--> Capacity[[SectionCapacity]]
    Capacity e5@--> Check[[Check fold]]
    Check e6@--> Verdict[(Utilisation verdict)]
    Mint f1@-.->|"seed-law refusal"| Fault[/Banded fault rail/]
    Check f2@-.->|"basis refusal"| Fault
```

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Rasm.Materials appearance spine
    accDescr: How a compiled material program becomes a content-keyed baked set the wire and the seam re-bind.
    Graph[[MaterialGraph.Compile]] e1@--> Press[[TexturePress.Press]]
    Press e2@--> Planes[[TexturePlane products]]
    Planes e3@--> SetMint[[TextureSet mint]]
    SetMint e4@--> Bind[[SetBind rebind]]
    SetMint e5@--> WireEg[/Generated Set egress/]
    Press f1@-.->|"content-identity veto"| Fault2[/RasterFault band/]
    WireEg f2@-.->|"container refusal"| Fault2
```

## [05]-[ROUTING]

| [INDEX] | [CHANGE]                            | [OWNER_SURFACE]             | [SHAPE_OF_THE_EDIT]                                               |
| :-----: | :---------------------------------- | :-------------------------- | :---------------------------------------------------------------- |
|  [01]   | new standardized component family   | `Component/component.md`    | one `ComponentFamily` row carrying its `ComponentClass`           |
|  [02]   | new anchor or fastened product      | `Component/fastener.md`     | one `FastenerKind` arm or `StockRow` roster row                   |
|  [03]   | new panel or board product          | `Component/panel.md`        | one `PanelKind` row on the specification-armed policy column      |
|  [04]   | new scattering lobe                 | `Appearance/bsdf.md`        | one `BsdfLobe` case, admitted only where no parameterization fits |
|  [05]   | new material or finish              | `Appearance/graph.md`       | one `MaterialLibrary` row over the one `MaterialGraph`            |
|  [06]   | new standards table                 | its owning catalogue page   | one roster + one `SeedLaw` value on `ComponentSeed`               |
|  [07]   | new registered material grade       | `Component/component.md`    | one `MaterialGrade` row plus its `GradeProperties` arm            |
|  [08]   | new fault case                      | its owning fault page       | one arm at its `FaultBand` free frontier                          |
|  [09]   | new seam payload                    | `Rasm.Element` composition  | seam growth the projector composes, never a local remint          |
|  [10]   | new standard sky or daylight model  | `Appearance/environment.md` | one `CieSkyType` row over the group pair, or one `SkyModel` case  |
|  [11]   | new photo-to-PBR model              | `Appearance/neural.md`      | one `ModelCard` row carrying its licence class and contract       |
|  [12]   | new bakeable appearance field       | `Raster/set.md`             | one `TextureChannel` row carrying its twelve columns              |
|  [13]   | new plane container or block format | `Raster/codec.md`           | one `RasterFormat` row naming its engine, storage, and extension  |
|  [14]   | new plane transform or curve        | `Raster/filter.md`          | one `PlaneOp`, `RemapCurve`, or `HeightDerivative` case           |
|  [15]   | new tiling method                   | `Raster/tile.md`            | one `TileStrategy` row carrying its `Solve` delegate              |
|  [16]   | new GPU compute kernel              | `Raster/gpu.md`             | one `WgslKernel` row carrying source, layout, reduce, and golden  |
|  [17]   | new appearance wire document        | `Appearance/interchange.md` | one proto message, generated bindings, and one egress fold        |
|  [18]   | new seamless procedural lattice     | `Appearance/texture.md`     | one `NoiseBasis` row answering `Wrappable` plus its golden row    |
|  [19]   | new plane depth, arity, or storage  | `Raster/plane.md`           | one `IComponent` witness, texel struct, or `PlaneFormat` row      |
|  [20]   | new bake subject or execution lane  | `Raster/press.md`           | one `PressSubject` case or one `PressBackend` row                 |
|  [21]   | new photo-to-PBR capture modality   | `Appearance/acquisition.md` | one `CaptureSource` case and its `CaptureMethod` receipt row      |
|  [22]   | new declaration modality or EPD row | `Properties/assessment.md`  | one `AssessmentRecord` case with its `Admit` and resolution arms  |
|  [23]   | new durability binder or mix        | `Properties/properties.md`  | one `CementType` row plus its published `DurabilityMix` entries   |
|  [24]   | new design code over a cased family | `Component/capacity.md`     | one `DesignBasis` row plus the family page's per-basis arm        |
|  [25]   | new fatigue detail category         | `Component/capacity.md`     | one `EnFatigueCategory` or `AiscFatigueCategory` ladder rung      |
|  [26]   | new trade size or system            | its owning trade seed page  | one roster row or one system policy row, never a stocked sweep    |

## [06]-[BOUNDARIES]

- Materials owns substance, appearance, and buildable type: one `Component`, one capacity rail, one `MaterialGraph`, one `TextureSet`.
- Appearance CORE stays pointwise — a DAG node has no neighbours to read; neighbourhood work is the plane algebra's.
- Every filter, integration, and tiling kernel homes at `Raster/filter` or `Raster/tile`, never a node case.
- Persisted plane bytes are CPU-minted; the GPU lane is an accelerator whose products carry no content key.
- `ComponentFamily` closes the family axis and `ComponentClass` the structural-class axis, each family row carrying its class discriminant.
- `SEED_ROW_LAW` seats standards data as in-fence C# under per-column provenance, and every seed row flows the one catalogue-to-solver rail.
- `ComponentProjector.Project` stamps `Classification`/`PredefinedType` off its `IfcBinding` row, seed-excluded so a later attach never re-keys.
- Model authors mint Occurrence `Object`s and `Rasm.Bim` ingests `IfcElementType` into the same Type; the `Bake` inheritance is the seam's.
- `IIfcTypeReconciler` closes one loop: Bim's `ExportTypeCandidates` feeds `ComponentCatalogue.AdmitImported` by contract, never reference.
- Model owners stay host-neutral: none holds a host curve or transform, and run and layout geometry lands above the domain at the app root.
- `Rasm.Element` owns material-composition vocabulary, the perceptual owner color, and UnitsNet admits once per declared edge riding `MeasureValue`.
- Each concern composes its admitted engine, and a kernel the ecosystem leaves unowned lands hand-authored at its owning page.
- Every out-of-gamut, non-finite, or degenerate result rails to its banded fault, never a propagated NaN or sentinel.
- Composition-root decorators tap `MaterialsFact` onto `MaterialsPoint`, so owners emit nothing; `MaterialsDescriptors` rides the kernel SLO algebra.
- `StageResult` crosses from `Rasm.Compute` carrying the `ParityFresh` gate and `Coverage` floor whole; dropping either counts unmeasured.
