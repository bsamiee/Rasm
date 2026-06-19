# [MATERIALS_ARCHITECTURE]

The domain map of `Rasm.Materials` — the host-neutral AEC-DOMAIN materials owner. One `ProfileFamily`/`MaterialLibrary`/`ConnectionFamily`/`MaterialProperty` row per concept across the `Profiles`, `Connection`, `Appearance`, `Construction`, and `Properties` sub-domains, never a per-material or per-family type.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
Rasm.Materials/
├── Profiles/             # One polymorphic Profile over a closed ProfileFamily axis
│   ├── Profile.cs        # Profile polymorphic owner over ProfileFamily growth axis
│   ├── Masonry.cs        # Masonry ProfileFamily with regional catalogue rows
│   ├── Steel.cs          # Steel ProfileFamily over AISC v16.0 section-property record
│   ├── Cmu.cs            # CMU ProfileFamily over ASTM C90 cell/face-shell record
│   ├── Timber.cs         # Timber ProfileFamily over sawn/glulam/CLT lamella records
│   └── Glazing.cs        # Glazing ProfileFamily over pane/spacer/cavity records
├── Connection/           # One polymorphic ConnectionItem over ConnectionFamily axis
│   ├── Connection.cs     # ConnectionItem polymorphic owner over ConnectionFamily axis
│   ├── Reinforcement.cs  # Reinforcement ConnectionFamily over ASTM A615/A706 bar record
│   ├── Fastener.cs       # Fastener ConnectionFamily over ISO 898/SAE J429 thread record
│   └── Hanger.cs         # Hanger ConnectionFamily over carried-member/capacity record
├── Appearance/           # Measured appearance engine
│   ├── Bsdf.cs           # Closed seven-lobe BsdfLobe family and scene-linear spectral edge
│   ├── Graph.cs          # Node-DAG MaterialGraph program and MaterialLibrary row table
│   ├── Texture.cs        # TextureUv sampling fold over closed TextureSource union
│   ├── Photometric.cs    # Light-unit admission fold over PhotometricQuantity band
│   ├── Weathering.cs     # Weathering aging fold over closed WeatheringEffect union
│   ├── Acquisition.cs    # Acquisition import fold over closed CaptureSource union
│   ├── Finish.cs         # FinishMix Kubelka-Munk pigment-reflectance engine
│   └── Interchange.cs    # MaterialWire and MaterialX .mtlx interchange projection
├── Construction/         # Host-neutral construction model
│   ├── Assembly.cs       # IFC 4.3 material-assignment owner and Element placed-unit model
│   └── Layout.cs         # One ConstructionLayout.Resolve placement fold over any ProfileFamily
└── Properties/           # Typed engineering-property model
    └── Properties.cs     # MaterialProperty closed family with quantity-coercion fold
```

Implementation collapses to one owner per axis and one entrypoint family per rail: a new cross-section is a `ProfileFamily` row, a new material a `MaterialLibrary` row, a new lobe a `BsdfLobe` `[Union]` case, a new connection a `ConnectionFamily` row, a new engineering property a `MaterialProperty` case — never a new surface. The rail is named in the return type: a `SurfaceShade`/`Unicolour` carrier where the result is total, `Fin<T>` where a banded fault routes — `ProfileFault` 2300, `ConstructionFault` 2350, `ConnectionFault` 2360, `MaterialFault` 2450, all disjoint from the kernel `GeometryFault` band 2400. C# is the sole producer of the material wire: `Appearance/Interchange` `MaterialWire` and `MtlxDocument` mint the OpenPBR-vector and MaterialX `.mtlx` interchange once, and the TypeScript and Python peers decode both — a peer re-mint of the OpenPBR algebra, the `ConductorIor` table, or the MaterialX schema is the named cross-language drift defect.

## [02]-[SEAMS]

```text seams
Appearance/interchange  →  typescript:interchange/codec     # [WIRE]: MaterialWire OpenPBR vector wire
Appearance/bsdf         →  csharp:Rasm.Bim/Semantics        # [CONTENT_KEY]: BimAppearance
Appearance/bsdf         ←  csharp:Rasm.Compute/Symbolic     # [PORT]: QuantityFamily illuminance for emission
Appearance/photometric  ←  csharp:Rasm.Compute/Symbolic     # [PORT]: QuantityFamily illuminance seam
Connection              →  csharp:Rasm.Bim/Model            # [WIRE]: ConnectionItem IFC wire IfcReinforcingBar/IfcMechanicalFastener
Properties              →  csharp:Rasm.Fabrication/Process  # [WIRE]: Thermal Conductivity / SpecificHeat / Density scalars
Construction/assembly   →  csharp:Rasm.Bim/Semantics        # [PROJECTION]: MaterialAssignment IFC trichotomy LayerSet/ProfileSet/ConstituentSet
Appearance/bsdf         →  csharp:Rasm.AppUi/Render         # [BOUNDARY]: LayeredBsdf shading at path tracer
Appearance/graph        →  csharp:Rasm.AppUi/Render         # [BOUNDARY]: SurfaceShade to path tracer
```

## [03]-[DOMAIN_LAW]

The canonical-collapse law the three domains share — one owner per axis, one entrypoint family per rail, growth by data. The per-page boundary cards carry the concrete seams; this map states only the collapse rule and the closed counts the codemap enforces.

- One owner per concept: a cross-section is a `ProfileFamily` row over one `Profile`, a material a `MaterialLibrary` row over one `MaterialParameters`/`MaterialGraph`, an appearance variation an `AppearanceNode` case, a lobe a `BsdfLobe` `[Union]` case, a layering modifier a `Slab` case over one `SlabStack`, a measured metal a `ConductorMetal` row over one `ConductorIor` table, a material wire the one `MaterialWire`/`MtlxDocument`, a construction element an `Element`, an assignment a `MaterialAssignment` case, a layout the one `ConstructionLayout.Resolve` fold. A `BrickProfile`/`SteelSection`/`GoldMaterial` class, a `MetalFactory`, a per-family `Profile`/graph/layout variant, a peer-side OpenPBR re-mint, or a generic `IMaterial`/`IProfile`/`IAppearance` abstraction is the named defect; growth is a row or a closed-union case, never a new surface.
- Two closed counts the codemap fixes: the `BsdfLobe` family is closed at seven (a new lobe admits only when no parameterization of the set reproduces the measured physics, and then serves ALL materials), and the `MaterialAssignment` trichotomy is closed at three (`IfcMaterialList` deprecated, never admitted; a fourth case or a per-element-type assignment is the defect).
- The construction model is host-neutral: a `Placement` is a scalar tuple and a `Layout` a `Seq<Element>` of placements, never a `Rhino.Geometry` curve/transform — the host materializes the stream at the future APP root and the assignment serializes to IFC at the `Rasm.Bim` boundary, never an interior `IfcOpenShell` evaluation.
- Composition over re-mint at every seam: `Rasm.Materials` references no host-boundary package and re-mints no color axis, unit owner, vector, or dimension — color is Wacton.Unicolour consumed directly as the scene-linear/spectral owner, the photometric unit coercion rides the Compute `QuantityFamily` seam coerced once at admission, and dimensions/frames the `Rasm` kernel value-objects; only the documented author-kernel set (RGB→SPD, RRT/ODT, scene-referred tone-map, BSDF/Fresnel/GGX/noise) is hand-authored, and an out-of-gamut, non-finite, or degenerate result rails to a banded fault (`ProfileFault` 2300, `ConstructionFault` 2350, `MaterialFault` 2450 — all disjoint from the kernel `GeometryFault` band 2400), never a propagated NaN or sentinel.
