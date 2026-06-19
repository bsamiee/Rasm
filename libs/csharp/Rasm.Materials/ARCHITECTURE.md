# [MATERIALS_ARCHITECTURE]

The professional domain map of `Rasm.Materials` — the host-neutral AEC-DOMAIN materials owner. One `ProfileFamily`/`MaterialLibrary`/`ConnectionFamily`/`MaterialProperty` row per concept across five sub-domains (`Profiles`, `Connection`, `Appearance`, `Construction`, `Properties`), never a per-material or per-family type.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [1]-[DOMAIN_MAP]

```text codemap
Rasm.Materials/
├── Profiles/             # one polymorphic Profile over a closed ProfileFamily axis
│   ├── Profile.cs        # the Profile polymorphic owner over the ProfileFamily growth axis
│   ├── Masonry.cs        # the masonry ProfileFamily with regional catalogue rows
│   ├── Steel.cs          # the steel ProfileFamily over the AISC v16.0 section-property record
│   ├── Cmu.cs            # the CMU ProfileFamily over the ASTM C90 cell/face-shell record
│   ├── Timber.cs         # the timber ProfileFamily over sawn/glulam/CLT lamella records
│   └── Glazing.cs        # the glazing ProfileFamily over pane/spacer/cavity records
├── Connection/           # one polymorphic ConnectionItem over the ConnectionFamily axis
│   ├── Connection.cs     # the ConnectionItem polymorphic owner over the ConnectionFamily axis
│   ├── Reinforcement.cs  # the reinforcement ConnectionFamily over the ASTM A615/A706 bar record
│   ├── Fastener.cs       # the fastener ConnectionFamily over the ISO 898/SAE J429 thread record
│   └── Hanger.cs         # the hanger ConnectionFamily over the carried-member/capacity record
├── Appearance/           # the measured appearance engine
│   ├── Bsdf.cs           # the closed seven-lobe BsdfLobe family and the scene-linear spectral edge
│   ├── Graph.cs          # the node-DAG MaterialGraph program and the MaterialLibrary row table
│   ├── Texture.cs        # the TextureUv sampling fold over the closed TextureSource union
│   ├── Photometric.cs    # the light-unit admission fold over the PhotometricQuantity band
│   ├── Weathering.cs     # the Weathering aging fold over the closed WeatheringEffect union
│   ├── Acquisition.cs    # the Acquisition import fold over the closed CaptureSource union
│   ├── Finish.cs         # the FinishMix Kubelka-Munk pigment-reflectance engine
│   └── Interchange.cs    # the MaterialWire and MaterialX .mtlx interchange projection
├── Construction/         # the host-neutral construction model
│   ├── Assembly.cs       # the IFC 4.3 material-assignment owner and the Element placed-unit model
│   └── Layout.cs         # the one ConstructionLayout.Resolve placement fold over any ProfileFamily
└── Properties/           # the typed engineering-property model
    └── Properties.cs     # the MaterialProperty closed family with the quantity-coercion fold
```

Implementation collapses to one owner per axis and one entrypoint family per rail: a new cross-section is a `ProfileFamily` row, a new material a `MaterialLibrary` row, a new lobe a `BsdfLobe` `[Union]` case, a new connection a `ConnectionFamily` row, a new engineering property a `MaterialProperty` case — never a new surface. The rail is named in the return type: a `SurfaceShade`/`Unicolour` carrier where the result is total, `Fin<T>` where a banded fault routes — `ProfileFault` 2300, `ConstructionFault` 2350, `ConnectionFault` 2360, `MaterialFault` 2450, all disjoint from the kernel `GeometryFault` band 2400. C# is the sole producer of the material wire: `Appearance/Interchange` `MaterialWire` and `MtlxDocument` mint the OpenPBR-vector and MaterialX `.mtlx` interchange once, and the TypeScript and Python peers decode both — a peer re-mint of the OpenPBR algebra, the `ConductorIor` table, or the MaterialX schema is the named cross-language drift defect.

## [2]-[SEAMS]

```text seams
Appearance/interchange  →  typescript:interchange/codec     # MaterialWire OpenPBR vector wire (wire)
Appearance/bsdf         →  csharp:Rasm.Bim/Semantics        # BimAppearance (content-key)
Appearance/bsdf         ←  csharp:Rasm.Compute/Symbolic     # QuantityFamily illuminance for emission (port)
Appearance/photometric  ←  csharp:Rasm.Compute/Symbolic     # QuantityFamily illuminance seam (port)
Connection              →  csharp:Rasm.Bim/Model            # ConnectionItem IFC wire IfcReinforcingBar/IfcMechanicalFastener (wire)
Properties              →  csharp:Rasm.Fabrication/Process  # Thermal Conductivity / SpecificHeat / Density scalars (wire)
Construction/assembly   →  csharp:Rasm.Bim/Semantics        # MaterialAssignment IFC trichotomy LayerSet/ProfileSet/ConstituentSet (projection)
Appearance/bsdf         →  csharp:Rasm.AppUi/Render         # LayeredBsdf shading at path tracer (boundary)
Appearance/graph        →  csharp:Rasm.AppUi/Render         # SurfaceShade to path tracer (boundary)
```

## [3]-[DOMAIN_LAW]

The canonical-collapse law the three domains share — one owner per axis, one entrypoint family per rail, growth by data. The per-page boundary cards carry the concrete seams; this map states only the collapse rule and the closed counts the codemap enforces.

- One owner per concept: a cross-section is a `ProfileFamily` row over one `Profile`, a material a `MaterialLibrary` row over one `MaterialParameters`/`MaterialGraph`, an appearance variation an `AppearanceNode` case, a lobe a `BsdfLobe` `[Union]` case, a layering modifier a `Slab` case over one `SlabStack`, a measured metal a `ConductorMetal` row over one `ConductorIor` table, a material wire the one `MaterialWire`/`MtlxDocument`, a construction element an `Element`, an assignment a `MaterialAssignment` case, a layout the one `ConstructionLayout.Resolve` fold. A `BrickProfile`/`SteelSection`/`GoldMaterial` class, a `MetalFactory`, a per-family `Profile`/graph/layout variant, a peer-side OpenPBR re-mint, or a generic `IMaterial`/`IProfile`/`IAppearance` abstraction is the named defect; growth is a row or a closed-union case, never a new surface.
- Two closed counts the codemap fixes: the `BsdfLobe` family is closed at seven (a new lobe admits only when no parameterization of the set reproduces the measured physics, and then serves ALL materials), and the `MaterialAssignment` trichotomy is closed at three (`IfcMaterialList` deprecated, never admitted; a fourth case or a per-element-type assignment is the defect).
- The construction model is host-neutral: a `Placement` is a scalar tuple and a `Layout` a `Seq<Element>` of placements, never a `Rhino.Geometry` curve/transform — the host materializes the stream at the future APP root and the assignment serializes to IFC at the `Rasm.Bim` boundary, never an interior `IfcOpenShell` evaluation.
- Composition over re-mint at every seam: `Rasm.Materials` references no host-boundary package and re-mints no color axis, unit owner, vector, or dimension — color is Wacton.Unicolour consumed directly as the scene-linear/spectral owner, the photometric unit coercion rides the Compute `QuantityFamily` seam coerced once at admission, and dimensions/frames the `Rasm` kernel value-objects; only the documented author-kernel set (RGB→SPD, RRT/ODT, scene-referred tone-map, BSDF/Fresnel/GGX/noise) is hand-authored, and an out-of-gamut, non-finite, or degenerate result rails to a banded fault (`ProfileFault` 2300, `ConstructionFault` 2350, `MaterialFault` 2450 — all disjoint from the kernel `GeometryFault` band 2400), never a propagated NaN or sentinel.
