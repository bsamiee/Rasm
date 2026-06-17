# [MATERIALS_ARCHITECTURE]

The professional-domain folder-map of `Rasm.Materials`, the host-neutral AEC-domain materials owner: every cross-section is a `ProfileFamily` row, every appearance a node case, every material a library row, every lobe a closed-union case, every layout the one placement fold — never a per-material or per-family type. The codemap names the full sub-domain structure across the three domains (`profiles/`, `appearance/`, `construction/`), each a real domain concept — never a rail/axis/lane file-naming scheme — including planned sub-domains that hold no design page yet, so a planned-but-thin folder reads as a visible gap that fuels an idea or task. Mechanics live in the `.planning/` design pages; this map carries the structure, the boundary law, and the prohibitions.

## [1]-[DOMAIN_MAP]

Each sub-domain mirrors one eventual source sub-tree. The charter is the concern the folder owns; the page line is the design page that has landed under it.

```text codemap
Rasm.Materials/
├── profiles/           One polymorphic Profile over a closed ProfileFamily axis (masonry realized; cmu/steel/timber/glazing planned), IfcProfileDef-aligned cross-section and the ProfileCatalogue registered-row table.
│   ├── profile.md        The Profile polymorphic owner, the ProfileFamily growth axis, ProfileId/ProfileUnit/ProfileStandard, ProfileFault band 2300, and the registered-row catalogue.
│   ├── masonry.md        The first realized ProfileFamily: void-class/bond/orientation/cut/closure/special-shape algebra and the regional ProfileCatalogue masonry rows.
│   ├── cmu/              [planned] The concrete-masonry-unit ProfileFamily: cell/face-shell unit columns as the second family.
│   ├── steel/            [planned] The steel ProfileFamily: AISC Shapes Database v16.0 section-property columns (depth/flange/web/fillet/Ix/Sx).
│   ├── timber/           [planned] The timber ProfileFamily: sawn/glulam/CLT lamella and grade columns.
│   └── glazing/          [planned] The glazing ProfileFamily: IGU pane/spacer/frame profile columns.
├── appearance/         One measured appearance engine: a node MaterialGraph to one shading sink, the closed BSDF lobe family, the MaterialLibrary row table, and texture plus photometric admission folds.
│   ├── bsdf.md          The closed seven-lobe BsdfLobe family and the scene-linear spectral edge: per-lobe Evaluate/Sample/Pdf, GGX + Smith masking, Fresnel, Kulla-Conty multi-scatter, RGB→SPD, ACES tone-map; OpenPBR-alignment target.
│   ├── graph.md         The node-DAG appearance program (AppearanceNode union, typed PortValue channels, Kahn fold to SurfaceShade) and the MaterialLibrary row table keyed by MaterialId.
│   ├── texture.md       One TextureUv sampling fold over the closed TextureSource union with closed address/filter bands and the author-kernel ProceduralNoise (Perlin/OpenSimplex2/Worley/fBm).
│   └── photometric.md   The light-unit admission fold over the closed PhotometricQuantity band composing Compute QuantityFamily, with EmissionSpectrum through the Unicolour color seam.
└── construction/       The host-neutral construction model: elements assigned materials by layer-set (walls/slabs), profile-set (members), or constituent-set (components), resolved to portable scalar placement data.
    ├── assembly.md      The IFC 4.3 material-assignment owner (MaterialAssignment layer-set/profile-set/constituent-set), the Element placed-unit model, RunPath length algebra, and ConstructionFault band 2350.
    └── layout.md        The one ConstructionLayout.Resolve fold over any ProfileFamily, the station/elevation course placement stream, JointPolicy, and the queued opening/corner/arch/pier stages.
```

Implementation collapses to one owner per axis and one entrypoint family per rail: a new cross-section is a `ProfileFamily` row, a new material a `MaterialLibrary` row, a new appearance variation a node case, a new lobe a `[Union]` case, a new layout stage a fold extension — never a new surface, and a public type outside an owner region is the named defect. One rail per entrypoint, named in the return type: a `SurfaceShade`/`Unicolour` carrier where the result is total, `Fin<T>` where a band-2300 `ProfileFault`, band-2350 `ConstructionFault`, or band-2400 `MaterialFault` can route. The four planned `profiles/` families carry no page yet — each is a visible growth gap driving the `STRUCTURAL_FAMILY_VOCABULARY` idea; the `construction/assembly` `MaterialAssignment` owner is the realized canonical owner the linear-run-only model lacked.

## [2]-[DOMAIN_LAW]

The canonical-collapse law the three domains share — one owner per axis, one entrypoint family per rail, growth by data. The per-page boundary cards carry the concrete seams; this map states only the collapse rule and the closed counts the codemap enforces.

- One owner per concept: a cross-section is a `ProfileFamily` row over one `Profile`, a material a `MaterialLibrary` row over one `MaterialParameters`/`MaterialGraph`, an appearance variation an `AppearanceNode` case, a lobe a `BsdfLobe` `[Union]` case, a construction element an `Element`, an assignment a `MaterialAssignment` case, a layout the one `ConstructionLayout.Resolve` fold. A `BrickProfile`/`SteelSection`/`GoldMaterial` class, a `MetalFactory`, a per-family `Profile`/graph/layout variant, or a generic `IMaterial`/`IProfile`/`IAppearance` abstraction is the named defect; growth is a row or a closed-union case, never a new surface.
- Two closed counts the codemap fixes: the `BsdfLobe` family is closed at seven (a new lobe admits only when no parameterization of the set reproduces the measured physics, and then serves ALL materials), and the `MaterialAssignment` trichotomy is closed at three (`IfcMaterialList` deprecated, never admitted; a fourth case or a per-element-type assignment is the defect).
- The construction model is host-neutral: a `Placement` is a scalar tuple and a `Layout` a `Seq<Element>` of placements, never a `Rhino.Geometry` curve/transform — the host materializes the stream at the future APP root and the assignment serializes to IFC at the `Rasm.Bim` boundary, never an interior `IfcOpenShell` evaluation.
- Composition over re-mint at every seam: `Rasm.Materials` references no host-boundary package and re-mints no color axis, unit owner, vector, or dimension — color is Wacton.Unicolour consumed directly as the scene-linear/spectral owner, the photometric unit coercion rides the Compute `QuantityFamily` seam coerced once at admission, and dimensions/frames the `Rasm` kernel value-objects; only the documented author-kernel set (RGB→SPD, RRT/ODT, scene-referred tone-map, BSDF/Fresnel/GGX/noise) is hand-authored, and an out-of-gamut, non-finite, or degenerate result rails to a banded fault (`ProfileFault` 2300, `ConstructionFault` 2350, `MaterialFault` 2400), never a propagated NaN or sentinel.
